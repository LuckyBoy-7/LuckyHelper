local entity = {
    name = "LuckyHelper/Ball",

    placements = {
        name = "normal",
        data = {
            holdable = true,
            noDuplicateSelf = true,
            noDuplicateOthers = false,
            slowFall = false,
            slowRun = true,
            destroyable = true,
            tutorial = false,
            respawn = false,
            waitForGrab = false,

            bounceSpeedMultiplierX = -0.4,
            bounceSpeedMultiplierY = -0.4,
            decelerationMultiplierX = 0.2,
            decelerationMultiplierY = 1,
            decelerationOnIceMultiplierX = 0,
            rotateAnimationSpeed = 1,

            colliderSize = 16,
            spritePath = "LuckyHelper/objects/ball/ball",
            hitSideSound = "event:/game/05_mirror_temple/crystaltheo_hit_side",
            hitGroundSound = "event:/game/05_mirror_temple/crystaltheo_hit_ground",
        }
    },
    fieldInformation = {
        colliderSize = {
            fieldType = "integer",
        },
    },
    fieldOrder = {
        "x", "y",

        "bounceSpeedMultiplierX", "bounceSpeedMultiplierY",
        "decelerationMultiplierX", "decelerationOnIceMultiplierX",
        "decelerationMultiplierY", "spritePath",
        "colliderSize", "rotateAnimationSpeed",
        "hitSideSound", "hitGroundSound",

        "holdable", "waitForGrab",
        "noDuplicateSelf", "noDuplicateOthers",
        "slowFall", "slowRun",
        "destroyable", "respawn",
        "tutorial", "disableRotation",
    },
    texture = "LuckyHelper/objects/ball/ball",
    justification = { 0.5, 1 }
}

return entity
