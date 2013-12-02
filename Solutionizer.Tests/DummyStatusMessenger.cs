using Solutionizer.Services;

namespace Solutionizer.Tests {
    internal class DummyStatusMessenger : IStatusMessenger{
        public void Show(string status) {}
    }
}