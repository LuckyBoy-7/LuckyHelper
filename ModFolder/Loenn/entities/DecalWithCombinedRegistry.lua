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
        texture = "LuckyHelper/test_decal00",

        rotation = 0,
        color = "ffffff",


        decalRegistryPaths = "lucky_scale,lucky_rotate, lucky_floaty, lucky_smoke",
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
