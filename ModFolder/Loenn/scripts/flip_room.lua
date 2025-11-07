local snapshot = require("structs.snapshot")
local brushHelper = require("brushes")
local selectionUtils = require("selections")

local script = {
    name = "flipRoom",
    displayName = "Flip Room",
    parameters = {
        --customFlipRules = "",
        flipHorizontal = true,
        flipVertical = false,
        radicalFlip = true,
    },
    fieldOrder = { "flipHorizontal", "flipVertical", "radicalFlip" },
    fieldInformation = {

    },
    tooltip = "Flip room",
    tooltips = {
        customFlipRules = "This script can only do simple position flip, if you want to 'flip' specified entity's attributes, you have to do the corresponding conifgs",
        radicalFlip = "This will directly change the type of the entity like `spikesLeft` to `spikesRight`, `spikesUp` to `spikesDown` to flip them, but may break something",
    },
}

function script.run(room, args, ctx)

    local flipHorizontal = args.flipHorizontal
    local flipVertical = args.flipVertical
    local radicalFlip = args.radicalFlip
    local function forward(data)
        flipTiles(room, flipHorizontal, flipVertical)
        flipEntities(room, flipHorizontal, flipVertical, radicalFlip)
        flipNonEntities(room, flipHorizontal, flipVertical, radicalFlip)
    end

    local function backward(data)
        flipTiles(room, flipHorizontal, flipVertical)
        flipEntities(room, flipHorizontal, flipVertical, radicalFlip)
        flipNonEntities(room, flipHorizontal, flipVertical, radicalFlip)
    end

    forward()

    return snapshot.create(script.name, {}, backward, forward)
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

function flipEntities(room, horizontal, vertical, radicalFlip)

    local entities = room.entities;

    for i, entity in ipairs(entities) do
        flipEntity(room, entity, horizontal, vertical, radicalFlip)
    end

    return true
end
function flipNonEntities(room, horizontal, vertical, radicalFlip)

    local triggers = room.triggers;

    for i, trigger in ipairs(triggers) do
        flipTrigger(room, trigger, horizontal, vertical, radicalFlip)
    end

    local decalsFg = room.decalsFg;

    for i, trigger in ipairs(decalsFg) do
        flipDecal(room, trigger, "decalsFg", horizontal, vertical, radicalFlip)
    end
    local decalsBg = room.decalsBg;

    for i, trigger in ipairs(decalsBg) do
        flipDecal(room, trigger, "decalsBg", horizontal, vertical, radicalFlip)
    end

    return true
end
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

function flipEntity(room, entity, horizontal, vertical, radicalFlip)
    if horizontal then
        flipEntityByAxis(room, entity, axisX, radicalFlip);
    end

    if vertical then
        flipEntityByAxis(room, entity, axisY, radicalFlip);
    end
end

function flipTrigger(room, entity, horizontal, vertical, radicalFlip)
    if horizontal then
        flipTriggerByAxis(room, entity, axisX, radicalFlip);
    end

    if vertical then
        flipTriggerByAxis(room, entity, axisY, radicalFlip);
    end
end

function flipDecal(room, entity, layer, horizontal, vertical, radicalFlip)
    if horizontal then
        flipDecalByAxis(room, entity, layer, axisX, radicalFlip);
    end

    if vertical then
        flipDecalByAxis(room, entity, layer, axisY, radicalFlip);
    end
end

local specitalEntities = {
    "spike",
    "spring"
}
function entityIsSpetial(entity)
    for _, str in ipairs(specitalEntities) do
        if containsWord(entity._name, str) then
            return true
        end
    end
    return false
end

