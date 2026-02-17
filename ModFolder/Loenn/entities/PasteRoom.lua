local entity = {}

entity.name = "LuckyHelper/PasteRoom"

entity.placements = {
    name = "normal",
    data = {
        pastedFromRoom = "",
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
    "pastedFromRoom", "pasteOrder",
    "pasteEntity", "pasteTrigger",
    "pasteForegroundDecal", "pasteBackgroundDecal",
    "pasteForegroundTile", "pasteBackgroundTile",
}

entity.fieldInformation = {
    pasteOrder = {
        fieldType = "integer"
    },
    pastedFromRoom = {
        fieldType = "LuckyHelper.room_names"
    }
}

entity.texture = "LuckyHelper/room/copy_room"

return entity