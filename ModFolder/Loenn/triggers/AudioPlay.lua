local trigger = {}

trigger.name = "LuckyHelper/AudioPlay"
trigger.placements = {
    name = "normal",
    data = {
        audioEvent = "event:/new_content/game/10_farewell/quake_onset",
        playOnEnterRoom = false,
        onlyOnce = false
    }
}

trigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "audioEvent", "playOnEnterRoom",
    "onlyOnce", "",
}


return trigger
