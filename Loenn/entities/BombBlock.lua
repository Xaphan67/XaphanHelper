local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local BombBlock = {}

local frameNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local trailNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    useRealSize = true
}

local pathNinePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

local pathDepth = 8999
local trailDepth = 8999
local blockDepth = -9999

BombBlock.name = "XaphanHelper/BombBlock"
BombBlock.nodeLimits = {1, 1}
BombBlock.minimumSize = {24, 24}
BombBlock.fieldInformation = {
    speed = {
        fieldType = "integer",
    },
    leftSide = {
        options = {"Start Active", "Start Inactive", "Active Only Once", "None"},
        editable = false
    },
    rightSide = {
        options = {"Start Active", "Start Inactive", "Active Only Once", "None"},
        editable = false
    },
    topSide = {
        options = {"Start Active", "Start Inactive", "Active Only Once", "None"},
        editable = false
    },
    bottomSide = {
        options = {"Start Active", "Start Inactive", "Active Only Once", "None"},
        editable = false
    },
    particleColor1 = {
        fieldType = "color"
    },
    particleColor2 = {
        fieldType = "color"
    }
}
BombBlock.fieldOrder = {
    "x", "y", "width", "height", "particleColor1", "particleColor2", "leftSide", "flag", "topSide", "directory", "rightSide", "speed", "bottomSide", "toogle", "swapActiveSides", "deactivateAllSidesAfterUse"
}
BombBlock.placements = {
    name = "BombBlock",
    data = {
        width = 24,
        height = 24,
        directory = "objects/XaphanHelper/BombBlock",
        speed = 360,
        flag = "",
        toggle = false,
        leftSide = "None",
        rightSide = "None",
        topSide = "Start Active",
        bottomSide = "None",
        swapActiveSides = false,
        deactivateAllSidesAfterUse = false,
        particleColor1 = "FBF236",
        particleColor2 = "6ABE30"
    }
}

