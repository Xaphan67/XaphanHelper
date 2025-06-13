local drawableSprite = require("structs.drawable_sprite")

local DroneSwitch = {}

DroneSwitch.name = "XaphanHelper/DroneSwitch"
DroneSwitch.depth = 0
DroneSwitch.fieldOrder = {
    "x", "y", "side", "type", "flag", "registerInSaveData", "saveDataOnlyAfterCheckpoint", "onlyOnce", "tutorial"
}
DroneSwitch.canResize = {false, false}
DroneSwitch.fieldInformation = {
    side = {
        options = {"Left", "Right", "Down"},
        editable = false
    },
    type = {
        options = {"Beam", "Missile", "SuperMissile"},
        editable = false
    }
}
DroneSwitch.placements = {
    name = "DroneSwitch",
    data = {
        flag = "",
        side = "Left",
        onlyOnce = false,
        registerInSaveData = false,
        saveDataOnlyAfterCheckpoint = false,
        type = "Beam",
        tutorial = false
    }
}

function DroneSwitch.sprite(room, entity)
    local side = entity.side or "Left"
    local switchType = entity.type or "Beam"
    local sprite = nil
    if switchType == "Beam" then
        sprite = drawableSprite.fromTexture("objects/XaphanHelper/DroneSwitch/button00", entity)
    else
        sprite = drawableSprite.fromTexture("objects/XaphanHelper/DroneSwitch/button" .. switchType .. "00", entity)
    end
    sprite:addPosition(4, 4)

    if side == "Down" then
        sprite.rotation = -math.pi
    elseif side == "Left" then
        sprite.rotation = math.pi / 2
    elseif side == "Right" then
        sprite.rotation = -math.pi / 2
    end

    return sprite
end

return DroneSwitch