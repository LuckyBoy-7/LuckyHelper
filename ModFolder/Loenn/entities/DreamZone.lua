local dreamBlock = {}

dreamBlock.name = "LuckyHelper/DreamZone"
dreamBlock.fillColor = {0.0, 0.0, 0.0}
dreamBlock.borderColor = {1.0, 1.0, 1.0}
dreamBlock.nodeLineRenderType = "line"
dreamBlock.nodeLimits = {0, 1}
dreamBlock.placements = {
    name = "normal",
    data = {
        fastMoving = false,
        below = true,
        oneUse = false,
        width = 8,
        height = 8,
        stopPlayerOnCollide = true,
        killPlayerOnCollide = true,
    }
}

dreamBlock.fieldInformation =
{
    --color = {
    --    fieldType = "color"
    --}
}

function dreamBlock.depth(room, entity)
    return entity.below and 5000 or -11000
end

return dreamBlock