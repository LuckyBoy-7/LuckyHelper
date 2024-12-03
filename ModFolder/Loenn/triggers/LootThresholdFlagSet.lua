--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/LootThresholdFlagSet"
trigger.placements = {
    name = "normal",
    data = {
        threshold = 100,
        flag = ""
    }
}

trigger.fieldInformation = {
    threshold = {
        fieldType = "integer"
    }
}

return trigger