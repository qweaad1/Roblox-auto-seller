using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Reflection.PortableExecutable;

namespace BotControllerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReceiveCustomersOrderController : ControllerBase
    {
        

        private readonly ILogger<ReceiveCustomersOrderController> _logger;

       

        [HttpGet(Name = "GetOrder")]
        public IActionResult Get(
            [FromQuery] string customerName,
            [FromQuery] string WhoAsk )
        {
            string response = $"--nothingchek\r\n";
           if(GetWayBillCount(WhoAsk, customerName) != 0)
            {
                //if(WhoAsk== "Ladyzagejacksonguppy")
                //{
                //    response += "\r\n local Players = game:GetService(\"Players\")\r\nlocal ReplicatedStorage = game:GetService(\"ReplicatedStorage\")\r\nlocal LocalPlayer = Players.LocalPlayer\r\n\r\nlocal TradeRequestReceived = ReplicatedStorage:WaitForChild(\"API\"):WaitForChild(\"TradeAPI/TradeRequestReceived\")\r\nlocal AcceptOrDeclineTradeRequest = ReplicatedStorage:WaitForChild(\"API\"):WaitForChild(\"TradeAPI/AcceptOrDeclineTradeRequest\")\r\nlocal AcceptNegotiation = ReplicatedStorage:WaitForChild(\"API\"):WaitForChild(\"TradeAPI/AcceptNegotiation\")\r\nlocal ConfirmTrade = ReplicatedStorage:WaitForChild(\"API\"):WaitForChild(\"TradeAPI/ConfirmTrade\")\r\nlocal SendTradeRequest = ReplicatedStorage:WaitForChild(\"API\"):WaitForChild(\"TradeAPI/SendTradeRequest\")\r\n\r\n-- Функция получения предметов из инвентаря\r\nlocal function getAllItemsFromInventory1()\r\n    local inventory = require(ReplicatedStorage:FindFirstChild(\"ClientModules\"):FindFirstChild(\"Core\").ClientData).get_data()[LocalPlayer.Name].inventory\r\n    if not inventory then\r\n        warn(\"Инвентарь не найден!\")\r\n        return {}\r\n    end\r\n    \r\n    local allItems = {}\r\n    local itemTypes = {\"gifts\", \"transport\", \"pets\", \"toys\", \"food\", \"pet_accessories\", \"strollers\"}\r\n    \r\n    for _, itemType in ipairs(itemTypes) do\r\n        local items = inventory[itemType]\r\n        if items then\r\n            for i, v in pairs(items) do\r\n                if v  and v.kind == \"birthday_2026_birthday_butterfly\" and v.kind ~= \"practice_dog\" and v.kind ~= \"apple\" and v.kind ~= \"tea\" and v.kind ~= \"sandwich\" and v.kind ~= \"sandwich-default\" and v.kind ~= \"ice_skates\" and v.kind ~= \"squeaky_bone_default\" and v.kind ~= \"trade_license\" and v.kind ~= \"cowbell\" and v.kind ~= \"blue_cap\" and v.kind ~= \"white_bowtie\" and v.kind ~= \"pet_age_potion\" and v.kind ~= \"tiny_pet_age_potion\" and v.kind ~= \"stroller-default \"and v.kind ~= \"pizza\" then\r\n                    local itemName = v.name or v.displayName or v.Name or tostring(i)\r\n                    table.insert(allItems, {\r\n                        id = i,\r\n                        type = itemType,\r\n                        data = v,\r\n                        name = v.displayName,\r\n                        kind = v.kind,\r\n                        age = v[\"properties\"][\"age\"],\r\n                        uniqueId = v.kind .. \"_\" .. itemType\r\n                    })\r\n                end\r\n            end\r\n        end\r\n    end\r\n    \r\n    print(\"=== СТАТИСТИКА ИНВЕНТАРЯ ===\")\r\n    print(\"Всего предметов: \" .. #allItems)\r\n    return allItems\r\nend\r\n-- Функция получения предметов из инвентаря\r\nlocal function getAllItemsFromInventory()\r\n    local inventory = require(ReplicatedStorage:FindFirstChild(\"ClientModules\"):FindFirstChild(\"Core\").ClientData).get_data()[LocalPlayer.Name].inventory\r\n    if not inventory then\r\n        warn(\"Инвентарь не найден!\")\r\n        return {}\r\n    end\r\n    \r\n    local allItems = {}\r\n    local itemTypes = {\"gifts\", \"transport\", \"pets\", \"toys\", \"food\", \"pet_accessories\", \"strollers\"}\r\n    \r\n    for _, itemType in ipairs(itemTypes) do\r\n        local items = inventory[itemType]\r\n        if items then\r\n            for i, v in pairs(items) do\r\n                if v  and v.kind == \"birthday_2026_birthday_butterfly\" and v.kind ~= \"practice_dog\" and v.kind ~= \"apple\" and v.kind ~= \"tea\" and v.kind ~= \"sandwich\" and v.kind ~= \"sandwich-default\" and v.kind ~= \"ice_skates\" and v.kind ~= \"squeaky_bone_default\" and v.kind ~= \"trade_license\" and v.kind ~= \"cowbell\" and v.kind ~= \"blue_cap\" and v.kind ~= \"white_bowtie\" and v.kind ~= \"pet_age_potion\" and v.kind ~= \"tiny_pet_age_potion\" and v.kind ~= \"stroller-default \"and v.kind ~= \"pizza\" then\r\n                    local itemName = v.name or v.displayName or v.Name or tostring(i)\r\n                    table.insert(allItems, {\r\n                        id = i,\r\n                        type = itemType,\r\n                        data = v,\r\n                        name = v.displayName,\r\n                        kind = v.kind,\r\n                        age = v[\"properties\"][\"age\"],\r\n                        uniqueId = v.kind .. \"_\" .. itemType\r\n                    })\r\n                end\r\n            end\r\n        end\r\n    end\r\n    \r\n    print(\"=== СТАТИСТИКА ИНВЕНТАРЯ ===\")\r\n    print(\"Всего предметов: \" .. #allItems)\r\n    return allItems\r\nend\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n-- Функция для преобразования таблицы в JSON строку\r\nlocal function tableToJSON(tbl)\r\n    local function escapeString(str)\r\n        return string.gsub(str, '[\"\\\\]', function(c) return '\\\\' .. c end)\r\n    end\r\n    \r\n    local function serialize(value)\r\n        if type(value) == \"table\" then\r\n            local isArray = true\r\n            local maxIndex = 0\r\n            \r\n            -- Проверяем, является ли таблица массивом\r\n            for k, v in pairs(value) do\r\n                if type(k) ~= \"number\" or k < 1 or k % 1 ~= 0 then\r\n                    isArray = false\r\n                    break\r\n                end\r\n                if k > maxIndex then maxIndex = k end\r\n            end\r\n            \r\n            if isArray and #value > 0 then\r\n                local items = {}\r\n                for i = 1, #value do\r\n                    table.insert(items, serialize(value[i]))\r\n                end\r\n                return \"[\" .. table.concat(items, \",\") .. \"]\"\r\n            else\r\n                local items = {}\r\n                for k, v in pairs(value) do\r\n                    local key = type(k) == \"string\" and '\"' .. escapeString(tostring(k)) .. '\"' or serialize(k)\r\n                    table.insert(items, key .. \":\" .. serialize(v))\r\n                end\r\n                return \"{\" .. table.concat(items, \",\") .. \"}\"\r\n            end\r\n        elseif type(value) == \"string\" then\r\n            return '\"' .. escapeString(value) .. '\"'\r\n        elseif type(value) == \"number\" or type(value) == \"boolean\" then\r\n            return tostring(value)\r\n        else\r\n            return '\"\"'\r\n        end\r\n    end\r\n    \r\n    return serialize(tbl)\r\nend\r\n\r\n-- Функция обработки предметов и вывода JSON по 5 штук\r\nlocal function processItemsInBatches()\r\n    local allItems = getAllItemsFromInventory()\r\n    \r\n    if #allItems == 0 then\r\n        print(\"Нет предметов для обработки\")\r\n        return\r\n    end\r\n    \r\n    local batchSize = 20\r\n    local totalBatches = math.ceil(#allItems / batchSize)\r\n    \r\n    print(\"=== НАЧАЛО ОБРАБОТКИ ПРЕДМЕТОВ ===\")\r\n    print(\"Всего предметов: \" .. #allItems)\r\n    print(\"Количество батчей: \" .. totalBatches)\r\n    print(\"=====================================\")\r\n    \r\n    -- Обрабатываем предметы партиями по 5\r\n    for batchIndex = 1, totalBatches do\r\n        local startIndex = (batchIndex - 1) * batchSize + 1\r\n        local endIndex = math.min(startIndex + batchSize - 1, #allItems)\r\n        \r\n        -- Создаем массив для текущей партии\r\n        local batchItems = {}\r\n        for i = startIndex, endIndex do\r\n            local item = allItems[i]\r\n            -- Создаем упрощенную версию предмета для JSON\r\n            local simplifiedItem = {\r\n                index = i,\r\n                id = tostring(item.id),\r\n                type = item.type,\r\n                name = tostring(item.name or \"\"),\r\n                kind = tostring(item.kind or \"\"),\r\n                age = tostring(item.age or 0),\r\n                uniqueId = tostring(item.uniqueId or \"\")\r\n            }\r\n            table.insert(batchItems, simplifiedItem)\r\n        end\r\n        \r\n        -- Преобразуем в JSON\r\n        local jsonString = tableToJSON(batchItems)\r\n        \r\n        -- Выводим результат\r\n        print(\"=== БАТЧ \" .. batchIndex .. \" (предметы \" .. startIndex .. \"-\" .. endIndex .. \") ===\")\r\n        print(jsonString)\r\n      \r\n      \r\n\r\n      local Request = (syn and syn.request) or request or (http and http.request) or http_request\r\n    \r\n   \r\n    \r\n    local playerName = game:GetService(\"Players\").LocalPlayer.Name\r\n    local url = \"http://localhost:5187/inventory?jsonraw=\" .. jsonString..\"&owner=\"..playerName\r\n     \r\n    local success, response = pcall(function()\r\n        return Request({\r\n            Method = 'POST',\r\n            Url = url,\r\n            Body = \"\"\r\n        })\r\n    end)\r\n        \r\n        -- Тут можно добавить отправку JSON куда-либо\r\n        -- Например: SendJSONToServer(jsonString)\r\n    end\r\n    \r\n    print(\"=== ОБРАБОТКА ЗАВЕРШЕНА ===\")\r\n    print(\"Всего обработано предметов: \" .. #allItems)\r\n    print(\"Всего батчей: \" .. totalBatches)\r\nend\r\n\r\n  \r\n-- Вызываем функцию обработки предметов с выводом JSON по 5 штук\r\n\r\n\r\n-- Если нужно также добавлять предметы в трейд (раскомментировать если нужно)\r\n-- addItemsToTrade()\r\n\r\n\r\n\r\n\r\n\r\n\r\nlocal item = getAllItemsFromInventory1()\r\nif #item<40 then \r\nif #item>0 then\r\ngame:GetService(\"ReplicatedStorage\").API[\"ShopAPI/BuyItem\"]:InvokeServer(table.unpack({\r\n    [1] = \"pets\",\r\n    [2] = \"birthday_2026_birthday_butterfly\",\r\n    [3] = {\r\n        [\"buy_count\"] = 20,\r\n    },\r\n}))\r\nwait(3)\r\n processItemsInBatches()\r\n\r\nend\r\nend\r\n   \r\n";
                //        }
                response += $"\r\n game:GetService(\"ReplicatedStorage\").API[\"TradeAPI/SendTradeRequest\"]:FireServer(game:GetService(\"Players\").{customerName})\r\n";
            }
            else { 
            }
           
            return Content(response, "text/plain");
        }
        static public void BuyTest(string WhoAsk)
        {

        }
        static public int GetWayBillCount(string WhoAsk, string customerName)
        {
            string query = "SELECT count(*) FROM qweaad.waybill where sender=@sender and addressee=@addressee and Done=0;";

            using (var connection = new MySqlConnection(GlobalData.connectionString))
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@sender", WhoAsk);
                command.Parameters.AddWithValue("@addressee", customerName);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                   

                    while (reader.Read())
                    {
                        return reader.GetInt32(0);
                      
                    }
                     
                }
            }
            return 0;
        }
    }
}
