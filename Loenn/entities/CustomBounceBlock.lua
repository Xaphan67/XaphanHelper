local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local CustomBounceBlock = {}

CustomBounceBlock.name = "XaphanHelper/CustomBounceBlock"
CustomBounceBlock.depth = 8990
CustomBounceBlock.warnBelowSize = {16, 16}
CustomBounceBlock.fieldOrder = {
    "x", "y", "width", "height", "directory", "bounceStrengthMultiplier", "respawnTime", "reformTime", "notCoreMode"
}
CustomBounceBlock.placements = {
    name = "CustomBounceBlock",
    data = {
        width = 16,
        height = 16,
        directory = "objects/BumpBlockNew",
        notCoreMode = false,
        bounceStrengthMultiplier = 1,
        respawnTime = 1.6,
        reformTime = 0.35
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local function getBlockTexture(entity)
    local directory = entity.directory or "objects/BumpBlockNew"
    if entity.notCoreMode then
        return directory .. "/ice00", directory .. "/ice_center00"
    else
        return directory .. "/fire00", directory .. "/fire_center00"
    end
end

function CustomBounceBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockTexture, crystalTexture = getBlockTexture(entity)

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, crystalSprite)

    return sprites
end

return CustomBounceBlock