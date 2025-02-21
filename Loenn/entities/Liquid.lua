local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local Liquid = {}

Liquid.name = "XaphanHelper/Liquid"
Liquid.depth = function(room, entity)
    local depth = 0
    if entity.foreground then
        depth = -19999
    else
        depth = -9999
    end
    return depth
end
Liquid.fieldOrder = {
    "x", "y", "width", "height", "liquidType", "directory", "surfaceHeight", "lowPosition", "color", "poisonedColor", "group", "transparency", "insideTransparency", "frameDelay", "riseDistance", "riseDelay", "riseSpeed", "riseFlag", "riseEndFlag", "appearFlags", "removeFlags", "purifyFlags", "airTimer", "variaPreventDying", "riseShake", "riseSound", "canSwim", "canDrown", "visualOnly", "foreground", "upsideDown", "poisoned"
}
Liquid.fieldInformation = {
    lowPosition = {
        fieldType = "integer",
    },
    liquidType = {
        options = {"acid", "acid_b", "lava", "quicksand", "water"},
        editable = false
    },
    color = {
        fieldType = "color"
    },
    transparency = {
        fieldType = "number",
        minimumValue = 0.01,
        maximumValue = 1
    },
    insideTransparency = {
        fieldType = "number",
        minimumValue = 0.01,
        maximumValue = 1
    },
    riseDistance = {
        fieldType = "integer",
    },
    riseSpeed = {
        fieldType = "integer",
    },
    surfaceHeight = {
        fieldType = "integer",
        minimumValue = 0
    },
    group = {
        fieldType = "integer",
        minimumValue = -1
    },
    poisonedColor = {
        fieldType = "color"
    },
}
Liquid.placements = {
    name = "Liquid",
    data = {
        width = 8,
        height = 8,
        lowPosition = 0,
        liquidType = "acid",
        frameDelay = 0.15,
        color = "88C098",
        transparency = 0.65,
        insideTransparency = 0.65,
        foreground = false,
        riseDelay = 0.00,
        riseDistance = 0,
        riseSpeed = 10,
        riseShake = false,
        riseFlag = "",
        riseEndFlag = "",
        riseSound = false,
        directory = "objects/XaphanHelper/liquid",
        surfaceHeight = 0,
        canSwim = false,
        visualOnly = false,
        appearFlags = "",
        removeFlags = "",
        upsideDown = false,
        group = -1,
        canDrown = false,
        airTimer = 15,
        variaPreventDying = false,
        poisoned = false,
        poisonedColor = "4c9a42",
        purifyFlags = "",
        invertPurifyFlags = false
    }
}

function Liquid.sprite(room, entity)
    local sprites = {}

    local directory = entity.directory or "objects/XaphanHelper/liquid"
    local liquidType = entity.liquidType or "acid"
    local surfaceHeight = entity.surfaceHeight or 0

    if surfaceHeight == 0 then
        if liquidType == "acid" or liquidType == "acid_b" then
            surfaceHeight = 24
        elseif liquidType == "lava" then
            surfaceHeight = 8
        elseif liquidType == "quicksand" or liquidType == "water" then
            surfaceHeight = 16
        end
    end

    local width = entity.width
    local height = entity.height

    local sprite = nil
    for i = 0, height / 8 do
        for j = 0, width / 8 - 1 do
            sprite = drawableSprite.fromTexture(directory .. "/" .. liquidType .. "/liquid00", entity)
            local spriteWidth = sprite.meta.width
            local spriteHeightMinusSurface = sprite.meta.height - surfaceHeight
            if i < surfaceHeight / 8 then
                sprite:useRelativeQuad((j * 8) % spriteWidth, i * 8, 8, 8)
            else
                local pos = i * 8 - surfaceHeight
                sprite:useRelativeQuad((j * 8) % spriteWidth, surfaceHeight + pos % spriteHeightMinusSurface, 8, 8)
            end
            local color = getSpriteColor(entity)
            sprite:setColor(color)
            sprite:addPosition(j * 8, i * 8 - 8)
            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

function getSpriteColor(entity)
    local color = getEntityColor(entity)
    local transparency = entity.transparency or 0.65
    local insideTransparency = entity.insideTransparency or 0.65
    local showTransparency = transparency
    if insideTransparency < transparency then
        showTransparency = insideTransparency
    end
    return {color[1], color[2], color[3], showTransparency}
end

function getEntityColor(entity)
    local rawColor = entity.color or "FFFFFF"
    local color = utils.getColor(rawColor)

    return color
end

return Liquid