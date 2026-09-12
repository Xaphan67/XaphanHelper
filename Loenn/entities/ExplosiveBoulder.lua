local ExplosiveBoulder = {}

ExplosiveBoulder.name = "XaphanHelper/ExplosiveBoulder"
ExplosiveBoulder.depth = -8499
ExplosiveBoulder.fieldOrder = {
    "x", "y", "directory", "bounceForce", "DashCooldown", "gravity", "RefillJump"
}
ExplosiveBoulder.placements = {
    name = "ExplosiveBoulder",
    data = {
        directory = "objects/XaphanHelper/ExplosiveBoulder",
        gravity = false,
        bounceForce = 280,
        RefillJump = false,
        DashCooldown = 0.2
    }
}

function ExplosiveBoulder.texture(room, entity)
    local directory = entity.directory or "objects/XaphanHelper/ExplosiveBoulder"
    if directory == "" then
        directory = "objects/XaphanHelper/ExplosiveBoulder"
    end
    return directory .. "/boulder00"
end

return ExplosiveBoulder