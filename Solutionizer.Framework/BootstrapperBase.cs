using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Solutionizer.Framework {
    public abstract class BootstrapperBase {
        protected BootstrapperBase() {
            Start();

            Application.Current.Startup += OnStartup;
            Application.Current.Exit += OnExit;
        }

        private void Start() {
            ConfigureLogging();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("UnhandledException", args.ExceptionObject as Exception);
            Application.Current.DispatcherUnhandledException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("DispatcherUnhandledException", args.Exception);
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("UnobservedTaskException", args.Exception);
            Application.Current.DispatcherUnhandledException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("DispatcherUnhandledException", args.Exception);

            Container = CreateContainer();
        }

        protected abstract string GetLogFolder();

        private void ConfigureLogging() {
            var fileTarget = new FileTarget {
                FileName = Path.Combine(GetLogFolder(), "log.xml"),
                ArchiveFileName = "log_{#####}.xml",
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveAboveSize = 1024*1024,
                Layout = new Log4JXmlEventLayout()
            };

            var config = new LoggingConfiguration();
            config.AddTarget("file", fileTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

            var debuggerTarget = new DebuggerTarget();
            config.AddTarget("debugger", debuggerTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debuggerTarget));

            if (Debugger.IsAttached) {
                var udpTarget = new NetworkTarget {
                    Address = "udp4://localhost:962",
                    Layout = new Log4JXmlEventLayout()
                };
                config.AddTarget("udp", udpTarget);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, udpTarget));
            }

            LogManager.Configuration = config;

            PresentationTraceSources.DataBindingSource.Listeners.Add(new NLogTraceListener());
        }

        private IContainer CreateContainer() {
            var builder = new ContainerBuilder();

            builder.RegisterModule<LogRequestsModule>();
            builder.RegisterType<WindowManager>().SingleInstance();
            builder.RegisterType<FlyoutManager>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DialogManager>().AsImplementedInterfaces().SingleInstance();

            ConfigureContainer(builder);

            var container = builder.Build();
            return container;
        }

        static protected internal IContainer Container { get; private set; }

        protected virtual void ConfigureContainer(ContainerBuilder builder) {
        }

        protected virtual void OnStartup(object sender, StartupEventArgs e) {
        }

        protected virtual void OnExit(object sender, ExitEventArgs e) {
        }
    }

    public abstract class BootstrapperBase<TViewModel> : BootstrapperBase {
        protected override void OnStartup(object sender, StartupEventArgs e) {
            Container.Resolve<WindowManager>().ShowWindow<TViewModel>();
        }
    }
}