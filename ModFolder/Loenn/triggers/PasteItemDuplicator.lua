local trigger = {}

trigger.name = "LuckyHelper/PasteItemDuplicator"

trigger.placements = {
    name = "normal",
    data = {
        startOffsetX = 0,
        startOffsetY = 0,
        placementOffsetX = 0,
        placementOffsetY = 0,
        pasteNumber = 3,
        duplicateFlag = "LuckyHelper_DuplicateItemFlag",
        duplicateOnEnterRoom = true,
        showDuplicatedOrderNumber = false,
    }
}

trigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "startOffsetX", "startOffsetY",
    "placementOffsetX", "placementOffsetY",
    "pasteNumber", "duplicateFlag",
    "duplicateOnEnterRoom", "showDuplicatedOrderNumber",
}

trigger.fieldInformation = {
    pasteNumber = {
        fieldType = "integer",
    }
}


return trigger