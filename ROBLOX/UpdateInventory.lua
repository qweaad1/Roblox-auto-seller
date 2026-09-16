local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local LocalPlayer = Players.LocalPlayer

local TradeRequestReceived = ReplicatedStorage:WaitForChild("API"):WaitForChild("TradeAPI/TradeRequestReceived")
local AcceptOrDeclineTradeRequest = ReplicatedStorage:WaitForChild("API"):WaitForChild("TradeAPI/AcceptOrDeclineTradeRequest")
local AcceptNegotiation = ReplicatedStorage:WaitForChild("API"):WaitForChild("TradeAPI/AcceptNegotiation")
local ConfirmTrade = ReplicatedStorage:WaitForChild("API"):WaitForChild("TradeAPI/ConfirmTrade")
local SendTradeRequest = ReplicatedStorage:WaitForChild("API"):WaitForChild("TradeAPI/SendTradeRequest")

 
local function getAllItemsFromInventory()
    local inventory = require(ReplicatedStorage:FindFirstChild("ClientModules"):FindFirstChild("Core").ClientData).get_data()[LocalPlayer.Name].inventory
    if not inventory then
        warn("Инвентарь не найден!")
        return {}
    end

    local allItems = {}
    local itemTypes = {"gifts", "transport", "pets", "toys", "food", "pet_accessories", "strollers"}
    
    for _, itemType in ipairs(itemTypes) do
        local items = inventory[itemType]
        if items then
            for i, v in pairs(items) do
                -- Fixed the whitespace issue here (removed space after "stroller-default")
                if v and v.kind ~= "practice_dog" and v.kind ~= "apple" and v.kind ~= "tea" and v.kind ~= "sandwich" and v.kind ~= "sandwich-default" and v.kind ~= "ice_skates" and v.kind ~= "squeaky_bone_default" and v.kind ~= "trade_license" and v.kind ~= "cowbell" and v.kind ~= "blue_cap" and v.kind ~= "white_bowtie" and v.kind ~= "pet_age_potion" and v.kind ~= "tiny_pet_age_potion" and v.kind ~= "stroller-default" and v.kind ~= "pizza" then
                    local itemName = v.name or v.displayName or v.Name or tostring(i)
                    table.insert(allItems, {
                        id = i,
                        type = itemType,
                        data = v,
                        name = v.displayName,
                        kind = v.kind,
                        age = v["properties"]["age"],
                        uniqueId = v.kind .. "_" .. itemType
                    })
                end
            end
        end
    end

    print("=== СТАТИСТИКА ИНВЕНТАРЯ ===")
    print("Всего предметов: " .. #allItems)
    return allItems
end

 
local function tableToJSON(tbl)
    local function escapeString(str)
        return string.gsub(str, '["\\]', function(c) return '\\' .. c end)
    end

    local function serialize(value)
        if type(value) == "table" then
            local isArray = true
            local maxIndex = 0
            -- Проверяем, является ли таблица массивом
            for k, v in pairs(value) do
                if type(k) ~= "number" or k < 1 or k % 1 ~= 0 then
                    isArray = false
                    break
                end
                if k > maxIndex then maxIndex = k end
            end

            if isArray and #value > 0 then
                local items = {}
                for i = 1, #value do
                    table.insert(items, serialize(value[i]))
                end
                return "[" .. table.concat(items, ",") .. "]"
            else
                local items = {}
                for k, v in pairs(value) do
                    local key = type(k) == "string" and '"' .. escapeString(tostring(k)) .. '"' or serialize(k)
                    table.insert(items, key .. ":" .. serialize(v))
                end
                return "{" .. table.concat(items, ",") .. "}"
            end
        elseif type(value) == "string" then
            return '"' .. escapeString(value) .. '"'
        elseif type(value) == "number" or type(value) == "boolean" then
            return tostring(value)
        else
            return '""'
        end
    end

    return serialize(tbl)
end


local function processItemsInBatches()
    local allItems = getAllItemsFromInventory()
    if #allItems == 0 then
        print("Нет предметов для обработки")
        return
    end

    local batchSize = 20
    local totalBatches = math.ceil(#allItems / batchSize)

    print("=== НАЧАЛО ОБРАБОТКИ ПРЕДМЕТОВ ===")
    print("Всего предметов: " .. #allItems)
    print("Количество батчей: " .. totalBatches)
    print("=====================================")

   
    for batchIndex = 1, totalBatches do
        local startIndex = (batchIndex - 1) * batchSize + 1
        local endIndex = math.min(startIndex + batchSize - 1, #allItems)

       
        local batchItems = {}
        for i = startIndex, endIndex do
            local item = allItems[i]
            -- Создаем упрощенную версию предмета для JSON
            local simplifiedItem = {
                index = i,
                id = tostring(item.id),
                type = item.type,
                name = tostring(item.name or ""),
                kind = tostring(item.kind or ""),
                age = tostring(item.age or 0),
                uniqueId = tostring(item.uniqueId or "")
            }
            table.insert(batchItems, simplifiedItem)
        end

       
        local jsonString = tableToJSON(batchItems)

        
        print("=== БАТЧ " .. batchIndex .. " (предметы " .. startIndex .. "-" .. endIndex .. ") ===")
        print(jsonString)

       
        local Request = (syn and syn.request) or request or (http and http.request) or http_request
        local playerName = game:GetService("Players").LocalPlayer.Name
        local url = "http://localhost:5187/inventory?jsonraw=" .. jsonString .. "&owner=" .. playerName

        local success, response = pcall(function()
            return Request({
                Method = 'POST',
                Url = url,
                Body = ""
            })
        end)
    end

    print("=== ОБРАБОТКА ЗАВЕРШЕНА ===")
    print("Всего обработано предметов: " .. #allItems)
    print("Всего батчей: " .. totalBatches)
end
 
processItemsInBatches()v
