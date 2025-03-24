local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local FlagTempleGate = {}

FlagTempleGate.name = "XaphanHelper/FlagTempleGate"
FlagTempleGate.depth = -9000
FlagTempleGate.fieldOrder = {
    "x", "y", "flag", "spriteName", "horizontal", "attachRight", "startOpen", "openOnHeartCollection", "silent"
}
FlagTempleGate.ignoredFields = {
    "_name", "_id", "width", "height"
}
FlagTempleGate.fieldInformation = {
    spriteName = {
        options = {"default", "mirror", "theo"}
    }
}
FlagTempleGate.canResize = {false, false}
FlagTempleGate.placements = {
    name = "FlagTempleGate",
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

function FlagTempleGate.sprite(room, entity)
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

function FlagTempleGate.selection(room, entity)
    local horizontal = entity.horizontal or false
    local width = 8
    local height = 48
    if horizontal then
        width = 48
        height = 8
    end
    return utils.rectangle(entity.x, entity.y , width, height)
end

FlagTempleGate.offset = {4, 0}

return FlagTempleGate