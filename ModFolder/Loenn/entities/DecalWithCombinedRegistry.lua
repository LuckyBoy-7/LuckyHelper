local drawableSprite = require("structs.drawable_sprite")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local consts = require('mods').requireFromPlugin('libraries.consts')

local entity = {}

entity.name = "LuckyHelper/DecalWithCombinedRegistry"
entity.placements = {
    name = "normal",
    data = {
        x = 0,
        y = 0,

        scaleX = 1,
        scaleY = 1,

        depth = -10500,
        texture = "LuckyHelper/test_decal00",

        rotation = 0,
        color = "ffffff",


        decalRegistryPaths = "lucky_scale_big,lucky_rotate,lucky_floaty,lucky_smoke,lucky_banner,lucky_bloom,lucky_animationSpeed,lucky_mirror,lucky_small_staticMover",
    }
}

entity.fieldInformation =
{
    color = {
        fieldType = "color",
        useAlpha = true,
    },
    depth = {
        fieldType = "integer",
        options = consts.depthOptions,
        editable = true,
    },
    texture = vivUtil.GetFilePathWithNoTrailingNumbers(true, "Gameplay/decals"),
    decalRegistryPaths = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
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
