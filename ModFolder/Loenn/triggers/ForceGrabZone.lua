local trigger = {}

trigger.name = "LuckyHelper/ForceGrabZone"
trigger.placements = {
    name = "normal",
    data = {
        disableFlag = "LuckyHelper_DisableForceGrabZone",
    }
}

trigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "disableFlag",
}


return trigger
