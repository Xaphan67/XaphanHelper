local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local CustomMoveBlock = {}

CustomMoveBlock.name = "XaphanHelper/CustomMoveBlock"
CustomMoveBlock.depth = 8995
CustomMoveBlock.fieldOrder = {
    "x", "y", "width", "height", "directory", "direction", "idleColor", "moveColor", "breakColor", "particlesColor", "steerSides", "speed", "acceleration", "respawnTime", "buttonPressedOffset", "magneticCeilingOffset", "addBorder", "glow", "buttonIgnoreColors", "oneUse"
}
CustomMoveBlock.minimumSize = {16, 16}
CustomMoveBlock.fieldInformation = {
    direction = {
        options = {"Up", "Down", "Left", "Right"},
        editable = false
    },
    steerSides = {
        options = {"None", "TopOnly", "BottomOnly", "LeftOnly", "RightOnly", "TopAndBottom", "LeftAndRight"},
        editable = false
    },
    idleColor = {
        fieldType = "color"
    },
    moveColor = {
        fieldType = "color"
    },
    breakColor = {
        fieldType = "color"
    },
    particlesColor = {
        fieldType = "color"
    },
    speed = {
        fieldType = "integer"
    },
    acceleration = {
        fieldType = "integer"
    },
    respawnTime = {
        minimumValue = 0
    },
    buttonPressedOffset = {
        fieldType = "integer",
        minimumValue = 1
    },
    magneticCeilingOffset = {
        fieldType = "integer",
        minimumValue = 0
    }
}
CustomMoveBlock.placements = {}

