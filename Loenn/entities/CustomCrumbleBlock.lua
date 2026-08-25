local utils = require("utils")
local drawableNinePatch = require("structs.drawable_nine_patch")

local CustomCrumbleBlock = {}

CustomCrumbleBlock.name = "XaphanHelper/CustomCrumbleBlock"
CustomCrumbleBlock.depth = -2
CustomCrumbleBlock.fieldOrder = {
    "x", "y", "width", "height", "texture", "rotation", "crumbleDelay", "sideCrumbleDelay", "respawnTime", "group", "oneUse", "canBypassCrumbleDelay", "canBypassSideCrumbleDelay", "light"
}
CustomCrumbleBlock.fieldInformation = {
    rotation = {
        fieldType = "integer",
        options = {["0°"] = 0, ["90°"] = 1, ["180°"] = 2, ["270°"] = 3},
        editable = false
    },
    group = {
        fieldType = "integer",
        minimumValue = -1
    }
}
CustomCrumbleBlock.placements = {
    name = "CustomCrumbleBlock",
    data = {
        width = 8,
        height = 8,
        texture = "objects/crumbleBlock/default",
        respawnTime = 2,
        oneUse = false,
        group = -1,
        crumbleDelay = 0.4,
        sideCrumbleDelay = 0.8,
        rotation = 0,
        canBypassCrumbleDelay = true,
        canBypassSideCrumbleDelay = false,
        light = true
    }
}

local ninePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

function CustomCrumbleBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 8
    local height = entity.height or 8

    local texture = entity.texture
    if texture == "" then
        texture = "objects/crumbleBlock/default"
    end
    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, height)

    return ninePatch
end

function CustomCrumbleBlock.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width, entity.height)
end

return CustomCrumbleBlock