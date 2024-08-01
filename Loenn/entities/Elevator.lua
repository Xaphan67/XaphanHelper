local Elevator = {}

Elevator.name = "XaphanHelper/Elevator"
Elevator.depth = 8999
Elevator.fieldOrder = {"x", "y", "sprite", "canTalk", "flag", "oneUse", "timer", "endPosition", "toChapter", "destinationRoom", "spawnRoomX", "spawnRoomY", "endAreaEntrance"}
Elevator.ignoredFields = {
    "_name", "_id", "width", "height"
}
Elevator.canResize = {false, false}
Elevator.fieldInformation = {
    endPosition = {
        fieldType = "integer",
    },
    toChapter = {
        fieldType = "integer",
    },
    spawnRoomX = {
        fieldType = "integer",
    },
    spawnRoomY = {
        fieldType = "integer",
    }
}
Elevator.placements = {
    name = "Elevator",
    data = {
        width = 32,
        height = 8,
        sprite = "objects/XaphanHelper/Elevator",
        endPosition = 0,
        canTalk = false,
        timer = 1.00,
        endAreaEntrance = false,
        toChapter = 0,
        destinationRoom = "",
        spawnRoomX = 0,
        spawnRoomY = 0,
        oneUse = false,
        flag = ""
    }
}

function Elevator.texture(room, entity)
    local sprite = entity.sprite or "objects/XaphanHelper/Elevator"
    texture = sprite .. "/elevator00"

    return texture
end

Elevator.offset = {-16, -3}

return Elevator