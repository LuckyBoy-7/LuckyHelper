local enums = require "consts.celeste_enums"

local activationTypes = {
    ["OnJump"] = "OnJump",
}


local triggerTrigger = {}

triggerTrigger.triggerText = "T_T"
triggerTrigger.name = "LuckyHelper/TriggerTrigger"

triggerTrigger.nodeLimits = {1, -1}
triggerTrigger.nodeLineRenderType = "fan"

triggerTrigger.fieldInformation = {
    activationType = {
        editable = false,
        options = activationTypes
    },
}

--function triggerTrigger.ignoredFields(entity)
--    local ignored = {
--        "_id",
--        "_name",
--        "comparisonType",
--        "absoluteValue",
--        "talkBubbleX",
--        "talkBubbleY",
--        "flag",
--        "deaths",
--        "dashCount",
--        "requiredSpeed",
--        "timeToWait",
--        "coreMode",
--        "entityTypeToCollide",
--        "collideCount",
--        "solidType",
--        "entityType",
--        "inputType",
--        "holdInput",
--        "excludeTalkers",
--        "onlyIfSafe",
--        "playerState",
--        "includeCoyote"
--    }
--
--    local function doNotIgnore(value)
--        for i = #ignored, 1, -1 do
--            if ignored[i] == value then
--                table.remove(ignored, i)
--                return
--            end
--        end
--    end
--
--    local atype = entity.activationType or "Flag"
--    local iscomparison = false
--
--    if atype == "Flag" then
--        doNotIgnore("flag")
--    elseif atype == "DashCount" then
--        doNotIgnore("dashCount")
--        iscomparison = true
--    elseif atype == "DeathsInRoom" or atype == "DeathsInLevel" then
--        doNotIgnore("deaths")
--        iscomparison = true
--    elseif atype == "SpeedX" or atype == "SpeedY" then
--        doNotIgnore("requiredSpeed")
--        iscomparison = true
--    elseif atype == "TimeSinceMovement" then
--        doNotIgnore("timeToWait")
--        iscomparison = true
--    elseif atype == "CoreMode" then
--        doNotIgnore("coreMode")
--    elseif atype == "OnEntityCollide" then
--        doNotIgnore("entityType")
--        doNotIgnore("collideCount")
--        iscomparison = true
--    elseif atype == "OnInteraction" then
--        doNotIgnore("talkBubbleX")
--        doNotIgnore("talkBubbleY")
--    elseif atype == "OnSolid" then
--        doNotIgnore("solidType")
--    elseif atype == "OnEntityEnter" then
--        doNotIgnore("entityType")
--    elseif atype == "OnInput" then
--        doNotIgnore("inputType")
--        doNotIgnore("holdInput")
--        if entity.inputType == "Interact" then
--            doNotIgnore("excludeTalkers")
--        end
--    elseif atype == "OnGrounded" then
--        doNotIgnore("onlyIfSafe")
--        doNotIgnore("includeCoyote")
--    elseif atype == "OnPlayerState" then
--        doNotIgnore("playerState")
--    end
--
--    if iscomparison then
--        doNotIgnore("comparisonType")
--        doNotIgnore("absoluteValue")
--    end
--
--    return ignored
--end

triggerTrigger.placements = {}
for _, mode in pairs(activationTypes) do
    local placement = {
        name = string.lower(mode),
        data = {
            oneUse = false,
            invertCondition = false,
            delay = 0.0,
            coverRoom = false,
            randomize = false,
            matchPosition = true,
            activationType = mode,
            tryTriggerEveryFrame = true,
        }
    }
    table.insert(triggerTrigger.placements, placement)
end

return triggerTrigger
