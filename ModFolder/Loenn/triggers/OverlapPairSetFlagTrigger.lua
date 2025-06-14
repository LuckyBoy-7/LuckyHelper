local trigger = {}

trigger.name = "LuckyHelper/OverlapPairSetFlagTrigger"
trigger.placements = {
    name = "normal",
    data = {
        main = true,
        id = "1",
        flag = ""
    }
}

trigger.fieldInformation = {
}

trigger.ignoredFields = function(trigger)
    if trigger.main == false then
        return { "flag" }
    end

    return {}
end


return trigger
