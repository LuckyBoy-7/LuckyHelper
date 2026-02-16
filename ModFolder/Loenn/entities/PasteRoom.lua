local entity = {}

entity.name = "LuckyHelper/PasteRoom"

entity.placements = {
    name = "normal",
    data = {
        pastedFromID = "exampleID",
        pasteOrder = 0,
        offsetX = 0,
        offsetY = 0,
        
        pasteEntity = true,
        pasteTrigger = true,
        
        pasteForegroundDecal = true,
        pasteBackgroundDecal = true,
        
        pasteForegroundTile = true,
        pasteBackgroundTile = true,
    }
}

entity.fieldOrder = {
    "x", "y",
    "offsetX", "offsetY",
    "pastedFromID", "pasteOrder",
    "pasteEntity", "pasteTrigger",
    "pasteForegroundDecal", "pasteBackgroundDecal",
    "pasteForegroundTile", "pasteBackgroundTile",
}

entity.fieldInformation = {
    pasteOrder = {
        fieldType = "integer"
    }
}

entity.texture = "LuckyHelper/room/paste_room"

return entity