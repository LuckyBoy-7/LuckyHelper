local entity = {}

entity.name = "LuckyHelper/TimerText"
entity.placements = {
    name = "normal",
    data = {
        scale = 1.25,
        alpha = 1,
        color = "FFFFFF",
        outline = false,
        thickness = 2,
        outlineColor = "000000",
        
        format = "mm\\:ss\\:ff",
        hiddenOnPause = false,
        remainCurrentValue = false,
        showType = "CurrentRoom",
        savedPath = "where the time will be added to"
    }
}

entity.fieldInformation = 
{
    color = {
        fieldType = "color"
    },
    format = {
        fieldType = "string"
    },
    outline = {
        fieldType = "boolean"
    },
    outlineColor = {
        fieldType = "color"
    },
    hiddenOnPause = {
        fieldType = "boolean"
    },
    remainCurrentValue = {
        fieldType = "boolean"
    },
    showType = {
        options = {
            "CurrentRoom",
            "SinceStart",
            "FromSavedPath",
            "SinceLastCheckpoint"
        },
        editable = false
    }
}

entity.texture = "LuckyHelper/timer"
return entity
