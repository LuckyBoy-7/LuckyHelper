local trigger = {}

trigger.name = "LuckyHelper/CopyItem"
trigger.nodeLimits = { 1, -1 }

trigger.placements = {
    name = "normal",
    data = {
        copiedToID = "exampleID",
    }
}

trigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "copydFromID", 
}

trigger.fieldInformation = {

}

function trigger.triggerText(room, trigger)
    return string.format("Copy Item\n(%s)", trigger.copiedToID)
end

return trigger