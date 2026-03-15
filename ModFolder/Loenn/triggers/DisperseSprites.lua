local trigger = {}

trigger.name = "LuckyHelper/DisperseSpritesTrigger"
trigger.placements = {
    name = "normal",
    data = {
        whiteList = "",
        blackList = "Player,SolidTiles",
        disperseFlag = "",
        disperseAudioEvent = "event:/new_content/char/granny/dissipate",
        disperseDirX = 1,
        disperseDirY = -0.1,
        depth = 0,
        delay = -1,
        
        disableEntityUpdate = true,
        disableEntityVisible = true,
        disableEntityCollision = true,
        fadeOutLight = true,
        fadeOutSound = true,
        fadeOutTalk = true,
        dontLoadAfterFade = false,
    }
}

trigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "whiteList", "blackList",
    "disperseDirX", "disperseDirY",
    "disperseFlag", "disperseAudioEvent",
    "depth", "delay",
    "disableEntityUpdate", "disableEntityVisible",
    "disableEntityCollision", "",
    "fadeOutLight", "fadeOutSound",
    "fadeOutTalk", "dontLoadAfterFade",
}

trigger.fieldInformation = {
    depth = {
        fieldType = "integer"
    },
    whiteList = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    },
    blackList = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    }
}


return trigger
