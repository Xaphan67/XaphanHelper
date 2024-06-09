local WarpStation = {}

WarpStation.name = "XaphanHelper/WarpStation"
WarpStation.depth = -9000
WarpStation.fieldOrder = {"x", "y", "sprite", "index", "beamColor", "confirmSfx", "flag", "noInteractFlag", "forceInactiveFlag", "wipeType", "wipeDuration", "noBeam", "onlyCurrentChapter"}
WarpStation.ignoredFields = {
    "_name", "_id", "width", "height"
}
WarpStation.canResize = {false, false}
WarpStation.fieldInformation = {
    beamColor = {
        fieldType = "color"
    },
    index = {
        fieldType = "integer",
    },
    wipeType = {
        options = {"Spotlight", "Curtain", "Mountain", "Dream", "Starfield", "Wind", "Drop", "Fall", "KeyDoor", "Angled", "Heart", "Fade"},
        editable = false
    }
}
WarpStation.placements = {
    name = "WarpStation",
    data = {
        width = 32,
        height = 16,
        beamColor = "FFFFFF",
        noBeam = false,
        flag = "",
        sprite = "objects/XaphanHelper/WarpStation",
        confirmSfx = "",
        index = 0,
        wipeType = "Fade",
        wipeDuration = 0.75,
        noInteractFlag = "",
        forceInactiveFlag = "",
        onlyCurrentChapter = false
    }
}

function WarpStation.texture(room, entity)
    local texture = entity.sprite or "objects/XaphanHelper/WarpStation"

    return texture .. "/idle00"
end

WarpStation.offset = {0, 0}

return WarpStation