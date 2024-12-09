--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/DeathCountSavedIn"
trigger.placements = {
    name = "normal",
    data = {
        savedIn = "where the death count will be added to",
    }
}

trigger.fieldInformation = {
}

return trigger