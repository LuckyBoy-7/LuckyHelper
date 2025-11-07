-- copyed from JaThePlayer: https://github.com/JaThePlayer/LoennScripts/blob/58333c21befb86a2dc06925cfa7ba9c02755cc2c/Loenn/scripts/registerScript.lua
local utils = require("utils")
local scriptsLibrary = require("mods").requireFromPlugin("library.scriptsLibrary")
local notifications = require("ui.notification")
local script = {
    name = "registerScript",
    displayName = "_Register Script",
    parameters = {
        from = "",
        cloneFile = true,
        name = "",
    },
    fieldInformation = {
        from = {
            fieldType = "loennScripts.directFilepath",
            extension = "lua"
        }
    },
    tooltip = "Registers a new script from any .lua file.\nYou'll be able to use it later, even after reboots\nMake sure to understand what the script does before running it!",
    tooltips = {
        from = "The file from which the script will be loaded",
        cloneFile = "Whether the file's contents should be cloned to persistence.\nIf true, you don't need the original file on your computer for lönn to load the script\nOtherwise, the file needs to stay in that directory",
        name = "The name which the script will have in the script list",
    },
}

function script.prerun(args)
    local name = args.name
    if name == "" then
        name = scriptsLibrary.filename(args.from, true)
    end

    -- Ensure that this is a proper handler... by running it
    --[[
    local file = io.open(args.from)
    local l = file:read("*a")
    file:close()
    local f = assert(loadstring(l))
    local handler = f()

    if type(handler) ~= "table" then
        -- This wasn't a handler, we can't register this!
        notifications.notify("This doesn't seem to be a valid script handler!")
        return
    end
    ]]


    if args.cloneFile then
        local file = io.open(args.from)
        local l = file:read("*a")
        file:close()

        scriptsLibrary.registerCustomScriptFromString(l, name)
    else
        scriptsLibrary.registerCustomScriptFilepath(args.from, name)
    end

    script.scriptsTool.reset(true)
end

return script