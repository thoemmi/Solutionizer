using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autofac;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Solutionizer.Infrastructure;
using Solutionizer.Services;
using Solutionizer.ViewModels;
using Solutionizer.Views;
using TinyLittleMvvm;

namespace Solutionizer {
    public class AppBootstrapper : BootstrapperBase<IShell> {
        public AppBootstrapper() {
            InitializeLogging();
        }

        protected override void ConfigureContainer(ContainerBuilder builder) {
            base.ConfigureContainer(builder);

            builder.RegisterType<ShellViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ShellView>().SingleInstance();
            builder.RegisterType<UpdateManager>().SingleInstance().As<IUpdateManager>();
            builder.RegisterType<ViewModelFactory>().SingleInstance().As<IViewModelFactory>();
            builder.Register(c => new SettingsProvider()).SingleInstance();
            builder.Register(c => c.Resolve<SettingsProvider>().Settings).As<ISettings>().SingleInstance();
            builder.RegisterType<GithubReleaseProvider>().SingleInstance().As<IReleaseProvider>();
            builder.RegisterType<MostRecentUsedFoldersRepository>().SingleInstance().As<IMostRecentUsedFoldersRepository>();

            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => (t.Name.EndsWith("View") || t.Name.EndsWith("ViewModel")) && !t.Name.StartsWith("Shell"))
                .AsSelf();
        }

        private static void InitializeLogging()
        {
           var config = new LoggingConfiguration();

           var debuggerTarget = new DebuggerTarget();
           config.AddTarget("debugger", debuggerTarget);
           config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debuggerTarget));

           var logFolder = Path.Combine(
              Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
              Assembly.GetEntryAssembly().GetName().Name,
              "Logs");

           if (!Directory.Exists(logFolder))
           {
              Directory.CreateDirectory(logFolder);
           }

           var fileTarget = new FileTarget
           {
              FileName = Path.Combine(logFolder, "log.xml"),
              ArchiveFileName = "log_{#####}.xml",
              ArchiveNumbering = ArchiveNumberingMode.Rolling,
              ArchiveAboveSize = 1024 * 1024,
              Layout = new Log4JXmlEventLayout()
           };
           config.AddTarget("file", fileTarget);
           config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

           LogManager.Configuration = config;

           PresentationTraceSources.DataBindingSource.Listeners.Add(new NLogTraceListener());
        }
   }
}