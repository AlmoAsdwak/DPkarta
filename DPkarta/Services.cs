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
        public string CookieAttempt()
        {
            string json = GetCookie();
            switch (json)
            {
                //TODO do it
                case "nologinstored":
                    break;
                case "error":
                    break;
                case "nointernet":
                    break;
            }
            var user = JsonSerializer.Deserialize<User>(json);
            if (user == null || !user.success || user.wertyzUser == null)
                return "badlogincredentials";
            SecureStorage.SetAsync("user", user.wertyzUser.cards[0].snr);
            return "ok";
        }
        public string GetCookie()
        {
            var username = SecureStorage.GetAsync("username").Result;
            var password = SecureStorage.GetAsync("password").Result;
            if (username == null || password == null)
                return "nologinstored";
            var errcode = Post(MainPage.tokenURI, $"{{\"organizationSystemEntityId\": {MainPage.dpmhkID},\"login\":\"{username}\",\"password\":\"{password}\"}}", out Cookie? cookie);
            if (cookie == null)
                return "error";
            switch (errcode)
            {
                case "notfound":
                    return "nointernet";
                case "error":
                    return "error";
                default:
                    SecureStorage.SetAsync("cookie", cookie.ToString());
                    return "ok";

            }
        }
        public string GetIdentification()
        {
            var snr = SecureStorage.GetAsync("user").Result;
            if (snr == null) return "nosnr";
            var cookiestring = SecureStorage.GetAsync("cookie").Result;
            if (cookiestring == null) return "nocookie";
            Cookie cookie = Cookiefier(cookiestring);
            if (cookie.Name == null || cookie.Value == null) return "badcookie";
            var errcode = CookiePost(MainPage.loginURI, $"{{\"organizationSystemEntityId\": {MainPage.dpmhkID},\"cardSnr\": \"{snr}\"}}", cookie);
            return errcode switch
            {
                "notfound" => "nointernet",
                "error" => "badlogincredentials",
                _ => errcode,
            };
        }
        public static Cookie Cookiefier(string cookie)
        {
            int index = cookie.IndexOf('=');
            return new Cookie(cookie[..index], cookie[(index + 1)..]);
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
                cookie = null;
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
