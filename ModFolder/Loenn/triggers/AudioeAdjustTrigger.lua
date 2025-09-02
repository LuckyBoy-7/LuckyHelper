local trigger = {}

trigger.name = "LuckyHelper/AudioAdjustTrigger"
trigger.placements = {
    name = "normal",
    data = {
        offsetFrom = 0,
        offsetTo = 1,
        positionMode = "LeftToRight",
        targets = "",
        replacedTargets = "",
        affectVolume = true,
        affectPitch = false,
        replaceAudio = false,
    }
}

trigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "offsetFrom", "offsetTo",
    "targets", "replacedTargets",
    "", "",
}

trigger.fieldInformation = {
    positionMode = {
        options = {
            "NoEffect",
            "LeftToRight",
            "RightToLeft",
            "TopToBottom",
            "BottomToTop",
            "HorizontalCenter",
            "VerticalCenter"
        },
        editable = false
    },
    targets = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    },
    replacedTargets = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    }
}

return trigger
