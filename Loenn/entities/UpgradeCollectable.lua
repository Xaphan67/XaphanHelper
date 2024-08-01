local utils = require("utils")

local UpgradeCollectable = {}

UpgradeCollectable.name = "XaphanHelper/UpgradeCollectable"
UpgradeCollectable.depth = 0
UpgradeCollectable.fieldOrder = {
    "x", "y", "upgrade", "customName", "customSprite", "collectSound", "newMusic", "nameColor", "descColor", "particleColor", "mapShardIndex"
}
UpgradeCollectable.fieldInformation = {
    upgrade = {
        options = {"Map", "MapShard", "Binoculars", "Bombs", "ClimbingKit", "DashBoots", "DroneTeleport", "EtherealDash", "GoldenFeather", "GravityJacket", "HoverJet", "IceBeam", "JumpBoost", "LightningDash", "LongBeam", "MegaBombs", "MissilesModule", "PortableStation", "PowerGrip", "PulseRadar", "RemoteDrone", "ScrewAttack", "SpaceJump", "SpiderMagnet", "SuperMissilesModule", "VariaJacket", "WaveBeam"},
        editable = false
    },
    nameColor = {
        fieldType = "color"
    },
    descColor = {
        fieldType = "color"
    },
    particleColor = {
        fieldType = "color"
    },
    mapShardIndex = {
        fieldType = "integer"
    }
}
UpgradeCollectable.placements = {
    name = "UpgradeCollectable",
    data = {
        collectSound = "event:/game/07_summit/gem_get",
        newMusic = "",
        upgrade = "Map",
        nameColor = "FFFFFF",
        descColor = "FFFFFF",
        particleColor = "FFFFFF",
        customName = "",
        customSprite = "",
        mapShardIndex = 0
    }
}

function UpgradeCollectable.texture(room, entity)
    local sprite = entity.upgrade
    local customSprite = entity.customSprite

    if sprite == "MapShard" then
        sprite = "map"
    end
    if customSprite == "" then
        customSprite = "collectables/XaphanHelper/UpgradeCollectable"
    end

    return customSprite .. "/" .. sprite .. "00"
end

function UpgradeCollectable.selection(room, entity)
    return utils.rectangle(entity.x, entity.y , 16, 16)
end

UpgradeCollectable.offset = {-8, -8}

return UpgradeCollectable