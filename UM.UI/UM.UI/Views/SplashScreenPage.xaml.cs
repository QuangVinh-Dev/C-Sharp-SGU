using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace UM.UI.Views
{
    public sealed partial class SplashScreenPage : Page
    {
        public SplashScreenPage()
        {
            this.InitializeComponent();
            this.Loaded += SplashScreenPage_Loaded;
        }

        private async void SplashScreenPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Bắt đầu các animation
            FadeInStoryboard.Begin();
            PulseStoryboard.Begin();
            DotsStoryboard.Begin();

            // Chờ 2000ms
            await Task.Delay(2000);

            // Bắt đầu fade out
            FadeOutStoryboard.Begin();
        }

        private void FadeOutStoryboard_Completed(object sender, object e)
        {
            // Dừng các animation lặp vô hạn
            PulseStoryboard.Stop();
            DotsStoryboard.Stop();
            
            // Chuyển sang LoginPage
            this.Frame.Navigate(typeof(LoginPage));
        }
    }
}
