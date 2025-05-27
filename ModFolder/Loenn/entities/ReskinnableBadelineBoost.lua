local badelineBoost = {}

badelineBoost.name = "LuckyHelper/ReskinnableBadelineBoost"
badelineBoost.depth = -1000000
badelineBoost.nodeLineRenderType = "line"
badelineBoost.texture = "objects/badelineboost/idle00"
badelineBoost.nodeLimits = {0, -1}
badelineBoost.placements = {
    name = "normal",
    data = {
        lockCamera = true,
        canSkip = false,
        finalCh9Boost = false,
        finalCh9GoldenBoost = false,
        finalCh9Dialog = false,
        XMLId = "badelineBoost",
        stretchTexturePath = "objects/badelineboost/stretch"
    }
}

return badelineBoost