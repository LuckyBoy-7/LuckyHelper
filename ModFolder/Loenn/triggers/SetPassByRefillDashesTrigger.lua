local trigger = {}

trigger.name = "LuckyHelper/SetPassByRefillDashesTrigger"
trigger.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,
        dashes = 2
    }
}

trigger.fieldInformation = 
{
    dashes = {
        fieldType = "integer"
    }
}

return trigger