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
       
        lowSpeedAccelerationX = 400,
        highSpeedAccelerationX = 1000,
        maxFallSpeed = 160,
        maxFastFallSpeed = 240,
       
        maxSpeedXMultiplier = 1,
        activationType = "Set"
    }
}

trigger.fieldOrder = {
    "width", "height",
    "x", "y",
    "jumpForceX", "jumpForceY",
    "wallJumpForceX", "wallJumpForceY",
    "jumpKeepSpeedTime", "wallJumpKeepSpeedTime",
    "lowSpeedAccelerationX", "highSpeedAccelerationX",
    "maxFallSpeed", "maxFastFallSpeed",
    "maxSpeedXMultiplier", "",
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
