using System;
using System.Net;
using System.Text;
using System.Text.Json;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace DPkarta
{
    public class Services
    {
        string Post(string url, string jsonBody)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (!response.IsSuccessStatusCode)
                throw new Exception("Error: " + response.StatusCode);

            return response.Content.ReadAsStringAsync().Result;

        }

        public User Login(string login, string password, int dpmhkID)
        {
            string loginURI = "https://m.dpmhk.qrbus.me/api/auth/signIn";
            try
            {
                var user = JsonSerializer.Deserialize<User>(Post(loginURI, $"{{\"organizationSystemEntityId\": {dpmhkID},\"login\":\"{login}\",\"password\":\"{password}\"}}"));

                return user ?? new User();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public CardImage GetCard(string cardSnr, int dpmhkID)
        {
            string cardURI = "https://m.dpmhk.qrbus.me/api/rest/publiccards/encryptCardData";
            try
            {
                var card = JsonSerializer.Deserialize<CardImage>(Post(cardURI, $"{{\"organizationSystemEntityId\": {dpmhkID},\"cardSnr\": \"{cardSnr}\"}}"));
                return card ?? new CardImage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public SvgRenderer.SvgImage GetQR(string data)
        {
            var writer = new BarcodeWriterSvg
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 250,
                    Width = 250,
                    Margin = 1
                }
            };

            return writer.Write(data);
        }
    }
}
