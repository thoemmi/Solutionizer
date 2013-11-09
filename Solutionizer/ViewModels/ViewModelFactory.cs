using System;
using System.Collections.Generic;
using System.Windows.Input;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public interface IViewModelFactory {
        SettingsViewModel CreateSettingsViewModel();
        AboutViewModel CreateAboutViewModel();
        UpdateViewModel CreateUpdateViewModel(bool checkForUpdates);
        ProjectRepositoryViewModel CreateProjectRepositoryViewModel(ICommand doubleClickCommand);
        FileScanningViewModel CreateFileScanningViewModel(string path);
        SolutionViewModel CreateSolutionViewModel(string rootPath, IDictionary<string, Project> projects);
    }

    public class ViewModelFactory : IViewModelFactory {
        private readonly Func<SettingsViewModel> _getSettingsViewModel;
        private readonly Func<AboutViewModel> _getAboutViewModel;
        private readonly UpdateViewModel.Factory _getUpdateViewModel;
        private readonly ProjectRepositoryViewModel.Factory _getProjectRepositoryViewModel;
        private readonly FileScanningViewModel.Factory _getFileScanningViewModel;
        private readonly SolutionViewModel.Factory _getSolutionViewModel;

        public ViewModelFactory(
            Func<SettingsViewModel> getSettingsViewModel,
            Func<AboutViewModel> getAboutViewModel, 
            UpdateViewModel.Factory getUpdateViewModel, 
            ProjectRepositoryViewModel.Factory getProjectRepositoryViewModel,
            FileScanningViewModel.Factory getFileScanningViewModel,
            SolutionViewModel.Factory getSolutionViewModel) {
            _getSettingsViewModel = getSettingsViewModel;
            _getAboutViewModel = getAboutViewModel;
            _getUpdateViewModel = getUpdateViewModel;
            _getProjectRepositoryViewModel = getProjectRepositoryViewModel;
            _getFileScanningViewModel = getFileScanningViewModel;
            _getSolutionViewModel = getSolutionViewModel;
        }

        public SettingsViewModel CreateSettingsViewModel() {
            return _getSettingsViewModel();
        }

        public AboutViewModel CreateAboutViewModel() {
            return _getAboutViewModel();
        }

        public UpdateViewModel CreateUpdateViewModel(bool checkForUpdates) {
            return _getUpdateViewModel(checkForUpdates);
        }

        public ProjectRepositoryViewModel CreateProjectRepositoryViewModel(ICommand doubleClickCommand) {
            return _getProjectRepositoryViewModel(doubleClickCommand);
        }

        public FileScanningViewModel CreateFileScanningViewModel(string path) {
            return _getFileScanningViewModel(path);
        }

        public SolutionViewModel CreateSolutionViewModel(string rootPath, IDictionary<string, Project> projects) {
            return _getSolutionViewModel(rootPath, projects);
        }
    }
}