local function addBlockSprites(sprites, entity, frameTexture, middleTexture)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8

    local frameNinePatch = drawableNinePatch.fromTexture(frameTexture, frameNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()
    local middleSprite = drawableSprite.fromTexture(middleTexture, entity)

    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    middleSprite.depth = blockDepth

    for _, sprite in ipairs(frameSprites) do
        sprite.depth = blockDepth

        table.insert(sprites, sprite)
    end

    local directory = entity.directory or "objects/swapblock"
    local topSide = entity.topSide
    local bottomSide = entity.bottomSide
    local leftSide = entity.leftSide
    local rightSide = entity.rightSide

    if topSide ~= "None" then
        local side = ""
        if topSide == "Start Active" then
            side = "/sideActive"
        elseif topSide == "Start Inactive" then
            side = "/sideInactive"
        else
            side = "/sideOneUse"
        end

        local topLeftSprite = drawableSprite.fromTexture(directory .. side, entity)
        topLeftSprite:useRelativeQuad(0, 0, 8, 8)
        topLeftSprite.depth = blockDepth
        table.insert(sprites, topLeftSprite)

        for i = 1, (width / 8 - 2) do
            local topMiddleSprite = drawableSprite.fromTexture(directory .. side, entity)
            topMiddleSprite:useRelativeQuad(8, 0, 8, 8)
            topMiddleSprite:addPosition(i * 8, 0)
            topMiddleSprite.depth = blockDepth
            table.insert(sprites, topMiddleSprite)
        end

        local topRightSprite = drawableSprite.fromTexture(directory .. side, entity)
        topRightSprite:useRelativeQuad(16, 0, 8, 8)
        topRightSprite:addPosition(width - 8, 0)
        topRightSprite.depth = blockDepth
        table.insert(sprites, topRightSprite)
    end

    if bottomSide ~= "None" then
        local side = ""
        if bottomSide == "Start Active" then
            side = "/sideActive"
        elseif bottomSide == "Start Inactive" then
            side = "/sideInactive"
        else
            side = "/sideOneUse"
        end

        local bottomLeftSprite = drawableSprite.fromTexture(directory .. side, entity)
        bottomLeftSprite:useRelativeQuad(0, 0, 8, 8)
        bottomLeftSprite:addPosition(0, height)
        bottomLeftSprite:setScale(1, -1)
        bottomLeftSprite.depth = blockDepth
        table.insert(sprites, bottomLeftSprite)

        for i = 1, (width / 8 - 2) do
            local bottomMiddleSprite = drawableSprite.fromTexture(directory .. side, entity)
            bottomMiddleSprite:useRelativeQuad(8, 0, 8, 8)
            bottomMiddleSprite:setScale(1, -1)
            bottomMiddleSprite:addPosition(i * 8, height)
            bottomMiddleSprite.depth = blockDepth
            table.insert(sprites, bottomMiddleSprite)
        end

        local bottomRightSprite = drawableSprite.fromTexture(directory .. side, entity)
        bottomRightSprite:useRelativeQuad(16, 0, 8, 8)
        bottomRightSprite:setScale(1, -1)
        bottomRightSprite:addPosition(width - 8, height)
        bottomRightSprite.depth = blockDepth
        table.insert(sprites, bottomRightSprite)
    end

    if leftSide ~= "None" then
        local side = ""
        if leftSide == "Start Active" then
            side = "/sideActive"
        elseif leftSide == "Start Inactive" then
            side = "/sideInactive"
        else
            side = "/sideOneUse"
        end

        local leftBottomSprite = drawableSprite.fromTexture(directory .. side, entity)
        leftBottomSprite:useRelativeQuad(0, 0, 8, 8)
        leftBottomSprite.rotation = -math.pi / 2
        leftBottomSprite:addPosition(0, height)
        leftBottomSprite.depth = blockDepth
        table.insert(sprites, leftBottomSprite)

        for i = 1, (height / 8 - 2) do
            local leftMiddleSprite = drawableSprite.fromTexture(directory .. side, entity)
            leftMiddleSprite:useRelativeQuad(8, 0, 8, 8)
            leftMiddleSprite.rotation = -math.pi / 2
            leftMiddleSprite:addPosition(0, 8 + i * 8)
            leftMiddleSprite.depth = blockDepth
            table.insert(sprites, leftMiddleSprite)
        end

        local leftTopSprite = drawableSprite.fromTexture(directory .. side, entity)
        leftTopSprite:useRelativeQuad(16, 0, 8, 8)
        leftTopSprite.rotation = -math.pi / 2
        leftTopSprite:addPosition(0, 8)
        leftTopSprite.depth = blockDepth
        table.insert(sprites, leftTopSprite)
    end

    if rightSide ~= "None" then
        local side = ""
        if rightSide == "Start Active" then
            side = "/sideActive"
        elseif rightSide == "Start Inactive" then
            side = "/sideInactive"
        else
            side = "/sideOneUse"
        end

        local rightTopSprite = drawableSprite.fromTexture(directory .. side, entity)
        rightTopSprite:useRelativeQuad(16, 0, 8, 8)
        rightTopSprite.rotation = math.pi / 2
        rightTopSprite:addPosition(width, height - 8)
        rightTopSprite.depth = blockDepth
        table.insert(sprites, rightTopSprite)

        for i = 1, (height / 8 - 2) do
            local rightMiddleSprite = drawableSprite.fromTexture(directory .. side, entity)
            rightMiddleSprite:useRelativeQuad(8, 0, 8, 8)
            rightMiddleSprite.rotation = math.pi / 2
            rightMiddleSprite:addPosition(width, i * 8)
            rightMiddleSprite.depth = blockDepth
            table.insert(sprites, rightMiddleSprite)
        end

        local rightBottomSprite = drawableSprite.fromTexture(directory .. side, entity)
        rightBottomSprite:useRelativeQuad(0, 0, 8, 8)
        rightBottomSprite.rotation = math.pi / 2
        rightBottomSprite:addPosition(width, 0)
        rightBottomSprite.depth = blockDepth
        table.insert(sprites, rightBottomSprite)
    end

    table.insert(sprites, middleSprite)
end



local function addNodeSprites(sprites, entity, cogTexture, centerX, centerY, centerNodeX, centerNodeY)
    local nodeCogSprite = drawableSprite.fromTexture(cogTexture, entity)

    nodeCogSprite:setPosition(centerNodeX, centerNodeY)
    nodeCogSprite:setJustification(0.5, 0.5)

    local deactivateAllSidesAfterUse = entity.deactivateAllSidesAfterUse

    local ropeColor = nil
    if not deactivateAllSidesAfterUse then
        ropeColor= {102 / 255, 57 / 255, 49 / 255}
    else
        ropeColor= {75 / 255, 75 / 255, 75 / 255}
    end

    local points = {centerX, centerY, centerNodeX, centerNodeY}
    local leftLine = drawableLine.fromPoints(points, ropeColor, 1)
    local rightLine = drawableLine.fromPoints(points, ropeColor, 1)

    leftLine:setOffset(0, 4.5)
    rightLine:setOffset(0, -4.5)

    leftLine.depth = 5000
    rightLine.depth = 5000

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, nodeCogSprite)
end

function BombBlock.sprite(room, entity)
    local sprites = {}

    local directory = entity.directory or "objects/swapblock"

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    local centerX, centerY = x + halfWidth, y + halfHeight
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    addNodeSprites(sprites, entity, "objects/zipmover/cog", centerX, centerY, centerNodeX, centerNodeY)
    addBlockSprites(sprites, entity, directory .. "/blockRed", directory .. "/midBlockRed00")

    return sprites
end

function BombBlock.nodeSprite(room, entity)
    local sprites = {}

    local directory = entity.directory or "objects/swapblock"

    addBlockSprites(sprites, entity, directory .. "/blockRed", directory .. "/midBlockRed00")

    return sprites
end

function BombBlock.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local cogSprite = drawableSprite.fromTexture("objects/zipmover/cog", entity)
    local cogWidth, cogHeight = cogSprite.meta.width, cogSprite.meta.height

    local mainRectangle = utils.rectangle(x, y, width, height)
    local nodeRectangle = utils.rectangle(centerNodeX - math.floor(cogWidth / 2), centerNodeY - math.floor(cogHeight / 2), cogWidth, cogHeight)

    return mainRectangle, {nodeRectangle}
end

return BombBlock