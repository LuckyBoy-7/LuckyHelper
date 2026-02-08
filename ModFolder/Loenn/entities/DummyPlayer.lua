local entity = {
    name = "LuckyHelper/DummyPlayer",

    placements = {
        name = "normal",
        data = {
            sendOriginalPlayerToTrigger = false,
            triggerDashFlag = "LuckyHelper_TriggerDashFlag"
        }
    },
    texture = "characters/player/sitDown00",
    justification = { 0.5, 1 },
    color = {0.3, 0.3, 0.3}
}

return entity
