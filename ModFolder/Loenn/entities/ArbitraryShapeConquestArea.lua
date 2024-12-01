local arbitraryShapeEntity = require("mods").requireFromPlugin("libraries.arbitraryShapeEntity")

local conquestArea = {
    name = "LuckyHelper/ArbitraryShapeConquestArea",
    nodeLimits = { 2, 999 },
    depth = -math.huge + 6
}

conquestArea.placements = {
    name = "normal",
    data = {
        totalAmount = 100,
        conquestSpeed = 10,
        blueCamp = "Player",
        redCamp = "Seeker, BadelineOldsite, Strawberry",
        flagOnCompleted = "ConquestTestFlag",
        ignoreCollidable = true,
        ignoreCollidableWhiteList = "BadelineOldsite",
        useSpriteShapeAsFallback = true,

        polygonFillColor = "00FFFF",
        polygonOutlineColor = "FFFFFF",
        polygonAlpha = 0.3,
        polygonOutlineWith = 3,

        conquestAreaName = "along the river",
        conquestAreaId = "A1",
        idFontSize = 0.7,
        idFontColor = "FFFFFF",
        placeFontSize = 0.4,
        placeFontColor = "FFFFFF",
        UITotalWidth = 1000,
        UITotalHeight = 40,
        UIOutlineWidth = 8,
        UIFillColor = "008888",
        UIBackgroundColor = "888888",
        UIOutlineColor = "000000",
    }
}

conquestArea.fieldInformation = {
    ignoreCollidable = {
        fieldType = "boolean"
    },
    useSpriteShapeAsFallback = {
        fieldType = "boolean"
    },
    polygonFillColor = {
        fieldType = "color"
    },
    polygonOutlineColor = {
        fieldType = "color"
    },
    idFontColor = {
        fieldType = "color"
    },
    placeFontColor = {
        fieldType = "color"
    },
    UIFillColor = {
        fieldType = "color"
    },
    UIBackgroundColor = {
        fieldType = "color"
    },
    UIOutlineColor = {
        fieldType = "color"
    }
}

conquestArea.sprite = arbitraryShapeEntity.getSpriteFunc("ffffff", "ffffff", "ffffff19")
conquestArea.nodeSprite = arbitraryShapeEntity.nodeSprite
conquestArea.selection = arbitraryShapeEntity.selection

return conquestArea