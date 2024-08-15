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
        public string Post(string url, string jsonBody)
        {
            try
            {
                HttpClient client = new();
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync(url, content).Result;

                if (!response.IsSuccessStatusCode)
                    return "error";

                return response.Content.ReadAsStringAsync().Result;
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
