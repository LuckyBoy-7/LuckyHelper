local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")
local triggerHandler = require("triggers")

local entity = {}

entity.name = "LuckyHelper/PasteItem"

entity.placements = {
    name = "normal",
    data = {
        pastedFromID = "exampleID",
        
        pasteEntity = true,
        pasteTrigger = true,
        
        pasteForegroundDecal = true,
        pasteBackgroundDecal = true,
    }
}

entity.fieldOrder = {
    "x", "y",
    "pastedFromID", 
    "pasteEntity", "pasteTrigger",
    "pasteForegroundDecal", "pasteBackgroundDecal",
}

entity.fieldInformation = {
    pasteOrder = {
        fieldType = "integer"
    }
}

function entity.sprite(room, entity)
    local sprites = {}

    local texture = drawableSprite.fromTexture("LuckyHelper/paste_item", entity)
    --leftSprite:addPosition(-124, 0)
    --leftSprite:addPosition(leftSprite.meta.width, 0)

    local width = 100
    local height = 10
    local x = (entity.x or 0) - width / 2 - 2
    local y = (entity.y or 0) - height / 2 + 0.5
    
    local text = drawableText.fromText(entity.pastedFromID, x, y, width, height, nil, triggerHandler.triggerFontSize, { 0, 1, 0, 1 })
    local shadowText = drawableText.fromText(entity.pastedFromID, x + 0.5, y + 0.5, width, height, nil, triggerHandler.triggerFontSize, { 0, 0, 0, 1 })

    table.insert(sprites, texture)
    table.insert(sprites, shadowText)
    table.insert(sprites, text)

    return sprites
end

return entity