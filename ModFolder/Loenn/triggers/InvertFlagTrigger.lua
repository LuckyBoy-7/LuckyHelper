local trigger = {}

trigger.name = "LuckyHelper/InvertFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        types = "Player",
        flag = "LuckyHelper_InvertFlag",
        invertOnEnter = true,
        invertOnLeave = false,
    }
}

return trigger