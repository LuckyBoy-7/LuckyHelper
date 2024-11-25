--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/KillGhostTrigger"
trigger.placements = {
    name = "normal",
    data = {
        killPlayerToo = true,
    }
}

trigger.fieldInformation = {
    killPlayerToo = {
        fieldType = "boolean"
    }
}

return trigger