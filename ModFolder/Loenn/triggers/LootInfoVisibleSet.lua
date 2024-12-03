--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/LootInfoVisibleSet"
trigger.placements = {
    name = "normal",
    data = {
        visible = true
    }
}

trigger.fieldInformation = {
    visible = {
        fieldType = "boolean"
    }
}

return trigger