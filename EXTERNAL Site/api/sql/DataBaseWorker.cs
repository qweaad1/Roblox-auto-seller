using Dapper;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Text.Json;

namespace qweaadAPI.sql
{
    public static class DataBaseWorker
    {
        private const int DEFAULT_PRICE = 10000000;

        public static async Task<string> CreateOrderIdAsync()
        {
            try
            {
                int countorder = 1;
                while (countorder > 0)
                {
                    var rnd = new Random();
                    string orderName = $"ORD_{DateTimeOffset.UtcNow:MMddyyyy_HHmmssf}-{rnd.Next(100, 999)}";

                    countorder = await DatabaseService.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(*) FROM qweaad.orders WHERE id_order_ref_funpay = @order",
                        new { order = orderName });

                    if (countorder == 0)
                        return orderName;
                }

                throw new Exception("Не удалось сгенерировать уникальный ID заказа");
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, "CreateOrderId");
                throw new Exception("ОШИБКА СОЗДАНИЯ НОМЕРА ЗАКАЗА", ex);
            }
        }

        public static async Task<int> AddOrderAsync(string id, string player, string serviceName, string itemName, string countToSend, double price)
        {
            try
            {
                string query = @"INSERT INTO orders 
                    (id_order_ref_funpay, addressee_player, servise_name, item_name, count_to_send, price, status) 
                    VALUES (@id, @player, @serviceName, @itemName, @countToSend, @price, @status)";

                return await DatabaseService.ExecuteAsync(query, new
                {
                    id,
                    player,
                    serviceName,
                    itemName,
                    countToSend,
                    price,
                    status = "-999"
                });
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"AddOrder: {id}");
                throw;
            }
        }

        public static async Task<object> ItemValidatorAsync(string orderId, string player, string serviceName, string itemName, int count, string pay, bool finish = false, bool cart = false)
        {
            try
            {
                
                string query = @"SELECT COUNT(*) FROM inventory 
                    LEFT JOIN waybill ON inventory.id_string = waybill.item_id 
                    WHERE inventory.name = @itemName AND waybill.item_id IS NULL";

                int available = await DatabaseService.QueryFirstOrDefaultAsync<int>(query, new { itemName });

                if (available <= 0)
                    return new { itemName, Valid = 0, Reason = "Недостаточно товара" };

              
                double basePrice = await sql.primitive.sqlprimitives.GetPriceCATALOGAsync(itemName);
                double unitPrice = pay == "sbp"
                    ? basePrice * GlobalData.sbp
                    : basePrice * GlobalData.other;
                double totalSum = Math.Round(unitPrice * available, 2);
              
                if (cart && !finish)
                {
                     
                   
                    return new
                    {
                        itemName,
                        allowed = available,
                        wanted = count,
                        paySum = Math.Round(unitPrice * Math.Min(available, count), 2),
                        Valid = 1,
                        unitPrice = Math.Round(unitPrice, 2)
                    };
                }
                if (cart && finish)
                {
                   
                    return new
                    {
                        itemName,
                        allowed = available,
                        wanted = count,
                        paySum = Math.Round(unitPrice * Math.Min(available, count), 2),
                        Valid = 1,
                        unitPrice = Math.Round(unitPrice, 2)
                    };
                }

              
                int allowed = Math.Min(available, count);

                if (allowed <= 0)
                    return new { itemName, Valid = 0, Reason = "Недостаточно товара" };

              
                double finalTotalSum = Math.Round(unitPrice * allowed, 2);

                if (!finish)
                {
                    var addResult = await AddOrderAsync(orderId, player, serviceName, itemName, allowed.ToString(), finalTotalSum);
                    if (addResult != 1)
                        return new { itemName, Valid = 0, Reason = "Ошибка добавления заказа" };
                }

                return new
                {
                    OrderId = orderId,
                    Player = player,
                    ItemName = itemName,
                    Allowed = allowed,
                    PaySum = finalTotalSum,
                    Valid = 1,
                    UnitPrice = Math.Round(unitPrice, 2)
                };
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"ItemValidator: {itemName}");
                return new { itemName, Valid = 0, Error = ex.Message };
            }
        }

        public static async Task<string> GetPayUrlAsync(string order, string username, string email)
        {
            try
            {
                double paySum = await sql.primitive.sqlprimitives.PayAmountAsync(order);
               
                if (paySum == 896753.51) 
                {
                    LogBuffer.Log($"Invalid paySum for order {order}", LogLevelType.Warning);
                    return "REJECTED";
                }

                var body = @$"{{ ""amount"": {{ ""value"": ""{Math.Round(paySum, 2).ToString(CultureInfo.InvariantCulture)}"", ""currency"": ""RUB"" }}, ""capture"": true, ""confirmation"": {{ ""type"": ""redirect"", ""return_url"": ""{GlobalData.return_url}?username={username}"" }}, ""description"": ""Оплата заказа {order}"" }}";

                var response = UMoney.CreatOrder(order, body);

                if (string.IsNullOrEmpty(response.Content) || response.Content.Length <= 2)
                {
                    LogBuffer.Log($"Empty response from YooKassa for order {order}", LogLevelType.Warning);
                    return "REJECTED";
                }

                using var doc = JsonDocument.Parse(response.Content);
                var confirmation = doc.RootElement.GetProperty("confirmation");
                var confirmationUrl = confirmation.GetProperty("confirmation_url").GetString();
                var paymentId = doc.RootElement.GetProperty("id").GetString();
                var status = doc.RootElement.GetProperty("status").GetString();

                string query = @"INSERT INTO payments 
                    (payment_id, order_id, username, email, amount, status, payment_data) 
                    VALUES (@paymentId, @orderId, @username, @email, @amount, @status, @paymentData)";

                await DatabaseService.ExecuteAsync(query, new
                {
                    paymentId,
                    orderId = order,
                    username,
                    email,
                    amount = paySum,
                    status,
                    paymentData = response.Content
                });

                return confirmationUrl ?? "REJECTED";
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"GetPayUrl for order: {order}");
                return "REJECTED";
            }
        }

        public static async Task<object> UpdateAllPaymentStatusMainAsync(string username)
        {
            try
            {
                string query = @"SELECT * FROM qweaad.payments WHERE status = 'pending' LIMIT 30";
                var payments = await DatabaseService.QueryAsync<dynamic>(query);

                var changed = new List<object>();

                foreach (var payment in payments)
                {
                    string paymentId = payment.payment_id;
                    string dbStatus = payment.status;

                    var response = UMoney.GetPaymentStatus(paymentId);

                    if (string.IsNullOrEmpty(response.Content))
                        continue;

                    using var doc = JsonDocument.Parse(response.Content);
                    var newStatus = doc.RootElement.GetProperty("status").GetString();

                    if (dbStatus != newStatus)
                    {
                        changed.Add(new
                        {
                            Old = dbStatus,
                            New = newStatus,
                            OrderId = payment.order_id,
                            PaymentId = paymentId
                        });

                        await sql.primitive.sqlprimitives.UpdatePaymentDataAsync(paymentId, newStatus, response.Content, payment.order_id);
                    }
                }

                var playerOrders = await ReternUsernameOrdersAsync(username);

                return new
                {
                    ChangedOrders = changed,
                    PlayerOrders = playerOrders,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, "UpdateAllPaymentStatusMain");
                return new { Error = ex.Message, Timestamp = DateTime.UtcNow };
            }
        }

        public static async Task<object> ReternUsernameOrdersAsync(string username)
        {
            try
            {
                var data = await DatabaseService.QueryAsync<dynamic>(
                    "SELECT * FROM qweaad.orders WHERE addressee_player = @username",
                    new { username });

                return new
                {
                    Success = true,
                    Data = data,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"ReternUsernameOrders: {username}");
                return new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
