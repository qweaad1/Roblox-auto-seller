using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace BotControllerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WayBill : ControllerBase
    {
        

        private readonly ILogger<WayBill> _logger;

       

        [HttpGet(Name = "GetWayBill")]
        public IActionResult Get(
            [FromQuery] string CustomerName,
            [FromQuery] string WhoAsk )
        {
            string response = "";
            string[] ids = GetItemIdsFromWaybill(WhoAsk, CustomerName);
            if (ids.Count() > 0)
            {
                foreach (var s in ids)
                {
                    response += ($"\r\ngame:GetService(\"ReplicatedStorage\").API[\"TradeAPI/AddItemToOffer\"]:FireServer(\"{s}\") \r\n ");
                }
            }
            else {
                response += ($" game:GetService(\"ReplicatedStorage\").API[\"TradeAPI/DeclineTrade\"]:FireServer() \r\n ");
            }
           
           
            return Content(response, "text/plain");
        }
        [HttpPost(Name = "GetWayBill")]
        public IActionResult Post(

            [FromQuery] string WhoAsk)
        {
            string response = $"";
            string[] ids = WayBillMarkdown(WhoAsk);
            if (ids.Count() > 0)
            {
                foreach (var s in ids)
                {
                    response += ($"{s}\r\n");
                }
            }
            else
            {
                response = ($"");
            }


            return Content(response, "text/plain");
        }
            [HttpPatch(Name = "GetWayBill")]
            public IActionResult PATCH(
                 [FromQuery] string itemID,
           [FromQuery] string WhoAsk)
            {
                string response = $"";

            WayConfrim(WhoAsk,itemID);
                return Content(response, "text/plain");
            
        
        
        
        }
            static public string[] WayBillMarkdown(string WhoAsk)
        {
            string query = "SELECT item_id FROM qweaad.waybill WHERE sender = @sender AND Done = 0";

            using (var connection = new MySqlConnection(GlobalData.connectionString))
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@sender", WhoAsk);
                

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var itemIds = new List<string>();

                    while (reader.Read())
                    {
                        string itemId = reader.GetString(0);
                        itemIds.Add(itemId);
                    }

                    return itemIds.ToArray();
                }
            }
        }
        static public string[] GetItemIdsFromWaybill(string WhoAsk, string customerName)
        {
            string query = "SELECT item_id FROM qweaad.waybill WHERE sender = @sender AND Done = 0 and addressee=@addressee LIMIT 18;";

            using (var connection = new MySqlConnection(GlobalData.connectionString))
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@sender", WhoAsk);
                command.Parameters.AddWithValue("@addressee", customerName);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var itemIds = new List<string>();

                    while (reader.Read())
                    {
                        string itemId = reader.GetString(0);
                        itemIds.Add(itemId);
                    }

                    return itemIds.ToArray();
                }
            }
        }
        static public void WayConfrim(string WhoAsk, string ItemId)
        {
            //переделать ответ в json и распарс до 18 шт
            string query = "Update qweaad.waybill Set done=1 where item_id=@item_id and sender=@sender;";

            using (var connection = new MySqlConnection(GlobalData.connectionString))
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@sender", WhoAsk);
                command.Parameters.AddWithValue("@item_id", ItemId);

                connection.Open();
                command.ExecuteNonQuery();
                 
            }
        }
    }
}
