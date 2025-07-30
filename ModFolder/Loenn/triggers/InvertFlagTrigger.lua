local trigger = {}

trigger.name = "LuckyHelper/InvertFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        types = "Player",
        entityTriggerMode = "OnEntityEnter",
        flag = "InvertFlagTrigger_OutputFlag"
    }
}

trigger.fieldInformation = {
    entityTriggerMode = {
        options = {
            "OnEntityEnter",
            "OnEntityStay",
            "OnEntityLeave",
            "Always",
        },
        editable = false
    }
}

return trigger