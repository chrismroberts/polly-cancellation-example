using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PollyCancellationTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _shouldRetry;
        private CancellationTokenSource _policyCTS;        

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartPolicy(object sender, RoutedEventArgs e)
        {
            _shouldRetry = true;
            var retryPolicy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(
                    sleepDurationProvider: (retry, ts) => { return TimeSpan.FromSeconds(5); },
                    onRetry: (Exception ex, int retry, TimeSpan ts, Context ctx) =>
                    {
                        UpdateStatus($"Retrying (retry count {retry})");
                        if (!_shouldRetry)
                        {
                            var cts = ctx["CancellationTokenSource"] as CancellationTokenSource;
                            cts.Cancel();
                        }
                    });

            _policyCTS = new CancellationTokenSource();
            var policyContext = new Context("RetryContext");

            policyContext.Add("CancellationTokenSource", _policyCTS);
            var policyResult = await retryPolicy.ExecuteAndCaptureAsync((ctx, ct) => DivideByZero(), policyContext, _policyCTS.Token);

            UpdateStatus($"Policy finished with Outcome type: {policyResult.Outcome}");
        }


        private Task<int> DivideByZero()
        {
            var zero = 0;
            return Task.FromResult(1 / zero);
        }

        private void CancelPolicyImmediately(object sender, RoutedEventArgs e)
        {
            _policyCTS.Cancel();
        }

        private void CancelOnNextRetry(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Policy cancellation requested for next retry..");
            _shouldRetry = false;
        }

        private void UpdateStatus(string text)
        {
            if (!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.Invoke(() => UpdateStatus(text));
                return;
            }

            tbStatus.Text = text;
        }
    }
}
