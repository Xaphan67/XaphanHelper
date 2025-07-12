local UpgradeTrigger = {}

UpgradeTrigger.name = "XaphanHelper/UpgradeTrigger"
UpgradeTrigger.fieldInformation = {
    upgrade = {
        options = {"All", "Binoculars", "Bombs", "ClimbingKit", "DashBoots", "DroneTeleport", "EtherealDash", "GoldenFeather", "GravityJacket", "HoverBoots", "IceBeam", "JumpBoost", "LightningDash", "LongBeam", "MegaBombs", "MissilesModule", "PortableStation", "PowerGrip", "PulseRadar", "RemoteDrone", "ScrewAttack", "SpaceJump", "SpiderMagnet", "SuperMissilesModule", "VariaJacket", "WaveBeam"},
        editable = false
    }
}
UpgradeTrigger.placements = {
    name = "UpgradeTrigger",
    data = {
        upgrade = "All",
        disable = false,
        onlyOnce = false
    }
}

return UpgradeTrigger