using System.IO;
using Autofac;
using Solutionizer.Framework;
using Solutionizer.Services;
using Solutionizer.ViewModels;
using Solutionizer.Views;

namespace Solutionizer {
    public class AppBootstrapper : BootstrapperBase<IShell> {
        protected override void ConfigureContainer(ContainerBuilder builder) {
            base.ConfigureContainer(builder);

            builder.RegisterType<ShellViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ShellView>().SingleInstance();
            builder.Register(c => new SettingsProvider()).SingleInstance();
            builder.Register(c => c.Resolve<SettingsProvider>().Settings).As<ISettings>().SingleInstance();

            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => t.Name.EndsWith("View") || t.Name.EndsWith("ViewModel"))
                .AsSelf().SingleInstance();
        }

        protected override string GetLogFolder() {
            return Path.GetTempPath();
        }
    }
}