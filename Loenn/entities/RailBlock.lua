local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local RailBlock = {}

RailBlock.name = "XaphanHelper/RailBlock"
RailBlock.nodeLimits = {1, -1}
RailBlock.nodeLineRenderType = "line"
RailBlock.nodeVisibility = "always"
RailBlock.fieldOrder = {
    "x", "y", "speedMult", "directory", "lineColorA", "lineColorB", "particlesColorA", "particlesColorB", "canDash", "playerMomentum", "noReturn", "drawTrack", "particles"
}
RailBlock.fieldInformation = {
    speedMult = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 1.5
    },
    lineColorA = {
        fieldType = "color"
    },
    lineColorB = {
        fieldType = "color"
    },
    particlesColorA = {
        fieldType = "color"
    },
    particlesColorB = {
        fieldType = "color"
    }
}
RailBlock.placements = {
    name = "RailBlock",
    data = {
        directory = "objects/XaphanHelper/RailBlock",
        speedMult = 1,
        drawTrack = true,
        lineColorA = "2A251F",
        lineColorB = "C97F35",
        particles = true,
        particlesColorA = "696A6A",
        particlesColorB = "700808",
        canDash = true,
        playerMomentum = false,
        noReturn = false
    }
}

function tablelength(T)
    local count = 0
    for _ in pairs(T) do count = count + 1 end
    return count
  end

function RailBlock.sprite(room, entity)
    local directory = entity.directory
    local lineColorA = entity.lineColorA or "2A251F"
    local lineColorB = entity.lineColorB or "C97F35"
    if directory == "" then
        directory = "objects/XaphanHelper/RailBlock"
    end
    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {}
    local lineSprite = {}
    if tablelength(nodes) > 0 then
        local line = drawing.getSimpleCurve({x, y}, {nodes[1].x, nodes[1].y})
        lineSprite = drawableLine.fromPoints(line, lineColorB, 1):getDrawableSprite()
    end
    local sprite = drawableSprite.fromTexture(directory .. "/idle00", entity)
    table.insert(lineSprite, sprite)

    return lineSprite
end

function RailBlock.nodeSprite(room, entity, node, nodeIndex)
    local directory = entity.directory
    local lineColorA = entity.lineColorA or "2A251F"
    local lineColorB = entity.lineColorB or "C97F35"
    if directory == "" then
        directory = "objects/XaphanHelper/RailBlock"
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

function RailBlock.rectangle(room, entity)
    local directory = entity.directory
    if directory == "" then
        directory = "objects/XaphanHelper/RailBlock"
    end
    local sprite = drawableSprite.fromTexture(directory .. "/idle00", entity)

    return sprite:getRectangle()
end

function RailBlock.nodeRectangle(room, entity, node, nodeIndex)
    local directory = entity.directory
    if directory == "" then
        directory = "objects/XaphanHelper/RailBlock"
    end
    local nodes = entity.nodes or {}
    local sprite = drawableSprite.fromTexture(directory .. "/node00", entity)
    sprite:addPosition(nodes[nodeIndex].x - entity.x, nodes[nodeIndex].y - entity.y)

    return sprite:getRectangle()
end

return RailBlock