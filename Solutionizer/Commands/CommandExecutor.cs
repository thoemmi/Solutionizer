using System;
using System.Threading.Tasks;
using System.Windows;

namespace Solutionizer.Commands {
    public class CommandExecutor : DependencyObject {
        private readonly TaskScheduler _scheduler;
        private static CommandExecutor _instance;

        public static Task<T> ExecuteAsync<T>(string message, Func<T> command) {
            if (_instance == null) {
                throw new InvalidOperationException("No instance set");
            }
            return _instance.ExecuteAsyncInternal(message, command);
        }

        private Task<T> ExecuteAsyncInternal<T>(string message, Func<T> command) {
            Dispatcher.Invoke((Action) (() => {
                BusyMessage = message;
                IsBusy = true;
            }));
            Task<T> task = Task.Factory.StartNew(command);
            task.ContinueWith(result => {
                    IsBusy = false;
                    BusyMessage = String.Empty;
                }, _scheduler);
            return task;
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