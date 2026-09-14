local drawableSprite = require("structs.drawable_sprite")

local CustomFollower = {}

CustomFollower.name = "XaphanHelper/CustomFollower"
CustomFollower.depth = 0
CustomFollower.fieldInformation = {
    type = {
        options = {"Energy Tank", "Fire Rate Module", "Missile", "Super Missile"},
        editable = false
    }
}
CustomFollower.placements = {
    name = "CustomFollower",
    data = {
        directory = "collectables/XaphanHelper/CustomFollower",
        type = "Energy Tank"
    }
}

function CustomFollower.sprite(room, entity)
    local directory = entity.directory or "collectables/XaphanHelper/CustomFollower"
    local type = entity.type:sub(1, 1):upper()..entity.type:sub(2):gsub("%s+", "")

    local sprite = drawableSprite.fromTexture( directory .. "/" .. type .. "/idle00", entity)

    return sprite
end

return CustomFollower