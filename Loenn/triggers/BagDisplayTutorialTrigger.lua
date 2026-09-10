local BagDisplayTutorialTrigger = {}

BagDisplayTutorialTrigger.name = "XaphanHelper/BagDisplayTutorialTrigger"
BagDisplayTutorialTrigger.fieldInformation = {
    slot = {
        options = {"Bag", "Misc"},
        editable = false
    }
}
BagDisplayTutorialTrigger.placements = {
    name = "BagDisplayTutorialTrigger",
    data = {
        slot = "Bag",
        keepUIOpenOnLeave = false,
        onlyOnce =false
    }
}

return BagDisplayTutorialTrigger