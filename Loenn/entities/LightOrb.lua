local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local LightOrb = {}

LightOrb.name = "XaphanHelper/LightOrb"
LightOrb.depth = 0
LightOrb.placements = {
    name = "LightOrb",
    data = {
        directory = "objects/XaphanHelper/LightOrb",
        temporary = false,
        time = 3
    }
}

function LightOrb.sprite(room, entity)
    local directory = entity.directory or "objects/XaphanHelper/LightOrb"

    local sprites = {}

    local orbSprite = nil
    local outlineSprite = nil

    if directory == "" then
        directory = "objects/XaphanHelper/LightOrb"
    end
    local temporary = entity.temporary or false
    if temporary then
        orbSprite = drawableSprite.fromTexture(directory .. "/light-small00", entity)
    else
        orbSprite = drawableSprite.fromTexture(directory .. "/light00", entity)
    end

    table.insert(sprites, orbSprite)

    if temporary then
        outlineSprite = drawableSprite.fromTexture(directory .. "/outline-small00", entity)
    else
        outlineSprite = drawableSprite.fromTexture(directory .. "/outline00", entity)
    end

    table.insert(sprites, outlineSprite)

    return sprites
end

return LightOrb