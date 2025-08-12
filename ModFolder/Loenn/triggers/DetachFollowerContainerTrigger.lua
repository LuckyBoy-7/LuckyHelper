local trigger = {}

trigger.name = "LuckyHelper/DetachFollowerContainerTrigger"
trigger.fillColor = {0.4, 0.4, 1.0, 0.4}
trigger.borderColor = {0.4, 0.4, 1.0, 1.0}
trigger.nodeLimits = {1, 1}
trigger.placements = {
    name = "normal",
    data = {
        width = 8,
        height = 8
    }
}

return trigger

