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
        public string CookiePost(string url, string jsonBody, Cookie cookie)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri(url), cookie);
            var handler = new HttpClientHandler
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

        public string Post(string url, string jsonBody, out Cookie? cookie)
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
