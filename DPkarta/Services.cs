using System;
using System.Net;
using System.Text;
using System.Text.Json;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

namespace DPkarta
{
    public class Services
    {
        public string GetCookie()
        {
            var username = SecureStorage.GetAsync("username").Result;
            var password = SecureStorage.GetAsync("password").Result;
            if (username == null || password == null)
                return "logout";
            var result = Post(MainPage.loginURI, $"{{\"organizationSystemEntityId\": {MainPage.dpmhkID},\"login\":\"{username}\",\"password\":\"{password}\"}}", out Cookie? cookie);

            switch (result)
            {
                case "notfound":
                    return "nointernet";
                case null:
                case "error":
                    return "error";
                default:
                    if (cookie == null || cookie.Name == null || cookie.Value == null)
                        return "logout";
                    SecureStorage.SetAsync("cookie", cookie.ToString());
                    SecureStorage.SetAsync("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;

            }

            var user = JsonSerializer.Deserialize<User>(result);
            if (user == null || !user.success || user.wertyzUser == null)
                return "badlogincredentials";
            SecureStorage.SetAsync("user", user.wertyzUser.cards[0].snr);
            return "ok";
        }
        public string GetQRcodeString()
        {
            var snr = SecureStorage.GetAsync("user").Result;
            if (snr == null) return "nosnr";
            var cookiestring = SecureStorage.GetAsync("cookie").Result;
            if (cookiestring == null) return "nocookie";

            int index = cookiestring.IndexOf('=');
            Cookie cookie = new(cookiestring[..index], cookiestring[(index + 1)..]);

            if (cookie.Name == null || cookie.Value == null) return "badcookie";
            var errcode = CookiePost(MainPage.tokenURI, $"{{\"organizationSystemEntityId\": {MainPage.dpmhkID},\"cardSnr\": \"{snr}\"}}", cookie);
            return errcode switch
            {
                "notfound" => "nointernet",
                "error" => "badlogincredentials",
                _ => errcode,
            };
        }
        private string CookiePost(string url, string jsonBody, Cookie cookie)
        {
            try
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(url), cookie);
                var handler = new HttpClientHandler
                {
                    CookieContainer = cookieContainer
                };
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                using (var client = new HttpClient(handler))
                {
                    HttpResponseMessage response = client.PostAsync(url, content).Result;
                    return response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : "error";
                }

            }
            catch (Exception)
            {
                return "notfound";
            }
        }
        private string Post(string url, string jsonBody, out Cookie? cookie)
        {
            cookie = null;
            try
            {
                var client = new HttpClient();
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                cookie = new Cookie("connect.sid", response.Headers.GetValues("Set-Cookie")
                .Where(x => x.StartsWith("connect.sid="))
                .First()
                .Split(';')
                .FirstOrDefault()?
                ["connect.sid=".Length..]);


                return response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : "error";
            }
            catch (Exception)
            {

                return "notfound";
            }
        }
        public SvgRenderer.SvgImage GetQR(string data)
        {

            var options = new QrCodeEncodingOptions
            {
                Height = 250,
                Width = 250,
                Margin = 1,
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
                QrVersion = 5,
            };

            var writer = new BarcodeWriterSvg
            {

                Format = BarcodeFormat.QR_CODE,
                Options = options
            };

            return writer.Write(data);
        }
    }
}
