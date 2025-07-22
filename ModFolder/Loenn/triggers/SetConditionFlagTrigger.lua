--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/SetConditionFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        conditionType = "LuckyHelper_OnJump",
        activationType = "Stay",
        flag = "OnJumpFlag",
        removeFlagDelayedFrames = -1
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