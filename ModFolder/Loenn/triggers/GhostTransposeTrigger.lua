--local enums = require("consts.celeste_enums")
local trigger = {}

trigger.name = "LuckyHelper/GhostTransposeTrigger"
trigger.placements = {
    name = "normal",
    data = {
        --width = 16,
        --height = 16,
        ghostOutOfBoundsAction = "TreatAsSolid",
        transposeDirType = "TwoSides",
        enableGhostTranspose = true;
        ghostSpeed = 500;
        useDashKey = true;
        color = "FFFFFF",
        alpha = 0.5,
        maxGhostNumber = 1,
    }
}

trigger.fieldInformation = {
    ghostOutOfBoundsAction = {
        options = {
            "None",
            "TreatAsSolid",
            "KillPlayer",
        },
        editable = false
    },
    transposeDirType = {
        options = {
            "TwoSides",
            "EightSides"
        },
        editable = false
    },
    enableGhostTranspose = {
        fieldType = "boolean"
    },
    useDashKey = {
        fieldType = "boolean"
    },
    color = {
        fieldType = "color"
    },
    maxGhostNumber = {
        fieldType = "integer"
    }
}

return trigger