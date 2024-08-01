local TeleportToChapterTrigger = {}

TeleportToChapterTrigger.name = "XaphanHelper/TeleportToChapterTrigger"
TeleportToChapterTrigger.fieldOrder = {
    "x", "y", "width", "height", "destinationChapter", "destinationRoom", "spawnRoomX", "spawnRoomY", "wipeType", "wipeDuration", "canInteract", "registerCurrentChapterAsCompelete"
}
TeleportToChapterTrigger.fieldInformation = {
    wipeType = {
        options = {"Spotlight", "Curtain", "Mountain", "Dream", "Starfield", "Wind", "Drop", "Fall", "KeyDoor", "Angled", "Heart", "Fade"},
        editable = false
    }
}
TeleportToChapterTrigger.placements = {
    name = "TeleportToChapterTrigger",
    data = {
        spawnRoomX = 0,
        spawnRoomY = 0,
        destinationRoom = "",
        registerCurrentChapterAsCompelete= false,
        wipeType = "Fade",
        wipeDuration = 1.35,
        canInteract = false,
        destinationChapter = ""
    }
}

return TeleportToChapterTrigger