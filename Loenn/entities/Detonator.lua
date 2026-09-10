local drawableSprite = require("structs.drawable_sprite")

local Detonator = {}

Detonator.name = "XaphanHelper/Detonator"

Detonator.fieldOrder = {
    "x", "y", "directory", "flag", "side", "speed", "registerInSaveData"
}
Detonator.fieldInformation = {
    side = {
        options = {"Up", "Down", "Left", "Right"},
        editable = false
    },
    speed = {
        minimumValue = 0.01,
        maximumValue = 0.3
    }
}
Detonator.ignoredFields = {
    "_id", "_name", "width", "height"
}
Detonator.placements = {
    name = "Detonator",
    data = {
        directory = "objects/XaphanHelper/Detonator",
        side = "Up",
        speed = 0.1,
        flag = "",
        registerInSaveData = false
    }
}

function Detonator.sprite(room, entity)
    local directory = entity.directory or ""
    local side = entity.side or "Right"

    local sprite = drawableSprite.fromTexture(directory .. "/idle00", entity)

    if side == "Up" then
    elseif side == "Down" then
        sprite.rotation = -math.pi
    elseif side == "Left" then
        sprite.rotation = -math.pi / 2
    elseif side == "Right" then
        sprite.rotation = math.pi / 2
    end
    sprite:addPosition(0, 0)

    return sprite
end

return Detonator