-- copyed from JathePlayer: https://github.com/JaThePlayer/LoennScripts/blob/main/Loenn/library/scriptsLibrary.lua

local mods = require("mods")
local config = require("utils.config")
local utils = require("utils")
local viewportHandler = require("viewport_handler")

local scriptsLibrary = {}

function scriptsLibrary.getModPersistence()
    local settings = mods.getModPersistence()

    -- setup default values
    settings.customScripts = settings.customScripts or { }

    return settings
end

function scriptsLibrary.getCustomScripts()
    return scriptsLibrary.getModPersistence().customScripts
end

function scriptsLibrary.savePersistence()
    config.writeConfig(scriptsLibrary.getModPersistence(), true)
end

local function addScript(name, value)
    local settings = scriptsLibrary.getModPersistence()
    settings.customScripts[name] = value
    scriptsLibrary.savePersistence()
end

function scriptsLibrary.registerCustomScriptFilepath(filepath, scriptName)
    print("Registered custom script from filepath", scriptName, filepath)
    addScript(scriptName, filepath)
end

function scriptsLibrary.registerCustomScriptFromString(str, scriptName)
    print("Registered custom script from string", scriptName, str)
    addScript(scriptName, str)
end

function scriptsLibrary.filename(filepath, humanize)
    local name = utils.filename(utils.stripExtension(filepath):gsub("\\", "/"), "/")
    return humanize and utils.humanizeVariableName(name) or name
end

---A wrapper over viewportHandler.getRoomCoordinates that uses the mispelled func if on an old version of lonn
---@param room table
---@param x number|nil
---@param y number|nil
---@return number roomX, number roomY
function scriptsLibrary.safeGetRoomCoordinates(room, x, y)
    local f = viewportHandler.getRoomCoordinates or viewportHandler.getRoomCoordindates
    return f(room, x, y)
end

return scriptsLibrary