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
                SecureStorage.Remove("user");
                QRWebView.IsVisible = false;
                UsernameEntry.IsVisible = true;
                PasswordEntry.IsVisible = true;
                RefreshButton.IsVisible = false;
                CardPicker.IsVisible = false;
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
                snr = user.wertyzUser.cards[0].snr;
                SecureStorage.SetAsync("user", snr);
                QRWebView.IsVisible = true;
                UsernameEntry.IsVisible = false;
                PasswordEntry.IsVisible = false;
                RefreshButton.IsVisible = true;
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
                snr = CardPicker.Items[CardPicker.SelectedIndex];
                SecureStorage.SetAsync("user", snr);
                QRWebView.IsVisible = true;
                UsernameEntry.IsVisible = false;
                PasswordEntry.IsVisible = false;
                RefreshButton.IsVisible = true;
                CardPicker.IsVisible = false;
                Button.Text = "Logout";
                LoadImage();
            }
        }

        protected override void OnAppearing()
        {
            snr = SecureStorage.GetAsync("user").Result ?? "";
            if (snr != "")
            {
                UsernameEntry.IsVisible = false;
                PasswordEntry.IsVisible = false;
                QRWebView.IsVisible = true;
                RefreshButton.IsVisible = true;
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
                DisplayAlert("Error", "No card found, logging out", "OK");
                SecureStorage.Remove("user");
                QRWebView.IsVisible = false;
                UsernameEntry.IsVisible = true;
                PasswordEntry.IsVisible = true;
                RefreshButton.IsVisible = false;
                Button.Text = "Login";
                return;
            }
            QRWebView.Source = new HtmlWebViewSource
            {
                Html = services.GetQR(card).Content
            };
        }

        private void RefreshButton_Clicked(object sender, EventArgs e) => LoadImage();
    }
}
