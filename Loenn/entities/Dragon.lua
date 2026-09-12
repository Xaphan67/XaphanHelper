local drawableSprite = require("structs.drawable_sprite")

local Dragon = {}

Dragon.name = "XaphanHelper/Dragon"
Dragon.depth = 0
Dragon.fieldOrder = {
    "x", "y", "shootDirection", "shootStrength", "initialDelay", "fireballTimer", "fireballs", "idleTimer"
}
Dragon.fieldInformation = {
    shootDirection = {
        options = {"Left", "Right", "Both"},
        editable = false
    },
    shootStrength = {
        minimumValue = 0.5,
        maximumValue = 1.5
    },
    fireballTimer = {
        minimumValue = 0.4
    },
    fireballs = {
        fieldType = "integer",
        minimumValue = 1
    },
    idleTimer = {
        minimumValue = 0
    }
}
Dragon.placements = {
    name = "Dragon",
    data = {
        shootDirection = "Both",
        shootStrength = 1,
        initialDelay = 0,
        fireballTimer = 0.7,
        fireballs = 3,
        idleTimer = 2
    }
}

function Dragon.sprite(room, entity)
    local sprites = {}

    local head = drawableSprite.fromTexture("enemies/Xaphan/Dragon/head00", entity)
    local body = drawableSprite.fromTexture("enemies/Xaphan/Dragon/body00", entity)
    head:addPosition(9, 6)
    body:addPosition(9, 26)

    table.insert(sprites, head)
    table.insert(sprites, body)

    return sprites
end

return Dragon