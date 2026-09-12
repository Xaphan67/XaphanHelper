local drawableSpriteStruct = require("structs.drawable_sprite")

local BubbleBlock = {}

BubbleBlock.name = "XaphanHelper/BubbleBlock"
BubbleBlock.canResize = {true, true}
BubbleBlock.minimumSize = {16, 16}
BubbleBlock.fieldOrder = {
    "x", "y", "height", "width", "length"
}
BubbleBlock.fieldInformation = {

}
BubbleBlock.placements = {
    name = "BubbleBlock",
    data = {
        width = 16,
        height = 16,
        directory = "objects/XaphanHelper/BubbleBlock"
    }
}

function BubbleBlock.sprite(room, entity)
    local texture = entity.directory .. "/bubbles" or "objects/XaphanHelper/BubbleBlock/bubbles"

    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 8
    local height = entity.height or 8

    local sprites = {}

    for i = 0, math.floor(width / 8) - 1 do
       for j = 0, math.floor(height / 8) - 1 do
        local sprite = drawableSpriteStruct.fromTexture(texture, entity)

        sprite:setJustification(0, 0)
        sprite:addPosition(i * 8, j * 8)
        sprite:useRelativeQuad(0, 0, 8, 8)

        table.insert(sprites, sprite)
       end
    end

    return sprites
end

return BubbleBlock