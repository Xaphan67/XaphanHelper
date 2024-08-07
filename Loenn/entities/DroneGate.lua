local drawableSprite = require("structs.drawable_sprite")

local DroneGate = {}

DroneGate.name = "XaphanHelper/DroneGate"
DroneGate.depth = -10000
DroneGate.canResize = {false, false}
DroneGate.fieldInformation = {
    side = {
        options = {"Left", "Right", "Top", "Bottom"},
        editable = false
    }
}
DroneGate.placements = {
    name = "DroneGate",
    data = {
        flag = "",
        side = "Left",
        directory = "objects/XaphanHelper/DroneGate",
        forceInactiveFlag = ""
    }
}

function DroneGate.sprite(room, entity)
    local side = entity.side or "Left"
    local directory = entity.directory or "objects/XaphanHelper/DroneGate"

    local sprites = {}

    local spriteRed = nil
    local sprite = drawableSprite.fromTexture(directory .. "/closed00", entity)

    if side == "Top" then
        sprite.rotation = 0
        spriteRed = drawableSprite.fromTexture(directory .. "/redT00", entity)
    elseif side == "Bottom" then
        sprite.rotation = math.pi
        spriteRed = drawableSprite.fromTexture(directory .. "/redB00", entity)
    elseif side == "Left" then
        sprite.rotation = -math.pi / 2
        spriteRed = drawableSprite.fromTexture(directory .. "/redL00", entity)
    elseif side == "Right" then
        sprite.rotation = math.pi / 2
        spriteRed = drawableSprite.fromTexture(directory .. "/redR00", entity)
    end

    if sprite then
        sprite:addPosition(4, 4)
        table.insert(sprites, sprite)
    end
    if spriteRed then
        spriteRed:addPosition(4, 4)
        table.insert(sprites, spriteRed)
    end

    return sprites
end

return DroneGate