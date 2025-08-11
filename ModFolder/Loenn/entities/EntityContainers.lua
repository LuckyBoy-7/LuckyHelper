local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local depths = require("consts.object_depths")
local drawing = require("utils.drawing")
local utils = require("utils")

local containerFill = { 1.0, 0.6, 0.6, 0.4 }
local containerBorder = { 1.0, 0.6, 0.6, 1 }

local modifierFill = { 0.6, 1.0, 0.6, 0.4 }
local modifierBorder = { 0.6, 1.0, 0.6, 1 }

local followerContainer = {
    name = "LuckyHelper/FollowerContainer",
    fillColor = containerFill,
    borderColor = containerBorder,

    placements = {
        name = "normal",
        data = {
            width = 8,
            height = 8,
            whitelist = "",
            containMode = "RoomStart",
            containFlag = "",
            fitContained = false,
            ignoreAnchors = false,
            forceStandardBehavior = false,
            ignoreContainerBounds = false,
        }
    },
}

local containers = {
    followerContainer
}

local containModes = { "RoomStart", "FlagChanged", "Always", "DelayedRoomStart" }
local directions = { "Left", "Right" }
local easeTypes = { "Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut" }

-- stolen from Eevee Helper(乐
-- https://github.com/CommunalHelper/EeveeHelper/blob/d63385cdf6f1b0fa0bdf7e7a1ebca3b704f8ae2f/Loenn/entities/entityContainers.lua#L513
local depths = {
    { "BG Terrain (10000)", 10000 },
    { "BG Mirrors (9500)", 9500 },
    { "BG Decals (9000)", 9000 },
    { "BG Particles (8000)", 8000 },
    { "Solids Below (5000)", 5000 },
    { "Below (2000)", 2000 },
    { "NPCs (1000)", 1000 },
    { "Theo Crystal (100)", 100 },
    { "Player (0)", 0 },
    { "Dust (-50)", -50 },
    { "Pickups (-100)", -100 },
    { "Seeker (-200)", -200 },
    { "Particles (-8000)", -8000 },
    { "Above (-8500)", -8500 },
    { "Solids (-9000)", -9000 },
    { "FG Terrain (-10000)", -10000 },
    { "FG Decals (-10500)", -10500 },
    { "Dream Blocks (-11000)", -11000 },
    { "Crystal Spinners (-11500)", -11500 },
    { "Player Dreamdashing (-12000)", -12000 },
    { "Enemy (-12500)", -12500 },
    { "Fake Walls (-13000)", -13000 },
    { "FG Particles (-50000)", -50000 },
    { "Top (-1000000)", -1000000 },
    { "Formation Sequences (-2000000)", -2000000 },
}

local sharedFieldInformation = {
    containMode = {
        options = containModes,
        editable = false
    },
    attachMode = {
        options = containModes,
        editable = false
    },
    easing = {
        options = easeTypes,
        editable = false
    },
    direction = {
        options = directions,
        editable = false
    },
    depth = {
        fieldType = "integer",
        options = depths
    },
    inactiveColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    },
}

for _, container in ipairs(containers) do
    container.fieldInformation = container.fieldInformation or {}
    for k, v in pairs(sharedFieldInformation) do
        -- only add shared field information if it doesn't already exist
        if container.fieldInformation[k] == nil then
            container.fieldInformation[k] = v
        end
    end
    container.depth = math.huge -- make containers render below everything
end

return containers