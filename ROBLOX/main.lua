print("MAIN LOAD")
wait(10)
print("MAIN START")
while true do
for _, player in ipairs(game:GetService("Players"):GetPlayers()) do
if (player.Name ~=game:GetService("Players").LocalPlayer.Name) then 
   loadstring(game:HttpGet("http://localhost:5187/ReceiveCustomersOrder?customerName="..player.Name.."&WhoAsk="..tostring(game:GetService("Players").LocalPlayer.Name) ))()

end
end
  
--chek



 local step2allow=false
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
                if v and v.kind ~= "practice_dog" and v.kind ~= "apple" and v.kind ~= "tea" and v.kind ~= "sandwich" and v.kind ~= "sandwich-default" and v.kind ~= "ice_skates" and v.kind ~= "squeaky_bone_default" and v.kind ~= "trade_license" and v.kind ~= "cowbell" and v.kind ~= "blue_cap" and v.kind ~= "white_bowtie" and v.kind ~= "pet_age_potion" and v.kind ~= "tiny_pet_age_potion" and v.kind ~= "stroller-default "and v.kind ~= "pizza" then
                    local itemName = v.name or v.displayName or v.Name or tostring(i)
                    table.insert(allItems, {
                        id = i,
                        type = itemType,
                        data = v,
                        name = itemName,
                        kind = v.kind,
                        uniqueId = v.kind .. "_" .. itemType
                    })
                end
            end
        end
    end
    
   
    return allItems
end





local function checkItemsFromAPI()
    
    local inventoryItems = getAllItemsFromInventory()
    
     
    local inventoryIds = {}
    for _, item in ipairs(inventoryItems) do
        if item.id then
            inventoryIds[item.id] = true
        end
    end
    
    
    local Request = (syn and syn.request) or request or (http and http.request) or http_request
    
   
    
    local playerName = game:GetService("Players").LocalPlayer.Name
    local url = "http://localhost:5187/WayBill?WhoAsk=" .. playerName
     
    local success, response = pcall(function()
        return Request({
            Method = 'POST',
            Url = url,
            Body = ""
        })
    end)
    
     
    local apiItems = {}
    for id in string.gmatch(response.Body, "[^\r\n]+") do
        if id ~= "" then
            table.insert(apiItems, id)
        end
    end
    
    if #apiItems == 0 then
        
        return {}
    end
     
    
   
    local missingItems = {}
    local foundItems = {}
    
    for _, apiId in ipairs(apiItems) do
         
        if inventoryIds[apiId] then
            table.insert(foundItems, apiId)
        else
            table.insert(missingItems, apiId)
        end
    end
    
    
    
    print("РЕЗУЛЬТАТЫ ПРОВЕРКИ")
    
    print("Всего предметов в Базе: " .. #apiItems)
    print("Ожидает передачи: " .. #foundItems)
    --print("Передача : " .. #missingItems)
    
   -- if #foundItems > 0 then
     --   print("\nОжидает передачи:")
      --  for _, itemId in ipairs(foundItems) do
        --    print(itemId)
     --   end
  --  end
    
    if #missingItems > 0 then

       -- print("\nПереданные ПРЕДМЕТЫ:")
        for _, itemId in ipairs(missingItems) do

           
             local Response = Request {
        Method = 'PATCH',
        Url = "http://localhost:5187/WayBill?itemID=".. itemId.."&WhoAsk="..game:GetService("Players").LocalPlayer.Name,
        Body = Body
    }

   
print(Response.Body)
        end
     --   print("\n Переданно " .. #missingItems .. " предметов")
    else
        
    end
    
    print(string.rep("=", 50))
    
    return missingItems
end
 
checkItemsFromAPI()




--SEND
local inactiveTrade = game:GetService("Players").LocalPlayer:WaitForChild("PlayerGui"):WaitForChild("TradeApp"):WaitForChild("Frame"):WaitForChild("NegotiationFrame"):WaitForChild("Header"):WaitForChild("PartnerFrame"):WaitForChild("NameLabel")
  local confrimData = game:GetService("Players").LocalPlayer:WaitForChild("PlayerGui"):WaitForChild("TradeApp"):WaitForChild("Frame"):WaitForChild("ConfirmationFrame"):WaitForChild("PartnerLabel").Text
local secondFrame = game:GetService("Players").LocalPlayer.PlayerGui:FindFirstChild("TradeApp"):FindFirstChild("Frame"):FindFirstChild("NegotiationFrame")
local mainFrame = game:GetService("Players").LocalPlayer.PlayerGui:FindFirstChild("TradeApp"):FindFirstChild("Frame")


local acceptedWay=""
acceptedWay=inactiveTrade.Text

  
  confrimData = "ANTIFROZE"
    for i = 1, 30 do
   
    wait(1)
     
      if  mainFrame.Visible then 
       break 
      end
   
end 

 if  mainFrame.Visible then
    step2allow=true
Wait(1)
         loadstring(game:HttpGet("http://localhost:5187/WayBill?customerName="..inactiveTrade.Text.."&WhoAsk="..game:GetService("Players").LocalPlayer.Name))()

end
wait(5.5)
  if  step2allow then
print("Step 2")

   inactiveTrade.Text = "ANTIFROZE"  
 local success, err = pcall(function()
    game:GetService("ReplicatedStorage"):WaitForChild("API"):WaitForChild("TradeAPI/AcceptNegotiation"):FireServer()
end)

if not success then
    warn("ERR:", err)
end

  for attempt = 1, 30 do
   
    wait(1)
    local success, err = pcall(function()
    game:GetService("ReplicatedStorage"):WaitForChild("API"):WaitForChild("TradeAPI/AcceptNegotiation"):FireServer()
end)

if not success then
    warn("Ошибка:", err)
end
    if secondFrame.Visible == false  then 
        
        warn("Confrim Negotiation")
        break 
    end 
end



   wait(2)

   confrimData = "ANTIFROZE"
   wait(1)

   local TradeConfrim = false  
for attempt = 1, 10 do
   
    wait(1)
     confrimData = game:GetService("Players").LocalPlayer:WaitForChild("PlayerGui"):WaitForChild("TradeApp"):WaitForChild("Frame"):WaitForChild("ConfirmationFrame"):WaitForChild("PartnerLabel").Text
   
    if acceptedWay == confrimData then 
        TradeConfrim = true
        warn("Confrim trade")
        break 
    end 
end

 
  if TradeConfrim == true then 
 wait(1)
     game:GetService("ReplicatedStorage"):WaitForChild("API"):WaitForChild("TradeAPI/ConfirmTrade"):FireServer()
     wait(1)
     game:GetService("ReplicatedStorage"):WaitForChild("API"):WaitForChild("TradeAPI/ConfirmTrade"):FireServer()
for i = 1, 30 do
   
    wait(1)
     game:GetService("ReplicatedStorage"):WaitForChild("API"):WaitForChild("TradeAPI/ConfirmTrade"):FireServer()

      if not mainFrame.Visible then 
       break 
      end
    if mainFrame.Visible then 
       
       
game:GetService("ReplicatedStorage"):WaitForChild("API"):WaitForChild("TradeAPI/ConfirmTrade"):FireServer()

       
    end 
end
 end

wait(5)
checkItemsFromAPI()
end
--droptrade
game:GetService("ReplicatedStorage").API["TradeAPI/DeclineTrade"]:FireServer()



wait(5)
end
-- next loop   
