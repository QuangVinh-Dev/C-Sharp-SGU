using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace UM.UI.Views
{
    public sealed partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            this.InitializeComponent();
            Loaded += ForgotPasswordPage_Loaded;
        }

        private void ForgotPasswordPage_Loaded(object sender, RoutedEventArgs e)
        {
            SharedShadow.Receivers.Add(ShadowReceiver);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }
    }
}
