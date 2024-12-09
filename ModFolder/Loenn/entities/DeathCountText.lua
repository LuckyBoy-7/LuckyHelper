local entity = {}

entity.name = "LuckyHelper/DeathCountText"
entity.placements = {
    name = "normal",
    data = {
        scale = 1.25,
        alpha = 1,
        color = "FFFFFF",
        outline = false,
        thickness = 2,
        outlineColor = "000000",

        showType = "CurrentRoom",
        hiddenOnPause = false,
        savedPath = "where the death count will be added to"
    }
}

entity.fieldInformation = 
{
    color = {
        fieldType = "color"
    },
    outline = {
        fieldType = "boolean"
    },
    outlineColor = {
        fieldType = "color"
    },
    showType = {
        options = {
            "CurrentRoom",
            "TotalDeathFromStart",
            "ReadFromSavedPath"
        },
        editable = false
    },
    hiddenOnPause = {
        fieldType = "boolean"
    }
}

entity.texture = "LuckyHelper/skeleton"
return entity
