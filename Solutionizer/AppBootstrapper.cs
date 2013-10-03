using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caliburn.Micro;
using Caliburn.Micro.Logging.NLog;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Solutionizer.Services;
using LogManager = NLog.LogManager;

namespace Solutionizer {
    public class AppBootstrapper : Bootstrapper<IShell> {
        private CompositionContainer _container;
        private SettingsProvider _settingsProvider;

        private static readonly string _dataFolder;

        static AppBootstrapper() {
            if (!Execute.InDesignMode) {
                _dataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Assembly.GetEntryAssembly().GetName().Name);
            }
        }

        /// <summary>
        /// By default, we are configured to use MEF
        /// </summary>
        protected override void Configure() {
            if (!Directory.Exists(_dataFolder)) {
                Directory.CreateDirectory(_dataFolder);
            }

            _settingsProvider = new SettingsProvider(_dataFolder);

            ConfigureLogging();

            //AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            //    LogManager.GetCurrentClassLogger().ErrorException("FirstChanceException", args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => 
                LogManager.GetCurrentClassLogger().ErrorException("UnhandledException", args.ExceptionObject as Exception);
            Application.DispatcherUnhandledException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("DispatcherUnhandledException", args.Exception);
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                LogManager.GetCurrentClassLogger().ErrorException("UnobservedTaskException", args.Exception);

            var catalog = new AggregateCatalog(
                AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
                );

            _container = new CompositionContainer(catalog);

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(_settingsProvider.Settings);
            batch.AddExportedValue(_container);
            batch.AddExportedValue(catalog);

            _container.Compose(batch);
        }

        private static void ConfigureLogging() {
            var fileTarget = new FileTarget {
                FileName = Path.Combine(_dataFolder, "log.xml"),
                ArchiveFileName = "log_{#####}.xml",
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveAboveSize = 1024*1024,
                Layout = new Log4JXmlEventLayout()
            };

            var config = new LoggingConfiguration();
            config.AddTarget("file", fileTarget);

            var debuggerTarget = new DebuggerTarget();
            config.AddTarget("debugger", debuggerTarget);

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));
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

            Caliburn.Micro.LogManager.GetLog = type => new NLogLogger(type);

            PresentationTraceSources.DataBindingSource.Listeners.Add(new NLogTraceListener());
        }

        protected override object GetInstance(Type serviceType, string key) {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = _container.GetExportedValues<object>(contract);

            var export = exports.FirstOrDefault();
            if (export != null) {
                return export;
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType) {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance) {
            _container.SatisfyImportsOnce(instance);
        }

        protected override void OnExit(object sender, EventArgs e) {
            _settingsProvider.Save();
            base.OnExit(sender, e);
        }
    }
}