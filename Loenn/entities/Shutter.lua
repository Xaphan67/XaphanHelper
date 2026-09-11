local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local Shutter = {}

Shutter.name = "XaphanHelper/Shutter"
Shutter.canResize = {false, false}
Shutter.fieldOrder = {
    "x", "y", "directory", "direction", "speed", "length", "flag", "sound", "startOpen", "silent"
}
Shutter.ignoredFields = {
    "_id", "_name", "width", "height"
}
Shutter.fieldInformation = {
    direction = {
        options = {"Bottom", "Top", "Left", "Right"},
        editable = false
    },
    length = {
        fieldType = "integer",
        minimumValue = 1
    },
    speed = {
        fieldType = "integer"
    }
}
Shutter.placements = {
    name = "Shutter",
    data = {
        direction = "Bottom",
        length = 4,
        speed = 30,
        directory = "objects/XaphanHelper/Shutter",
        flag = "",
        sound = "event:/game/xaphan/shutter",
        startOpen = false,
        silent = false
    }
}

function Shutter.selection(room, entity)
    local direction = entity.direction or "Bottom"
    local length = entity.length * 8 or 32
    if direction == "Left" or direction == "Right" then
        return utils.rectangle(entity.x or 0, entity.y or 0, math.max(length + 8 or 0, 8), 8)
    end

    return utils.rectangle(entity.x or 0, entity.y or 0, 8, math.max(length + 8 or 0, 8))

end

--[[function Shutter.onResize(room, entity, offsetX, offsetY, directionX, directionY)
    if directionX == 1 and entity.length >= 16 and entity.length <= 32 then
        entity.height = 8
        entity.length += offsetX
    end
    if directionX == 1 and offsetX > 0 and entity.length == 8 then
        entity.height = 8
        entity.length += offsetX
    end
    if directionX == 1 and offsetX < 0 and entity.length == 40 then
        entity.height = 8
        entity.length += offsetX
    end
    if directionY == 1 and entity.length >= 16 and entity.length <= 32 then
        entity.width = 8
        entity.length += offsetY
    end
    if directionY == 1 and offsetY > 0 and entity.length == 8 then
        entity.width = 8
        entity.length += offsetY
    end
    if directionY == 1 and offsetY < 0 and entity.length == 40 then
        entity.width = 8
        entity.length += offsetY
    end
    return true
end

function Shutter.canResize(room, entity)
    local direction = entity.direction or "Bottom"
    if direction == "Left" or direction == "Right" then
        return true, false
    end

    return false, true
end

function Shutter.minimumSize(room, entity)
    local direction = entity.direction or "Bottom"
    if direction == "Left" or direction == "Right" then
        return 16, 8
    end

    return 8, 16
end

function Shutter.maximumSize(room, entity)
    local direction = entity.direction or "Bottom"
    if direction == "Left" or direction == "Right" then
        return 48, 8
    end

    return 8, 48
end]]

function Shutter.sprite(room, entity)
    local directory = entity.directory or ""
    if directory == "" then
        directory = "objects/XaphanHelper/Shutter"
    end
    local direction = entity.direction or "Bottom"
    local length = entity.length or 32
    local sprites = {}

    local lightSprite = drawableSprite.fromTexture(directory .. "/top00", entity)
    if direction == "Bottom" then
        lightSprite:addPosition(4, 4)
    elseif direction == "Top" then
        lightSprite.rotation = -math.pi
        lightSprite:addPosition(4, 4 + length * 8)
    elseif direction == "Left" then
        lightSprite.rotation = math.pi / 2
        lightSprite:addPosition(4 + length * 8, 4)
    elseif direction == "Right" then
        lightSprite.rotation = -math.pi / 2
        lightSprite:addPosition(4, 4)
    end

    table.insert(sprites, lightSprite)

    for i = 1, entity.length do
        local gateSprite = drawableSprite.fromTexture(directory .. "/gate", entity)

        if direction == "Bottom" then
            gateSprite:addPosition(4, 4 + i * 8)
        elseif direction == "Top" then
            gateSprite.rotation = -math.pi
            gateSprite:addPosition(4, 4 + (i - 1) * 8)
        elseif direction == "Left" then
            gateSprite.rotation = math.pi / 2
            gateSprite:addPosition(4 + (i - 1) * 8, 4)
        elseif direction == "Right" then
            gateSprite.rotation = -math.pi / 2
            gateSprite:addPosition(4 + i * 8, 4)
        end
        

        table.insert(sprites, gateSprite)
    end

    return sprites
end

return Shutter