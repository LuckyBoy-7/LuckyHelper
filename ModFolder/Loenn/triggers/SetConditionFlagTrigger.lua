--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/SetConditionFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        on = true,
        stateType = "OnJump",
        flag = "OnJumpFlag",
        removeFlagDelayedFrames = -1
    }
}

trigger.fieldInformation = {
    stateType = {
        options = {
            "OnJump"
        },
        editable = false,
    },
    removeFlagDelayedFrames = {
        fieldType = "integer",
    },
}

return trigger