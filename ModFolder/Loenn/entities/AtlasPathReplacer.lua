local entity = {
    name = "LuckyHelper/AtlasPathReplacer",

    placements = {
        name = "normal",
        data = {
            from = "objects/refill/outline,objects/refill/idle,objects/refill/flash",
            to = "LuckyHelper/objects/refill/outline,LuckyHelper/objects/refill/idle,LuckyHelper/objects/refill/flash",
            atlasType = "Game",
            rooms = "*"
        }
    },
    fieldInformation = {
        from = {
            fieldType = "list",
            elementOptions = {
                fieldType = "string",
            }
        },
        to = {
            fieldType = "list",
            elementOptions = {
                fieldType = "string",
            }
        },
        rooms = {
            fieldType = "list",
            elementOptions = {
                fieldType = "string",
            }
        },
        atlasType = {
            options = {
                "Game",
                "Gui",
                "Opening",
                "Misc",
                "Portraits",
                "ColorGrades",
                "Others"
            },
            editable = false
        }
    },
    fieldOrder = {
        "x", "y",
        "from",
        "to",
        "atlasType",
        "rooms"
    },
    texture = "LuckyHelper/objects/atlas_path_replacer",
    justification = { 0.5, 0.5 }
}

return entity
