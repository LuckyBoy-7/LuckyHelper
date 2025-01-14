local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local gondola = {}

gondola.name = "LuckyHelper/CustomGondola"
gondola.depth = -10500
gondola.nodeVisibility = "always"  -- 主节点的显示状态
-- entity.nodes里存的是除主节点外的所有节点
gondola.nodeLimits = { 1, 1 }  -- 初始额外节点数, 最大额外节点数, 每个entity初始有个节点为主节点(此时有两个node)
gondola.placements = {
    {
        name = "normal",
        placementType = "line",
        data = {
            canInteractOnMove = true,
            addCeiling = true,
            startPositionOffsetX = 100,
            endPositionOffsetX = -100,
            rotationSpeed = 0.7,
            accelerationDuration = 0.5,
            moveDuration = 2,
            frontTexturePath = "objects/gondola/front",
            leverTexturePath = "objects/gondola/lever",
            backTexturePath = "objects/gondola/back",
            cliffsideLeftTexturePath = "objects/gondola/cliffsideLeft",
            cliffsideRightTexturePath = "objects/gondola/cliffsideRight",
            topTexturePath = "objects/gondola/top",
            
            startPositionType = "CloseToPlayer",
            positionFlag = "GondolaPositionOnStartFlag",

            moveToStartFlag = "moveToStartFlag",
            moveToEndFlag = "moveToEndFlag",
            smoothFlagMove = true,
            disableInteractOnFlagMove = false,
        }
    }
}
gondola.fieldOrder = {
    "x", "y", 
    "frontTexturePath", "leverTexturePath", 
    "backTexturePath", "topTexturePath", 
    "cliffsideLeftTexturePath", "cliffsideRightTexturePath",
    "startPositionOffsetX", "endPositionOffsetX", 
    "rotationSpeed", "accelerationDuration", 
    "moveDuration", "addCeiling", "canInteractOnMove", 
    "startPositionType", "positionFlag",
    "moveToStartFlag", "moveToEndFlag",
    "smoothFlagMove", "disableInteractOnFlagMove",
}
gondola.fieldInformation = {
    autoAlignToStartPosition = {
        fieldType = "boolean"
    },
    canInteractOnMove = {
        fieldType = "boolean"
    },
    addCeiling = {
        fieldType = "boolean"
    },
    startPositionOffsetX = {
        fieldType = "integer"
    },
    endPositionOffsetX = {
        fieldType = "integer"
    },
    autoAlignToGoundPositionOfPlayer = {
        fieldType = "boolean"
    },
    startPositionType = {
        options = {
            "Start",
            "End",
            "CloseToPlayer",
            "ByFlag"
        },
        editable = false
    }
}

local leftTexture = "objects/gondola/cliffsideLeft"
local rightTexture = "objects/gondola/cliffsideRight"

local wireColor = { 0, 0, 0, 1 }
local wireThickness = 1

local function getLeftSprite(room, entity)
    local leftSprite = drawableSprite.fromTexture(leftTexture, entity)
    --leftSprite:addPosition(-124, 0)
    --leftSprite:addPosition(leftSprite.meta.width, 0)
    leftSprite:setJustification(0.0, 1.0)
    leftSprite.depth = 8998

    return leftSprite
end

local function getRightSprite(room, entity)
    local nodes = entity.nodes or { { x = 0, y = 0 } }
    local node = nodes[1]
    local rightSprite = drawableSprite.fromTexture(rightTexture, node)
    --rightSprite:addPosition(144, -104)  -- 初始放置时的偏移
    rightSprite:setJustification(0.0, 0.5)
    rightSprite:setScale(-1, 1)  -- 左右翻转
    rightSprite.depth = 8998

    return rightSprite
end

function gondola.sprite(room, entity)
    local sprites = {}

    local leftSprite = getLeftSprite(room, entity)
    local rightSprite = getRightSprite(room, entity)

    local wireLeftX = leftSprite.x + 40
    local wireLeftY = leftSprite.y - 12
    local wireRightX = rightSprite.x - 40
    local wireRightY = rightSprite.y - 4

    local wire = drawableLine.fromPoints({ wireLeftX, wireLeftY, wireRightX, wireRightY }, wireColor, wireThickness)

    wire.depth = 8999

    table.insert(sprites, wire:getDrawableSprite()[1])  -- 第二个返回的是类似纹理一样的东西
    table.insert(sprites, leftSprite)

    return sprites
end

-- Define custom main entity rectangle otherwise the cable etc. is automatically considered part of it
function gondola.selection(room, entity)
    return getLeftSprite(room, entity):getRectangle(), { getRightSprite(room, entity):getRectangle() }

end

function gondola.nodeSprite(room, entity, node)
    return getRightSprite(room, entity)
end

return gondola
