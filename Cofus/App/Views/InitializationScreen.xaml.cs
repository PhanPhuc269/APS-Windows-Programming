using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System.Threading;

namespace App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InitializationScreen : Page
    {
        private string _currentTime;

        public InitializationScreen()
        {
            this.InitializeComponent();
            ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromSeconds(1));
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            // Update _currentTime on the main UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                _currentTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                timeTextBlock.Text = _currentTime; // Update TextBlock with current time
            });
        }
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                UIElement? shell = App.GetService<AuthenticationPage>();
                App.MainWindow.Content = shell ?? new Frame();
            }
            catch (Exception ex)
            {
                // Log error details to the output
                System.Diagnostics.Debug.WriteLine("Navigation failed: " + ex.ToString());
            }
        }


    }
}
