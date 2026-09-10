local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")
local enums = require("consts.celeste_enums")
local brushes = require("brushes")

local FuseBreakBlock = {}

local celesteRender = require("celeste_render")

function getTilesetsPaths()
    local paths = {}
    local tilesets = celesteRender.tilesMetaFg

    for id, tileset in pairs(tilesets) do
        local cleanPath = string.sub(tileset.path, 10)

        paths[cleanPath] = cleanPath
    end

    return paths
end

FuseBreakBlock.name = "XaphanHelper/FuseBreakBlock"
FuseBreakBlock.depth = -10002
FuseBreakBlock.fieldInformation = {
    soundIndex = {
        options = enums.tileset_sound_ids,
        editable = false
    },
    texture = {
        options = getTilesetsPaths(),
        editable = false
    },
}
FuseBreakBlock.placements = {
    name = "FuseBreakBlock",
    data = {
        width = 8,
        height = 8,
        soundIndex = 8,
        texture = "dirt"
    }
}

local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name
    end
end

local function getTileSprite(entity, x, y, frame, depth, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local positionXLastDigit = math.abs(drawX % 10);
    local positionYLastDigit = math.abs(drawY % 10);
    local seed = positionXLastDigit + positionYLastDigit;

    local variation = 0;
    local variationPadding = 0;

    if seed == 1 or seed == 5 or seed == 9 or seed == 13 or seed == 17 then
        variation = 8;
    elseif seed == 2 or seed == 6 or seed == 10 or seed == 14 or seed == 18 then
        variation = 16;
    elseif seed == 3 or seed == 7 or seed == 11 or seed == 15 then
        variation = 24;
    end

    if seed <= 9 then
        variationPadding = seed * 8
    else
        variationPadding = (seed - 10) * 8
    end

    local closedUpLeft = hasAdjacent(entity, drawX - 8, drawY - 8, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedUpRight = hasAdjacent(entity, drawX + 8, drawY - 8, rectangles)
    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedDownLeft = hasAdjacent(entity, drawX - 8, drawY + 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local closedDownRight = hasAdjacent(entity, drawX + 8, drawY + 8, rectangles)
    local closedFarUp = hasAdjacent(entity, drawX, drawY - 16, rectangles)
    local closedFarLeft = hasAdjacent(entity, drawX - 16, drawY, rectangles)
    local closedFarRight = hasAdjacent(entity, drawX + 16, drawY, rectangles)
    local closedFarDown = hasAdjacent(entity, drawX, drawY + 16, rectangles)
    local interior = closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and closedDownRight and closedFarUp and closedFarLeft and closedFarRight and closedFarDown

    local quadX, quadY = false, false

    if interior then
            quadX, quadY = 40, 112
    elseif closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 0
    elseif closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and closedDownRight then
        quadX, quadY = 32, 8
    elseif closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and closedDownRight then
        quadX, quadY = 32, 16
    elseif not closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and closedDownRight then
        quadX, quadY = 32, 24
    elseif closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 32
    elseif not closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and closedDownRight then
        quadX, quadY = 32, 40
    elseif not closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and closedDownRight then
        quadX, quadY = 32, 48
    elseif closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 56
    elseif not closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 64
    elseif not closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and closedDownRight then
        quadX, quadY = 32, 72
    elseif not closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 80
    elseif closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 88
    elseif not closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 96
    elseif closedUpLeft and closedUp and not closedUpRight and closedLeft and closedRight and not closedDownLeft and closedDown and closedDownRight then
        quadX, quadY = 32, 104
    elseif not closedUpLeft and closedUp and closedUpRight and closedLeft and closedRight and closedDownLeft and closedDown and not closedDownRight then
        quadX, quadY = 32, 112
    elseif not closedUp and closedLeft and closedRight and closedDown then
        quadX, quadY = variation, 0
    elseif closedUp and closedLeft and closedRight and not closedDown then
        quadX, quadY = variation, 8
    elseif closedUp and not closedLeft and closedRight and closedDown then
        quadX, quadY = variation, 16
    elseif closedUp and closedLeft and not closedRight and closedDown then
        quadX, quadY = variation, 24
    elseif not closedUp and closedLeft and closedRight and not closedDown then
        quadX, quadY = variation, 32
    elseif closedUp and not closedLeft and not closedRight and closedDown then
        quadX, quadY = variation, 40
    elseif not closedUp and not closedLeft and not closedRight and closedDown then
        quadX, quadY = variation, 48
    elseif closedUp and not closedLeft and not closedRight and not closedDown then
        quadX, quadY = variation, 56
    elseif not closedUp and not closedLeft and closedRight and not closedDown then
        quadX, quadY = variation, 64
    elseif not closedUp and closedLeft and not closedRight and not closedDown then
        quadX, quadY = variation, 72
    elseif not closedUp and not closedLeft and not closedRight and not closedDown then
        quadX, quadY = variation, 80
    elseif not closedUp and not closedLeft and closedRight and closedDown then
        quadX, quadY = variation, 88
    elseif not closedUp and closedLeft and not closedRight and closedDown then
        quadX, quadY = variation, 96
    elseif closedUp and not closedLeft and closedRight and not closedDown then
        quadX, quadY = variation, 104
    elseif closedUp and closedLeft and not closedRight and not closedDown then
        quadX, quadY = variation, 112
    else
        quadX, quadY = 40, variationPadding
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(frame, entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)

        sprite.depth = depth

        return sprite
    end
end

function FuseBreakBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local frame = "tilesets/" ..entity.texture or "tilesets/dirt"
    local depth = -10002

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, frame, depth, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

return FuseBreakBlock