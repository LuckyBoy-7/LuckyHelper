local uiElements = require("ui.elements")
local languageRegistry = require("language_registry")
local utils = require("utils")
local widgetUtils = require("ui.widgets.utils")
local form = require("ui.forms.form")

local contextWindow = {}

local contextGroup
local activeWindows = {}
local windowPreviousX = 0
local windowPreviousY = 0

-- Remove values that would very want to be exposed for any type of selection item
local globallyFilteredKeys = {
    _type = true
}

local function getItemIgnoredFields(scriptHandler)
    local ignored = scriptHandler.ignoredFields and utils.callIfFunction(scriptHandler.ignoredFields) or {}
    local ignoredSet = {}

    for _, name in ipairs(ignored) do
        ignoredSet[name] = true
    end

    return ignoredSet
end

local function contextWindowUpdate(orig, self, dt)
    orig(self, dt)

    windowPreviousX = self.x
    windowPreviousY = self.y
end

local function getItemFieldOrder(scriptHandler)
    local fieldOrder = scriptHandler.fieldOrder and utils.callIfFunction(scriptHandler.fieldOrder) or {}

    return utils.deepcopy(fieldOrder)
end

local function getItemFieldInformation(scriptHandler)
    local fieldInformation = scriptHandler.fieldInformation and utils.callIfFunction(scriptHandler.fieldInformation) or {}

    return utils.deepcopy(fieldInformation)
end

local function getLanguageKey(key, language, default)
    if language[key]._exists then
        return tostring(language[key])
    end

    return default
end

function contextWindow.prepareFormData(scriptHandler, language)
    local dummyData = {}

    local fieldsAdded = {}
    local fieldInformation = getItemFieldInformation(scriptHandler)
    local fieldOrder = getItemFieldOrder(scriptHandler)
    local fieldIgnored = getItemIgnoredFields(scriptHandler)

    local parameters = scriptHandler.parameters

    for _, field in ipairs(fieldOrder) do
        local value = parameters[field]

        if value ~= nil then
            local humanizedName = utils.humanizeVariableName(field)
            local tooltip = scriptHandler.tooltips and scriptHandler.tooltips[field] or nil

            if not fieldInformation[field] then
                fieldInformation[field] = {}
            end

            fieldsAdded[field] = true
            dummyData[field] = utils.deepcopy(value)
            fieldInformation[field].displayName = humanizedName
            fieldInformation[field].tooltipText = tooltip
        end
    end

    for field, value in pairs(parameters) do
        -- Some fields should not be exposed automatically
        -- Any fields already added should not be added again
        if not globallyFilteredKeys[field] and not fieldsAdded[field] and not fieldIgnored[field] then
            local humanizedName = utils.humanizeVariableName(field)
            local tooltip = scriptHandler.tooltips and scriptHandler.tooltips[field] or nil

            table.insert(fieldOrder, field)

            if not fieldInformation[field] then
                fieldInformation[field] = {}
            end

            dummyData[field] = utils.deepcopy(value)
            fieldInformation[field].displayName = humanizedName
            fieldInformation[field].tooltipText = tooltip
        end
    end

    return dummyData, fieldInformation, fieldOrder
end

local function removeWindow(window)
    for i, w in ipairs(activeWindows) do
        if w == window then
            table.remove(activeWindows, i)
            widgetUtils.focusMainEditor()

            break
        end
    end

    window:removeSelf()
end

function contextWindow.createContextMenu(scriptHandler, callbackOnAccept, contextTable, callbackOnClose)
    local window
    local windowX = windowPreviousX
    local windowY = windowPreviousY
    local language = languageRegistry.getLanguage()

    -- Don't stack windows on top of each other
    if #activeWindows > 0 then
        windowX, windowY = 0, 0
    end

    local dummyData, fieldInformation, fieldOrder = contextWindow.prepareFormData(scriptHandler, language)
    local buttons = {
        {
            text = "Run Script", --tostring(language.ui.room_window.save_changes),
            formMustBeValid = true,
            callback = function(formFields)
                local formData = form.getFormData(formFields)
                callbackOnAccept(scriptHandler, formData, contextTable)

                -- make sure that parameter values persist between script usages
                for k,v in pairs(formData) do
                    scriptHandler.parameters[k] = v
                end
            end
        },
        {
            text = tostring(language.ui.room_window.close_window),
            callback = function(formFields)
                callbackOnClose()
                removeWindow(window)
            end
        }
    }

    local windowTitle = "Editing Script Parameters\n" .. scriptHandler.displayName --tostring(language.ui.selection_context_window.title)
    local selectionForm = form.getForm(buttons, dummyData, {
        fields = fieldInformation,
        fieldOrder = fieldOrder
    })

    window = uiElements.window(windowTitle, selectionForm):with({
        x = windowX,
        y = windowY,

        updateHidden = true
    }):hook({
        update = contextWindowUpdate
    })

    table.insert(activeWindows, window)

    --print(contextGroup) -- nil
    --print(contextGroup.parent)

    --contextGroup.parent:addChild(window)
    require("ui.windows").windows["scriptParameterWindow"].parent:addChild(window)

    form.prepareScrollableWindow(window)
    --widgetUtils.addWindowCloseButton(window) needs callback

    return window
end

-- Group to get access to the main group and sanely inject windows in it
function contextWindow.getWindow()
    contextGroup = uiElements.group({})
    return contextGroup
end

return contextWindow