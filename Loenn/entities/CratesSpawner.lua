local drawableSprite = require("structs.drawable_sprite")

local CratesSpawner = {}

CratesSpawner.name = "XaphanHelper/CratesSpawner"

CratesSpawner.fieldOrder = {
    "x", "y", "directory", "maxCrates", "type", "cooldown", "flag", "forceInactiveFlag"
}
CratesSpawner.fieldInformation = {
    maxCrates = {
        fieldType = "integer"
    },
    type = {
        options = {"Wood", "Metal"},
        editable = false
    }
}
CratesSpawner.ignoredFields = {
    "_id", "_name", "width", "height"
}
CratesSpawner.placements = {
    name = "CratesSpawner",
    data = {
        directory = "objects/XaphanHelper/CratesSpawner",
        maxCrates = 1,
        flag = "",
        forceInactiveFlag = "",
        cooldown = 3.0,
        type = "Wood"
    }
}

function CratesSpawner.sprite(room, entity)
    local directory = entity.directory or ""
    local buttonSprite = drawableSprite.fromTexture(directory .. "/spawner00", entity)
    buttonSprite:addPosition(12, 4)

    return buttonSprite
end

return CratesSpawner