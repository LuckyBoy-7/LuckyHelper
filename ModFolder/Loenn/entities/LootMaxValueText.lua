local entity = {}

entity.name = "LuckyHelper/LootMaxValueText"
entity.placements = {
    name = "normal",
    data = {
        scale = 1.25,
        alpha = 1,
        color = "FFFFFF",
        outline = false,
        thickness = 2,
        outlineColor = "000000",
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
    hiddenOnPause = {
        fieldType = "boolean"
    }
}

entity.texture = "LuckyHelper/loot_icon"
return entity
