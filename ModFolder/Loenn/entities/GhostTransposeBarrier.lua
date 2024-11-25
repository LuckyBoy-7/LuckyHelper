local entity = {}

entity.name = "LuckyHelper/GhostTransposeBarrier"
entity.placements = {
    name = "normal",
    data = {
        width = 8,
        height = 8,
        backgroundColor = "ff65b4",
        particleColor = "6675ff",
    }
}

entity.fieldInformation = 
{
    backgroundColor = {
        fieldType = "color"
    },
    particleColor = {
        fieldType = "color"
    }
}
entity.borderColor = {37 / 175, 37 / 255, 37 / 255, 1}
entity.fillColor = {37 / 175, 37 / 255, 37 / 255, 139 / 255}
return entity
