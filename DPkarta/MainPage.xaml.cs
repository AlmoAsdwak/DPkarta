﻿using ZXing;
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
                        ScreenBrightness.Default.Brightness = normalBrightness;
                        break;
                }
            });
        }
        private void OnLoginClicked(object sender, EventArgs e)
        {
            if (SecureStorage.GetAsync("user").Result != null)
            {
                //logging out
                SecureStorage.RemoveAll();
                ChangeScreens(true);
                Button.Text = "Login";
                ScreenBrightness.Default.Brightness = normalBrightness;
                return;
            }
            SecureStorage.SetAsync("username", UsernameEntry.Text);
            SecureStorage.SetAsync("password", PasswordEntry.Text);
            var errcode = services.CookieAttempt();

            switch (errcode)
            {
                //TODO ..
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
