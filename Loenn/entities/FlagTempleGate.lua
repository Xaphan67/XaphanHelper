local mods = require("mods")
local customTempleGate = mods.requireFromPlugin("entities.customTempleGate")

local legacy = {}
for key, value in pairs(customTempleGate) do
    legacy[key] = value
end

legacy.name = "XaphanHelper/FlagTempleGate"
legacy.placements = nil

return legacy