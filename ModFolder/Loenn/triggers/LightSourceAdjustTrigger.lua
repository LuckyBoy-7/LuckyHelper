local trigger = {}

trigger.name = "LuckyHelper/LightSourceAdjustTrigger"
trigger.placements = {
    name = "normal",
    data = {
        offsetFrom = 0,
        offsetTo = 1,
        positionMode = "NoEffect",
        targets = "Player, TheoCrystal, PlayerDeadBody",
        affectRadius = true,
        affectAlpha = true,
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
