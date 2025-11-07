local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")

local dropdownField = {}

dropdownField.fieldType = "loennScripts.dropdown"

dropdownField._MT = {}
dropdownField._MT.__index = {}

function dropdownField._MT.__index:setValue(value)
    self.currentValue = value
end

function dropdownField._MT.__index:getValue()
    return self.currentValue
end

function dropdownField._MT.__index:fieldValid()
    return true
end

local function dropdownChanged(self)
    return function(element, new)
        self.currentValue = new
    end
end

local function indexOf(tbl, item, defaultValue)
    for i,v in ipairs(tbl) do
        if v == item then return i end
    end
    return defaultValue or -1
end

function dropdownField.getElement(name, value, options)
    local formField = {}

    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160

    local label = uiElements.label(options.displayName or name)
    local dropdown = uiElements.dropdown(options.options or {""}, dropdownChanged(formField)):with({
        minWidth = minWidth,
        maxWidth = maxWidth
    })

    local index = (options.options and value) and indexOf(options.options, value, 1) or 1
    dropdown:setSelectedIndex(index)

    if options.tooltipText then
        label.interactive = 1
        label.tooltipText = options.tooltipText
    end

    label.centerVertically = true

    formField.label = label
    formField.dropdown = dropdown
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.width = 2
    formField.elements = {
        label, dropdown
    }

    return setmetatable(formField, dropdownField._MT)
end

return dropdownField