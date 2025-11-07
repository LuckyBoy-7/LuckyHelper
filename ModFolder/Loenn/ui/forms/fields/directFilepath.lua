local uiElements = require("ui.elements")
local filesystem = require("utils.filesystem")
local fileLocations = require("file_locations")
local utils = require("utils")

local directFilepathField = {}

directFilepathField.fieldType = "loennScripts.directFilepath"

directFilepathField._MT = {}
directFilepathField._MT.__index = {}

function directFilepathField._MT.__index:setValue(value)
    self.currentValue = value
end

function directFilepathField._MT.__index:getValue()
    return self.currentValue
end

function directFilepathField._MT.__index:fieldValid()
    local value = self:getValue()
    return type(value) == "string" and filesystem.isFile(value)
end

local function updateButtonLabel(button, newFilepath)
    if newFilepath then
        button.label.text = utils.filename(utils.stripExtension(newFilepath):gsub("\\", "/"), "/")
        button.label.interactive = 1
        button.label.tooltipText = newFilepath
    end
end

local function createSelectFileCallback(self, button)
    return function(filepath)
        updateButtonLabel(button, filepath)
        self.currentValue = filepath
        self:notifyFieldChanged()
    end
end

local function buttonPressed(self, extension)
    return function(button)
        filesystem.openDialog(fileLocations.getCelesteDir(), extension, createSelectFileCallback(self, button))
    end
end

function directFilepathField.getElement(name, value, options)
    local formField = {}

    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160

    local label = uiElements.label(options.displayName or name)
    local button = uiElements.button("", buttonPressed(formField, options.extension or "bin")):with({
        minWidth = minWidth,
        maxWidth = maxWidth
    })

    if value ~= "" then
        updateButtonLabel(button, value)
    end

    if options.tooltipText then
        label.interactive = 1
        label.tooltipText = options.tooltipText
    end

    label.centerVertically = true

    formField.label = label
    formField.button = button
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.width = 2
    formField.elements = {
        label, button
    }

    return setmetatable(formField, directFilepathField._MT)
end

return directFilepathField