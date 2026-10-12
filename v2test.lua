print("MAIN LOAD")
wait(10)
print("MAIN START")

-- ═══════════════════════════════════════════════════════════
-- Сервисы и API
-- ═══════════════════════════════════════════════════════════
local Players          = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local LocalPlayer      = Players.LocalPlayer

local API              = ReplicatedStorage:WaitForChild("API")
local TradeRequestReceived      = API:WaitForChild("TradeAPI/TradeRequestReceived")
local AcceptOrDeclineTradeRequest = API:WaitForChild("TradeAPI/AcceptOrDeclineTradeRequest")
local AcceptNegotiation         = API:WaitForChild("TradeAPI/AcceptNegotiation")
local ConfirmTrade              = API:WaitForChild("TradeAPI/ConfirmTrade")
local SendTradeRequest          = API:WaitForChild("TradeAPI/SendTradeRequest")
local DeclineTrade              = API:WaitForChild("TradeAPI/DeclineTrade")

local Request = (syn and syn.request) or request or (http and http.request) or http_request

-- ═══════════════════════════════════════════════════════════
-- UIManager — читаем состояние трейда без GUI
-- ═══════════════════════════════════════════════════════════
local Core = ReplicatedStorage:WaitForChild("ClientModules"):WaitForChild("Core")
local UIManager = require(Core.UIManager:WaitForChild("UIManager"))

local function getTradeApp()
    if UIManager.apps and UIManager.apps.TradeApp then
        return UIManager.apps.TradeApp
    end
    return nil
end

local function getTradeInfo()
    local app = getTradeApp()
    if not app then return nil end

    local ok, state = pcall(function() return app:_get_local_trade_state() end)
    if not ok or not state then return nil end

    local okP, partner = pcall(function() return app:_get_partner() end)
    local okS, stage   = pcall(function() return app:_get_current_trade_stage() end)

    return {
        partner = okP and partner or nil,
        stage   = okS and stage or nil,
        trade_id = state.trade_id,
        raw_state = state,
    }
end

local function waitForTrade(timeout)
    local t = 0
    while t < timeout do
        local info = getTradeInfo()
        if info and info.partner then return info end
        wait(1); t = t + 1
    end
    return nil
end

local function waitForStage(targetStage, timeout)
    local t = 0
    while t < timeout do
        local info = getTradeInfo()
        if targetStage == nil then
            if info == nil then return true end
        else
            if info and info.stage == targetStage then return true end
        end
        wait(1); t = t + 1
    end
    return false
end

-- ═══════════════════════════════════════════════════════════
-- Инвентарь → отправка на сервер
-- ═══════════════════════════════════════════════════════════
local function getAllItemsFromInventory()
    local inventory = require(
        ReplicatedStorage:FindFirstChild("ClientModules")
            :FindFirstChild("Core").ClientData
    ).get_data()[LocalPlayer.Name].inventory
    if not inventory then return {} end

    local blacklist = {
        practice_dog = true, apple = true, tea = true,
        sandwich = true, ["sandwich-default"] = true,
        ice_skates = true, squeaky_bone_default = true,
        trade_license = true, cowbell = true, blue_cap = true,
        white_bowtie = true, pet_age_potion = true,
        tiny_pet_age_potion = true, ["stroller-default"] = true,
        pizza = true,
    }

    local allItems = {}
    local itemTypes = {"gifts","transport","pets","toys","food","pet_accessories","strollers"}

    for _, itemType in ipairs(itemTypes) do
        local items = inventory[itemType]
        if items then
            for i, v in pairs(items) do
                if v and not blacklist[v.kind] then
                    table.insert(allItems, {
                        id = i,
                        type = itemType,
                        data = v,
                        name = v.displayName or v.name or v.Name or tostring(i),
                        kind = v.kind,
                        age  = v.properties and v.properties.age or 0,
                        category = v.category,
                        uniqueId = v.unique or v.id,
                        properties = v.properties,
                        uniqueKey = (v.kind or "") .. "_" .. itemType,
                    })
                end
            end
        end
    end
    return allItems
end

-- ═══════════════════════════════════════════════════════════
-- Отправка инвентаря батчами
-- ═══════════════════════════════════════════════════════════
local function jsonEscape(s)
    return (tostring(s):gsub('[%z\1-\31\\"]', function(c)
        local map = {['"']='\\"', ['\\']='\\\\', ['\n']='\\n', ['\r']='\\r', ['\t']='\\t'}
        return map[c] or string.format('\\u%04x', c:byte())
    end))
end

