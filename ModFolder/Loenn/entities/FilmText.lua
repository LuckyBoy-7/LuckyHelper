function getEntity()
    local entity = {}

    entity.name = "LuckyHelper/FilmText"

    entity.placements = {
        name = "normal",
        data = {
            triggerMode = "OnRoomLoaded",
            triggerFlag = "FilmText_TriggeredFlag",
            positionMode = "World",
            dialogID = "dialog_ID",

            textScale = 1,
            textColor = "FFFFFF",
            showOutline = true,
            outlineThickness = 2,
            textOutlineColor = "000000",

            fadeInTime = 1,
            fadeInEase = "Linear",
            fadeOutTime = 1,
            fadeOutEase = "Linear",
            fadeOutDelayTime = 1,

            typeInTime = 0,
            typeInEase = "Linear",
            typeOutTime = 0,
            typeOutEase = "Linear",
            typeOutDelayTime = 0,
            playTypeAudio = true,

            slideInTime = 0,
            slideInEase = "Linear",
            slideInOffsetX = 0,
            slideInOffsetY = 0,
            slideOutTime = 0,
            slideOutEase = "Linear",
            slideOutOffsetX = 0,
            slideOutOffsetY = 0,
            globalSlideSpeedX = 0,
            globalSlideSpeedY = 0,
            slideOutDelayTime = 0,

            simulationSpeed = 1,

            placeholder = false

        }
    }

    entity.fieldOrder = {
        "x", "y",
        "triggerMode", "triggerFlag",
        "positionMode", "dialogID",

        "textScale", "textColor",
        "outlineThickness", "textOutlineColor",

        "fadeInTime", "fadeInEase",
        "fadeOutTime", "fadeOutEase",
        "fadeOutDelayTime", "placeholder",

        "typeInTime", "typeInEase",
        "typeOutTime", "typeOutEase",
        "typeOutDelayTime", "playTypeAudio",

        "slideInTime", "slideInEase",
        "slideInOffsetX", "slideInOffsetY",
        "slideOutTime", "slideOutEase",
        "slideOutOffsetX", "slideOutOffsetY",
        "slideOutDelayTime", "simulationSpeed",
        "globalSlideSpeedX", "globalSlideSpeedY",

        "showOutline", "",
    }

    local easeTypes = { "Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut" }

    entity.fieldInformation = {
        positionMode = {
            options = {
                "World",
                "Screen",
            },
            editable = false
        },
        triggerMode = {
            options = {
                "OnRoomLoaded",
                "OnFlag",
            },
            editable = false
        },
        textColor = {
            fieldType = "color"
        },
        textOutlineColor = {
            fieldType = "color"
        },
        fadeInEase = {
            options = easeTypes,
            editable = false
        },
        fadeOutEase = {
            options = easeTypes,
            editable = false
        },
        typeInEase = {
            options = easeTypes,
            editable = false
        },
        typeOutEase = {
            options = easeTypes,
            editable = false
        },
        slideInEase = {
            options = easeTypes,
            editable = false
        },
        slideOutEase = {
            options = easeTypes,
            editable = false
        }
    }

    entity.texture = "LuckyHelper/film_text"
    entity.justification = { 0.0, 0.0 }
    return entity
end

local rawEntity = getEntity()
rawEntity.name = "LuckyHelper/FilmTextRaw"

local bSideEntity = getEntity()

bSideEntity.name = "LuckyHelper/FilmTextBSide"
bSideEntity.placements.data.fadeOutDelayTime = 1.45
bSideEntity.placements.data.slideInTime = 1
bSideEntity.placements.data.slideInOffsetX = -32
bSideEntity.placements.data.slideInEase = "CubeOut"
bSideEntity.placements.data.slideOutTime = 1
bSideEntity.placements.data.slideOutOffsetX = 32
bSideEntity.placements.data.slideOutEase = "CubeIn"
bSideEntity.placements.data.slideOutDelayTime = 1.45
bSideEntity.placements.data.globalSlideSpeedX = 32

local typeInOutEntity = getEntity()

typeInOutEntity.name = "LuckyHelper/FilmTextTypeInOut"
typeInOutEntity.placements.data.typeInTime = 1
typeInOutEntity.placements.data.typeOutTime = 1
typeInOutEntity.placements.data.typeOutDelayTime = 1
typeInOutEntity.placements.data.fadeInTime = 0
typeInOutEntity.placements.data.fadeOutTime = 0
typeInOutEntity.placements.data.fadeOutDelayTime = 0

local typeInFadeOutEntity = getEntity()

typeInFadeOutEntity.name = "LuckyHelper/FilmTextTypeInFadeOut"
typeInFadeOutEntity.placements.data.typeInTime = 1
typeInFadeOutEntity.placements.data.fadeInTime = 0
typeInFadeOutEntity.placements.data.fadeOutTime = 2
typeInFadeOutEntity.placements.data.fadeOutDelayTime = 3


local entities = {
    rawEntity,
    bSideEntity,
    typeInOutEntity,
    typeInFadeOutEntity,
}

return entities