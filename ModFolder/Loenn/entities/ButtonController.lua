local entity = {}

entity.name = "LuckyHelper/MenuButtonController"
entity.placements = {
    name = "normal",
    data = {
        buttonNames = "menu_pause_resume,menu_pause_retry",
        disabledIfFlags = "LuckyHelper_menu_pause_resume_disable_flag,LuckyHelper_menu_pause_retry_disable_flag",
    }
}

entity.fieldInformation = 
{
    buttonNames = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    },
    disabledIfFlags = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        }
    }
}

entity.texture = "LuckyHelper/textmenu"

return entity
