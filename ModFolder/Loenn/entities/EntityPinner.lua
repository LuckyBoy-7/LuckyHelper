local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")

local entity = {}
local depth = 9999

entity.name = "LuckyHelper/EntityPinner"
entity.placements = {
    name = "normal",
    data = {
        depth = depth,
        radius = 8,
        attractSpeed = 80,
        adjustX = 0,
        adjustY = 0,
        naiveMove = false,
        colliderType = "Circle",
        boxWidth = 30,
        boxHeight = 10,
        types = "TheoCrystal, Glider",
        spriteXMLID = "entityPinner",
        borderColor = "217dff",
        innerColor = "86ffff",
        alpha = 0.5,
        showBorder = true,
        showBackground = true
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
    "types", "spriteXMLID",
    "naiveMove", "showBorder", "showBackground"
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
}


function entity.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local color = { 0.1, 0.5, 1, 1 }
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

    local sprite = drawableSprite.fromTexture("LuckyHelper/entity_pinner", entity)
    sprite.depth = depth

    table.insert(sprites, line1:getDrawableSprite()[1])
    table.insert(sprites, line2:getDrawableSprite()[1])
    table.insert(sprites, line3:getDrawableSprite()[1])
    table.insert(sprites, line4:getDrawableSprite()[1])
    table.insert(sprites, sprite)

    return sprites
end

entity.depth = depth

return entity
