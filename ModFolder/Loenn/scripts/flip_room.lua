local snapshot = require("structs.snapshot")
local brushHelper = require("brushes")
local selectionUtils = require("selections")

local script = {
    name = "flipRoom",
    displayName = "Flip Room",
    parameters = {
        flipContentHorizontal = true,
        flipContentVertical = false,
        flipRoomHorizontal = false,
        flipRoomVertical = false,
        radicalFlip = true,
    },
    fieldOrder = { "flipContentHorizontal", "flipContentVertical", "flipRoomHorizontal", "flipRoomVertical", "radicalFlip" },
    fieldInformation = {

    },
    tooltip = "Flip room",
    tooltips = {
        radicalFlip = "This will directly change the type of the entity like `spikesLeft` to `spikesRight`, `spikesUp` to `spikesDown` to flip them, but may break something",
    },
}

function script.run(room, args, ctx)

    local flipContentHorizontal = args.flipContentHorizontal
    local flipContentVertical = args.flipContentVertical
    local flipRoomHorizontal = args.flipRoomHorizontal
    local flipRoomVertical = args.flipRoomVertical
    local radicalFlip = args.radicalFlip

    local function forward(data)
        flipTiles(room, flipContentHorizontal, flipContentVertical)
        flipItems(room, flipContentHorizontal, flipContentVertical, radicalFlip)
        flipRoom(room, flipRoomHorizontal, flipRoomVertical)
    end

    local function backward(data)
        flipTiles(room, flipContentHorizontal, flipContentVertical)
        flipItems(room, flipContentHorizontal, flipContentVertical, radicalFlip)
        flipRoom(room, flipRoomHorizontal, flipRoomVertical)
    end

    forward()

    return snapshot.create(script.name, {}, backward, forward)
end

local entityOffset = {
    triggerspike = {
        horizontal = {
            left = { 3, 0 },
            right = { 3, 0 }
        },
        vertical = {
            up = { 0, 3 },
            down = { 0, 3 }
        }
    },
    spike = {
        horizontal = {
            left = { 6, 0 },
            right = { 6, 0 }
        },
        vertical = {
            up = { 0, 6 },
            down = { 0, 6 }
        }
    },
    spring = {
        horizontal = {
            left = { 3, 0 },
            right = { 3, 0 },
            down = { 12, 0 },
            up = { 12, 0 }, -- 或者 default
        },
        vertical = {
            left = { 0, 12 },
            right = { 0, 12 },
            down = { 0, 3 },
            up = { 0, 3 },
        }
    }
}

local horizontalDirectionMapping = {
    ["left"] = "right",
    ["right"] = "left",

    ["Left"] = "Right",
    ["Right"] = "Left",
}

local verticalDirectionMapping = {
    ["up"] = "down",
    ["down"] = "up",

    ["Up"] = "Down",
    ["Down"] = "Up",
}

local axisX = {
    getPos = function(v)
        return v.x
    end,
    setPos = function(v, value)
        v.x = value
    end,

    getSize = function(v)
        return v.width
    end,
}

local axisY = {
    getPos = function(v)
        return v.y
    end,
    setPos = function(v, value)
        v.y = value
    end,

    getSize = function(v)
        return v.height
    end,
}

function flipRoom(room, horizontal, vertical)

    if horizontal then
        room.x = -room.x - room.width
    end

    if vertical then
        room.y = -room.y - room.height
    end
end

function flipTiles(room, horizontal, vertical)

    local matrix = room.tilesFg.matrix;
    matrix:flip(horizontal, vertical);
    brushHelper.placeTile(room, room.x, room.y, room.tilesFg.matrix, "tilesFg")

    matrix = room.tilesBg.matrix;
    matrix:flip(horizontal, vertical);
    brushHelper.placeTile(room, room.x, room.y, room.tilesFg.matrix, "tilesBg")

    return true
end

