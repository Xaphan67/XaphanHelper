local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local LaserEmitter = {}

LaserEmitter.name = "XaphanHelper/LaserEmitter"
LaserEmitter.depth = -2000
LaserEmitter.canResize = {false, false}
LaserEmitter.fieldInformation = {
    side = {
        options = {"Left", "Right", "Top", "Bottom"},
        editable = false
    },
    type = {
        options = {"Kill", "Must Dash", "No Dash"},
        editable = false
    }
}
LaserEmitter.placements = {
    name = "LaserEmitter",
    data = {
        flag = "",
        forceInactiveFlag = "",
        side = "Right",
        type = "Kill",
        directory = "objects/XaphanHelper/LaserEmitter",
        inverted = false,
        base = true,
        noBeam = false
    }
}

function LaserEmitter.sprite(room, entity)
    local side = entity.side or "Left"
    local directory = entity.directory or "objects/XaphanHelper/LaserEmitter"
    local base = entity.base

    local sprites = {}

    local sprite = drawableSprite.fromTexture(directory .. "/idle00", entity)
    local baseSprite = drawableSprite.fromTexture(directory .. "/baseActive" .. side .. "00", entity)

    sprite:addPosition(4, 4)

    if side == "Bottom" then
        sprite.rotation = math.pi
        baseSprite:addPosition(4, -4)
    elseif side == "Top" then
        sprite.rotation = 0
        baseSprite:addPosition(4, 12)
    elseif side == "Left" then
        sprite.rotation = -math.pi / 2
        baseSprite:addPosition(12, 4)
    elseif side == "Right" then
        sprite.rotation = math.pi / 2
        baseSprite:addPosition(-4, 4)
    end

    sprite:setColor("White")

    if sprite then
        table.insert(sprites, sprite)
    end
    if base and baseSprite then
        table.insert(sprites, baseSprite)
    end

    return sprites
end

function LaserEmitter.selection(room, entity)
    local side = entity.side or "Left"
    local base = entity.base
    local xAdjust = 0
    local width = 0
    local yAdjust = 0
    local height = 0
    if side == "Bottom" then
        width = 8
        height = 2
        if base then
            height = 10
            yAdjust = -8
        end
    elseif side == "Top" then
        width = 8
        height += 2
        yAdjust = 6
        if base then
            height = 10
        end
    elseif side == "Left" then
        height = 8
        width = 2
        xAdjust = 6
        if base then
            width = 10
        end
    elseif side == "Right" then
        height = 8
        width = 2
        if base then
            width = 10
            xAdjust = -8
        end
    end

    return utils.rectangle(entity.x + xAdjust, entity.y + yAdjust, width, height)
end

return LaserEmitter