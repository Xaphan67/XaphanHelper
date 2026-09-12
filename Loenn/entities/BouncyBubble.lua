local drawableSprite = require("structs.drawable_sprite")

local BouncyBubble = {}

BouncyBubble.name = "XaphanHelper/BouncyBubble"
BouncyBubble.depth = 0
BouncyBubble.placements = {
    name = "BouncyBubble",
    data = {
        respawnTime = 0
    }
}

function BouncyBubble.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/XaphanHelper/BouncyBubble/idle00", entity)
    sprite:addPosition(0, 0)

    return sprite
end

return BouncyBubble