local trigger = {}

trigger.name = "LuckyHelper/AudioPlay"
trigger.placements = {
    name = "normal",
    data = {
        audioEvent = "",
        playOnEnterRoom = false
    }
}

trigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "audioEvent", "playOnEnterRoom",
}


return trigger
