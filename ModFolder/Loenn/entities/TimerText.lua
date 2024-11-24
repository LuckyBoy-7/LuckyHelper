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
        countPauseTime = false,
        isShowTotalTime = false,
        hiddenOnPause = false
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
    countPauseTime = {
        fieldType = "boolean"
    },
    isShowTotalTime = {
        fieldType = "boolean"
    },
    hiddenOnPause = {
        fieldType = "boolean"
    }
}

entity.texture = "LuckyHelper/timer"
return entity
