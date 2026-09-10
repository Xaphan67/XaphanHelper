local utils = require("utils")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local enums = require("consts.celeste_enums")

local PushBlock = {}

PushBlock.name = "XaphanHelper/PushBlock"
PushBlock.depth = -9999
PushBlock.minimumSize = {16, 16}
PushBlock.fieldOrder = {
    "x", "y", "width", "height", "soundIndex", "directory", "particleColor1", "particleColor2", "mustDash"
}
PushBlock.fieldInformation = {
    soundIndex = {
        options = enums.tileset_sound_ids,
        editable = false
    },
    particleColor1 = {
        fieldType = "color"
    },
    particleColor2 = {
        fieldType = "color"
    }
}
PushBlock.placements = {
    name = "PushBlock",
    data = {
        width = 16,
        height = 16,
        directory = "objects/XaphanHelper/PushBlock",
        soundIndex = 8,
        mustDash = false,
        particleColor1 = "4D6B68",
        particleColor2 = "1F2E2D"
    }
}

function PushBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
    local mustDash = entity.mustDash or false
    local directory = entity.directory or "objects/XaphanHelper/PushBlock"

    local blockNinePatch = nil
    local glowNinePatch = drawableNinePatch.fromTexture(directory .. "/glow", ninePatchOptions, x, y, width, height)
    local glowDashNinePatch = nil

    if mustDash then
        blockNinePatch = drawableNinePatch.fromTexture(directory .. "/blockDash", ninePatchOptions, x, y, width, height) 
        glowDashNinePatch = drawableNinePatch.fromTexture(directory .. "/glowDash", ninePatchOptions, x, y, width, height)
    else
        blockNinePatch = drawableNinePatch.fromTexture(directory .. "/block", ninePatchOptions, x, y, width, height)
    end

    local blockNinePatchSprite = blockNinePatch:getDrawableSprite()
    local glowNinePatchSprite = glowNinePatch:getDrawableSprite()
    local glowDashNinePatchSprite = nil

    if mustDash then
        glowDashNinePatchSprite = glowDashNinePatch:getDrawableSprite()
    end

    local sprites = {}
    
    for _, sprite in ipairs(blockNinePatchSprite) do

        table.insert(sprites, sprite)
    end
    for _, sprite in ipairs(glowNinePatchSprite) do

        table.insert(sprites, sprite)
    end
    if glowDashNinePatchSprite then
        for _, sprite in ipairs(glowDashNinePatchSprite) do

            table.insert(sprites, sprite)
        end
    end
    return sprites
end

return PushBlock