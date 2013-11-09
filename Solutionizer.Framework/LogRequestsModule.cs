using Autofac;
using Autofac.Core;
using NLog;

namespace Solutionizer.Framework {
    internal class LogRequestsModule : Module {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
            registration.Preparing += (sender, args) =>
                _log.Debug("Resolving concrete type {0}", args.Component.Activator.LimitType);
        }
    }
}