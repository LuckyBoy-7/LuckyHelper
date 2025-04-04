local drawableSprite = require("structs.drawable_sprite")

local entity = {}

entity.name = "LuckyHelper/DecalWithCombinedRegistry"
entity.placements = {
    name = "normal",
    data = {
        x = 0,
        y = 0,

        scaleX = 1,
        scaleY = 1,

        depth = 1,
        texture = "",

        rotation = 0,
        color = "ffffff",


        decalRegistryPaths = "",
    }
}

entity.fieldInformation =
{
    color = {
        fieldType = "color"
    },
    depth = {
        fieldType = "integer"
    }
}
function entity.fieldOrder(layer, decal)
    return { "x", "y",
        "scaleX", "scaleY",
        "texture", "depth",
        "rotation", "color",
        "decalRegistryPaths" }
end

local function getSprite(room, entity)
    local sprite = drawableSprite.fromTexture("decals/" .. entity.texture, entity)
    sprite:setScale(entity.scaleX, entity.scaleY)
    sprite.rotation = math.rad(entity.rotation or 0)
    sprite.depth = entity.depth
    return sprite
end

function entity.sprite(room, entity)
    --local sprites = {}
    --
    --local sprite = getSprite(room, entity)
    --table.insert(sprites, sprite)
    return getSprite(room, entity)
end

return entity
