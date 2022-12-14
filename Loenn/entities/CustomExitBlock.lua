local fakeTilesHelper = require("helpers.fake_tiles")

local CustomExitBlock = {}

CustomExitBlock.name = "XaphanHelper/CustomExitBlock"
CustomExitBlock.depth = -13000
CustomExitBlock.fieldOrder = {
    "x", "y", "width", "height", "flag", "mode", "tiletype", "flagTiletype", "group", "closeSound"
}
function CustomExitBlock.fieldInformation(entity)
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        flagTiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        mode = {
            options = {"Block", "Wall"},
            editable = false
        },
        group = {
            fieldType = "integer"
        }     
    }
end
CustomExitBlock.placements = {
    name = "CustomExitBlock",
    data = {
        width = 8,
        height = 8,
        tiletype = "3",
        flagTiletype = "3",
        flag ="",
        closeSound = false,
        mode = "Block",
        group = 0
    }
}

CustomExitBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})

return CustomExitBlock