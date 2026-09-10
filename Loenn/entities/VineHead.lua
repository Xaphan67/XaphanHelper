local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local VineHead = {}

VineHead.name = "XaphanHelper/VineHead"
VineHead.nodeLineRenderType = false
VineHead.depth = -8501
VineHead.placements = {
    name = "VineHead",
    data = {
        directory = "objects/XaphanHelper/Vine",
        growSpeed = 50,
        pauseTime = 3,
        flag = ""
    }
}

function tablelength(T)
    local count = 0
    for _ in pairs(T) do count = count + 1 end
    return count
  end

function VineHead.sprite(room, entity)
    local directory = entity.directory
    if directory == "" then
        directory = "objects/XaphanHelper/Vine"
    end
    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local lineSprite = {}
    local sprite = drawableSprite.fromTexture(directory .. "/head00", entity)
    sprite:addPosition(4, 4)
    table.insert(lineSprite, sprite)

    return lineSprite
end

function VineHead.rectangle(room, entity)
    local directory = entity.directory
    if directory == "" then
        directory = "objects/XaphanHelper/Vine"
    end
    local sprite = drawableSprite.fromTexture(directory .. "/head00", entity)
    sprite:addPosition(4, 4)

    return sprite:getRectangle()
end

return VineHead