local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")

local entity = {}
local depth = 9999

entity.name = "LuckyHelper/SpeedRedirect"
entity.placements = {
    name = "normal",
    data = {
        depth = depth,
        radius = 8,
        attractSpeed = 200,
        adjustX = 0,
        adjustY = 0,
        naiveMove = false,
        colliderType = "Rectangle",
        boxWidth = 16,
        boxHeight = 16,

        spriteXMLID = "speedRedirect",
        borderColor = "888888",
        innerColor = "666666",
        alpha = 0.5,
        showBorder = true,
        showBackground = true,
        showSprite = true,
        flipSpriteX = false,
        flipSpriteY = false,
        spriteRotation = -45,

        triggerRedirectTiming = "OnAttractedToCenter",
        speedRedirectDirType = "Custom",
        speedRedirectStrengthType = "PlayerEnterSpeed",
        redirectDirX = 0.5,
        redirectDirY = -0.5,
        shootSpeedMultiplier = 1.2,
        minShootSpeed = 0,
        maxShootSpeed = 9999,
        fixedShootSpeed = 240,
        minCorrectionSpeed = 300,
        maxCorrectionSpeed = 400
    }
}
entity.fieldOrder = {
    "x", "y",
    "adjustX", "adjustY",
    "boxWidth", "boxHeight",
    "borderColor", "innerColor",
    "alpha",
    "depth", "radius",
    "attractSpeed", "colliderType",
    "spriteXMLID",
    "naiveMove", "showBorder", "showBackground", "showSprite",
    "flipSpriteX", "flipSpriteY", "spriteRotation",

    "redirectDirX", "redirectDirY",
    "minShootSpeed", "maxShootSpeed",
    "minCorrectionSpeed", "maxCorrectionSpeed",
    "shootSpeedMultiplier", "fixedShootSpeed",
    "triggerRedirectTiming", "speedRedirectDirType",
    "speedRedirectStrengthType"
}
entity.fieldInformation =
{
    depth = {
        fieldType = "integer"
    },
    radius = {
        fieldType = "integer"
    },
    attractSpeed = {
        fieldType = "integer"
    },
    adjustX = {
        fieldType = "integer"
    },
    adjustY = {
        fieldType = "integer"
    },
    boxWidth = {
        fieldType = "integer"
    },
    boxHeight = {
        fieldType = "integer"
    },
    borderColor = {
        fieldType = "color"
    },
    innerColor = {
        fieldType = "color"
    },
    colliderType = {
        options = {
            "Circle",
            "Rectangle"
        },
        editable = false
    },
    triggerRedirectTiming = {
        options = {
            "OnEnter",
            "OnAttractedToCenter"
        },
        editable = false
    },
    speedRedirectDirType = {
        options = {
            "Custom",
            "flipX",
            "flipY",
            "Rebound"
        },
        editable = false
    },
    speedRedirectStrengthType = {
        options = {
            "PlayerEnterSpeed",
            "Fixed",
            "Correction"
        },
        editable = false
    },
}


function entity.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local color = { 0.5, 0.5, 0.5, 1 }
    local thickness = 1
    local halfWidth = entity.radius
    local halfHeight = entity.radius
    if entity.colliderType == "Rectangle" then
        halfWidth = entity.boxWidth / 2
        halfHeight = entity.boxHeight / 2
    end

    local line1 = drawableLine.fromPoints({ x + halfWidth, y + halfHeight, x + halfWidth, y - halfHeight }, color,
        thickness)
    local line2 = drawableLine.fromPoints({ x + halfWidth, y - halfHeight, x - halfWidth, y - halfHeight }, color,
        thickness)
    local line3 = drawableLine.fromPoints({ x - halfWidth, y - halfHeight, x - halfWidth, y + halfHeight }, color,
        thickness)
    local line4 = drawableLine.fromPoints({ x - halfWidth, y + halfHeight, x + halfWidth, y + halfHeight }, color,
        thickness)

    line1.depth = depth
    line2.depth = depth
    line3.depth = depth
    line4.depth = depth
    
    -- 图片部分
    local sprite = drawableSprite.fromTexture("LuckyHelper/right_arrow", entity)
    sprite.depth = depth
    local DegToRad = 0.017453292;
   
    if entity.flipSpriteX then
        sprite.scaleX = sprite.scaleX * -1
    end
    if entity.flipSpriteY then
        sprite.scaleY = sprite.scaleY * -1
    end
     
     sprite.rotation = entity.spriteRotation * DegToRad or 0

    table.insert(sprites, line1:getDrawableSprite()[1])
    table.insert(sprites, line2:getDrawableSprite()[1])
    table.insert(sprites, line3:getDrawableSprite()[1])
    table.insert(sprites, line4:getDrawableSprite()[1])

    if entity.showSprite then
        table.insert(sprites, sprite)
    end
    return sprites
end

entity.depth = depth

return entity
