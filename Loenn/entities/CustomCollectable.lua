local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local CustomCollectable = {}

CustomCollectable.name = "XaphanHelper/CustomCollectable"
CustomCollectable.depth = 0
CustomCollectable.fieldOrder = {
    "x", "y", "sprite", "particlesColor", "lightColor", "flag", "collectSound", "loopSound", "staticTime", "newMusic", "mapIcon", "wiggle", "collectGoldenStrawberry", "mustDash", "canRespawn", "loopBurst", "changeMusic", "endChapter", "registerInSaveData", "ignoreGolden"
}
CustomCollectable.fieldInformation = {
    particlesColor = {
        fieldType = "color",
        allowEmpty = true
    },
    lightColor = {
        fieldType = "color",
        allowEmpty = true
    }
}
CustomCollectable.placements = {
    name = "CustomCollectable",
    data = {
        sprite = "collectables/XaphanHelper/CustomCollectable/collectable",
        collectSound = "event:/game/07_summit/gem_get",
        changeMusic = false,
        newMusic = "",
        flag = "",
        mapIcon = "",
        mustDash = false,
        collectGoldenStrawberry = false,
        endChapter = false,
        registerInSaveData = false,
        ignoreGolden = false,
        canRespawn = false,
        loopSound = "",
        loopBurst = false,
        staticTime = 0.8,
        particlesColor = "",
        lightColor = "",
        wiggle = false
    }
}

function CustomCollectable.sprite(room, entity)
    local texture = entity.sprite or "collectables/XaphanHelper/CustomCollectable/collectable"
    local sprite = drawableSprite.fromTexture(texture .. "00", entity)
    sprite:addPosition(8, 8)

    return sprite
end

function CustomCollectable.selection(room, entity)
    return utils.rectangle(entity.x, entity.y , 16, 16)
end

return CustomCollectable