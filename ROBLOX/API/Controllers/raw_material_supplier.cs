using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Text.Json;

namespace BotControllerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class raw_material_supplier : ControllerBase
    {


        



        [HttpGet(Name = "GetBuy")]
        public IActionResult Get(

            [FromQuery] string Name,
            [FromQuery] string on_boot_bucks,
             [FromQuery] string on_boot_potions,
             [FromQuery] string buy
             )
        {
            try
            {
               
                    double potions = double.Parse(on_boot_potions);
                double buyCount = Math.Ceiling(potions / 4) ;
                if (buy == "0")
                {
                    buyCount = 0;
                }
                if (!Name.Contains("qweaad"))
                {

                
                using MySqlConnection connection = new MySqlConnection(GlobalData.connectionString);
                connection.Open();




                string sql = "INSERT IGNORE INTO raw_material_supplier (NAME, buy_count, on_boot_bucks, on_boot_potions)\r\nVALUES (@NAME, @buy_count, @on_boot_bucks, @on_boot_potions);";
                    // SQL запрос
                  
                    using MySqlCommand cmd = new MySqlCommand(sql, connection);

                    cmd.Parameters.AddWithValue("@NAME", Name);
                    cmd.Parameters.AddWithValue("@buy_count", buyCount);
                    cmd.Parameters.AddWithValue("@on_boot_bucks", on_boot_bucks.Replace(",", ""));
                    cmd.Parameters.AddWithValue("@on_boot_potions", on_boot_potions.Replace(",", ""));




                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine(rowsAffected);

                    string returnA = "";
                connection.Close();
                    if (buy == "1")
                    {
                        if (rowsAffected == 1)
                        {
                            while (buyCount > 98)
                            {

                                returnA += "game:GetService(\"ReplicatedStorage\").API[\"ShopAPI/BuyItem\"]:InvokeServer(table.unpack({\r\n    [1] = \"pets\",\r\n    [2] = \"sky_ux_2023_grinmoire\",\r\n    [3] = {\r\n        [\"buy_count\"] = " + (99) + ",\r\n    },\r\n}))\r\n";
                                buyCount = buyCount - 99;
                            }
                            returnA += "game:GetService(\"ReplicatedStorage\").API[\"ShopAPI/BuyItem\"]:InvokeServer(table.unpack({\r\n    [1] = \"pets\",\r\n    [2] = \"sky_ux_2023_grinmoire\",\r\n    [3] = {\r\n        [\"buy_count\"] = " + buyCount + ",\r\n    },\r\n}))\r\n";
                            return Content(returnA, "text/plain");
                        }
                    }
                    else
                    {
                        return Content(" ", "text/plain");
                    }
                    

                }
                else
                {
                    return Content(" ", "text/plain");
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(value);

            }
            string response = $" ";



            return Content(response, "text/plain");
        }
        [HttpPut(Name = "GetBuy")]
        public IActionResult Put(

           [FromQuery] string Name,
           [FromQuery] string bucks,
            [FromQuery] string potions
            )
        {
            try
            {
                    using MySqlConnection connection = new MySqlConnection(GlobalData.connectionString);
                    connection.Open();




                    string sql = "UPDATE raw_material_supplier SET bucks = @bucks, potions = @potions WHERE NAME = @NAME;\r\n";
                    // SQL запрос

                    using MySqlCommand cmd = new MySqlCommand(sql, connection);

                    cmd.Parameters.AddWithValue("@NAME", Name); 
                    cmd.Parameters.AddWithValue("@bucks", bucks.Replace(",",""));
                    cmd.Parameters.AddWithValue("@potions", potions);




                    int rowsAffected = cmd.ExecuteNonQuery();
                    

                    string returnA = "";
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
