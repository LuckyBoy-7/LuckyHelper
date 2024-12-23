local entity = {}

entity.name = "LuckyHelper/CustomWater"
entity.placements = {
    name = "normal",
    data = {
        width = 8,
        height = 8,
        color = "7DCEFA",
        hasBottom = false,
        disableSurfaceJump = false,
        disableSwimRise = false,
        maxSpeedMultiplierX = 1,
        maxSpeedMultiplierY = 1,
        accelerationMultiplierX = 1,
        accelerationMultiplierY = 1,
        killPlayer = false,
        killPlayerDelay = 0,
        playerFlashTimeBeforeKilled = 2,
        playerFlashColor = "50a4d4",
        playerLoseControl = false,
        playerGravity = 0,
        playerCanJump = false,
        refillExtraJump = false,
        disableRay = false,
    }
}

entity.fieldOrder = {
    "x", "y", 
    "width", "height",
    "color", "disableRay", "hasBottom",
    "maxSpeedMultiplierX", "maxSpeedMultiplierY",
    "accelerationMultiplierX", "accelerationMultiplierY",
    "playerFlashTimeBeforeKilled", "playerFlashColor",
    "killPlayerDelay", "killPlayer",
    "playerGravity", "",
    "", "disableSurfaceJump",
    "disableSwimRise", "playerLoseControl",
    "playerCanJump", "refillExtraJump",
}

entity.fieldInformation = 
{
    color = {
        fieldType = "color"
    },
    width = {
        fieldType = "integer"
    },
    height = {
        fieldType = "integer"
    },
    hasBottom = {
        fieldType = "boolean"
    },
    disableSurfaceJump = {
        fieldType = "boolean"
    },
    disableSwimRise = {
        fieldType = "boolean"
    },
    killPlayer = {
        fieldType = "boolean"
    },
    playerLoseControl = {
        fieldType = "boolean"
    },
    playerCanJump = {
        fieldType = "boolean"
    },
    refillExtraJump = {
        fieldType = "boolean"
    },
    disableRay = {
        fieldType = "boolean"
    },
    playerFlashColor = {
        fieldType = "color"
    }
}
entity.borderColor = {30 / 255, 164 / 255, 231 / 255}
entity.fillColor = {85 / 175, 164 / 255, 220 / 255, 0.6}
return entity
