local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local PowerLine = {}

PowerLine.name = "XaphanHelper/PowerLine"
PowerLine.placements = {
    name = "PowerLine",
    data = {
        width = 8,
        height = 8,
        flag = "",
        inverted = false,
        directory = "objects/XaphanHelper/PowerLine",
        background = false
    }
}

local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name
    end
end

local function hasAdjacentThinSide(entities, selfEntity, cellX, cellY, axis)
    local selfX, selfY = selfEntity.x, selfEntity.y
    local selfWidth, selfHeight = selfEntity.width or 8, selfEntity.height or 8

    for _, other in ipairs(entities) do
        local ox, oy = other.x, other.y
        local ow, oh = other.width or 8, other.height or 8

        local overlapsCell = cellX < ox + ow and cellX + 8 > ox
                          and cellY < oy + oh and cellY + 8 > oy

        if overlapsCell then
            local overlapLength

            if axis == "y" then
                local top = math.max(selfY, oy)
                local bottom = math.min(selfY + selfHeight, oy + oh)
                overlapLength = bottom - top
            else
                local left = math.max(selfX, ox)
                local right = math.min(selfX + selfWidth, ox + ow)
                overlapLength = right - left
            end

            if overlapLength <= 8 then
                return true
            end
        end
    end

    return false
end

local function getTileSprite(entity, x, y, frame, depth, relevantBlocks)
    local drawX, drawY = (x - 1) * 8, (y - 1) * 8
    local absX, absY = entity.x + drawX, entity.y + drawY

    local closedLeft = hasAdjacentThinSide(relevantBlocks, entity, absX - 8, absY, "y")
    local closedRight = hasAdjacentThinSide(relevantBlocks, entity, absX + 8, absY, "y")
    local closedUp = hasAdjacentThinSide(relevantBlocks, entity, absX, absY - 8, "x")
    local closedDown = hasAdjacentThinSide(relevantBlocks, entity, absX, absY + 8, "x")

    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
            quadX, quadY = 0, 24
    else
        if not closedUp and closedDown and not closedLeft and closedRight then
            quadX, quadY = 0, 0
        elseif not closedUp and closedDown and closedLeft and not closedRight then
            quadX, quadY = 16, 0
        elseif closedUp and closedDown and not closedLeft and not closedRight then
            quadX, quadY = 0, 8
        elseif closedUp and not closedDown and not closedLeft and closedRight then
            quadX, quadY = 0, 16
        elseif not closedUp and not closedDown and closedLeft and closedRight then
            quadX, quadY = 8, 16
        elseif closedUp and not closedDown and closedLeft and not closedRight then
            quadX, quadY = 16, 16
        elseif not closedUp and not closedDown and not closedLeft and not closedRight then
            quadX, quadY = 8, 24
        elseif closedUp and closedDown and not closedLeft and closedRight then
            quadX, quadY = 24, 0
        elseif not closedUp and closedDown and closedLeft and closedRight then
            quadX, quadY = 32, 0
        elseif closedUp and not closedDown and closedLeft and closedRight then
            quadX, quadY = 24, 8
        elseif closedUp and closedDown and closedLeft and not closedRight then
            quadX, quadY = 32, 8
        elseif closedUp and not closedDown and not closedLeft and not closedRight then
            quadX, quadY = 24, 16
        elseif not closedUp and closedDown and not closedLeft and not closedRight then
            quadX, quadY = 32, 16
        elseif not closedUp and not closedDown and closedLeft and not closedRight then
            quadX, quadY = 24, 24
        elseif not closedUp and not closedDown and not closedLeft and closedRight then
            quadX, quadY = 32, 24
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(frame, entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)

        if entity.background then
            depth = 9999
        else
            depth = -19999
        end

        sprite.depth = depth

        return sprite
    end
end

function PowerLine.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local inverted = entity.inverted
    local frame = entity.directory .. "/frame" or "objects/XaphanHelper/PowerLine/frame"
    local lineFrameOff =  entity.directory .. "/off" or "objects/XaphanHelper/PowerLine/off"
    local lineFrameOn = entity.directory .. "/on" or "objects/XaphanHelper/PowerLine/on"
    local depth = -19999

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, frame, depth, rectangles)
            local lineSprite = nil
            if not inverted then
                lineSprite = getTileSprite(entity, x, y, lineFrameOff, depth, rectangles)
            else
                lineSprite = getTileSprite(entity, x, y, lineFrameOn, depth, rectangles)
            end

            if sprite then
                table.insert(sprites, sprite)
            end
            if lineSprite then
                table.insert(sprites, lineSprite)
            end
        end
    end

    return sprites
end

function PowerLine.onResize(room, entity, offsetX, offsetY, directionX, directionY)
    local newWidth = entity.width + offsetX
    local newHeight = entity.height + offsetY

    if newWidth > 8 and newHeight > 8 then
        return false
    end
end

return PowerLine