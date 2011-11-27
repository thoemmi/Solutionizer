using System;
using System.Threading.Tasks;
using System.Windows;

namespace Solutionizer.Commands {
    public class CommandExecutor : DependencyObject {
        private readonly TaskScheduler _scheduler;
        private static CommandExecutor _instance;

        public static void ExecuteAsync<T>(string message, Func<T> command, Action<T> callback) {
            if (_instance == null) {
                throw new InvalidOperationException("No instance set");
            }
            _instance.ExecuteAsyncInternal(message, command, callback);
        }

        private void ExecuteAsyncInternal<T>(string message, Func<T> command, Action<T> callback) {
            Dispatcher.Invoke((Action) (() => {
                BusyMessage = message;
                IsBusy = true;
            }));
            Task.Factory.StartNew(command)
                .ContinueWith(result => {
                    IsBusy = false;
                    BusyMessage = String.Empty;
                    callback(result.Result);
                }, _scheduler);
        }

        public CommandExecutor() {
            if (_instance != null) {
                throw new InvalidOperationException("Only one instance allowed");
            }
            _instance = this;
            _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof (bool), typeof (CommandExecutor),
                                        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsBusy {
            get { return (bool) GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty BusyMessageProperty =
            DependencyProperty.Register("BusyMessage", typeof (string), typeof (CommandExecutor), new PropertyMetadata(default(string)));

        public string BusyMessage {
            get { return (string) GetValue(BusyMessageProperty); }
            set { SetValue(BusyMessageProperty, value); }
        }
    }
}