function flipItems(room, horizontal, vertical, radicalFlip)

    local entities = room.entities;
    local triggers = room.triggers;
    local decalsFg = room.decalsFg;
    local decalsBg = room.decalsBg;

    for i, entity in ipairs(entities) do
        flipItem(room, entity, "entities", horizontal, vertical, radicalFlip)
    end

    for i, trigger in ipairs(triggers) do
        flipItem(room, trigger, "triggers", horizontal, vertical, false)
    end

    for i, decal in ipairs(decalsFg) do
        flipItem(room, decal, "decalsFg", horizontal, vertical, false)
    end

    for i, decal in ipairs(decalsBg) do
        flipItem(room, decal, "decalsBg", horizontal, vertical, false)
    end
end

function flipItem(room, entity, layer, horizontal, vertical, radicalFlip)
    if horizontal then
        flipItemByAxis(room, entity, layer, axisX, radicalFlip);
    end

    if vertical then
        flipItemByAxis(room, entity, layer, axisY, radicalFlip);
    end
end

function flipItemByAxis(room, entity, layer, axis, radicalFlip)
    if entity == nil then
        return
    end

    local isEntity = layer == "entities"
    local isDecal = layer == "decalsBg" or layer == "decalsFg"
    local horizontal = axis == axisX
    -- https://github.com/CelestialCartographers/Loenn/blob/fd55ffb63a290f38bed45de3309a47b504b5ce54/src/selections.lua#L12
    --function selectionUtils.getSelectionsForItem(room, layer, item, rectangles)
    local selection = selectionUtils.getSelectionsForItem(room, layer, entity)

    -- node
    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            local nodeRectangle = selection[i + 1]
            axis.setPos(node, getFlippedPosWithSelectionRectangle(room, node, nodeRectangle, axis))
        end
    end

    -- main
    local mainRectangle = selection[1]
    --print(mainRectangle.x, mainRectangle.y, mainRectangle.width, mainRectangle.height)
    if isEntity and entityIsSpetial(entity) then
        axis.setPos(entity, getFlippedPosWithEntityPos(room, entity, mainRectangle, axis))
        addCorrectedOffsetToEntity(entity, horizontal)
    else
        axis.setPos(entity, getFlippedPosWithSelectionRectangle(room, entity, mainRectangle, axis))
    end

    -- radicalFlip
    if isEntity and radicalFlip then
        swapDirectionInName(entity, horizontal)
        invertLeftField(entity, horizontal)
        invertDownField(entity, not horizontal)
        invertDirection(entity, horizontal)
    end

    if isDecal then
        reverseDecalScale(entity, room, layer, horizontal, flipRectangle(room, mainRectangle))
    end
end

function flipRectangle(room, rectangle)
    rectangle.x = room.width - rectangle.x - rectangle.width
    rectangle.y = room.height - rectangle.y - rectangle.height
    return rectangle
end

function reverseDecalScale(entity, room, layer, horizontal, flippedRectangle)
    -- 主要用来反转 decal 那些
    -- 因为 decal 反转的时候是按原图翻的, 所以如果原图有空白区域反转就会错误, 因为我们是按 selection 换位置的, 所以框的位置是对的, 但是贴图的位置不对
    -- 所以我们要重新算一下框把位置补偿回去

    local flipX = entity.scaleX ~= nil and horizontal
    local flipY = entity.scaleY ~= nil and not horizontal
    if flipX then
        entity.scaleX = entity.scaleX * -1
    end
    if flipY then
        entity.scaleY = entity.scaleY * -1
    end

    local selection = selectionUtils.getSelectionsForItem(room, layer, entity)
    local rectangleAfterScale = selection[1]

    if flipX then
        entity.x = entity.x + flippedRectangle.x - rectangleAfterScale.x;
    end
    if flipY then
        entity.y = entity.y + flippedRectangle.y - rectangleAfterScale.y;
    end
end

function entityIsSpetial(entity)
    -- 推断实体类型（spike, spring等）
    for key in pairs(entityOffset) do
        if entityIs(entity, key) then
            return true
        end
    end
    return false
end

