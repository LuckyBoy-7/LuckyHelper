local trigger = {}

trigger.name = "LuckyHelper/LogicFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        types = "Player",
        flagTriggerMode = "OnEntityEnter",
        conditionFlagExpression = "!LuckyHelper_Flag1 && LuckyHelperFlag2 || LuckyHelperFlag3",
        flag = "LuckyHelper_OutputFlag",
        debug = false
    }
}

trigger.fieldInformation = {
    flagTriggerMode = {
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