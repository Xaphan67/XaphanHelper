local drawableSprite = require("structs.drawable_sprite")

local ArrowHole = {}

ArrowHole.name = "XaphanHelper/ArrowHole"
ArrowHole.depth = -15000
ArrowHole.fieldInformation = {
    side = {
        options = {"Left", "Right", "Top", "Bottom"},
        editable = false
    }
}
ArrowHole.placements = {
    name = "ArrowHole",
    data = {
        side = "Left",
        directory = "objects/XaphanHelper/ArrowHole"
    }
}

function ArrowHole.sprite(room, entity)
    local texture = entity.directory or "objects/XaphanHelper/ArrowHole"
    local side = entity.side or "Right"

    local sprite = drawableSprite.fromTexture(texture .. "/hole00", entity)

    if side == "Right" then
        sprite.rotation = math.pi / 2
    elseif side == "Left" then
        sprite.rotation = -math.pi / 2
    elseif side == "Top" then
        sprite.rotation = math.pi
        sprite:addPosition(4, -4)
    elseif side == "Bottom" then
        sprite:addPosition(4, -4)
    end
    sprite:addPosition(4, 8)

    return sprite
end

return ArrowHole