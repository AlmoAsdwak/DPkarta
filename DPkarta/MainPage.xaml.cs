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
        string loginURI = "https://m.dpmhk.qrbus.me/api/auth/signIn";
        public float normalBrightness = 1;
        const int dpmhkID = 3903132;
        string snr = "";
        Services services = new();
        public MainPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<DPkarta.Message>(this, (sender, args) =>
            {
                switch (args.message)
                {
                    case "resume":
                        normalBrightness = ScreenBrightness.Default.Brightness;
                        ScreenBrightness.Default.Brightness = 1;
                        LoadImage();
                        break;
                    case "sleep":
                        ScreenBrightness.Default.Brightness =normalBrightness;
                        break;
                }
            });
        }
        private void OnLoginClicked(object sender, EventArgs e)
        {
            if (SecureStorage.GetAsync("user").Result != null)
            {
                //logging out
                SecureStorage.Remove("user");
                ChangeScreens(true);
                Button.Text = "Login";
                ScreenBrightness.Default.Brightness = normalBrightness;
                return;
            }

            var json = services.Post(loginURI, $"{{\"organizationSystemEntityId\": {dpmhkID},\"login\":\"{UsernameEntry.Text}\",\"password\":\"{PasswordEntry.Text}\"}}");
            if (json == "notfound")
            {
                DisplayAlert("Error", "No internet connection", "OK");
                return;
            }
            if (json == "error")
            {
                DisplayAlert("Error", "Something went wrong, please contant developer", "OK");
                return;
            }
            if (json.Contains("loginError.password") || json.Contains("loginError.login"))
            {
                DisplayAlert("Error", "Wrong password", "OK");
                return;
            }

            var user = JsonSerializer.Deserialize<User>(json);

            if (user == null || !user.success || user.wertyzUser == null)
            {
                DisplayAlert("Error", "Wrong password", "OK");
                return;
            }

            switch (user)
            {
                case { wertyzUser.cards.Length: 1 }:
                    snr = user.wertyzUser.cards[0].snr;
                    SecureStorage.SetAsync("user", snr);
                    ChangeScreens(false); Button.Text = "Logout";
                    LoadImage();
                    normalBrightness = ScreenBrightness.Default.Brightness;
                    ScreenBrightness.Default.Brightness = 1;
                    break;
                case { wertyzUser.cards.Length: > 1 }:
                    CardPicker.ItemsSource = user.wertyzUser.cards.Select(c => c.ownerFirstName + " " + c.ownerLastName).ToList();
                    CardPicker.IsVisible = true;
                    break;
                default: DisplayAlert("Error", "No card found", "OK"); break;
            }
        }
        private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (CardPicker.SelectedIndex != -1)
            {
                //logging in
                snr = CardPicker.Items[CardPicker.SelectedIndex];
                SecureStorage.SetAsync("user", snr);
                ChangeScreens(false);
                Button.Text = "Logout";
                LoadImage();
                normalBrightness = ScreenBrightness.Default.Brightness;
                ScreenBrightness.Default.Brightness = 1;
            }
        }
        protected override void OnAppearing()
        {
            try
            {
                snr = SecureStorage.GetAsync("user").Result ?? "";
                if (snr != "")
                {
                    //logging in
                    ChangeScreens(false);
                    Button.Text = "Logout";
                    LoadImage();
                    normalBrightness = ScreenBrightness.Default.Brightness;
                    ScreenBrightness.Default.Brightness = 1;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                base.OnAppearing();
            }

        }
        private void LoadImage()
        {
            string cardURI = "https://m.dpmhk.qrbus.me/api/rest/publiccards/encryptCardData";

            var json = services.Post(cardURI, $"{{\"organizationSystemEntityId\": {dpmhkID},\"cardSnr\": \"{snr}\"}}");
            if (json == "notfound")
            {
                DisplayAlert("Error", "No internet connection", "OK");
                return;
            }
            if (json == "error")
            {
                DisplayAlert("Error", "Something went wrong, please contant developer", "OK");
                return;
            }

            var cardstruct = JsonSerializer.Deserialize<CardImage>(json);



            if (cardstruct == null || !cardstruct.success || cardstruct.data == null)
            {
                //logging out
                DisplayAlert("Error", "No card found, logging out", "OK");
                SecureStorage.Remove("user");
                ChangeScreens(true);
                Button.Text = "Login";
                return;
            }

            var card = cardstruct.data;


            QRWebView.Source = new HtmlWebViewSource
            {
                Html = services.GetQR(card).Content
            };
        }
        private void ChangeScreens(bool isLogin)
        {
            LabelA.IsVisible = isLogin;
            FrameA.IsVisible = isLogin;
            FrameB.IsVisible = isLogin;
            QRWebView.IsVisible = !isLogin;
            UsernameEntry.IsVisible = isLogin;
            PasswordEntry.IsVisible = isLogin;
            RefreshButton.IsVisible = !isLogin;
        }
        private void RefreshButton_Clicked(object sender, EventArgs e) => LoadImage();
        private async void OpenPrivacyPolicy(object sender, EventArgs e) => await Launcher.OpenAsync(new Uri("http://whoisalmo.cz/soukromi"));

    }
}
