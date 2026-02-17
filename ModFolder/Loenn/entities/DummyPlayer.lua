local drawableSprite = require("structs.drawable_sprite")
local jautils = require("mods").requireFromPlugin("libraries.jautils")

local entity = {
    name = "LuckyHelper/DummyPlayer",

    placements = {
        name = "normal",
        data = {
            sendOriginalPlayerToTrigger = false,
            triggerDashFlag = "LuckyHelper_TriggerDashFlag",
            triggerRidingFlag = "LuckyHelper_TriggerRidingFlag",
            triggerCollisionFlag = "LuckyHelper_TriggerCollisionFlag",
            whiteList = "",
            blackList = "",
            affectRadius = 100000,
            affectedByWind = false
        }
    },
    color = { 0.3, 0.3, 0.3 },
    
    fieldOrder = {
        "x", "y",
        "whiteList", "blackList",
        "triggerDashFlag", "triggerRidingFlag", "triggerCollisionFlag",
        "affectRadius", "affectedByWind"
    },

    fieldInformation = {
        whiteList = {
            fieldType = "list",
            elementOptions = {
                fieldType = "string",
            }
        },
        blackList = {
            fieldType = "list",
            elementOptions = {
                fieldType = "string",
            }
        }
    }
}

function entity.sprite(room, entity)
    local sprites = {}

    local texture = drawableSprite.fromTexture("characters/player/sitDown00", entity)
    texture:setJustification(0.5, 1)
    texture.color = {0.5, 0.5, 0.5, 1}
    local circle = jautils.getCircleSprite(entity.x, entity.y, entity.affectRadius or 100000, { 1, 0, 0, 1 })

    table.insert(sprites, texture)
    table.insert(sprites, circle)

    return sprites
end

return entity
