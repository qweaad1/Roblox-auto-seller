using RestSharp;
using RestSharp.Authenticators;

namespace qweaadAPI
{
    public static class UMoney
    {
        private static readonly object _lock = new();
        private static RestClient? _client;
        private static int _requestCount = 0;
        private static DateTime _resetTime = DateTime.UtcNow;

        private static RestClient GetClient()
        {
            lock (_lock)
            {
                if ((DateTime.UtcNow - _resetTime).TotalMinutes >= 1)
                {
                    _requestCount = 0;
                    _resetTime = DateTime.UtcNow;
                }

                if (_client == null)
                {
                    var options = new RestClientOptions("https://api.yookassa.ru")
                    {
                        Authenticator = new HttpBasicAuthenticator(GlobalData.shopId, GlobalData.apiKeyU),
                        Timeout = TimeSpan.FromSeconds(30)
                    };
                    _client = new RestClient(options);
                }

                return _client;
            }
        }

        public static RestResponse CreatOrder(string order, string body)
        {
            try
            {
                var client = GetClient();
                var request = new RestRequest("/v3/payments", Method.Post);
                request.AddHeader("Idempotence-Key", $"<{order}>");
                request.AddHeader("Content-Type", "application/json");
                request.AddStringBody(body, DataFormat.Json);

                var response = client.Execute(request);

               
                LogBuffer.Log($"YooKassa CreateOrder: {order} -> {response.StatusCode}", qweaadAPI.LogLevelType.Debug);

                if (!response.IsSuccessful)
                {
                    LogBuffer.Log($"YooKassa error: {response.Content}", qweaadAPI.LogLevelType.Warning);
                }

                return response;
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"YooKassa CreateOrder: {order}");
                throw;
            }
        }

        public static RestResponse GetPaymentStatus(string payment_id)
        {
            try
            {
                var client = GetClient();
                var request = new RestRequest($"/v3/payments/{payment_id}", Method.Get);
                var response = client.Execute(request);

               
                LogBuffer.Log($"YooKassa GetStatus: {payment_id} -> {response.StatusCode}", qweaadAPI.LogLevelType.Debug);

                return response;
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"YooKassa GetPaymentStatus: {payment_id}");
                throw;
            }
        }

        public static int SWStatus(string statusRAW)
        {
            return statusRAW switch
            {
                "canceled" => -1000,
                "succeeded" => 0,
                _ => -1001
            };
        }

        public static async Task<bool> IsYooKassaAvailableAsync()
        {
            try
            {
                var client = GetClient();
                var request = new RestRequest("/v3/payments", Method.Get);
                var response = await client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch
            {
                return false;
            }
        }
    }
}