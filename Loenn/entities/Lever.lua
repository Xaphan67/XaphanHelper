local drawableSprite = require("structs.drawable_sprite")

local Lever = {}

Lever.name = "XaphanHelper/Lever"
Lever.nodeLimits = {0, -1}
Lever.nodeLineRenderType = "fan"

Lever.fieldOrder = {
    "x", "y", "directory", "side", "group", "flag", "canSwapFlag", "registerInSaveData", "saveDataOnlyAfterCheckpoint"
}
Lever.ignoredFields = {
    "_id", "_name", "width", "height"
}
Lever.fieldInformation = {
    side = {
        options = {"Up", "Down", "Left", "Right"},
        editable = false
    },
    group = {
        fieldType = "integer"
    }
}
Lever.placements = {
    name = "Lever",
    data = {
        directory = "objects/XaphanHelper/Lever",
        side = "Up",
        group = 0,
        flag = "",
        canSwapFlag = false,
        registerInSaveData = false,
        saveDataOnlyAfterCheckpoint = false
    }
}

function Lever.sprite(room, entity)
    local directory = entity.directory or ""
    local side = entity.side or "Up"

    local leverSprite = drawableSprite.fromTexture(directory .. "/lever00", entity)
    if side == "Up" then
    elseif side == "Down" then
        leverSprite.rotation = -math.pi
        leverSprite:setScale({-1, 1})
    elseif side == "Left" then
        leverSprite.rotation = -math.pi / 2
        leverSprite:setScale({-1, 1})
    elseif side == "Right" then
        leverSprite.rotation = math.pi / 2
    end
    leverSprite:addPosition(8, 0)

    return leverSprite
end

return Lever