local dreamBlock = {}

dreamBlock.name = "LuckyHelper/DreamZone"
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
        starColors1 = "FFEF11,FF00D0,08a310",
        starColors2 = "5fcde4,7fb25e,E0564C",
        starColors3 = "5b6ee1,CC3B3B,7daa64",
        backgroundColor = "000000",
        outlineColor = "FFFFFF",
        backgroundAlpha = 1,
        outlineAlpha = 1,
        disableWobble = false,
        disableInteraction = false
    }
}

dreamBlock.fieldInformation = {
    backgroundColor = {
        fieldType = "color"
    },
    outlineColor = {
        fieldType = "color"
    },
    startColors1 = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
    startColors2 = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
    startColors3 = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
    starColors1 = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
    starColors2 = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    },
    starColors3 = {
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