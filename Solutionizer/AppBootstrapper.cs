using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Logging.NLog;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Solutionizer.Infrastructure;
using Solutionizer.Services;
using LogManager = NLog.LogManager;
using PropertyChangedBase = Solutionizer.Infrastructure.PropertyChangedBase;

namespace Solutionizer {
    public class AppBootstrapper : Bootstrapper<IShell> {
        private SettingsProvider _settingsProvider;
        private IContainer _container;

        static AppBootstrapper() {
            if (!Execute.InDesignMode) {
            }
        }

        /// <summary>
        /// By default, we are configured to use MEF
        /// </summary>
        protected override void Configure() {
            _settingsProvider = new SettingsProvider();

            ConfigureLogging();

            //AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            //    LogManager.GetCurrentClassLogger().ErrorException("FirstChanceException", args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => 
                LogManager.GetCurrentClassLogger().ErrorException("UnhandledException", args.ExceptionObject as Exception);
            Application.DispatcherUnhandledException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("DispatcherUnhandledException", args.Exception);
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("UnobservedTaskException", args.Exception);

            _container = CreateContainer();}

        private IContainer CreateContainer() {
            var builder = new ContainerBuilder();

            builder.RegisterType<WindowManager>().As<IWindowManager>();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>();
            builder.RegisterInstance(_settingsProvider.Settings);

            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => ShouldRegister(t))
                .AsImplementedInterfaces();

            var container = builder.Build();
            return container;
        }

        private static bool ShouldRegister(Type t) {
            if (typeof (IScreen).IsAssignableFrom(t)) {
                return true;
            }
            if (typeof (PropertyChangedBase).IsAssignableFrom(t)) {
                return true;
            }
            if (typeof (Caliburn.Micro.PropertyChangedBase).IsAssignableFrom(t)) {
                return true;
            }
            return false;
        }

        private static void ConfigureLogging() {
            var fileTarget = new FileTarget {
                FileName = Path.Combine(AppEnvironment.DataFolder, "log.xml"),
                ArchiveFileName = "log_{#####}.xml",
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveAboveSize = 1024*1024,
                Layout = new Log4JXmlEventLayout()
            };

            var config = new LoggingConfiguration();
            config.AddTarget("file", fileTarget);
            SetLoggingRulesForTarget(config, fileTarget);

            var debuggerTarget = new DebuggerTarget();
            config.AddTarget("debugger", debuggerTarget);
            SetLoggingRulesForTarget(config, debuggerTarget);

            if (Debugger.IsAttached) {
                var udpTarget = new NetworkTarget {
                    Address = "udp4://localhost:962",
                    Layout = new Log4JXmlEventLayout()
                };
                config.AddTarget("udp", udpTarget);
                SetLoggingRulesForTarget(config, udpTarget);
            }

            LogManager.Configuration = config;

            Caliburn.Micro.LogManager.GetLog = type => new NLogLogger(type);

            PresentationTraceSources.DataBindingSource.Listeners.Add(new NLogTraceListener());
        }

        private static void SetLoggingRulesForTarget(LoggingConfiguration config, Target target) {
            config.LoggingRules.Add(new LoggingRule("Action", LogLevel.Warn, target) { Final = true });
            config.LoggingRules.Add(new LoggingRule("ActionMessage", LogLevel.Warn, target) { Final = true });
            config.LoggingRules.Add(new LoggingRule("ViewModelBinder", LogLevel.Warn, target) { Final = true });
            config.LoggingRules.Add(new LoggingRule("Screen", LogLevel.Warn, target) { Final = true });
            config.LoggingRules.Add(new LoggingRule("ConventionManager", LogLevel.Warn, target) { Final = true });
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
        }


        protected override object GetInstance(Type serviceType, string key) {
            return _container.Resolve(serviceType);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType) {
            var type = typeof (IEnumerable<>).MakeGenericType(serviceType);
            return (IEnumerable<object>) _container.Resolve(type);
        }

        protected override void BuildUp(object instance) {
            _container.InjectProperties(instance);
        }

        protected override void OnExit(object sender, EventArgs e) {
            _settingsProvider.Save();
            base.OnExit(sender, e);
        }
    }
}