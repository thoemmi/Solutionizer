using System.Diagnostics;
using System.IO;
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

        protected override string GetLogFolder() {
            return Path.GetTempPath();
        }

        protected override void ConfigureLogging(LoggingConfiguration config) {
            base.ConfigureLogging(config);

            if (Debugger.IsAttached) {
                var udpTarget = new NetworkTarget {
                    Address = "udp4://localhost:962",
                    Layout = new Log4JXmlEventLayout()
                };
                config.AddTarget("udp", udpTarget);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, udpTarget));
            }
        }
    }
}