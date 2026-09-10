local LightModeTrigger = {}

LightModeTrigger.name = "XaphanHelper/LightModeTrigger"
LightModeTrigger.fieldInformation = {
    mode = {
        options = {"Light", "Dark", "None"},
        editable = false,
    }
}
LightModeTrigger.placements = {
    name = "LightModeTrigger",
    data = {
        mode = "None"
    }
}

return LightModeTrigger