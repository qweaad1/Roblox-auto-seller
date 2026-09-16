using MySql.Data.MySqlClient;
using RestSharp;
using System.Globalization;
using System.Text.Json;

namespace qweaadAPI.sql.primitive
{
    public static class sqlprimitives
    {
        private const double MAGIC_PRICE_ERROR = 896753.51;
        private const int ORDER_STATUS_PENDING = -999;
        private const int ORDER_STATUS_SUCCESS = 0;
        private const int ORDER_STATUS_CANCELED = -1000;
        private const int ORDER_STATUS_UNKNOWN = -1001;

        public static async Task<int> UpdatePaymentDataAsync(string payment_id, string status, string payment_data, string orderid)
        {
            try
            {
                string query = @"UPDATE payments SET status = @status, payment_data = @payment_data, updated_at = CURRENT_TIMESTAMP WHERE payment_id = @payment_id LIMIT 1;";

                var result = await DatabaseService.ExecuteAsync(query, new { payment_id, status, payment_data });

                if (result > 0)
                {
                    await UpdateOrderStatusAsync(orderid, status);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"UpdatePaymentData for payment_id: {payment_id}");
                return -1;
            }
        }

        public static async Task<int> UpdateOrderStatusAsync(string orderID, string statusRAW)
        {
            try
            {
                if (statusRAW == "pending")
                    return -1;

                int status = statusRAW switch
                {
                    "canceled" => ORDER_STATUS_CANCELED,
                    "succeeded" => ORDER_STATUS_SUCCESS,
                    _ => ORDER_STATUS_UNKNOWN
                };

                LogBuffer.Log($"Updating order status: {orderID} -> {status} ({statusRAW})", LogLevelType.Debug);

                string query = @"UPDATE orders SET status = @status WHERE id_order_ref_funpay LIKE CONCAT(@orderID, '_%')";

                return await DatabaseService.ExecuteAsync(query, new { orderID, status });
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"UpdateOrderStatus for order: {orderID}");
                return -1;
            }
        }

        public static async Task<double> GetPriceCATALOGAsync(string itemName)
        {
            try
            {
                string query = @"SELECT 
                    price,
                    CASE 
                        WHEN SPPRICE IS NOT NULL AND SPPRICE != 0 THEN SPPRICE
                        ELSE price
                    END AS final_price
                FROM qweaad.catalog 
                WHERE raw_name = @item_name
                LIMIT 1";

                var result = await DatabaseService.QueryFirstOrDefaultAsync<dynamic>(query, new { item_name = itemName });

                if (result != null)
                {
                    return Convert.ToDouble(result.final_price);
                }

                LogBuffer.Log($"Price not found for item: {itemName}", LogLevelType.Warning);
                return 0;
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"GetPriceCATALOG for item: {itemName}");
                throw new Exception($"ОШИБКА ПОЛУЧЕНИЯ ЦЕНЫ: {ex.Message}");
            }
        }

        public static async Task<double> PayAmountAsync(string order)
        {
            try
            {
                string query = @"SELECT COALESCE(SUM(price), 0) FROM qweaad.orders WHERE id_order_ref_funpay LIKE CONCAT(@order, '_%')";

               
                var result = await DatabaseService.QueryFirstOrDefaultAsync<decimal?>(query, new { order });

                if (result.HasValue && result.Value > 0)
                {
                     
                    var sum = Convert.ToDouble(result.Value, CultureInfo.InvariantCulture);
                    Console.WriteLine($"PayAmount for order {order}: {sum}");
                    return sum;
                }

                return MAGIC_PRICE_ERROR;
            }
            catch (Exception ex)
            {
                LogBuffer.LogError(ex, $"PayAmount for order: {order}");
                return MAGIC_PRICE_ERROR;
            }
        }
    }
}
