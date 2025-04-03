local InGameMapRoomAdjustController = {}

InGameMapRoomAdjustController.name = "XaphanHelper/InGameMapRoomAdjustController"
InGameMapRoomAdjustController.depth = -100000
InGameMapRoomAdjustController.fieldOrder = {
    "x", "y", "positionX", "positionY", "sizeX", "sizeY", "hiddenTiles", "display", "ignoreIcons", "removeEntrance0", "removeEntrance1", "removeEntrance2", "removeEntrance3", "removeEntrance4", "removeEntrance5",
    "removeEntrance6", "removeEntrance7", "removeEntrance8", "removeEntrance9"
}
InGameMapRoomAdjustController.fieldInformation = {
    positionX = {
        fieldType = "integer",
    },
    positionY = {
        fieldType = "integer",
    },
    sizeX = {
        fieldType = "integer",
    },
    sizeY = {
        fieldType = "integer",
    },
    display = {
        options = {"Always", "Area map only", "World map only"},
        editable = false
    }
}
InGameMapRoomAdjustController.placements = {
    name = "InGameMapRoomAdjustController",
    data = {
        positionX = 0,
        positionY = 0,
        sizeX = 0,
        sizeY = 0,
        hiddenTiles = "",
        removeEntrance0 = false,
        removeEntrance1 = false,
        removeEntrance2 = false,
        removeEntrance3 = false,
        removeEntrance4 = false,
        removeEntrance5 = false,
        removeEntrance6 = false,
        removeEntrance7 = false,
        removeEntrance8 = false,
        removeEntrance9 = false,
        ignoreIcons = false,
        display = "Always"
    }
}

InGameMapRoomAdjustController.texture = "util/XaphanHelper/Loenn/roomAdjustController"

return InGameMapRoomAdjustController