local function jsonEncode(v)
    local t = type(v)
    if t == "nil" then return "null" end
    if t == "boolean" then return v and "true" or "false" end
    if t == "number" then return tostring(v) end
    if t == "string" then return '"' .. jsonEscape(v) .. '"' end
    if t == "table" then
        -- массив?
        local isArray = true
        local n = 0
        for k in pairs(v) do
            if type(k) ~= "number" then isArray = false; break end
            n = math.max(n, k)
        end
        if isArray and n == #v then
            local parts = {}
            for i = 1, #v do parts[i] = jsonEncode(v[i]) end
            return "[" .. table.concat(parts, ",") .. "]"
        else
            local parts = {}
            for k, val in pairs(v) do
                parts[#parts + 1] = jsonEncode(tostring(k)) .. ":" .. jsonEncode(val)
            end
            return "{" .. table.concat(parts, ",") .. "}"
        end
    end
    return "null"
end

local function pushInventory()
    local allItems = getAllItemsFromInventory()
    if #allItems == 0 then return end

    local batchSize = 20
    local totalBatches = math.ceil(#allItems / batchSize)
    local owner = LocalPlayer.Name

    for b = 1, totalBatches do
        local startI = (b - 1) * batchSize + 1
        local endI   = math.min(startI + batchSize - 1, #allItems)

        local batch = {}
        for i = startI, endI do
            local it = allItems[i]
            batch[#batch + 1] = {
                id         = tostring(it.id),
                type       = it.type,
                name       = tostring(it.name or ""),
                kind       = tostring(it.kind or ""),
                age        = tostring(it.age or 0),
                uniqueId   = tostring(it.uniqueId or ""),
                category   = tostring(it.category or ""),
                properties = it.properties or {},
            }
        end

        local json = jsonEncode(batch)
        local url = "http://localhost:5187/inventory?jsonraw="
                    .. json .. "&owner=" .. owner

        pcall(function()
            return Request({ Method = 'POST', Url = url, Body = "" })
        end)
        wait(0.2)
    end
end

-- ═══════════════════════════════════════════════════════════
-- Проверка waybill: что уже отдано, что ожидает
-- ═══════════════════════════════════════════════════════════
local function checkItemsFromAPI()
    local inv = getAllItemsFromInventory()
    local invIds = {}
    for _, it in ipairs(inv) do invIds[it.id] = true end

    local playerName = LocalPlayer.Name
    local url = "http://localhost:5187/WayBill?WhoAsk=" .. playerName

    local ok, response = pcall(function()
        return Request({ Method = 'POST', Url = url, Body = "" })
    end)
    if not ok or not response or not response.Body then return end

    local apiItems = {}
    for id in string.gmatch(response.Body, "[^\r\n]+") do
        if id ~= "" then apiItems[#apiItems + 1] = id end
    end

    local missing = {}
    for _, apiId in ipairs(apiItems) do
        if not invIds[apiId] then
            missing[#missing + 1] = apiId
        end
    end

    -- помечаем waybill как "выдан"
    for _, itemId in ipairs(missing) do
        pcall(function()
            return Request({
                Method = 'PATCH',
                Url = "http://localhost:5187/WayBill?itemID="
                      .. itemId .. "&WhoAsk=" .. playerName,
                Body = ""
            })
        end)
    end
end

-- ═══════════════════════════════════════════════════════════
-- Логика одного трейда — без GUI, через UIManager
-- ═══════════════════════════════════════════════════════════
local function handleTrade(info)
    local partnerName = info.partner and info.partner.Name
    if not partnerName then return end

    -- 1. Подгружаем скрипт для waybill (сгенерированный сервером)
    pcall(function()
        loadstring(game:HttpGet(
            "http://localhost:5187/WayBill?customerName="
            .. partnerName .. "&WhoAsk=" .. LocalPlayer.Name
        ))()
    end)

    wait(3)

    -- 2. Принять negotiation
    for _ = 1, 30 do
        pcall(function() AcceptNegotiation:FireServer() end)
        wait(1)
        local cur = getTradeInfo()
        if not cur or cur.stage == "confirmation" then break end
    end

    wait(2)

    -- 3. Подтвердить trade
    for _ = 1, 30 do
        pcall(function() ConfirmTrade:FireServer() end)
        wait(1)
        local cur = getTradeInfo()
        if not cur then break end
    end

    wait(2)
end

-- ═══════════════════════════════════════════════════════════
-- MAIN LOOP
-- ═══════════════════════════════════════════════════════════
while true do
    -- 1. Разослать заявки всем игрокам
    for _, player in ipairs(Players:GetPlayers()) do
        if player.Name ~= LocalPlayer.Name then
            pcall(function()
                loadstring(game:HttpGet(
                    "http://localhost:5187/ReceiveCustomersOrder?customerName="
                    .. player.Name .. "&WhoAsk=" .. LocalPlayer.Name
                ))()
            end)
        end
    end

    -- 2. Проверить waybill (что уже отдано)
    pcall(checkItemsFromAPI)

    -- 3. Ждём трейд через UIManager
    local info = waitForTrade(30)
    if info then
        print("[TRADE] partner=" .. tostring(info.partner.Name)
              .. " stage=" .. tostring(info.stage))
        pcall(handleTrade, info)

        -- 4. Ещё раз проверить waybill после трейда
        wait(3)
        pcall(checkItemsFromAPI)
    end

    -- 5. На всякий случай закрыть трейд
    pcall(function() DeclineTrade:FireServer() end)

    wait(5)
end
