local fakeTilesHelper = require("helpers.fake_tiles")

local CustomFallingBlock = {}

CustomFallingBlock.name = "XaphanHelper/CustomFallingBlock"
CustomFallingBlock.depth = function(room, entity)
    local depth = -9000
    if entity.behind then
        depth = 5000
    end
    return depth
end
CustomFallingBlock.fieldOrder = {
    "x", "y", "width", "height", "tiletype", "climbFall", "behind", "fallIfNoSolidOnTop", "magnetingCeilingsDoNotTrigger", "canFloat"
}
function CustomFallingBlock.fieldInformation(entity)
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        }
    }
end
CustomFallingBlock.placements = {
    name = "CustomFallingBlock",
    data = {
        tiletype = "3",
        climbFall = true,
        behind = false,
        width = 8,
        height = 8,
        fallIfNoSolidOnTop = false,
        magnetingCeilingsDoNotTrigger = false,
        canFloat = false
    }
}

CustomFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)

return CustomFallingBlock