function flipEntityByAxis(room, entity, axis, radicalFlip)
    if entity == nil then
        return
    end

    -- https://github.com/CelestialCartographers/Loenn/blob/fd55ffb63a290f38bed45de3309a47b504b5ce54/src/selections.lua#L12
    --function selectionUtils.getSelectionsForItem(room, layer, item, rectangles)
    local selection = selectionUtils.getSelectionsForItem(room, "entities", entity)

    -- node
    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            local nodeRectangle = selection[i + 1]
            axis.setPos(node, getFlippedPosWithSelectionRectangle(room, node, nodeRectangle, axis))
        end
    end

    -- main
    local mainRectangle = selection[1]
    if entityIsSpetial(entity) then
        axis.setPos(entity, getFlippedPosWithEntityPos(room, entity, mainRectangle, axis))
        addCorrectedOffsetToEntity(entity, axis)
    else
        axis.setPos(entity, getFlippedPosWithSelectionRectangle(room, entity, mainRectangle, axis))
    end

    -- radicalFlip
    if radicalFlip then
        swapDirectionInName(entity, axis)
        invertLeftField(entity, axis)
        invertDirection(entity, axis)
    end
end

function flipTriggerByAxis(room, trigger, axis)
    flipNonEntityByAxis(room, trigger, "triggers", axis)
end

function flipDecalByAxis(room, decal, layer, axis)
    flipNonEntityByAxis(room, decal, layer, axis)
end

function flipNonEntityByAxis(room, entity, layer, axis)
    if entity == nil then
        return
    end

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
    axis.setPos(entity, getFlippedPosWithSelectionRectangle(room, entity, mainRectangle, axis))

    -- 主要用来反转 decal 那些
    local horizontal = axis == axisX
    if entity.scaleX ~= nil then
        if horizontal then
            entity.scaleX = entity.scaleX * -1
        end
    end
    if entity.scaleY ~= nil then
        if not horizontal then
            entity.scaleY = entity.scaleY * -1
        end
    end

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

    -- 补充锚点(代码越来越屎了😭), 然后突然意识到我好像拿不到这方面的数据(或者还没找到)
    --local horizontal = axis == axisX
    --local justification = 0
    --print(111)
    --if entity.justification ~= nil then
    --    print(justification)
    --    if horizontal then
    --        justification = entity.justification[1]
    --    else
    --        justification = entity.justification[2]
    --    end
    --end

    --local rightPadding = size * (1 - justification)

    return axis.getSize(room) - axis.getPos(entity) - size
end

function addCorrectedOffsetToEntity(entity, axis)
    local name = entity._name
    local horizontal = axis == axisX
    local offset = { 0, 0 }
    if containsWord(name, "spike") then
        if (horizontal) then
            if (containsWord(name, "left")) then
                offset = { 6, 0 }
            elseif (containsWord(name, "right")) then
                offset = { 6, 0 }
            end
        else
            if (containsWord(name, "up")) then
                offset = { 0, 6 }
            elseif (containsWord(name, "down")) then
                offset = { 0, 6 }
            end
        end
    elseif containsWord(name, "spring") then
        if (horizontal) then
            if (containsWord(name, "left")) then
                offset = { 3, 0 }
            elseif (containsWord(name, "right")) then
                offset = { 3, 0 }
            elseif (containsWord(name, "down")) then
                offset = { 12, 0 }
            else
                -- 因为朝上的 spring 有可能写了 up, 也有可能什么都没写
                offset = { 12, 0 }
            end
        else
            if (containsWord(name, "left")) then
                offset = { 0, 12 }
            elseif (containsWord(name, "right")) then
                offset = { 0, 12 }
            elseif (containsWord(name, "down")) then
                offset = { 0, 3 }
            else
                offset = { 0, 3 }
            end
        end
    end

    entity.x = entity.x + offset[1]
    entity.y = entity.y + offset[2]
end
function containsWord(str, word)
    if str == nil then
        return false
    end
    return str:lower():find(word:lower(), 1, true) ~= nil
end

function invertLeftField(entity, axis)
    if axis == axisX and entity.left ~= nil and type(entity.left) == "boolean" then
        entity.left = not entity.left
    end
end

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

function invertDirection(entity, axis)
    local name = entity._name
    local horizontal = axis == axisX
    if containsWord(name, "moveblock") then
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

function swapDirectionInName(entity, axis)
    local name = entity._name
    if not name then
        return
    end

    if axis == axisX then
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
    end
    if axis == axisY then
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

