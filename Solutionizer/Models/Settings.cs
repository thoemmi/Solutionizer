using System;
using System.IO;
using System.Reflection;
using System.Windows.Markup;
using System.Xml;
using GalaSoft.MvvmLight;

namespace Solutionizer.Models {
    public class Settings : ViewModelBase {
        private static readonly string _settingsPath;
        private bool _scanOnStartup = true;
        private bool _isFlatMode;
        private bool _hideRootNode;
        private string _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Visual Studio 2010", "Projects");
        private bool _isDirty;

        static Settings() {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Assembly.GetEntryAssembly().GetName().Name,
                "settings.xml");
        }

        public static Settings LoadSettings() {
            Settings settings;
            if (File.Exists(_settingsPath)) {
                using (var stream = File.OpenRead(_settingsPath)) {
                    settings = (Settings) XamlReader.Load(stream);
                }
            } else {
                settings = new Settings();
            }
            settings.IsDirty = false;
            return settings;
        }

        public void Save() {
            if (!IsDirty) {
                return;
            }

            var path = Path.GetDirectoryName(_settingsPath);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            using (var stream = XmlWriter.Create(_settingsPath, new XmlWriterSettings {Indent = true, NewLineOnAttributes = true})) {
                XamlWriter.Save(this, stream);
            }

            IsDirty = false;
        }

        public bool IsFlatMode {
            get { return _isFlatMode; }
            set {
                if (_isFlatMode != value) {
                    _isFlatMode = value;
                    RaisePropertyChanged(() => IsFlatMode);
                    IsDirty = true;
                }
            }
        }

        public bool HideRootNode {
            get { return _hideRootNode; }
            set {
                if (_hideRootNode != value) {
                    _hideRootNode = value;
                    RaisePropertyChanged(() => HideRootNode);
                    IsDirty = true;
                }
            }
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    RaisePropertyChanged(() => RootPath);
                    IsDirty = true;
                }
            }
        }

        public bool IsDirty {
            get { return _isDirty; }
            private set {
                if (_isDirty != value) {
                    _isDirty = value;
                    RaisePropertyChanged(() => IsDirty);
                }
            }
        }

        public bool ScanOnStartup {
            get { return _scanOnStartup; }
            set {
                if (_scanOnStartup != value) {
                    _scanOnStartup = value;
                    RaisePropertyChanged(() => ScanOnStartup);
                }
            }
        }
    }
}