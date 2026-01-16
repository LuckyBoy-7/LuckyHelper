local entity = {}

entity.name = "LuckyHelper/StrawberryJamJar"
entity.placements = {
    name = "normal",
    data = {
        map = "Celeste/1-ForsakenCity",
        returnToLobbyMode = "SetReturnToHere",
        allowSaving = true,
        spriteId = "LuckyHelper_jamJar_trailer",
        soundPath = "event:/LuckyHelper/StrawberryJam/TestJamJarFilled",
        loennImagePath = "LuckyHelper/trailer/jarfull_idle00",
        debugMode = false,
    }
}

entity.fieldInformation = {
    returnToLobbyMode = {
        options = { "SetReturnToHere", "RemoveReturn", "DoNotChangeReturn" },
        editable = false
    }
}

entity.fieldOrder = {
    "x", "y",
    "loennImagePath", "map",
    "spriteId", "soundPath",
    "returnToLobbyMode", "allowSaving",
    "debugMode", "",
}

entity.justification = {0.5, 1}

function entity.texture(room, entity)
    return entity.loennImagePath
end

return entity
