using ZXing;
using ZXing.Common;
using System.Text.Json;
using System.Net;
using System.Text;

namespace DPkarta
{
    public partial class MainPage : ContentPage
    {
        int dpmhkID = 3903132;
        string snr = "";
        Services services = new();
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            if (SecureStorage.GetAsync("user").Result != null)
            {
                //logging out
                SecureStorage.Remove("user");
                ChangeScreens(true);
                Button.Text = "Login";
                return;
            }

            User user = services.Login(UsernameEntry.Text, PasswordEntry.Text, dpmhkID);
            if (user == null)
            {
                DisplayAlert("Error", "Wrong password", "OK");
            }
            else if (user.wertyzUser.cards.Count() == 1)
            {
                //logging in
                snr = user.wertyzUser.cards[0].snr;
                SecureStorage.SetAsync("user", snr);
                ChangeScreens(false);
                Button.Text = "Logout";
                LoadImage();
            }
            else if (user.wertyzUser.cards.Count() > 1)
            {
                CardPicker.ItemsSource = user.wertyzUser.cards.Select(c => c.ownerFirstName + " " + c.ownerLastName).ToList();
                CardPicker.IsVisible = true;
            }
            else
            {
                DisplayAlert("Error", "No card found", "OK");
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
            }
        }

        protected override void OnAppearing()
        {
            snr = SecureStorage.GetAsync("user").Result ?? "";
            if (snr != "")
            {
                //logging in
                ChangeScreens(false);
                Button.Text = "Logout";
                LoadImage();
            }
            base.OnAppearing();
        }

        private void LoadImage()
        {
            var card = services.GetCard(snr, dpmhkID).data;
            if (card == null)
            {
                //logging out
                DisplayAlert("Error", "No card found, logging out", "OK");
                SecureStorage.Remove("user");
                ChangeScreens(true);
                Button.Text = "Login";
                return;
            }
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
    }
}
