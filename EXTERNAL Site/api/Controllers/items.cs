using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Text.Json;
using qweaadAPI.sql;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace qweaadAPI.Controllers;

// ============================================
// 1. INVENTORY CONTROLLER - /api/inventory
// ============================================
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    // GET /api/inventory
    [HttpGet]
    public async Task<IActionResult> GetInventory()
    {
        try
        {
            var query = @"
                SELECT 
                    c.raw_name, 
                    c.clean_name_en, 
                    c.price * @multiprice as price, 
                    c.SPPRICE * @multiprice as SPPRICE, 
                    c.FIRE,
                    COALESCE(COUNT(DISTINCT i.id_string) - COUNT(DISTINCT w.item_id), 0) AS count
                FROM catalog c
                LEFT JOIN inventory i ON c.raw_name = i.name
                LEFT JOIN waybill w ON i.id_string = w.item_id AND w.Done = '1'
                GROUP BY c.raw_name, c.clean_name_en, c.price, c.SPPRICE, c.FIRE
HAVING COALESCE(COUNT(DISTINCT i.id_string) - COUNT(DISTINCT w.item_id), 0) > 0
                ORDER BY c.raw_name";

            var data = await DatabaseService.QueryAsync<dynamic>(query, new
            {
                multiprice = GlobalData.sbp.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });

            LogBuffer.LogApiRequest("/api/inventory", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, "GetInventory");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    // GET /api/inventory/{itemName}
    [HttpGet("{itemName}")]
    public async Task<IActionResult> GetItem(string itemName)
    {
        try
        {
            var query = @"
                SELECT 
                    c.raw_name, 
                    c.clean_name_en, 
                    c.price * @multiprice as price, 
                    c.SPPRICE * @multiprice as SPPRICE, 
                    c.FIRE,
                    COALESCE(COUNT(DISTINCT i.id_string) - COUNT(DISTINCT w.item_id), 0) AS count
                FROM catalog c
                LEFT JOIN inventory i ON c.raw_name = i.name
                LEFT JOIN waybill w ON i.id_string = w.item_id AND w.Done = '1'
                WHERE c.raw_name = @itemName
                GROUP BY c.raw_name, c.clean_name_en, c.price, c.SPPRICE, c.FIRE";

            var data = await DatabaseService.QueryFirstOrDefaultAsync<dynamic>(query, new
            {
                itemName,
                multiprice = GlobalData.sbp.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });

            if (data == null)
                return NotFound(new { Success = false, Message = "Item not found" });

            LogBuffer.LogApiRequest($"/api/inventory/{itemName}", "GET", 200);

            return Ok(new
            {
                Success = true,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetItem: {itemName}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }
}

// ============================================
// 2. ORDERS CONTROLLER - /api/orders
// ============================================
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // GET /api/orders/{orderId}
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId)
    {
        try
        {
            var data = await DatabaseService.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM qweaad.orders WHERE id_order_ref_funpay = @orderId",
                new { orderId });

            LogBuffer.LogApiRequest($"/api/orders/{orderId}", "GET", data != null ? 200 : 404);

            return data == null
                ? NotFound(new { Success = false, Message = "Order not found" })
                : Ok(new { Success = true, Data = data, Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetOrder: {orderId}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    // GET /api/orders/user/{username}
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUserOrders(string username)
    {
        try
        {
            var data = await DatabaseService.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.orders WHERE addressee_player = @username ORDER BY created_at DESC",
                new { username });

            LogBuffer.LogApiRequest($"/api/orders/user/{username}", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetUserOrders: {username}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    // GET /api/orders/status/{status}
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(int status)
    {
        try
        {
            var data = await DatabaseService.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.orders WHERE status = @status ORDER BY created_at DESC",
                new { status });

            LogBuffer.LogApiRequest($"/api/orders/status/{status}", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetOrdersByStatus: {status}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }
}

// ============================================
// 3. BOT CONTROLLER - /api/bot
// ============================================
[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    // GET /api/bot/status
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        LogBuffer.LogApiRequest("/api/bot/status", "GET", 200);
        return Ok(new
        {
            Success = true,
            Bots = new { qweaad000 = true },
            Status = "Online",
            Timestamp = DateTime.UtcNow
        });
    }

    // GET /api/bot/health
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        LogBuffer.LogApiRequest("/api/bot/health", "GET", 200);
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }

    // GET /api/bot
    [HttpGet]
    public IActionResult GetBotInfo()
    {
        LogBuffer.LogApiRequest("/api/bot", "GET", 200);
        return Ok(new
        {
            Success = true,
            Name = "qweaad Bot",
            Version = "1.0.0",
            Status = "Online",
            Timestamp = DateTime.UtcNow
        });
    }
}

// ============================================
// 4. PAYMENTS CONTROLLER - /api/payments
// ============================================
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    // POST /api/payments - создание платежа
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] JsonElement json)
    {
        try
        {
            var username = json.GetProperty("username").GetString();
            var locator = json.GetProperty("locator").GetString();
            var paymentMethod = json.GetProperty("payment_method").GetString();
            var email = json.GetProperty("email").GetString();
            var dataArray = json.GetProperty("data").EnumerateArray();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(locator))
            {
                return BadRequest(new { Success = false, Error = "Username and locator are required" });
            }

            string orderId = await DataBaseWorker.CreateOrderIdAsync();
            int index = 0;
            var responseItems = new List<object>();

            foreach (var item in dataArray)
            {
                index++;
                var rawName = item.GetProperty("raw_name").GetString();
                var count = item.GetProperty("count").GetInt32();

                var result = await DataBaseWorker.ItemValidatorAsync(
                    $"{orderId}_{index}",
                    username,
                    locator,
                    rawName,
                    count,
                    paymentMethod
                );

                responseItems.Add(result);
            }

            string payLink = await DataBaseWorker.GetPayUrlAsync(orderId, username, email);

            LogBuffer.LogApiRequest("/api/payments", "POST", 200, $"Order: {orderId}, Items: {responseItems.Count}");

            return Ok(new
            {
                Status = "ok",
                Pay = payLink,
                OrderId = orderId,
                Total = responseItems.Count,
                Items = responseItems
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, "CreatePayment");
            return StatusCode(500, new { Success = false, Error = ex.Message });
        }
    }

    // POST /api/payments/cart - расчет корзины
    [HttpPost("cart")]
    public async Task<IActionResult> CalculateCart([FromBody] JsonElement json, [FromQuery] string finish = "false")
    {
        try
        {
            bool finishB = finish == "true";

            var username = json.GetProperty("username").GetString();
            var locator = json.GetProperty("locator").GetString();
            var paymentMethod = json.GetProperty("payment_method").GetString();
            var email = json.GetProperty("email").GetString();
            var dataArray = json.GetProperty("data").EnumerateArray();

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { Success = false, Error = "Username is required" });
            }

            int index = 0;
            var responseItems = new List<object>();
            double paySum = 0;

            foreach (var item in dataArray)
            {
                index++;
                var rawName = item.GetProperty("raw_name").GetString();
                var count = item.GetProperty("count").GetInt32();

                var result = await DataBaseWorker.ItemValidatorAsync(
                    "-",
                    username,
                    locator,
                    rawName,
                    count,
                    paymentMethod,
                    finishB,
                    true
                );

                dynamic dynamicResult = result;
                var properties = ((Type)dynamicResult.GetType()).GetProperties()
                    .Select(p => p.Name)
                    .ToList();

                if (properties.Contains("paySum"))
                {
                    var paySumValue = dynamicResult.paySum;
                    if (paySumValue != null && paySumValue.ToString().Length > 0)
                    {
                        paySum += Convert.ToDouble(paySumValue);
                    }
                }
            }

            LogBuffer.LogApiRequest("/api/payments/cart", "POST", 200, $"User: {username}, Items: {responseItems.Count}, Finish: {finishB}");

            return Ok(new
            {
                Status = "ok",
                PayAmountSBP = Math.Round(paySum, 2),
                PayAmountCARD = Math.Round(paySum / GlobalData.sbp * GlobalData.other, 2),
                CardFee = Math.Round(((GlobalData.other * 100 - GlobalData.sbp * 100)), 0) + "%",
                Total = responseItems.Count,
                Items = responseItems
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, "CalculateCart");
            return StatusCode(500, new { Success = false, Error = ex.Message });
        }
    }

    // PUT /api/payments/{paymentId}/status - обновление статуса платежа
    [HttpPut("{paymentId}/status")]
    public async Task<IActionResult> UpdatePaymentStatus(string paymentId)
    {
        try
        {
            var result = await DataBaseWorker.UpdateAllPaymentStatusMainAsync(paymentId);

            LogBuffer.LogApiRequest($"/api/payments/{paymentId}/status", "PUT", 200);

            return Ok(new { PaymentUpdate = result });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, "UpdatePaymentStatus");
            return StatusCode(500, new { Success = false, Error = ex.Message });
        }
    }

    // GET /api/payments/user/{username}/active - активные платежи пользователя
    [HttpGet("user/{username}/active")]
    public async Task<IActionResult> GetActivePayments(string username)
    {
        try
        {
            var update = await DataBaseWorker.UpdateAllPaymentStatusMainAsync(username);

            var conn = new MySqlConnection(GlobalData.connectionString);
            await conn.OpenAsync();

            var hexGrid = await conn.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.waybill WHERE addressee = @UserName",
                new { UserName = username });

            var pendingPayments = await conn.QueryAsync<dynamic>(
                "SELECT payment_id, order_id, username, email, amount, created_at FROM qweaad.payments WHERE username = @UserName AND status = 'pending'",
                new { UserName = username });

            var pendingOrders = await conn.QueryAsync<dynamic>(
                "SELECT item_name, count_to_send FROM qweaad.orders WHERE addressee_player = @UserName AND status = -999",
                new { UserName = username });

            LogBuffer.LogApiRequest($"/api/payments/user/{username}/active", "GET", 200);

            return Ok(new
            {
                PaymentUpdate = update,
                HEXGRID = hexGrid,
                Pending = pendingPayments,
                Order = pendingOrders
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, "GetActivePayments");
            return StatusCode(500, new { Success = false, Error = ex.Message });
        }
    }

    // GET /api/payments/{paymentId} - получить платеж по ID
    [HttpGet("{paymentId}")]
    public async Task<IActionResult> GetPayment(string paymentId)
    {
        try
        {
            var data = await DatabaseService.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM qweaad.payments WHERE payment_id = @paymentId",
                new { paymentId });

            LogBuffer.LogApiRequest($"/api/payments/{paymentId}", "GET", data != null ? 200 : 404);

            return data == null
                ? NotFound(new { Success = false, Message = "Payment not found" })
                : Ok(new { Success = true, Data = data, Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetPayment: {paymentId}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    // GET /api/payments/user/{username} - все платежи пользователя
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUserPayments(string username)
    {
        try
        {
            var data = await DatabaseService.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.payments WHERE username = @username ORDER BY created_at DESC",
                new { username });

            LogBuffer.LogApiRequest($"/api/payments/user/{username}", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetUserPayments: {username}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    // Friend endpoints (для совместимости со старым кодом)
    // GET /api/payments/friend?name=xxx
    [HttpGet("friend")]
    public IActionResult GetFriend([FromQuery] string name)
    {
        LogBuffer.LogApiRequest($"/api/payments/friend?name={name}", "GET", 200);
        return Ok(new { Find = true });
    }

    // POST /api/payments/friend?name=xxx
    [HttpPost("friend")]
    public IActionResult PostFriend([FromQuery] string name)
    {
        LogBuffer.LogApiRequest($"/api/payments/friend?name={name}", "POST", 200);
        return Ok(new { Add = true });
    }
}

// ============================================
// 5. WAYBILL CONTROLLER - /api/waybill
// ============================================
[ApiController]
[Route("api/[controller]")]
public class WaybillController : ControllerBase
{
    // GET /api/waybill/{orderId}
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetWaybillByOrder(string orderId)
    {
        try
        {
            var data = await DatabaseService.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.waybill WHERE order_id = @orderId",
                new { orderId });

            LogBuffer.LogApiRequest($"/api/waybill/{orderId}", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetWaybillByOrder: {orderId}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    // GET /api/waybill/user/{username}
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetWaybillByUser(string username)
    {
        try
        {
            var data = await DatabaseService.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.waybill WHERE addressee = @username ORDER BY created_at DESC",
                new { username });

            LogBuffer.LogApiRequest($"/api/waybill/user/{username}", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetWaybillByUser: {username}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }
}

// ============================================
// 6. USER CONTROLLER - /api/users (было /api/User)
// ============================================
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // GET /api/users/{username}/orders
    [HttpGet("{username}/orders")]
    public async Task<IActionResult> GetUserOrders(string username)
    {
        try
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(new { Success = false, Error = "UserName is required" });

            var data = await DatabaseService.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.orders WHERE addressee_player = @username ORDER BY created_at DESC",
                new { username });

            LogBuffer.LogApiRequest($"/api/users/{username}/orders", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetUserOrders: {username}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    // GET /api/users/{username}/payments
    [HttpGet("{username}/payments")]
    public async Task<IActionResult> GetUserPayments(string username)
    {
        try
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(new { Success = false, Error = "UserName is required" });

            var data = await DatabaseService.QueryAsync<dynamic>(
                "SELECT * FROM qweaad.payments WHERE username = @username ORDER BY created_at DESC",
                new { username });

            LogBuffer.LogApiRequest($"/api/users/{username}/payments", "GET", 200, $"Count: {data.Count()}");

            return Ok(new
            {
                Success = true,
                Count = data.Count(),
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            LogBuffer.LogError(ex, $"GetUserPayments: {username}");
            return StatusCode(500, new { Success = false, Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }
}
