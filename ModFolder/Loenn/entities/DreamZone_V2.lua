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
        activeBackgroundColor = "000000",
        disabledBackgroundColor = "1f2e2d",
        activeLineColor = "000000",
        disabledLineColor = "6a8480",
        
        activeBackgroundAlpha = 0,
        disabledBackgroundAlpha = 0,
        activeLineAlpha = 0,
        disabledLineAlpha = 0,
        activeStarAlpha = 0.9,
        disabledStarAlpha = 0.9,
        disableWobble = false,
        disableInteraction = false,
        cancelDreamDashOnNotDreaming = false,
        disableVerticalJump = true,
        disableInsideDreamJump = false,
        getVerticalCoyote = false,
        starNumberPerUnit = 0.7,
        conserveSpeed = false,
        dashesToRefill = 1,
    }
}

dreamBlock.fieldOrder = {
    "x", "y",
    "width", "height",
    "activeBackgroundAlpha", "activeBackgroundColor",
    "disabledBackgroundAlpha", "disabledBackgroundColor",
    "activeLineAlpha", "activeLineColor",
    "disabledLineAlpha", "disabledLineColor",
    
    "activeStarAlpha", "smallStarColors",
    "disabledStarAlpha", "mediumStarColors",
    "starNumberPerUnit", "bigStarColors",
    "dashesToRefill", "",
    "", "",
    "", "",
}

dreamBlock.fieldInformation = {
    activeBackgroundColor = {
        fieldType = "color"
    },
    disabledBackgroundColor = {
        fieldType = "color"
    },
    activeLineColor = {
        fieldType = "color"
    },
    disabledLineColor = {
        fieldType = "color"
    },
    dashesToRefill = {
        fieldType = "integer"
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