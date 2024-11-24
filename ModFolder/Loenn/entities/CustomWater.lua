local entity = {}

entity.name = "LuckyHelper/CustomWater"
entity.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,
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
        playerLoseControl = false,
        playerGravity = 0,
        playerCanJump = false,
        refillExtraJump = false,
        disableRay = false,
    }
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
    }
}
entity.borderColor = {30 / 255, 164 / 255, 231 / 255}
entity.fillColor = {85 / 175, 164 / 255, 220 / 255, 0.6}
return entity
