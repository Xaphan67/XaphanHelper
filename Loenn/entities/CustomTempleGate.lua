local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local CustomTempleGate = {}

CustomTempleGate.name = "XaphanHelper/CustomTempleGate"
CustomTempleGate.depth = -9000
CustomTempleGate.fieldOrder = {
    "x", "y", "flag", "spriteName", "horizontal", "attachRight", "startOpen", "openOnHeartCollection", "silent"
}
CustomTempleGate.ignoredFields = {
    "_name", "_id", "width", "height"
}
CustomTempleGate.fieldInformation = {
    spriteName = {
        options = {"default", "mirror", "theo"}
    }
}
CustomTempleGate.canResize = {false, false}
CustomTempleGate.placements = {
    name = "CustomTempleGate",
    data = {
        width = 8,
        height = 48,
        flag = "",
        startOpen = false,
        spriteName = "default",
        openOnHeartCollection = false,
        horizontal = false,
        attachRight = false,
        silent = false
    }
}

function CustomTempleGate.sprite(room, entity)
    local horizontal = entity.horizontal or false
    local attachRight = entity.attachRight or false
    local directory = {}
    directory["default"] = "objects/door/TempleDoor00"
    directory["mirror"] = "objects/door/TempleDoorB00"
    directory["theo"] = "objects/door/TempleDoorC00"
    local texture = directory[entity.spriteName] or directory["mirror"]

    local sprite = drawableSprite.fromTexture(texture, entity)

    if horizontal then
        if attachRight then
            sprite.rotation = math.pi / 2
            sprite:addPosition(24, 4)
        else
            sprite.rotation = -math.pi / 2
            sprite:addPosition(24, 5)
        end

    else
        sprite.rotation = 0
        sprite:addPosition(4, 24)
    end

    return sprite
end

function CustomTempleGate.selection(room, entity)
    local horizontal = entity.horizontal or false
    local width = 8
    local height = 48
    if horizontal then
        width = 48
        height = 8
    end
    return utils.rectangle(entity.x, entity.y , width, height)
end

CustomTempleGate.offset = {4, 0}

return CustomTempleGate