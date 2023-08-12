local StartCountdownTrigger = {}

StartCountdownTrigger.name = "XaphanHelper/StartCountdownTrigger"
StartCountdownTrigger.fieldOrder = {
    "x", "y", "width", "height", "time", "startFlag", "activeFlag", "dialogID", "messageTimer", "messageColor", "shake", "explosions", "crossChapter", "fastMessageDisplay", "notVisible"
}
StartCountdownTrigger.fieldInformation = {
    messageColor = {
        fieldType = "color"
    }
}
StartCountdownTrigger.placements = {
    name = "StartCountdownTrigger",
    data = {
        time = 60.00,
        startFlag = "",
        activeFlag = "",
        shake = false,
        explosions= false,
        crossChapter = false,
        dialogID = "",
        messageTimer = 5.00,
        fastMessageDisplay = false,
        messageColor = "FFFFFF",
        notVisible = false
    }
}

return StartCountdownTrigger