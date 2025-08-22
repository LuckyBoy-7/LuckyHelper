local trigger = {}

trigger.name = "LuckyHelper/AudioAdjustTrigger"
trigger.placements = {
    name = "normal",
    data = {
        offsetFrom = 0,
        offsetTo = 1,
        positionMode = "LeftToRight",
        targets = "",
        affectVolume = true,
    }
}

trigger.fieldInformation = {
    positionMode =
    {
        options =
        {
            "NoEffect",
            "LeftToRight",
            "RightToLeft",
            "TopToBottom",
            "BottomToTop",
            "HorizontalCenter",
            "VerticalCenter"
        },
        editable = false
    }
}



return trigger
