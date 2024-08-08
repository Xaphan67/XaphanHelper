local drawableSprite = require("structs.drawable_sprite")

local CellLock = {}

CellLock.name = "XaphanHelper/CellLock"
CellLock.depth = 8999
CellLock.fieldOrder = {
    "x", "y", "sprite", "type", "cellOffsetX", "cellOffsetY", "color", "flag", "sound", "slotSound", "instant", "cellInside", "keepCell", "registerInSaveData", "onlyCellVisible"
}
CellLock.fieldInformation = {
    type = {
        options = {"Normal", "Floating"},
        editable = false
    },
    color = {
        options = {"Blue", "Red", "Green", "Yellow", "Grey"},
        editable = false
    },
    cellOffsetX = {
        fieldType = "integer"
    },
    cellOffsetY = {
        fieldType = "integer"
    }
}
CellLock.placements = {
    name = "CellLock",
    data = {
        sprite = "objects/XaphanHelper/CellLock",
        color = "Blue",
        flag = "",
        registerInSaveData = false,
        sound = "",
        cellInside = false,
        keepCell = false,
        slotSound = "event:/game/05_mirror_temple/button_activate",
        instant = false,
        type = "Normal",
        onlyCellVisible = false,
        cellOffsetX = 0,
        cellOffsetY = 0
    }
}

function CellLock.sprite(room, entity)
    local color = entity.color or "Blue"
    local cellInside = entity.cellInside
    local sprite = entity.Sprite or "objects/XaphanHelper/CellLock"
    local type = entity.type or "Normal"

    color = string.lower(color)
    type = string.lower(type)

    local sprites = {}

    local typeSprite = drawableSprite.fromTexture(sprite .. "/" .. type .. "00", entity)
    local colorSprite = drawableSprite.fromTexture(sprite .. "/" .. color .. "00", entity)
    local cellSprite = nil
    local leverSprite = nil

    if cellInside then
        cellSprite = drawableSprite.fromTexture(sprite .. "/bgCell00", entity)
        leverSprite = drawableSprite.fromTexture(sprite .. "/lever00", entity)
    end

    if typeSprite then
        typeSprite:setColor("White")
        table.insert(sprites, typeSprite)
    end
    if colorSprite then
        colorSprite:setColor("White")
        table.insert(sprites, colorSprite)
    end
    if cellSprite then
        cellSprite:addPosition(0, 5)
        cellSprite:setColor("White")
        table.insert(sprites, cellSprite)
    end
    if leverSprite then
        leverSprite:addPosition(0, 5)
        leverSprite:setColor("White")
        table.insert(sprites, leverSprite)
    end
  
    return sprites
end

return CellLock