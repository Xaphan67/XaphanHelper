local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local VinePath = {}

VinePath.name = "XaphanHelper/VinePath"
VinePath.depth = 8999
VinePath.placements = {
    name = "VinePath",
    data = {
        width = 8,
        height = 8,
        directory = "objects/XaphanHelper/Vine"
    }
}

local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name
    end
end

local function isAtEdge(pos, size, position)
    return size == 8 or position == pos or position + 8 == pos + size
end

local function hasAdjacentCapOnly(entities, selfEntity, cellX, cellY, axis)
    local selfPos, selfSize
    if axis == "x" then
        selfPos, selfSize = selfEntity.x, selfEntity.width or 8
    else
        selfPos, selfSize = selfEntity.y, selfEntity.height or 8
    end

    local cellPos = (axis == "x") and cellX or cellY

    if not isAtEdge(selfPos, selfSize, cellPos) then
        return false
    end

    for _, other in ipairs(entities) do
        local ox, oy = other.x, other.y
        local ow, oh = other.width or 8, other.height or 8

        local overlapsCell = cellX < ox + ow and cellX + 8 > ox
                          and cellY < oy + oh and cellY + 8 > oy

        if overlapsCell then
            local otherPos, otherSize
            if axis == "x" then
                otherPos, otherSize = ox, ow
            else
                otherPos, otherSize = oy, oh
            end

            if isAtEdge(otherPos, otherSize, cellPos) then
                return true
            end
        end
    end

    return false
end

local function getTileSprite(entity, x, y, frame, depth, relevantBlocks)
    local drawX, drawY = (x - 1) * 8, (y - 1) * 8
    local absX, absY = entity.x + drawX, entity.y + drawY

    local closedLeft = hasAdjacentCapOnly(relevantBlocks, entity, absX - 8, absY, "y")
    local closedRight = hasAdjacentCapOnly(relevantBlocks, entity, absX + 8, absY, "y")
    local closedUp = hasAdjacentCapOnly(relevantBlocks, entity, absX, absY - 8, "x")
    local closedDown = hasAdjacentCapOnly(relevantBlocks, entity, absX, absY + 8, "x")

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

        sprite.depth = depth

        return sprite
    end
end

function VinePath.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local sprites = {}

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local frame = entity.directory .. "/path" or "objects/XaphanHelper/Vine/path"
    local depth = 8999

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, frame, depth, relevantBlocks)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

function VinePath.onResize(room, entity, offsetX, offsetY, directionX, directionY)
    local newWidth = entity.width + offsetX
    local newHeight = entity.height + offsetY

    if newWidth > 8 and newHeight > 8 then
        return false
    end
end

return VinePath