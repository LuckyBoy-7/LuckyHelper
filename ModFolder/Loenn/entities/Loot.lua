local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local loot = {}

loot.name = "LuckyHelper/Loot"
loot.placements = {
    {
        name = "normal",
        data = {
            loennImagePath = "collectables/strawberry/normal00",
            animationPath = "LuckyHelper/collectables/strawberry/normal",
            colliderWidth = 14,
            colliderHeight = 14,
            value = 100
        }
    }
}

loot.fieldInformation = {
    colliderWidth = {
        fieldType = "integer"
    },
    colliderHeight = {
        fieldType = "integer"
    },
    value = {
        fieldType = "integer"
    }
}

loot.fieldOrder = {
    "x", "y",
    "loennImagePath", "animationPath",
    "colliderWidth", "colliderHeight",
    "value"
}

function loot.texture(room, entity)
    return entity.loennImagePath
end

return loot
