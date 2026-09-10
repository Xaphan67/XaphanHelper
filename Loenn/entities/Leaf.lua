local Leaf = {}

Leaf.name = "XaphanHelper/Leaf"
Leaf.depth = 0
Leaf.placements = {
    name = "Leaf",
    data = {
        directory = "objects/XaphanHelper/Leaf",
        flag = "",
        respawnTime = 2.5
    }
}

function Leaf.texture(room, entity)
    local directory = entity.directory or "objects/XaphanHelper/Leaf"
    if directory == "" then
        directory = "objects/XaphanHelper/Leaf"
    end
    return directory .. "/leaf00"
end


return Leaf