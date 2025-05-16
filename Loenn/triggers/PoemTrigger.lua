local PoemTrigger = {}

PoemTrigger.name = "XaphanHelper/PoemTrigger"
PoemTrigger.fieldOrder = {
    "x", "y", "height", "width", "flag", "triggerSound", "newMusic", "poemName", "poemColor", "poemParticlesColor", "poemSprite", "poemSpriteAnimationSpeed", "poemSpriteAnimationPause", "onlyOnce", "changeMusic", "endChapter", "registerInSaveData"
}
PoemTrigger.fieldInformation = {
    poemColor = {
        fieldType = "color",
        allowEmpty = true
    },
    poemParticlesColor = {
        fieldType = "color",
        allowEmpty = true
    },
    poemSpriteAnimationPause = {
        fieldType = "integer",
        minimumValue = 0
    }
}
PoemTrigger.placements = {
    name = "PoemTrigger",
    data = {
        triggerSound = "event:/game/general/crystalheart_blue_get",
        changeMusic = false,
        newMusic = "",
        flag = "",
        endChapter = false,
        registerInSaveData = false,
        onlyOnce = false,
        poemName = "",
        poemColor = "FFFFFF",
        poemParticlesColor = "FFFFFF",
        poemSprite = "collectables/heartgem/0/spin",
        poemSpriteAnimationSpeed = 0.1,
        poemSpriteAnimationPause = 10
    }
}

return PoemTrigger