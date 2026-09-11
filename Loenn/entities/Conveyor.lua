local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local Conveyor = {}

Conveyor.name = "XaphanHelper/Conveyor"
Conveyor.canResize = {true, true}
Conveyor.minimumSize = {8, 8}
Conveyor.fieldOrder = {
    "x", "y", "length", "directory", "speed", "direction", "activeFlag", "forceInactiveFlag", "swapFlag", "vertical", "flipX"
}
Conveyor.ignoredFields = {
    "_id", "_name", "width", "height", "length"
}
Conveyor.fieldInformation = {
    speed = {
        fieldType = "integer"
    },
    direction = {
        fieldType = "integer",
        options = {["Left / Top"] = -1, ["Right / Bottom"] = 1},
        editable = false
    }
}
Conveyor.placements = {
    name = "Conveyor",
    data = {
        width = 16,
        height = 8,
        length = 2,
        speed = 75,
        direction = -1,
        swapFlag = "",
        activeFlag = "",
        forceInactiveFlag = "",
        directory = "objects/XaphanHelper/Conveyor",
        vertical = false,
        flipX = false
    }
}

function Conveyor.selection(room, entity)
    local vertical = entity.vertical or false
    if not vertical then
        entity.height = 8
        entity.length = entity.width
        return utils.rectangle(entity.x or 0, entity.y or 0, math.max(entity.width or 0, 8), 8)
    else
        entity.width = 8
        entity.length = entity.height
        return utils.rectangle(entity.x or 0, entity.y or 0, 8, math.max(entity.height or 0, 8))
    end
end

function Conveyor.sprite(room, entity)
    local sprites = {}

    local directory = entity.directory
    local vertical = entity.vertical or false
    local length = 0
    if not vertical then
        length = entity.width
    else
        length = entity.height
    end
    local tileWidth = math.floor(length / 8)
    local flipX = entity.flipX or false

    for i = 2, tileWidth - 1 do
        local middleSprite = drawableSprite.fromTexture(directory .. "/bg01", entity)

        if not vertical then
            middleSprite:addPosition((i - 1) * 8, 0)
        else
            if flipX then
                middleSprite.rotation = -math.pi / 2
                middleSprite:setScale({-1, 1})
                middleSprite:addPosition(0, (i - 1) * 8)
            else
                middleSprite.rotation = math.pi / 2
                middleSprite:addPosition(8, (i - 1) * 8)
            end
        end
        middleSprite:setJustification(0.0, 0.0)

        table.insert(sprites, middleSprite)
    end

    local leftSprite = drawableSprite.fromTexture(directory .. "/bg00", entity)
    local rightSprite = drawableSprite.fromTexture(directory .. "/bg02", entity)

    if not vertical then
        rightSprite:addPosition(length - 8, 0)
    else
        if flipX then
            leftSprite.rotation = math.pi / 2
            leftSprite:setScale({1, -1})
            leftSprite:addPosition(0, 0)
            rightSprite.rotation = math.pi / 2
            rightSprite:setScale({1, -1})
            rightSprite:addPosition(0, length - 8)
        else
            leftSprite.rotation = math.pi / 2
            leftSprite:addPosition(8, 0)
            rightSprite.rotation = math.pi / 2
            rightSprite:addPosition(8, length - 8)
        end
    end

    leftSprite:setJustification(0.0, 0.0)
    rightSprite:setJustification(0.0, 0.0)

    table.insert(sprites, leftSprite)
    table.insert(sprites, rightSprite)

    for i = 1, tileWidth do
        local beltSprite = drawableSprite.fromTexture(directory .. "/belt", entity)

        if not vertical then
            beltSprite:addPosition((i - 1) * 8, 0)
        else
            if flipX then
                beltSprite.rotation = -math.pi / 2
                beltSprite:setScale({-1, 1})
                beltSprite:addPosition(0, (i - 1) * 8)
            else
                beltSprite.rotation = math.pi / 2
                beltSprite:addPosition(8, (i - 1) * 8)
            end
        end

        beltSprite:setJustification(0.0, 0.0)

        table.insert(sprites, beltSprite)
    end

    local leftFGSprite = drawableSprite.fromTexture(directory .. "/fg00", entity)
    local rightFGSprite = drawableSprite.fromTexture(directory .. "/fg01", entity)

    if not vertical then
        rightFGSprite:addPosition(length - 8, 0)
    else
        if flipX then
            leftFGSprite.rotation = math.pi / 2
            leftFGSprite:setScale({1, -1})
            leftFGSprite:addPosition(0, 0)
            rightFGSprite.rotation = math.pi / 2
            rightFGSprite:setScale({1, -1})
            rightFGSprite:addPosition(0, length - 8)
        else
            leftFGSprite.rotation = math.pi / 2
            leftFGSprite:addPosition(8, 0)
            rightFGSprite.rotation = math.pi / 2
            rightFGSprite:addPosition(8, length - 8)
        end
    end

    leftFGSprite:setJustification(0.0, 0.0)
    rightFGSprite:setJustification(0.0, 0.0)

    table.insert(sprites, leftFGSprite)
    table.insert(sprites, rightFGSprite)

    return sprites
end

return Conveyor