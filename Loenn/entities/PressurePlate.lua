local drawableSprite = require("structs.drawable_sprite")

local PressurePlate = {}

PressurePlate.name = "XaphanHelper/PressurePlate"

PressurePlate.fieldOrder = {
    "x", "y", "directory", "flag"
}
PressurePlate.ignoredFields = {
    "_id", "_name", "width", "height"
}
PressurePlate.placements = {
    name = "PressurePlate",
    data = {
        directory = "objects/XaphanHelper/PressurePlate",
        flag = ""
    }
}

function PressurePlate.sprite(room, entity)
    local directory = entity.directory or ""
    local buttonSprite = drawableSprite.fromTexture(directory .. "/button00", entity)
    buttonSprite:addPosition(8, 4)

    return buttonSprite
end

return PressurePlate