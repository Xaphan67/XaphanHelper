local utils = require("utils")

local ThornBarrier = {}

ThornBarrier.name = "XaphanHelper/ThornBarrier"
ThornBarrier.depth = 0
ThornBarrier.placements = {
    name = "ThornBarrier",
    data = {
        directory = "danger/XaphanHelper/ThornBarrier",
        flag = ""
    }
}

function ThornBarrier.texture(room, entity)
    local directory = entity.directory or "danger/XaphanHelper/ThornBarrier"
    if directory == "" then
        directory = "danger/XaphanHelper/ThornBarrier"
    end
    return directory .. "/light00"
end

function ThornBarrier.selection(room, entity)
    return utils.rectangle(entity.x -8, entity.y - 8, 16, 16)
end

return ThornBarrier