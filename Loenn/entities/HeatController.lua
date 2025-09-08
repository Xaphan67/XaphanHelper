local HeatController = {}

HeatController.name = "XaphanHelper/HeatController"
HeatController.depth = -100000
HeatController.fieldInformation = {
    exploredRoomColor = {
        fieldType = "color"
    }
}
HeatController.placements = {
    name = "HeatController",
    data = {
        maxDuration = 3.00,
        heatEffect = false,
        inactiveFlag = "",
        resetDuration = false
    }
}

HeatController.texture = "util/XaphanHelper/Loenn/heatController"

return HeatController