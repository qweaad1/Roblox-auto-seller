using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Text.Json;

namespace BotControllerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class inventory : ControllerBase
    {
        

        private readonly ILogger<inventory> _logger;



        [HttpPost(Name = "Getinventory")]
        public IActionResult Post(

            [FromQuery] string jsonraw,
            [FromQuery]   string owner)
        {
            try
            {
                string jsonr = "";
                int firstOpen = jsonraw.IndexOf('{');
                int lastClose = jsonraw.LastIndexOf('}');
                 
                for (int i = firstOpen; i <= lastClose; i++)
                {
                    jsonr += jsonraw[i];
                }

                string json = $"{{\"obj\":[{jsonr}]}}";

                Console.WriteLine(json);

               
                

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

               
                JsonElement objArray = root.GetProperty("obj");

                using MySqlConnection connection = new MySqlConnection(GlobalData.connectionString);
                connection.Open();

                foreach (JsonElement item in objArray.EnumerateArray())
                {

                    string type = item.GetProperty("type").GetString();
                    string kind = item.GetProperty("kind").GetString();
                    string age = item.GetProperty("age").GetString();

                    string id = item.GetProperty("id").GetString();
                    // int index = item.GetProperty("index").GetInt32();





                    
                    string sql = @"INSERT IGNORE INTO `qweaad`.`inventory` (`id_string`, `name`, `type`, `age`, `owner`) 
                   VALUES ( @id_string, @name, @type, @age, @owner);";
                      using MySqlCommand cmd = new MySqlCommand(sql, connection);

                    cmd.Parameters.AddWithValue("@id_string", id);
                    cmd.Parameters.AddWithValue("@name", kind);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@age", age);
                    cmd.Parameters.AddWithValue("@owner", owner);
                   
                    
                   
                    cmd.ExecuteNonQuery();

                }

                connection.Close();


            }
            catch (Exception value)
            {
                Console.WriteLine(value);
                
            }
            string response = $" ";
            


            return Content(response, "text/plain");
        }
           
            
    }
}
