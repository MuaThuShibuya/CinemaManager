using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CinemaManagement.Services
{
    public class MomoService
    {
        private readonly string partnerCode;
        private readonly string accessKey;
        private readonly string secretKey;
        private readonly string endpoint;
        private readonly string returnUrl;
        private readonly string notifyUrl;
        private readonly HttpClient _httpClient;

        public MomoService(IConfiguration configuration)
        {
            partnerCode = configuration["Momo:PartnerCode"];
            accessKey = configuration["Momo:AccessKey"];
            secretKey = configuration["Momo:SecretKey"];
            endpoint = configuration["Momo:Endpoint"];
            returnUrl = configuration["Momo:ReturnUrl"];
            notifyUrl = configuration["Momo:NotifyUrl"];

            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Tạo link thanh toán MoMo
        /// </summary>
        public async Task<string> GeneratePaymentQRCode(long amount, string orderId)
        {
            string requestId = Guid.NewGuid().ToString();
            string orderInfo = "Thanh toán vé xem phim";
            string extraData = ""; // rỗng vẫn gửi
            string requestType = "captureWallet";

            // Chuỗi tạo chữ ký chuẩn: thứ tự bắt buộc
            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";

            string signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
                signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash))).ToLower();

            var body = new
            {
                partnerCode,
                partnerName = "Cinema",
                storeId = "CinemaStore",
                requestId,
                amount = amount.ToString(),
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                extraData,
                requestType,
                signature,
                lang = "vi"
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"MoMo Response: {responseBody}");
            var result = JsonConvert.DeserializeObject<MomoResponse>(responseBody);

            if (result == null || result.ResultCode != 0)
                throw new Exception($"MoMo lỗi: {result?.Message} (Code {result?.ResultCode})");

            return result.PayUrl;
        }

        /// <summary>
        /// Tạo chữ ký HMAC SHA256
        /// </summary>
        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLower();
        }

        public class MomoResponse
        {
            [JsonProperty("payUrl")]
            public string PayUrl { get; set; }

            [JsonProperty("resultCode")]
            public int ResultCode { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
