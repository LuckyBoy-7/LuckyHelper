local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local entity = {}

entity.name = "LuckyHelper/FilmTextPivot"
entity.placements = {
    name = "normal",
    data = {

    }
}

function entity.sprite(room, entity)
    local sprites = {}
    
    local x = entity.x
    local y = entity.y
    
    local horizontal1 = drawableLine.fromPoints({ x, y, x + 320, y }, {1, 0, 0, 1}, 1)
    local horizontal2 = drawableLine.fromPoints({ x, y + 180, x + 320, y + 180 }, {1, 0, 0, 1}, 1)
    local vertical1 = drawableLine.fromPoints({ x, y, x, y + 180 }, {0, 1, 0, 1}, 1)
    local vertical2 = drawableLine.fromPoints({ x + 320, y, x + 320, y + 180 }, {0, 1, 0, 1}, 1)

    table.insert(sprites, horizontal1)
    table.insert(sprites, horizontal2)
    table.insert(sprites, vertical1)
    table.insert(sprites, vertical2)

    return sprites
end

function entity.selection(room, entity)
    local main = utils.rectangle(entity.x, entity.y, 320, 180)
    local nodes = {}

    return main, nodes
end

return entity
