local dreamBlock = {}

dreamBlock.name = "LuckyHelper/DreamZone_V2"
dreamBlock.fillColor = { 0.0, 0.0, 0.0 }
dreamBlock.borderColor = { 1.0, 1.0, 1.0 }
dreamBlock.nodeLineRenderType = "line"
dreamBlock.nodeLimits = { 0, 1 }
dreamBlock.placements = {
    name = "normal",
    data = {
        fastMoving = false,
        below = true,
        oneUse = false,
        width = 8,
        height = 8,
        stopPlayerOnCollide = true,
        killPlayerOnCollide = false,
        bigStarColors = "FFEF11,FF00D0,08a310",
        mediumStarColors = "5fcde4,7fb25e,E0564C",
        smallStarColors = "5b6ee1,CC3B3B,7daa64",
        backgroundColor = "000000",
        outlineColor = "FFFFFF",
        backgroundAlpha = 0,
        outlineAlpha = 0,
        starAlpha = 0.9,
        disableWobble = false,
        disableInteraction = false,
        cancelDreamDashOnNotDreaming = false,
        disableVerticalJump = true,
        starNumberPerUnit = 0.7
    }
}

dreamBlock.fieldOrder = {
    "x", "y",
    "width", "height",
    "backgroundAlpha", "backgroundColor",
    "outlineAlpha", "outlineColor",
    "starAlpha", "bigStarColors",
    "mediumStarColors", "smallStarColors",
    "starNumberPerUnit",
    "", "",
    "", "",
    "", "",
}

dreamBlock.fieldInformation = {
    backgroundColor = {
        fieldType = "color"
    },
    outlineColor = {
        fieldType = "color"
    },
    bigStarColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
    mediumStarColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
    smallStarColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
}

function dreamBlock.depth(room, entity)
    return entity.below and 5000 or -11000
end

return dreamBlock