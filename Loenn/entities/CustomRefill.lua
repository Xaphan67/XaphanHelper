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
        directory = "",
        type = "One Dash",
        oneUse = false,
        respawnTime = 2.5
    }
}

function CustomRefill.texture(room, entity)
    local directory = entity.directory or ""
    local type = entity.type or "One Dash"
    local oneUse = entity.oneUse or false

    if type == "Oxygen" then
        oneUse = true
    end

    if directory == "" then
        if oneUse then
            if type == "Missiles" then
                return "objects/XaphanHelper/CustomRefill/refillMissileOnce/idle00"
            elseif type == "Super Missiles" then
                return "objects/XaphanHelper/CustomRefill/refillSMissileOnce/idle00"
            elseif type == "Two Dashes" then
                return "objects/XaphanHelper/CustomRefill/refillTwoOnce/idle00"
            elseif type == "Max Jumps" then
                return "objects/XaphanHelper/CustomRefill/refillJumpsOnce/idle00"
            elseif type == "Oxygen" then
                return "objects/XaphanHelper/CustomRefill/refillOxygen/idle00"
            elseif type == "Oxygen Small" then
                return "objects/XaphanHelper/CustomRefill/refillOxygenSmall/idle00"
            else
                return "objects/XaphanHelper/CustomRefill/refillOnce/idle00"
            end
        else
            if type == "Missiles" then
                return "objects/XaphanHelper/CustomRefill/refillMissile/idle00"
            elseif type == "Super Missiles" then
                return "objects/XaphanHelper/CustomRefill/refillSMissile/idle00"
            elseif type == "Two Dashes" then
                return "objects/XaphanHelper/CustomRefill/refillTwo/idle00"
            elseif type == "Max Jumps" then
                return "objects/XaphanHelper/CustomRefill/refillJumps/idle00"
            elseif type == "Oxygen" then
                return "objects/XaphanHelper/CustomRefill/refillOxygen/idle00"
            elseif type == "Oxygen Small" then
                return "objects/XaphanHelper/CustomRefill/refillOxygenSmall/idle00"
            else
                return "objects/XaphanHelper/CustomRefill/refill/idle00"
            end
        end
    else
        return directory .. "/idle00"
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