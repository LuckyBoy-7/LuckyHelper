--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/SetConditionFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        conditionType = "OnJump",
        activationType = "Stay",
        flag = "LuckyHelper_OnJumpFlag",
        removeFlagDelayedFrames = -1,
        coverRoom = false
    }
}

trigger.fieldInformation = {
    conditionType = {
        options = {
            "OnJump"
        },
        editable = false,
    }, 
    activationType = {
        options = {
            "None",
            "Set",
            "Stay",
        },
        editable = false,
    },
    removeFlagDelayedFrames = {
        fieldType = "integer",
    },
}

return trigger