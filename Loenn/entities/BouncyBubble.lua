local drawableSprite = require("structs.drawable_sprite")

local BouncyBubble = {}

BouncyBubble.name = "XaphanHelper/BouncyBubble"
BouncyBubble.depth = 0
BouncyBubble.placements = {
    name = "BouncyBubble",
    data = {
        directory = "objects/XaphanHelper/BouncyBubble",
        respawnTime = 0
    }
}

function BouncyBubble.sprite(room, entity)
    local directory = entity.directory or "objects/XaphanHelper/BouncyBubble"
    local sprite = drawableSprite.fromTexture(directory .. "/idle00", entity)
    sprite:addPosition(0, 0)

    return sprite
end

return BouncyBubble