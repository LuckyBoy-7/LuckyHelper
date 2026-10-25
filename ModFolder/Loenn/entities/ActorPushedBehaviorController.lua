local entity = {}

entity.name = "LuckyHelper/ActorPushedBehaviorController"
entity.placements = {
    {
        name = "normal",
        data = {
            affectedActors = "Player",
            allowPushed = true,
            pushedOut = true,
            onlyActiveInSolid = true,
            activeFlag = "",
        }
    }
}

entity.fieldInformation = {
    affectedActors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    }
}

entity.texture = "LuckyHelper/player_pushed"
return entity
