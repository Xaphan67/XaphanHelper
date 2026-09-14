local utils = require("utils")

local CustomRefill = {}

CustomRefill.name = "XaphanHelper/CustomRefill"
CustomRefill.depth = -100
CustomRefill.fieldInformation = {
    type = {
        options = {"One Dash", "Two Dashes", "Max Jumps", "Missiles", "Super Missiles", "Oxygen", "Oxygen Small"},
        editable = false
    },
    airSections = {
        fieldType = "integer",
        minimumValue = 1
    }
}
CustomRefill.fieldOrder = {
    "x", "y", "directory", "type", "respawnTime", "oneUse"
}
CustomRefill.placements = {
    name = "CustomRefill",
    data = {
        directory = "objects/XaphanHelper/CustomRefill",
        type = "One Dash",
        oneUse = false,
        respawnTime = 2.5
    }
}

function CustomRefill.texture(room, entity)
    local directory = entity.directory or "objects/XaphanHelper/CustomRefill"
    local type = entity.type or "Max Dashes"
    local oneUse = entity.oneUse or false

    if type == "Oxygen" then
        oneUse = true
    end

    if oneUse then
        if type == "Missiles" then
            return directory .. "/refillMissileOnce/idle00"
        elseif type == "Super Missiles" then
            return directory .. "/refillSMissileOnce/idle00"
        elseif type == "Two Dashes" then
            return directory .. "/refillTwoOnce/idle00"
        elseif type == "Max Jumps" then
            return directory .. "/refillJumpsOnce/idle00"
        elseif type == "Oxygen" then
            return directory .. "/refillOxygen/idle00"
        elseif type == "Oxygen Small" then
            return directory .. "/refillOxygenSmall/idle00"
        else
            return directory .. "/refillOnce/idle00"
        end
    else
        if type == "Missiles" then
            return directory .. "/refillMissile/idle00"
        elseif type == "Super Missiles" then
            return directory .. "/refillSMissile/idle00"
        elseif type == "Two Dashes" then
            return directory .. "/refillTwo/idle00"
        elseif type == "Max Jumps" then
            return directory .. "/refillJumps/idle00"
        elseif type == "Oxygen" then
            return directory .. "/refillOxygen/idle00"
        elseif type == "Oxygen Small" then
            return directory .. "/refillOxygenSmall/idle00"
        else
            return directory .. "/refill/idle00"
        end
    end
end

function CustomRefill.selection(room, entity)
    local type = entity.type or "Max Dashes"
    if type == "Two Dashes" then
        return utils.rectangle(entity.x - 4, entity.y - 6, 8, 12)
    else
        return utils.rectangle(entity.x - 5, entity.y - 5, 10, 10)
    end

end

return CustomRefill