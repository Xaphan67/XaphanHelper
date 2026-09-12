local drawableSprite = require("structs.drawable_sprite")

local AirBubbles = {}

AirBubbles.name = "XaphanHelper/AirBubbles"
AirBubbles.depth = 100
AirBubbles.fieldInformation = {}
AirBubbles.placements = {
    name = "AirBubbles",
    data = {}
}

function AirBubbles.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/XaphanHelper/AirBubbles/idle00", entity)
    sprite:addPosition(0, 0)

    return sprite
end

return AirBubbles