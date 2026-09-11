local drawableSprite = require("structs.drawable_sprite")

local Crate = {}

Crate.name = "XaphanHelper/Crate"
Crate.depth = 100
Crate.fieldOrder = {
    "x", "y", "type"
}
Crate.fieldInformation = {
    type = {
        options = {"Wood", "Metal"},
        editable = false
    }
}
Crate.placements = {
    name = "Crate",
    data = {
        type = "Wood",
        noSpawnFlag = ""
    }
}

function Crate.sprite(room, entity)
    local type = entity.type or "Wood"
    local sprite = drawableSprite.fromTexture("objects/XaphanHelper/Crate/" .. type .. "00", entity)
    sprite:addPosition(0, -8)

    return sprite
end

return Crate