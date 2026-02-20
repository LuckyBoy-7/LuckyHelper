local entity = {}

entity.name = "LuckyHelper/CrackAdder"
entity.placements = {
    name = "normal",
    data = {
        crackLeftTexturePath = "decals/LuckyHelper/crack/left",
        crackMiddleTexturePath = "decals/LuckyHelper/crack/middle",
        sideCrackDensity = 0.5,
        middleCrackDensity = 0.01,
        hideFlag = "LuckyHelper_HideCrackFlag",
        targetAppliedTileIDs = "",
        color = "FFFFFFFF",
        depth = -10500,
    }
}

entity.fieldOrder = {
    "x", "y",
    "crackLeftTexturePath", "crackMiddleTexturePath",
    "sideCrackDensity", "middleCrackDensity",
    "hideFlag", "targetAppliedTileIDs",
    "color", "depth",
}

entity.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = true,
    },
    depth = {
        fieldType = "integer",
    }
}

entity.texture = "decals/LuckyHelper/crack/icon"
return entity
