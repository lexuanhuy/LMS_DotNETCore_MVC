using LMS_DotNETCore_MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text.Json;

namespace LMS_DotNETCore_MVC.Services
{
    public class MoMoService : IMoMoService
    {
        private readonly IOptions<MoMoOptionModel> _options;

        public MoMoService(IOptions<MoMoOptionModel> options)
        {
            _options = options;
        }

        public async Task<string> CreatePaymentAsync(Order order, HttpContext httpContext)
        {
            var model = _options.Value;

            string endpoint = model.PaymentUrl;
            string partnerCode = model.PartnerCode;
            string accessKey = model.AccessKey;
            string secretKey = model.SecretKey;
            string orderInfo = order.OrderInfo ?? "Thanh toan khoa hoc";
            string redirectUrl = model.ReturnUrl;
            string ipnUrl = model.IpnUrl;
            string requestType = "captureWallet";

            string amount = order.TotalAmount.ToString("0");
            string orderId = order.Id.ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType;

            string signature = GenerateHmacSha256(rawHash, secretKey);

            var message = new
            {
                partnerCode = partnerCode,
                partnerName = "Test",
                storeId = "MomoTestStore",
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = redirectUrl,
                ipnUrl = ipnUrl,
                lang = "vi",
                extraData = extraData,
                requestType = requestType,
                signature = signature
            };

            using var httpClient = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(endpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();

            var responseData = JsonSerializer.Deserialize<MoMoResponseModel>(responseString);

            return responseData?.payUrl ?? "";
        }

        public bool VerifySignature(string signature, string rawHash)
        {
            string expectedSignature = GenerateHmacSha256(rawHash, _options.Value.SecretKey);
            return signature.Equals(expectedSignature);
        }

        private string GenerateHmacSha256(string message, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
    }

    public class MoMoResponseModel
    {
        public string payUrl { get; set; }
    }
}