-- 使用 selection 可以方便的得到 那些 改过 justification, scale 等 的实体范围
-- 但是对于尖刺之类的最还是用普通的逻辑, 因为 selection 好像跟图像有关, 哪怕两个尖刺位置一样, 一个朝上一个朝下 selection 也会不一样的
function getFlippedPosWithSelectionRectangle(room, entity, selectionRectangle, axis)
    local sizeInOneAxis = axis.getSize(selectionRectangle)
    local paddingLeft = axis.getPos(entity) - axis.getPos(selectionRectangle)
    -- 图片锚点离左边的位置
    local rightPadding = sizeInOneAxis - paddingLeft
    return axis.getSize(room) - axis.getPos(selectionRectangle) - rightPadding
end

function getFlippedPosWithEntityPos(room, entity, selectionRectangle, axis)
    local size = axis.getSize(entity);
    if size == nil then
        size = axis.getSize(selectionRectangle)
    end

    return axis.getSize(room) - axis.getPos(entity) - size
end

function addCorrectedOffsetToEntity(entity, horizontal)

    local name = entity._name:lower()
    local direction = "up"

    -- 从名字推断方向
    if name:find("left") then
        direction = "left"
    elseif name:find("right") then
        direction = "right"
    elseif name:find("down") then
        direction = "down"
    elseif name:find("up") then
        direction = "up"
    end

    -- 推断实体类型（spike, spring等）
    local type
    for key in pairs(entityOffset) do
        if name:find(key) then
            if type == nil or #key > #type then
                -- 保证用长的 key, 比如 triggerSpike 和 spike 都存在的时候用前者
                type = key
            end
        end
    end

    if not type then
        return -- 未匹配到类型
    end

    local axisType = horizontal and "horizontal" or "vertical"
    local offsetTable = entityOffset[type][axisType]
    local offset = offsetTable[direction] or { 0, 0 }

    entity.x = entity.x + offset[1]
    entity.y = entity.y + offset[2]
end

function invertLeftField(entity, horizontal)
    if horizontal then
        if entity.left ~= nil and type(entity.left) == "boolean" then
            entity.left = not entity.left
        end
        if entity.leftSide ~= nil and type(entity.leftSide) == "boolean" then
            entity.leftSide = not entity.leftSide
        end

        -- 河豚
        if entityIs(entity, "eyebomb") and entity.right ~= nil and type(entity.right) == "boolean" then
            entity.right = not entity.right
        end
    end
end

function entityIs(entity, str)
    return entity._name:lower():find(str)
end

function invertDownField(entity, vertical)
    if vertical then
        if entityIs(entity, "dashswitch") and entity.ceiling ~= nil and type(entity.ceiling) == "boolean" then
            entity.ceiling = not entity.ceiling
        end
    end
end

function invertDirection(entity, horizontal)
    local name = entity._name:lower()
    if name:find("moveblock") then
        if entity.direction ~= nil then
            if horizontal then
                if horizontalDirectionMapping[entity.direction] ~= nil then
                    entity.direction = horizontalDirectionMapping[entity.direction]
                end
            else
                if verticalDirectionMapping[entity.direction] ~= nil then
                    entity.direction = verticalDirectionMapping[entity.direction]
                end
            end
        end
    end
end

function swapDirectionInName(entity, horizontal)
    local name = entity._name
    if not name then
        return
    end

    if horizontal then
        -- left ↔ right
        name = name:gsub("Left", "___TMP___")
                   :gsub("Right", "Left")
                   :gsub("___TMP___", "Right")

        name = name:gsub("left", "___tmp___")
                   :gsub("right", "left")
                   :gsub("___tmp___", "right")

        name = name:gsub("LEFT", "___TMP2___")
                   :gsub("RIGHT", "LEFT")
                   :gsub("___TMP2___", "RIGHT")
    else
        -- up ↔ down
        name = name:gsub("Up", "___TMP3___")
                   :gsub("Down", "Up")
                   :gsub("___TMP3___", "Down")

        name = name:gsub("up", "___tmp4___")
                   :gsub("down", "up")
                   :gsub("___tmp4___", "down")

        name = name:gsub("UP", "___TMP5___")
                   :gsub("DOWN", "UP")
                   :gsub("___TMP5___", "DOWN")

    end
    entity._name = name
end

return script

