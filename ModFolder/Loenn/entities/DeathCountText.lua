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
        
        isShowTotalDeath = false,
        hiddenOnPause = false
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
    isShowTotalDeath = {
        fieldType = "boolean"
    },
    hiddenOnPause = {
        fieldType = "boolean"
    }
}

entity.texture = "LuckyHelper/skeleton"
return entity
