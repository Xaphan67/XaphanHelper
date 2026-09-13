local drawableSprite = require("structs.drawable_sprite")

local ArrowTrap = {}

ArrowTrap.name = "XaphanHelper/ArrowTrap"
ArrowTrap.depth = 8999
ArrowTrap.fieldOrder = {
    "x", "y", "side", "mode", "directory", "initialDelay", "flag", "cooldown", "onlyOnce"
}
ArrowTrap.fieldInformation = {
    side = {
        options = {"Left", "Right", "Top", "Bottom"},
        editable = false
    },
    mode = {
        options = {"Triggered", "Automatic"},
        editable = false
    },
    cooldown = {
        fieldType = "number",
        minimumValue = 0.3
    }
}
ArrowTrap.placements = {
    name = "ArrowTrap",
    data = {
        side = "Right",
        mode = "Triggered",
        initialDelay = 1,
        cooldown = 1,
        flag = "",
        onlyOnce = false,
        directory= "objects/XaphanHelper/ArrowTrap"
    }
}

function ArrowTrap.sprite(room, entity)
    local texture = entity.directory or "objects/XaphanHelper/ArrowTrap"
    local side = entity.side or "Right"

    local sprite = drawableSprite.fromTexture(texture .. "/idle00", entity)

    if side == "Right" then
    elseif side == "Left" then
        sprite.rotation = -math.pi
    elseif side == "Top" then
        sprite.rotation = -math.pi / 2
        sprite:addPosition(4, -4)
    elseif side == "Bottom" then
        sprite.rotation = math.pi / 2
        sprite:addPosition(4, -4)
    end
    sprite:addPosition(4, 8)

    return sprite
end

return ArrowTrap