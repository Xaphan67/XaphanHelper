local drawableSprite = require("structs.drawable_sprite")

local Powamp = {}

Powamp.name = "XaphanHelper/Powamp"
Powamp.depth = 0
Powamp.fieldInformation = {
    grownDelay = {
        minimumValue = 0
    },
    grownTime = {
        minimumValue = 0
    }
}
Powamp.placements = {
    name = "Powamp",
    data = {
        grownDelay = 0.5,
        grownTime = 0
    }
}

function Powamp.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("enemies/Xaphan/Powamp/idle00", entity)
    sprite:addPosition(0, 0)

    return sprite
end

return Powamp