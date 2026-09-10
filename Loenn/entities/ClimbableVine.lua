local drawableSprite = require("structs.drawable_sprite")

local ClimbableVine = {}

ClimbableVine.name = "XaphanHelper/ClimbableVine"
ClimbableVine.minimumSize = {8, 16}
ClimbableVine.depth = -1
ClimbableVine.canResize = {false, true}
ClimbableVine.placements = {
    name = "ClimbableVine",
    data = {
        height = 16,
        directory = "objects/XaphanHelper/ClimbableVine",
        flag = ""
    }
}

function ClimbableVine.sprite(room, entity)
    local sprites = {}

    local directory = entity.directory or "objects/XaphanHelper/ClimbableVine"
    local height = entity.height / 8 or 1

    local sprite = nil
    for i = 1, (height - 1) do
        if (i % 2 == 0) then
            sprite = drawableSprite.fromTexture(directory .. "/light_b00", entity)
        else
            sprite = drawableSprite.fromTexture(directory .. "/light_a00", entity)
        end
        sprite:addPosition(4, (i - 1) * 8 + 4)
        if sprite then
            table.insert(sprites, sprite)
        end
    end

    local edgesprite = drawableSprite.fromTexture(directory .. "/light_edge00", entity)
    edgesprite:addPosition(4, (height - 1) * 8 + 4)
    if edgesprite then
        table.insert(sprites, edgesprite)
    end

    return sprites
end

return ClimbableVine