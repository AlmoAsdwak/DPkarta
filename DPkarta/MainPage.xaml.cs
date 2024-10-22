using ZXing;
using ZXing.Common;
using System.Text.Json;
using System.Net;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.ScreenBrightness;
namespace DPkarta
{
    public partial class MainPage : ContentPage
    {
        public static string loginURI = "https://m.dpmhk.qrbus.me/api/auth/signIn";
        public static string tokenURI = "https://m.dpmhk.qrbus.me/api/rest/publiccards/encryptCardData";
        public float normalBrightness = 1;
        public const int dpmhkID = 3903132;
        Services services = new();
        public MainPage()
        {
            InitializeComponent();
            normalBrightness = ScreenBrightness.Default.Brightness;
            WeakReferenceMessenger.Default.Register<DPkarta.Message>(this, (sender, args) =>
            {
                switch (args.message)
                {
                    case "resume":
                        if (SecureStorage.GetAsync("user").Result == null)
                            break;

                        normalBrightness = ScreenBrightness.Default.Brightness;
                        ScreenBrightness.Default.Brightness = 1;
                        LoadImage();
                        break;
                    case "sleep":
                        if (SecureStorage.GetAsync("user").Result == null)
                            break;
                        ScreenBrightness.Default.Brightness = normalBrightness;
                        break;
                }
            });
        }
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (SecureStorage.GetAsync("user").Result != null)
            {
                Logout();
                return;
            }
            await SecureStorage.SetAsync("username", UsernameEntry.Text);
            await SecureStorage.SetAsync("password", PasswordEntry.Text);
            var errcode = services.CookieAttempt();

            switch (errcode)
            {
                case "nologinstored":
                    Logout();
                    await DisplayAlert("Error", "Logging out, contact developer", "OK");
                    return;
                case "error":
                    await DisplayAlert("Error", "contact developer", "OK");
                    return;
                case "nointernet":
                    await DisplayAlert("Error", "No internet", "OK");
                    return;
            }

            ChangeScreens(false);
            Button.Text = "Logout";
            LoadImage();
            normalBrightness = ScreenBrightness.Default.Brightness;
            ScreenBrightness.Default.Brightness = 1;
        }

        private void LoadImage()
        {

            var json = services.GetIdentification();
            switch (json)
            {
                case "nointernet":
                    DisplayAlert("Error", "No internet", "OK");
                    return;
                case "nocookie":
                    switch (services.CookieAttempt())
                    {
                        case "error":
                        case "nologinstored":
                            Logout();
                            DisplayAlert("Error", "Logging out, contact developer", "OK");
                            return;
                        case "nointernet":
                            DisplayAlert("Error", "No internet", "OK");
                            return;
                    }
                    break;
                case "nosnr":
                case "badlogincredentials":
                case "badcookie":
                    Logout();
                    return;
            }

            var cardstruct = JsonSerializer.Deserialize<CardImage>(json);
            if (cardstruct == null || !cardstruct.success || cardstruct.data == null)
            {
                Logout();
                return;
            }

            var card = cardstruct.data;
            QRWebView.Source = new HtmlWebViewSource
            {
                Html = services.GetQR(card).Content
            };
        }
        public void Logout()
        {
            SecureStorage.RemoveAll();
            ChangeScreens(true);
            if (normalBrightness != 1)
                ScreenBrightness.Default.Brightness = normalBrightness;
        }
        private void ChangeScreens(bool isLogin)
        {
            Button.Text = isLogin ? "Login" : "Logout";
            LabelA.IsVisible = isLogin;
            FrameA.IsVisible = isLogin;
            FrameB.IsVisible = isLogin;
            UsernameEntry.IsVisible = isLogin;
            PasswordEntry.IsVisible = isLogin;
            QRWebView.IsVisible = !isLogin;
            RefreshButton.IsVisible = !isLogin;
        }
        private void RefreshButton_Clicked(object sender, EventArgs e) => LoadImage();
        private async void OpenPrivacyPolicy(object sender, EventArgs e) => await Launcher.OpenAsync(new Uri("http://whoisalmo.cz/soukromi"));

    }
}
