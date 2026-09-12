local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local Skultera = {}

Skultera.name = "XaphanHelper/Skultera"
Skultera.depth = 1
Skultera.fieldInformation = {
    direction = {
        options = {"Left", "Right", "Up", "Down"},
        editable = false
    },
    maxRange = {
        fieldType = "integer"
    },
    suctionStrength = {
        minimumValue = 1
    }
}
Skultera.placements = {
    name = "Skultera",
    data = {
        direction = "Left",
        maxRange = 14,
        suctionStrength = 150
    }
}

function Skultera.selection(room, entity)
    local direction = entity.direction or "Left"
    if direction == "Left" then
        return utils.rectangle(entity.x - 3, entity.y - 11, 11, 22)
    elseif direction == "Right" then
        return utils.rectangle(entity.x, entity.y - 11, 11, 22)
    elseif direction == "Down" then
        return utils.rectangle(entity.x - 11, entity.y, 22, 11)
    elseif direction == "Up" then
        return utils.rectangle(entity.x - 11, entity.y - 3, 22, 11)
    end
end

function Skultera.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("enemies/Xaphan/Skultera/idle00", entity)
    local direction = entity.direction or "Left"

    if direction == "Left" then
        sprite:addPosition(2, 4)
    elseif direction == "Right" then
        sprite:setScale({-1, 1})
        sprite:addPosition(6, 4)
    elseif direction == "Down" then
        sprite.rotation = -math.pi / 2
        sprite:addPosition(4, 6)
    elseif direction == "Up" then
        sprite.rotation = -math.pi / 2
        sprite:setScale({-1, 1})
        sprite:addPosition(4, 2)
    end

    return sprite
end

return Skultera