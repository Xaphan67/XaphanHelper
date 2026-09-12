local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local CustomMovingPlatform = {}

CustomMovingPlatform.name = "XaphanHelper/CustomMovingPlatform"
CustomMovingPlatform.nodeLimits = {1, -1}
CustomMovingPlatform.nodeLineRenderType = "line"
CustomMovingPlatform.nodeVisibility = "always"
CustomMovingPlatform.fieldOrder = {
    "x", "y", "mode", "directory", "lineColorA", "lineColorB", "particlesColorA", "particlesColorB", "orientation", "amount", "length", "speed", "startOffset", "spacingOffset", "attachedEntityPlatformsIndexes", "stopFlag", "swapFlag", "moveFlag", "forceInactiveFlag", "stopAtEachNode", "drawTrack", "particles"
}
CustomMovingPlatform.fieldInformation = {
    mode = {
        options = {"Restart", "Back And Forth", "Back And Forth - All Platforms", "Flag To Move"},
        editable = false
    },
    orientation = {
        options = {"Top", "Bottom", "Left", "Right"},
        editable = false
    },
    amount = {
        fieldType = "integer",
        minimumValue = 1
    },
    startOffset = {
        minimumValue = 0,
        maximumValue = 1
    },
    spacingOffset = {
        minimumValue = 0,
        maximumValue = 1
    },
    lineColorA = {
        fieldType = "color"
    } ,
    lineColorB = {
        fieldType = "color"
    },
    particlesColorA = {
        fieldType = "color"
    },
    particlesColorB = {
        fieldType = "color"
    },
    length = {
        fieldType = "integer",
        minimumValue = 1
    }
}
CustomMovingPlatform.placements = {
    name = "CustomMovingPlatform",
    data = {
        directory = "objects/XaphanHelper/CustomMovingPlatform",
        length = 3,
        mode = "Restart",
        speed = 60,
        stopFlag = "",
        swapFlag = "",
        moveFlag = "",
        drawTrack = true,
        orientation = "Top",
        amount = 3,
        startOffset = 0,
        spacingOffset = 0.3,
        attachedEntityPlatformsIndexes = "",
        lineColorA = "2A251F",
        lineColorB = "C97F35",
        particles = true,
        particlesColorA = "696A6A",
        particlesColorB = "700808",
        forceInactiveFlag = ""
    }
}

function tablelength(T)
    local count = 0
    for _ in pairs(T) do count = count + 1 end
    return count
  end

function CustomMovingPlatform.sprite(room, entity)
    local directory = entity.directory
    local lineColorA = entity.lineColorA or "2A251F"
    local lineColorB = entity.lineColorB or "C97F35"
    local orientation = entity.orientation or "Top"
    local length = entity.length or 3

    if directory == "" then
        directory = "objects/XaphanHelper/CustomMovingPlatform"
    end
    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {}
    local lineSprite = {}
    if tablelength(nodes) > 0 then
        local line = drawing.getSimpleCurve({x, y}, {nodes[1].x, nodes[1].y})
        lineSprite = drawableLine.fromPoints(line, lineColorB, 1):getDrawableSprite()
    end

    for i = 0, length - 1 do
        local platformSprite = drawableSprite.fromTexture(directory .. "/platform", entity)
        if orientation == "Top" then
            platformSprite:addPosition(4 + -(length * 8) / 2 + i * 8, 0)
        end
        if orientation == "Bottom" then
            platformSprite:addPosition(4 + -(length * 8) / 2 + i * 8, 0)
            platformSprite:setScale({1, -1})
        end
        if orientation == "Left" then
            platformSprite.rotation = -math.pi / 2
            platformSprite:setScale({-1, 1})
            platformSprite:addPosition(0, 4 + -(length * 8) / 2 + i * 8)
        end
        if orientation == "Right" then
            platformSprite.rotation = math.pi / 2
            platformSprite:addPosition(0, 4 + -(length * 8) / 2 + i * 8)
        end
        table.insert(lineSprite, platformSprite)
    end

    return lineSprite
end

function CustomMovingPlatform.nodeSprite(room, entity, node, nodeIndex)
    local directory = entity.directory
    local lineColorA = entity.lineColorA or "2A251F"
    local lineColorB = entity.lineColorB or "C97F35"

    if directory == "" then
        directory = "objects/XaphanHelper/CustomMovingPlatform"
    end
    local nodes = entity.nodes or {}
    local lineSprite = {}
    if tablelength(nodes) > nodeIndex and nodeIndex ~= tablelength(nodes) - 1 then
        local line = drawing.getSimpleCurve({nodes[nodeIndex].x, nodes[nodeIndex].y}, {nodes[nodeIndex + 1].x, nodes[nodeIndex + 1].y})
        lineSprite = drawableLine.fromPoints(line, lineColorB, 1):getDrawableSprite()
    end
    local sprite = drawableSprite.fromTexture(directory .. "/node00", entity)
    sprite:addPosition(nodes[nodeIndex].x - entity.x, nodes[nodeIndex].y - entity.y)
    table.insert(lineSprite, sprite)

    return lineSprite
end

function CustomMovingPlatform.rectangle(room, entity)
    local orientation = entity.orientation or "Top"
    local length = entity.length or 3

    if orientaton == "Top" or orientation == "Bottom" then
        return utils.rectangle(entity.x - (length * 8) / 2, entity.y - 4, length * 8, 8)
    end
    if orientation == "Left" or orientation == "Right" then
        return utils.rectangle(entity.x - 4, entity.y - (length * 8) / 2, 8, length * 8)
    end
    return utils.rectangle(entity.x - (length * 8) / 2, entity.y - 4, length * 8, 8)
end

function CustomMovingPlatform.nodeRectangle(room, entity, node, nodeIndex)
    local directory = entity.directory

    if directory == "" then
        directory = "objects/XaphanHelper/CustomMovingPlatform"
    end
    local nodes = entity.nodes or {}
    local sprite = drawableSprite.fromTexture(directory .. "/node00", entity)
    sprite:addPosition(nodes[nodeIndex].x - entity.x, nodes[nodeIndex].y - entity.y)

    return sprite:getRectangle()
end

return CustomMovingPlatform