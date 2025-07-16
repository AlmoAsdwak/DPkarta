using System.Globalization;
using System.Text.Json;
using System.Timers;
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
        public required System.Timers.Timer _timer;
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
                        LoadImage(false);
                        _timer?.Stop();
                        var datestring = SecureStorage.GetAsync("lastRefresh");
                        if (datestring.Result != null)
                        {
                            var lastRefresh = DateTime.ParseExact(datestring.Result, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            if (lastRefresh.AddSeconds(300).CompareTo(DateTime.Now) >= 0)
                            {
                                LoadImage(false);
                            }
                        }
                        StartTimer();
                        break;
                    case "sleep":
                        if (SecureStorage.GetAsync("user").Result == null)
                            break;
                        ScreenBrightness.Default.Brightness = normalBrightness;
                        _timer?.Stop();
                        break;
                }
            });
            if (SecureStorage.GetAsync("user").Result == null)
            {
                Logout();
                _timer?.Stop();
                return;
            }
            else
            {
                ChangeScreens(false);
                Button.Text = "Logout";
            }
            var datestring = SecureStorage.GetAsync("lastRefresh");
            if (datestring.Result != null)
            {
                var lastRefresh = DateTime.ParseExact(datestring.Result, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                if (lastRefresh.AddSeconds(300).CompareTo(DateTime.Now) >= 0)
                    LoadImage(false);
            }
            _timer?.Stop();
            if (SecureStorage.GetAsync("user").Result == null)
                return;
            StartTimer();
        }
        void StartTimer()
        {
            _timer = new System.Timers.Timer(30000); // 30,000 ms = 30 seconds
            _timer.Elapsed += (sender, e) => Bruh();
            _timer.AutoReset = true;
            _timer.Start();
        }
        private async void Bruh()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LoadImage(false);
            });
        }
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (SecureStorage.GetAsync("user").Result != null)
            {
                Logout();
                return;
            }
            _timer?.Stop();
            SecureStorage.RemoveAll();
            await SecureStorage.SetAsync("username", UsernameEntry.Text);
            await SecureStorage.SetAsync("password", PasswordEntry.Text);
            var errcode = services.GetCookie();

            switch (errcode)
            {
                case "badlogincredentials":
                case "logout":
                    Logout();
                    await DisplayAlert("Error", "Please log again", "OK");
                    return;
                case "error":
                    await DisplayAlert("Error", "contact developer", "OK");
                    return;
                case "nointernet":
                    await DisplayAlert("Error", "No internet", "OK");
                    return;
            }

            ChangeScreens(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Button.Text = "Logout";
            });
            await SecureStorage.SetAsync("lastRefresh", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LoadImage(false);
            });
            normalBrightness = ScreenBrightness.Default.Brightness;
            ScreenBrightness.Default.Brightness = 1;
            _timer?.Stop();
            StartTimer();
        }

        private void LoadImage(bool recursion)
        {

            var json = services.GetQRcodeString();
            switch (json)
            {
                case "nointernet":
                    DisplayAlert("Error", "No internet", "OK");
                    return;
                case "badcookie":
                case "badlogincredentials":
                case "nocookie":

                    switch (services.GetCookie())
                    {
                        case "badlogincredentials":
                        case "logout":
                            Logout();
                            DisplayAlert("Error", "Please log again", "OK");
                            return;
                        case "error":
                            DisplayAlert("Error", "contact developer", "OK");
                            return;
                        case "nointernet":
                            DisplayAlert("Error", "No internet", "OK");
                            return;
                    }
                    if (!recursion)
                        LoadImage(true);

                    return;
                case "nosnr":
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
            SecureStorage.SetAsync("lastRefresh", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        public void Logout()
        {
            _timer?.Stop();
            SecureStorage.RemoveAll();
            ChangeScreens(true);
            if (normalBrightness != 1)
                ScreenBrightness.Default.Brightness = normalBrightness;
        }
        private void ChangeScreens(bool isLogin)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
           {
               Button.Text = isLogin ? "Login" : "Logout";
               LabelA.IsVisible = isLogin;
               FrameA.IsVisible = isLogin;
               FrameB.IsVisible = isLogin;
               UsernameEntry.IsVisible = isLogin;
               PasswordEntry.IsVisible = isLogin;
               QRWebView.IsVisible = !isLogin;
               RefreshButton.IsVisible = !isLogin;
           });
        }
        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            MainThread.InvokeOnMainThreadAsync(() => { LoadImage(false); });
        }
        private async void OpenPrivacyPolicy(object sender, EventArgs e) => await Launcher.OpenAsync(new Uri("http://whoisalmo.cz/soukromi"));

    }
}
