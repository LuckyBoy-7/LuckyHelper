local controller = {}

controller.name = "LuckyHelper/LootSpeedrunController"
controller.placements = {
    {
        name = "normal",
        data = {
            totalTime = 100,
            timeReduceSpeedMultiplier = 5,
            teleportToRoomNameWhenTimeOver = "hub",
            exitWhenLootsAllCollected = false,
        }
    }
}
--controller.fieldOrder = {
--}
controller.fieldInformation = {
    exitWhenLootsAllCollected = {
        fieldType = "boolean"
    }
}


controller.texture = "LuckyHelper/loot_controller_icon"

return controller
