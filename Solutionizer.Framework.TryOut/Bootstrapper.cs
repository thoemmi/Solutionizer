﻿using Autofac;

namespace Solutionizer.Framework.TryOut {
    public class Bootstrapper : BootstrapperBase<ShellViewModel> {
        protected override void ConfigureContainer(ContainerBuilder builder) {
            base.ConfigureContainer(builder);

            builder.RegisterType<ShellView>();
            builder.RegisterType<ShellViewModel>();
            builder.RegisterType<SubView>();
            builder.RegisterType<SubViewModel>();
        }

        protected override string GetLogFolder() {
            return System.IO.Path.GetTempPath();
        }
    }
}