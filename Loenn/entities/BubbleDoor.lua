local drawableSprite = require("structs.drawable_sprite")

local BubbleDoor = {}

BubbleDoor.name = "XaphanHelper/BubbleDoor"
BubbleDoor.depth = -9000
BubbleDoor.fieldOrder = {
    "x", "y", "directory", "side", "color", "flags", "forceLockedFlag", "forceInactiveFlag", "openSound", "closeSound", "lockSound", "unlockSound", "onlyNeedOneFlag"
}
BubbleDoor.canResize = {false, false}
BubbleDoor.fieldInformation = {
    side = {
        options = {"Left", "Right", "Top", "Bottom"},
        editable = false
    },
    color = {
        options = {"Blue", "Red", "Green", "Yellow", "Grey"},
        editable = false
    }
}
BubbleDoor.placements = {
    name = "BubbleDoor",
    data = {
        side = "Left",
        directory = "objects/XaphanHelper/BubbleDoor",
        color = "Blue",
        flags = "",
        forceLockedFlag = "",
        forceInactiveFlag = "",
        openSound = "",
        closeSound = "",
        unlockSound = "",
        lockSound = "",
        onlyNeedOneFlag = false
    }
}

function BubbleDoor.sprite(room, entity)
    local side = entity.side or "Left"
    local color = entity.color or "Blue"
    local directory = entity.directory or "objects/XaphanHelper/BubbleDoor"

    local sprites = {}

    local sprite = drawableSprite.fromTexture(directory .. "/" .. color .. "/closed00", entity)
    local spriteStruct = drawableSprite.fromTexture(directory .. "/struct00", entity)

    if side == "Top" then
        sprite.rotation = math.pi / 2
        spriteStruct.rotation = math.pi / 2
        sprite:addPosition(20, 3)
        spriteStruct:addPosition(20, -4)
        sprite.scaleY = -1
        spriteStruct.scaleY = -1
    elseif side == "Bottom" then
        sprite.rotation = -math.pi / 2
        spriteStruct.rotation = -math.pi / 2
        sprite:addPosition(20, 5)
        spriteStruct:addPosition(20, 12)
    elseif side == "Left" then
        sprite.rotation = 0
        spriteStruct.rotation = 0
        sprite:addPosition(3, 20)
        spriteStruct:addPosition(-4, 20)
    elseif side == "Right" then
        sprite:addPosition(5, 20)
        spriteStruct:addPosition(12, 20)
        sprite.scaleX = -1
        spriteStruct.scaleX = -1
    end

    sprite:setColor("White")
    spriteStruct:setColor("White")

    if sprite then
        table.insert(sprites, sprite)
    end
    if spriteStruct then
        table.insert(sprites, spriteStruct)
    end

    return sprites
end

return BubbleDoor