--internal, do not use in other mods

local tools = require("tools")

local selectionDrawUtils = {}

function selectionDrawUtils.drawSelections()
    tools.tools["selection"].draw()
end

return selectionDrawUtils