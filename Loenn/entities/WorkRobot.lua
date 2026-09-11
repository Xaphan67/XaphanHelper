local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local WorkRobot = {}

WorkRobot.name = "XaphanHelper/WorkRobot"
WorkRobot.fieldOrder = {
    "x", "y", "directory", "size", "flag", "speed", "startWalkLeft"
}
WorkRobot.ignoredFields = {
    "_id", "_name", "width", "height"
}
WorkRobot.fieldInformation = {
    size = {
        options = {"Large", "Medium", "Small"},
        editable = false
    },
    speed = {
        fieldType = "integer"
    }
}
WorkRobot.placements = {
    name = "WorkRobot",
    data = {
        size = "Medium",
        startWalkLeft = false,
        flag = "",
        speed = 20,
        directory = "objects/XaphanHelper/WorkRobot"
    }
}

function WorkRobot.selection(room, entity)
    local size = entity.size or "Medium"
    if size == "Small" then
        return utils.rectangle(entity.x + 3 or 0, entity.y + 7 or 0, 10, 17)
    end
    if size == "Medium" then
        return utils.rectangle(entity.x + 1 or 0, entity.y - 2 or 0, 15, 26)
    end
    if size == "Large" then
        return utils.rectangle(entity.x - 2 or 0, entity.y - 11 or 0, 20, 35)
    end
end

function WorkRobot.sprite(room, entity)
    local size = entity.size or "Medium"
    local directory = entity.directory or "objects/XaphanHelper/WorkRobot"
    if directory == "" then
        directory = "objects/XaphanHelper/WorkRobot"
    end
    local startWalkLeft = entity.startWalkLeft
    local robotSprite = drawableSprite.fromTexture(directory .. "/" .. size .. "/walk00", entity)
    robotSprite:addPosition(8, 4)

    local left = 1
    if startWalkLeft then
        left = -1
    end

    robotSprite:setScale(left, 1)

    return robotSprite
end

return WorkRobot