CustomMoveBlock.placements = {
    name = "CustomMoveBlock",
    data = {
        width = 16,
        height = 16,
        directory = "objects/moveBlock",
        direction = "Right",
        steerSides = "None",
        idleColor = "474070",
        moveColor = "30B335",
        breakColor = "cc2541",
        particlesColor = "000000",
        speed = 60,
        acceleration = 300,
        respawnTime = 3.0,
        oneUse = false,
        addBorder = true,
        glow = false,
        buttonIgnoreColors = false,
        buttonPressedOffset = 2,
        magneticCeilingOffset = 2
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local buttonNinePatchOptions = {
    mode = "fill",
    border = 0
}

function GetFrameTexture(directory)
    return directory .. "/base"
end

function GetButtonTexture(directory)
    return directory .. "/button"
end

function GetArrowTexture(directory, direction)
    local arrowTextures = {
        up = "arrow02",
        left = "arrow04",
        right = "arrow00",
        down = "arrow06"
    }
    return directory .. "/" .. arrowTextures[direction]
end

function GetBlockTexture(directory, direction, steerSides)
    local blockTextures = {
        up = "base_v",
        left = "base_h",
        right = "base_h",
        down = "base_v"
    }
    if direction == "left" or direction == "right" then
        local variant = nil
        if steerSides == "TopOnly" then
            variant = ""
        elseif steerSides == "BottomOnly" then
            variant = "b"
        elseif steerSides == "TopAndBottom" then
            variant = "tb"
        end
        if variant ~= nil then
            return directory .. "/" .. blockTextures[direction] .. variant
        end
    else
        local variant = nil
        if steerSides == "LeftOnly" then
            variant = "l"
        elseif steerSides == "RightOnly" then
            variant = "r"
        elseif steerSides == "LeftAndRight" then
            variant = ""
        end
        if variant ~= nil then
            return directory .. "/" .. blockTextures[direction] .. variant
        end
    end
    return GetFrameTexture(directory)
end


-- How far the button peeks out of the block and offset to keep it in the "socket"
function GetButtonPopout(directory, direction)
    if directory == "objects/moveBlock" then
        return 3
    else
        return 4
    end
end

function GetButtonOffset(directory)
    if directory == "objects/moveBlock" then
        return 3
    else
        return 4
    end
end

function CustomMoveBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
    local directory = entity.directory or "objects/moveBlock"
    if directory == "" then
        directory = "objects/moveBlock"
    end
    local idleColor = entity.idleColor or "474070"
    local buttonIgnoreColors = entity.buttonIgnoreColors or false
    local addBorder = entity.addBorder

    local direction = string.lower(entity.direction or "up")
    local steerSides = entity.steerSides
    local buttonsOnSide = direction == "up" or direction == "down"

    local blockTexture = GetFrameTexture(directory)
    local arrowTexture = GetArrowTexture(directory, direction)

    if steerSides ~= "None" then
        blockTexture = GetBlockTexture(directory, direction, steerSides)
    end

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local highlightRectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, idleColor)

    local arrowSprite = drawableSprite.fromTexture(arrowTexture, entity)
    local arrowSpriteWidth, arrowSpriteHeight = arrowSprite.meta.width, arrowSprite.meta.height
    local arrowX, arrowY = x + math.floor((width - arrowSpriteWidth) / 2), y + math.floor((height - arrowSpriteHeight) / 2)
    local arrowRectangle = drawableRectangle.fromRectangle("fill", arrowX + 1, arrowY + 1, arrowSpriteWidth - 2, arrowSpriteHeight - 2, idleColor)

    arrowSprite:addPosition(math.floor(width / 2), math.floor(height / 2))

    local sprites = {}

    if addBorder then
        local border = drawableRectangle.fromRectangle("fill", x - 1, y - 1, width + 2, height + 2, "000000")
        table.insert(sprites, border:getDrawableSprite())
    end

    if steerSides ~= "None" then
        if buttonsOnSide then
            for oy = 4, height - 4, 8 do
                if steerSides:find("Left", 1, true) then
                    local leftQuadX = (oy == 4 and 16 or (oy == height - 4 and 0 or 8))               
                    local spriteLeft = drawableSprite.fromTexture(GetButtonTexture(directory), entity)
                    spriteLeft.rotation = -math.pi / 2
                    spriteLeft:addPosition(-GetButtonPopout(directory, direction), oy + GetButtonOffset(directory))
                    spriteLeft:useRelativeQuad(leftQuadX, 0, 8, 8)
                    if not buttonIgnoreColors then
                        spriteLeft:setColor(idleColor)
                    end
                    table.insert(sprites, spriteLeft)
                end

                if steerSides:find("Right", 1, true) then
                    local rightQuadX = (oy == 4 and 0 or (oy == height - 4 and 16 or 8))
                    local spriteRight = drawableSprite.fromTexture(GetButtonTexture(directory), entity)             
                    spriteRight.rotation = math.pi / 2
                    spriteRight:addPosition(width + GetButtonPopout(directory, direction), oy - GetButtonOffset(directory))
                    spriteRight:useRelativeQuad(rightQuadX, 0, 8, 8)
                    if not buttonIgnoreColors then
                        spriteRight:setColor(idleColor)
                    end
                    table.insert(sprites, spriteRight)
                end
            end
        else
            for ox = 4, width - 4, 8 do
                if steerSides:find("Top", 1, true) then
                    local quadX = (ox == 4 and 0 or (ox == width - 4 and 16 or 8))
                    local spritetop = drawableSprite.fromTexture(GetButtonTexture(directory), entity)
                    spritetop:addPosition(ox - GetButtonOffset(directory), -GetButtonPopout(directory, direction))
                    spritetop:useRelativeQuad(quadX, 0, 8, 8)
                    if not buttonIgnoreColors then
                        spritetop:setColor(idleColor)
                    end 
                    table.insert(sprites, spritetop)
                end
                if steerSides:find("Bottom", 1, true) then
                    local quadX = (ox == 4 and 0 or (ox == width - 4 and 16 or 8))
                    local spritebottom = drawableSprite.fromTexture(GetButtonTexture(directory), entity)
                    spritebottom:setScale(1, -1)
                    spritebottom:addPosition(ox - GetButtonOffset(directory), -GetButtonPopout(directory, direction) + height + 8)
                    spritebottom:useRelativeQuad(quadX, 0, 8, 8)
                    if not buttonIgnoreColors then
                        spritebottom:setColor(idleColor)
                    end 
                    table.insert(sprites, spritebottom)
                end
            end
        end
    end

    table.insert(sprites, highlightRectangle:getDrawableSprite())

    for _, sprite in ipairs(ninePatch:getDrawableSprite()) do  
        table.insert(sprites, sprite)
    end

    table.insert(sprites, arrowRectangle:getDrawableSprite())
    table.insert(sprites, arrowSprite)

    return sprites
end

return CustomMoveBlock