--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/PlayerMovementController"
trigger.placements = {
    name = "normal",
    data = {
        jumpForceX = 40,
        jumpForceY = -105,
        wallJumpForceX = 130,
        wallJumpForceY = -105,
        jumpKeepSpeedTime = 0.2,
        wallJumpKeepSpeedTime = 0.2,
        activationType = "None"
    }
}

trigger.fieldOrder = {
    "width", "height",
    "x", "y",
    "jumpForceX", "jumpForceY",
    "wallJumpForceX", "wallJumpForceY",
    "jumpKeepSpeedTime", "wallJumpKeepSpeedTime",
    "", "",
    "activationType", "",
}

trigger.fieldInformation = {
    activationType = {
        options = {
            "None",
            "Set",
            "Stay",
        },
        editable = false
    },
}

return trigger
