local InGameMapHintController = {}

InGameMapHintController.name = "XaphanHelper/InGameMapHintController"
InGameMapHintController.depth = -100000
InGameMapHintController.fieldOrder = {
    "x", "y", "displayFlags", "hideFlag", "type", "direction", "tileCords", "removeWhenReachedByPlayer", "hideOnWorldMap"
}
InGameMapHintController.fieldInformation = {
    type = {
        options = {"Target", "Arrow"},
        editable = false
    },
    direction = {
        options = {"Left", "Right", "Up", "Down"},
        editable = false
    }
}
InGameMapHintController.placements = {
    name = "InGameMapHintController",
    data = {
        displayFlags = "",
        hideFlag = "",
        tileCords= "0-0",
        removeWhenReachedByPlayer = false,
        type = "Target",
        direction = "Up",
        hideOnWorldMap = false
    }
}

InGameMapHintController.texture = "util/XaphanHelper/Loenn/hintController"

return InGameMapHintController