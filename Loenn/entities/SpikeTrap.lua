local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local SpikeTrap = {}

SpikeTrap.name = "XaphanHelper/SpikeTrap"
SpikeTrap.depth = -10001
SpikeTrap.fieldOrder = {
    "x", "y", "sprite", "direction", "triggerTime", "retract"
}
SpikeTrap.ignoredFields = {
    "_id", "_name", "width", "height"
}
SpikeTrap.fieldInformation = {
    direction = {
        fieldType = "integer",
        options = {["Up"] = 0, ["Down"] = 1, ["Left"] = 2, ["Right"] = 3},
        editable = false
    },
    length = {
        fieldType = "integer"
    }
}
SpikeTrap.placements = {
    name = "SpikeTrap",
    data = {
        width = 8,
        height = 8,
        sprite = "danger/XaphanHelper/SpikeTrap",
        direction = 0,
        retract = false,
        triggerTime = 1.5
    }
}

function SpikeTrap.onResize(room, entity, offsetX, offsetY, directionX, directionY)
    local direction = entity.direction or 0
    if direction <= 1 then
        entity.height = 8
    else
        entity.width = 8
    end

    return true
end

function SpikeTrap.canResize(room, entity)
    local direction = entity.direction or 0
    if direction <= 1 then
        return true, false
    end

    return false, true
end

function SpikeTrap.selection(room, entity)
    local width = entity.width or 8
    local height = entity.height or 8
    local direction = entity.direction or 0
    if direction == 0 then
        return utils.rectangle(entity.x or 0, entity.y + 2 or 0 , width or 0, 8)
    elseif direction == 1 then
        return utils.rectangle(entity.x or 0, entity.y - 2 or 0 , width or 0, 8)
    elseif direction == 2 then
        return utils.rectangle(entity.x + 2 or 0, entity.y or 0, 8, height or 0)
    else
        return utils.rectangle(entity.x - 2 or 0, entity.y or 0, 8, height or 0)
    end
end

function SpikeTrap.sprite(room, entity)
    local width = entity.width or 8
    local height = entity.height or 8
    local direction = entity.direction or 0

    local sprite = entity.sprite
    if sprite == "" then
        sprite = "danger/XaphanHelper/SpikeTrap"
    end

    local sprites = {}
    if direction <= 1 then
        for i = 1, width / 8 do
            local trapSprite = drawableSprite.fromTexture(sprite .. "/trap00", entity)

            if direction == 0 then
                trapSprite:addPosition(4 + (i - 1) * 8, 2)
            elseif direction == 1 then
                trapSprite.rotation = -math.pi
                trapSprite:addPosition(-4 + i * 8, 6)
            end

            table.insert(sprites, trapSprite)
        end
    else
        for i = 1, height / 8 do
            local trapSprite = drawableSprite.fromTexture(sprite .. "/trap00", entity)

            if direction == 2 then
                trapSprite.rotation = -math.pi / 2
                trapSprite:addPosition(2, 3 + (i - 1) * 8)
            elseif direction == 3 then
                trapSprite.rotation = math.pi / 2
                trapSprite:addPosition(6, -4 + i * 8)
            end

            table.insert(sprites, trapSprite)
        end
    end

    return sprites
end

return SpikeTrap