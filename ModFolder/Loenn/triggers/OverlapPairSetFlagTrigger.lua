local trigger = {}

trigger.name = "LuckyHelper/OverlapPairSetFlagTrigger"
trigger.nodeLimits = { 0, 1 }  -- 初始额外节点数, 最大额外节点数, 每个entity初始有个节点为主节点(此时有两个node)
trigger.placements = {
    name = "normal",
    data = {
        main = true,
        triggerID = "1",
        flag = ""
    }
}

trigger.fieldInformation = {
}

return trigger
