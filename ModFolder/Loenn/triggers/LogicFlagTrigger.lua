local trigger = {}

trigger.name = "LuckyHelper/LogicFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        types = "Player",
        entityTriggerMode = "OnEntityEnter",
        flag = "LogicFlagTrigger_OutputFlag",
        conditionFlagExpression = "!flag1 && (flag2 || flag3)",
        debug = false
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