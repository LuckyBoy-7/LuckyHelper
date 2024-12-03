--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/LootCollectArea"
trigger.placements = {
    name = "normal",
    data = {
        collectOnGround = true
    }
}

trigger.fieldInformation = {
    collectOnGround = {
        fieldType = "boolean"
    }
}

return trigger