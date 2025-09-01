local trigger = {}

trigger.name = "LuckyHelper/OrderedFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        types = "Player",
        entityTriggerMode = "OnEntityEnter",
        flags = "flag1, flag2, flag3, flag4",
        beforeSetFlagAction = "ClearOtherFlags",
        onFlagReachEndAction = "Cycle",
        startAtExistingFlag = true,
        updateCursorByCurrentFlags = true,
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
    },
    beforeSetFlagAction = {
        options = {
            "None",
            "ClearOtherFlags",
            "ClearPreviousFlag",
        },
        editable = false
    },
    onFlagReachEndAction = {
        options = {
            "Cycle",
            "Over",
            "PingPong",
            "Random",
        },
        editable = false
    },
    flags = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    },
    types = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    }
}

return trigger