local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local enums = require("consts.celeste_enums")
local mods = require("mods")

--#region Texture Cache
local cache = {}

---@return table baseTextures, table slopeTextures
local function getTextures(entity)
    local path = string.format("%s/%s", entity.customDirectory or "objects/XaphanHelper/Slope", entity.texture or "cement")

    if not cache[path] then
        local baseSprite = drawableSprite.fromTexture(path, {})

        local baseTextures = {}
        for i = 0, 6, 1 do
            baseTextures[i] = {}
            for j = 0, 15, 1 do
                local spr = drawableSprite.fromMeta(baseSprite.meta, entity)
                spr:useRelativeQuad(i * 8, j * 8, 8, 8)
                baseTextures[i][j] = spr
            end
        end
        local slopeTexturesUpsideDown = {}
        local slopeTextures = {}

        for i = 0, 32, 1 do
            local spr = drawableSprite.fromMeta(baseSprite.meta, entity)
            spr:useRelativeQuad(i, 8, 1, 8)
            slopeTexturesUpsideDown[i] = spr
        end

        for i = 0, 32, 1 do
            local spr = drawableSprite.fromMeta(baseSprite.meta, entity)
            spr:useRelativeQuad(i, 0, 1, 8)
            slopeTextures[i] = spr
        end

        cache[path] = {
            baseTextures = baseTextures,
            slopeTextures = slopeTextures,
            slopeTexturesUpsideDown = slopeTexturesUpsideDown,
        }

    end

    return cache[path].baseTextures, (entity.upsideDown or false) and cache[path].slopeTexturesUpsideDown or cache[path].slopeTextures
end
--#endregion

local Slope = {}

Slope.name = "XaphanHelper/Slope"
Slope.depth = -10001
Slope.fieldOrder = {
    "x", "y", "side", "flag", "customDirectory", "flagCustomDirectory", "texture", "flagTexture", "soundIndex", "slopeHeight", "tilesTop", "tilesBottom", "renderMethod", "gentle", "upsideDown", "canJumpThrough", "canSlide", "forceSlide", "preventRefillOnSliding", "affectPlayerSpeed", "stickyDash", "noRender", "rainbow"
}
Slope.fieldInformation = {
    side = {
        options = {"Left", "Right"},
        editable = false
    },
    soundIndex = {
        options = enums.tileset_sound_ids,
        editable = false
    },
    slopeHeight = {
        fieldType = "integer",
    },
    tilesTop = {
        options = {"Horizontal", "Horizontal Corner", "Vertical", "Edge", "Edge Corner", "Small Edge", "Small Edge Corner"},
        editable = false
    },
    tilesBottom = {
        options = {"Horizontal", "Vertical", "Vertical Corner", "Edge", "Edge Corner", "Small Edge", "Small Edge Corner"},
        editable = false
    },
    texture = {
        options = {"cement", "cliffside", "cliffsideAlt", "core", "deadgrass", "dirt", "girder", "grass", "lostlevels", "poolEdges", "reflection", "reflectionAlt", "rock", "scifi", "snow", "starJump", "stone", "summit", "summitNoSnow", "templeA", "templeB", "tower", "wood", "woodStoneEdges"}
    },
    flagTexture = {
        options = {"cement", "cliffside", "cliffsideAlt", "core", "deadgrass", "dirt", "girder", "grass", "lostlevels", "poolEdges", "reflection", "reflectionAlt", "rock", "scifi", "snow", "starJump", "stone", "summit", "summitNoSnow", "templeA", "templeB", "tower", "wood", "woodStoneEdges"}
    },
    renderMethod = {
        options = {"Type A", "Type B"},
        editable = false
    }
}
Slope.placements = {
    name = "Slope",
    data = {
        side = "Left",
        gentle = false,
        soundIndex = 8,
        slopeHeight = 1,
        tilesTop = "Horizontal",
        tilesBottom = "Horizontal",
        customDirectory = "objects/XaphanHelper/Slope",
        flagCustomDirectory = "objects/XaphanHelper/Slope",
        texture = "cement",
        flagTexture = "cement",
        flag = "",
        canSlide = false,
        forceSlide = false,
        upsideDown = false,
        noRender = false,
        stickyDash  = false,
        rainbow = false,
        canJumpThrough = false,
        affectPlayerSpeed = true,
        renderMethod = "Type A",
        preventRefillOnSliding = false
    }
}

function InlineCondition(condition, t, f)
    if condition then
        return t
    else
        return f
    end
end

--[UpsideDown][Side][TilesTop]
local spriteDefsTop = {
    [false] = {
        Left = {
            Horizontal = {
                { iVariation = true, x = -8, y = 0 },
                { iVariation = true, x = 0, y = 0, },
                { i = 5, jVariation = true, x = -8, y = 8, },
                { i = 5, jVariation = true, x = 0, y = 8, },
            },
            ["Horizontal Corner"] = {
                { i = 4, j = 1, x = -8, y = 0 },
                { iVariation = true, j = 0, x = 0, y = 0 },
                { i = 5, jVariation = true, x = -8, y = 8 },
                { i = 5, jVariation = true, x = 0, y = 8 },
            },
            Vertical = {
                { i = 4, j = 1, x = 0, y = 0 },
                { i = 5, jVariation = true, x = 0, y = 8 },
            },
            Edge = {
                { iVariation = true, j = 11, x = -8, y = 0 },
                { iVariation = true, j = 0, x = 0, y = 0 },
                { iVariation = true, j = 2, x = -8, y = 8 },
                { i = 5, jVariation = true, x = 0, y = 8 },
            },
            ["Edge Corner"] = {
                { iVariation = true, j = 11, x = -8, y = 0 },
                { iVariation = true, j = 0, x = 0, y = 0 },
                { i = 4, j = 3, x = -8, y = 8 },
                { i = 5, jVariation = true, x = 0, y = 8 },
            },
            ["Small Edge"] = {
                { iVariation = true, j = 11, x = 0, y = 0 },
                { iVariation = true, j = 2, x = 0, y = 8 },
            },
            ["Small Edge Corner"] = {
                { iVariation = true, j = 11, x = 0, y = 0 },
                { i = 4, j = 3, x = 0, y = 8 },
            }
        },
        Right = {
            Horizontal = {
                { iVariation = true, j = 0, x = 24, y = 0 },
                { iVariation = true, j = 0, x = 16, y = 0 },
                { i = 5, jVariationInner = true, x = 24, y = 8 },
                { i = 5, jVariationInner = true, x = 16, y = 8 },
            },
            ["Horizontal Corner"] = {
                { i = 4, j = 3, x = 24, y = 0 },
                { iVariation = true, j = 0, x = 16, y = 0 },
                { i = 5, jVariationInner = true, x = 24, y = 8 },
                { i = 5, jVariationInner = true, x = 16, y = 8 },
            },
            Vertical = {
                { i = 4, j = 3, x = 16, y = 0 },
                { i = 5, jVariationInner = true, x = 16, y = 8 },
            },
            Edge = {
                { iVariation = true, j = 12, x = 24, y = 0 },
                { iVariation = true, j = 0, x = 16, y = 0 },
                { iVariation = true, j = 3, x = 24, y = 8 },
                { i = 5, jVariationInner = true, x = 16, y = 8 },
            },
            ["Edge Corner"] = {
                { iVariation = true, j = 12, x = 24, y = 0 },
                { iVariation = true, j = 0, x = 16, y = 0 },
                { i = 4, j = 1, x = 24, y = 8 },
                { i = 5, jVariationInner = true, x = 16, y = 8 },
            },
            ["Small Edge"] = {
                { iVariation = true, j = 12, x = 16, y = 0 },
                { iVariation = true, j = 3, x = 16, y = 8 },
            },
            ["Small Edge Corner"] = {
                { iVariation = true, j = 12, x = 16, y = 0 },
                { i = 4, j = 1, x = 16, y = 8 },
            }
        }
    },
    [true] = {
        Left = {
            Horizontal = {
                { iVariation = true, j = 1, x = -8, y = 8 },
                { iVariation = true, j = 1, x = 0, y = 8 },
                { i = 5, jVariationInner = true, x = -8, y = 0 },
                { i = 5, jVariationInner = true, x = 0, y = 0 },
            },
            ["Horizontal Corner"] = {
                { i = 4, j = 0, x = -8, y = 8 },
                { iVariation = true, j = 1, x = 0, y = 8 },
                { i = 5, jVariationInner = true, x = -8, y = 0 },
                { i = 5, jVariationInner = true, x = 0, y = 0 },
            },
            Vertical = {
                { i = 4, j = 0, x = 0, y = 8 },
                { i = 5, jVariationInner = true, x = 0, y = 0 },
            },
            Edge = {
                { iVariation = true, j = 13, x = -8, y = 8 },
                { iVariation = true, j = 1, x = 0, y = 8 },
                { iVariation = true, j = 2, x = -8, y = 0 },
                { i = 5, jVariationInner = true, x = 0, y = 0 },
            },
            ["Edge Corner"] = {
                { iVariation = true, j = 13, x = -8, y = 8 },
                { iVariation = true, j = 1, x = 0, y = 8 },
                { i = 4, j = 2, x = -8, y = 0 },
                { i = 5, jVariationInner = true, x = 0, y = 0 },
            },
            ["Small Edge"] = {
                { iVariation = true, j = 13, x = 0, y = 8 },
                { iVariation = true, j = 2, x = 0, y = 0 },
            },
            ["Small Edge Corner"] = {
                { iVariation = true, j = 13, x = 0, y = 8 },
                { i = 4, j = 2, x = 0, y = 0 },
            }
        },
        Right = {
            Horizontal = {
                { iVariation = true, j = 1, x = 24, y = 8 },
                { iVariation = true, j = 1, x = 16, y = 8 },
                { i = 5, jVariationInner = true, x = 24, y = 0 },
                { i = 5, jVariationInner = true, x = 16, y = 0 },
            },
            ["Horizontal Corner"] = {
                { i = 4, j = 2, x = 24, y = 8 },
                { iVariation = true, j = 1, x = 16, y = 8 },
                { i = 5, jVariationInner = true, x = 24, y = 0 },
                { i = 5, jVariationInner = true, x = 16, y = 0 },
            },
            Vertical = {
                { i = 4, j = 2, x = 16, y = 8 },
                { i = 5, jVariationInner = true, x = 16, y = 0 },
            },
            Edge = {
                { iVariation = true, j = 14, x = 24, y = 8 },
                { iVariation = true, j = 1, x = 16, y = 8 },
                { iVariation = true, j = 3, x = 24, y = 0 },
                { i = 5, jVariationInner = true, x = 16, y = 0 },
            },
            ["Edge Corner"] = {
                { iVariation = true, j = 14, x = 24, y = 8 },
                { iVariation = true, j = 1, x = 16, y = 8 },
                { i = 4, j = 0, x = 24, y = 0 },
                { i = 5, jVariationInner = true, x = 16, y = 0 },
            },
            ["Small Edge"] = {
                { iVariation = true, j = 14, x = 16, y = 8 },
                { iVariation = true, j = 3, x = 16, y = 0 },
            },
            ["Small Edge Corner"] = {
                { iVariation = true, j = 14, x = 16, y = 8 },
                { i = 4, j = 0, x = 16, y = 0 },
            }
        },
    }
}

--[UpsideDown][Side][gentle][TilesBot]
local spriteDefsBot = {
    [false] = {
        Left = {
            [true] = {
                Vertical = {
                    { i = 5, jVariation = true, x = -24, y = 0 },
                    { i = 5, jVariation = true, x = -16, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariation = true, x = -24, y = 0 },
                    { i = 5, jVariation = true, x = -16, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 5, jVariation = true, x = - 8, y = 8 },
                    { i = 4, j = 1, x = 0, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { iVariation = true, j = 1, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -24, y = 0 },
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 4, j = 0, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -24, y = 0 },
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { i = 4, j = 0, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { i = 5, jVariationInner = true, x = -8, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { i = 5, jVariationInner = true, x = -8, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 5, jVariationInner = true, x = -8, y = 8 },
                    { i = 4, j = 1, x = 0, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { iVariation = true, j = 1, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 4, j = 0, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { i = 4, j = 0, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                }
            }
        },
        Right = {
            [true] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 40, y = 0 },
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 40, y = 0 },
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { i = 4, j = 3, x = 16, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { iVariation = true, j = 1, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 40, y = 0 },
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 4, j = 2, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 40, y = 0 },
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { i = 4, j = 2, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { i = 4, j = 3, x = 16, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { iVariation = true, j = 1, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 4, j = 2, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { i = 4, j = 2, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                }
            }
        },
    },
    [true] = {
        Left = {
            [true] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = -24, y = 8 },
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { iVariation = true, j = 3, x = 0, y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = -24, y = 8 },
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { iVariation = true, j = 3, x = 0, y = 8 },
                    { i = 5, jVariationInner = true, x = -8, y = 0 },
                    { i = 4, j = 0, x = 0, y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = 0, y = 8 },
                    { iVariation = true, j = 0, x = -8, y = 0 },
                    { iVariation = true, j = 12, x = 0, y = 0 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -24, y = 8 },
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { iVariation = true, j = 3, x = 0, y = 8 },
                    { i = 4, j = 1, x = -8, y = 0 },
                    { iVariation = true, j = 12, x = 0, y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = -8, y = 8 },
                    { iVariation = true, j = 12, x = 0, y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -24, y = 8 },
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { i = 4, j = 1, x = -8, y = 8 },
                    { iVariation = true, j = 12, x = 0, y = 8 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = (0) - 16, y = 8 },
                    { i = 5, jVariationInner = true, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 3, x = (0), y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = (0) - 16, y = 8 },
                    { i = 5, jVariationInner = true, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 3, x = (0), y = 8 },
                    { i = 5, jVariationInner = true, x = (0) - 8, y = 0 },
                    { i = 4, j = 0, x = (0), y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = (0), y = 8 },
                    { iVariation = true, j = 0, x = (0) - 8, y = 0 },
                    { iVariation = true, j = 12, x = (0), y = 0 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = (0) - 16, y = 8 },
                    { iVariation = true, j = 3, x = (0), y = 8 },
                    { i = 4, j = 1, x = (0) - 8, y = 0 },
                    { iVariation = true, j = 12, x = (0), y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 12, x = (0), y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = (0) - 16, y = 8 },
                    { i = 4, j = 1, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 12, x = (0), y = 8 },
                }
            }
        },
        Right = {
            [true] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 40, y = 8 },
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 40, y = 8 },
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { i = 4, j = 2, x = 16, y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { iVariation = true, j = 0, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 40, y = 8 },
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 4, j = 3, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 40, y = 8 },
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { i = 4, j = 3, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { i = 4, j = 2, x = 16, y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { iVariation = true, j = 0, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 4, j = 3, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { i = 4, j = 3, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                }
            }
        },
    }
}

--[UpsideDown][Side][gentle][TilesBot]
local spriteDefsBotOneHeight = {
    [false] = {
        Left = {
            [true] = {
                Vertical = {
                    { i = 5, jVariation = true, x = -16, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariation = true, x = -16, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 5, jVariation = true, x = - 8, y = 8 },
                    { i = 4, j = 1, x = 0, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { iVariation = true, j = 1, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 4, j = 0, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 0 },
                    { i = 4, j = 0, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = -8, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = -8, y = 0 },
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 5, jVariationInner = true, x = -8, y = 8 },
                    { i = 4, j = 1, x = 0, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { iVariation = true, j = 1, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Edge Corner"] = {
                    { iVariation = true, j = 3, x = 0, y = 0 },
                    { i = 4, j = 0, x = -8, y = 8 },
                    { iVariation = true, j = 14, x = 0, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 4, j = 0, x = -8, y = 0 },
                    { iVariation = true, j = 14, x = 0, y = 0 },
                }
            }
        },
        Right = {
            [true] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { i = 4, j = 3, x = 16, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { iVariation = true, j = 1, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 4, j = 2, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 0 },
                    { i = 4, j = 2, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { i = 4, j = 3, x = 16, y = 8 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { iVariation = true, j = 1, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Edge Corner"] = {
                    { iVariation = true, j = 2, x = 16, y = 0 },
                    { i = 4, j = 2, x = 24, y = 8 },
                    { iVariation = true, j = 13, x = 16, y = 8 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 1, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                },
                ["Small Edge Corner"] = {
                    { i = 4, j = 2, x = 24, y = 0 },
                    { iVariation = true, j = 13, x = 16, y = 0 },
                }
            }
        },
    },
    [true] = {
        Left = {
            [true] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { iVariation = true, j = 3, x = 0, y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { iVariation = true, j = 3, x = 0, y = 8 },
                    { i = 5, jVariationInner = true, x = -8, y = 0 },
                    { i = 4, j = 0, x = 0, y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = 0, y = 8 },
                    { iVariation = true, j = 0, x = -8, y = 0 },
                    { iVariation = true, j = 12, x = 0, y = 0 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { iVariation = true, j = 3, x = 0, y = 8 },
                    { i = 4, j = 1, x = -8, y = 0 },
                    { iVariation = true, j = 12, x = 0, y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = -8, y = 8 },
                    { iVariation = true, j = 12, x = 0, y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = -16, y = 8 },
                    { i = 4, j = 1, x = -8, y = 8 },
                    { iVariation = true, j = 12, x = 0, y = 8 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 3, x = (0), y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 3, x = (0), y = 8 },
                    { i = 5, jVariationInner = true, x = (0) - 8, y = 0 },
                    { i = 4, j = 0, x = (0), y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 3, x = (0), y = 8 },
                    { iVariation = true, j = 0, x = (0) - 8, y = 0 },
                    { iVariation = true, j = 12, x = (0), y = 0 },
                },
                ["Edge Corner"] = {
                    { iVariation = true, j = 3, x = (0), y = 8 },
                    { i = 4, j = 1, x = (0) - 8, y = 0 },
                    { iVariation = true, j = 12, x = (0), y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 12, x = (0), y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 4, j = 1, x = (0) - 8, y = 8 },
                    { iVariation = true, j = 12, x = (0), y = 8 },
                }
            }
        },
        Right = {
            [true] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { i = 4, j = 2, x = 16, y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { iVariation = true, j = 0, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 4, j = 3, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 5, jVariationInner = true, x = 32, y = 8 },
                    { i = 4, j = 3, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                }
            },
            [false] = {
                Vertical = {
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                },
                ["Vertical Corner"] = {
                    { i = 5, jVariationInner = true, x = 24, y = 8 },
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 5, jVariationInner = true, x = 24, y = 0 },
                    { i = 4, j = 2, x = 16, y = 0 },
                },
                Edge = {
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { iVariation = true, j = 0, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Edge Corner"] = {
                    { iVariation = true, j = 2, x = 16, y = 8 },
                    { i = 4, j = 3, x = 24, y = 0 },
                    { iVariation = true, j = 11, x = 16, y = 0 },
                },
                ["Small Edge"] = {
                    { iVariation = true, j = 0, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                },
                ["Small Edge Corner"] = {
                    { i = 4, j = 3, x = 24, y = 8 },
                    { iVariation = true, j = 11, x = 16, y = 8 },
                }
            }
        },
    }
}

local function getTile(baseTextures, entity, tileDef)
    local x, y = tileDef.x or 0, tileDef.y or 0
    local i, j = tileDef.i or 0, tileDef.j or 0

    if tileDef.iVariation then
        utils.setSimpleCoordinateSeed(x, y)
        i = i + math.floor(math.random(0, 4))
    end
    if tileDef.iVariationInner then
        utils.setSimpleCoordinateSeed(x, y)
        i = i + math.floor(math.random(0, 12))
    end

    if tileDef.jVariation then
        utils.setSimpleCoordinateSeed(x, y)
        j = j + math.floor(math.random(0, 4))
    end
    if tileDef.jVariationInner then
        utils.setSimpleCoordinateSeed(x, y)
        j = j + math.floor(math.random(0, 12))
    end

    local baseTexture = baseTextures[i][j]
    local newTexture = drawableSprite.fromMeta(baseTexture.meta, {x = entity.x + x, y = entity.y + y})
    newTexture.quad = baseTexture.quad
    newTexture.offsetX = baseTexture.offsetX
    newTexture.offsetY = baseTexture.offsetY

    return newTexture
end

local function getSlopeTile(slopeTextures, entity, tileDef)
    local ox, oy = tileDef.x or 0, tileDef.y or 0
    local dir, dirY = tileDef.dir or 1, tileDef.dirY or 1
    local j = tileDef.j
    local gentle = tileDef.gentle

    if dir == -1 then
        ox = ox + 7
    end
    if dirY == -1 then
        oy = oy + 7
    end

    local oyDelta = gentle and 0.5 or 1

    local sprites = {}

    for i = 0, 7, 1 do
        local baseTexture = slopeTextures[i]
        local newTexture = drawableSprite.fromMeta(baseTexture.meta, {
            x = entity.x + (ox + (i * dir)) + (j * (gentle and 16 or 8) * dir),
            y = entity.y + math.floor(oy + i * oyDelta * dirY) + (j * 8 * dirY)
        })
        newTexture.quad = baseTexture.quad
        newTexture.offsetX = baseTexture.offsetX
        newTexture.offsetY = baseTexture.offsetY

        table.insert(sprites, newTexture)
    end

    if gentle then
        for i = 0, 7, 1 do
            local baseTexture = slopeTextures[i]
            local newTexture = drawableSprite.fromMeta(baseTexture.meta, {
                x = entity.x + (ox + (i + 8) * dir) + (j * 16 * dir),
                y = entity.y + math.floor(oy + (i + 8) / 2 * dirY) + (j * 8 * dirY)
            })
            newTexture.quad = baseTexture.quad
            newTexture.offsetX = baseTexture.offsetX
            newTexture.offsetY = baseTexture.offsetY

            table.insert(sprites, newTexture)
        end
    end

    return sprites
end

local function getSetting(settingName, default)
    local settings = mods.getModSettings()

    local value = settings[settingName]
    if value == nil then
        value = default
        settings[settingName] = default
    end

    return value
end

local function getGentle(entity)
    return entity.gentle or false
end

local function getSide(entity)
    return entity.side or "Left"
end

local function getSlopeHeight(entity)
    return entity.slopeHeight or 1
end

local function getTilesTop(entity)
    return entity.tilesTop or "Horizontal"
end

local function getTilesBottom(entity)
    return entity.tilesBottom or "Horizontal"
end

local function getUpsideDown(entity)
    return entity.upsideDown or false
end

local function getTileSprites(sprites, entity)
    local gentle = getGentle(entity)
    local side = getSide(entity)
    local slopeHeight = getSlopeHeight(entity)
    local tilesTop = getTilesTop(entity)
    local tilesBottom = getTilesBottom(entity)
    local upsideDown = getUpsideDown(entity)

    local dir = side == "Left" and 1 or -1
    local dirY = upsideDown and -1 or 1
    local function flipX(n)
        return dir == 1 and n or -n + 16
    end
    local function flipY(n)
        return dirY == 1 and n or -n + 8
    end

    local baseTextures, slopeTextures = getTextures(entity)

    --#region Top Tiles
    local tileDefsTop = spriteDefsTop[upsideDown][side][tilesTop]
    if tileDefsTop then
        for _, tileDef in ipairs(tileDefsTop) do
            local newTexture = getTile(baseTextures, entity, tileDef)

            table.insert(sprites, newTexture)
        end
    end
    --#endregion

    --#region Hardcoded Tiles
    if gentle and tilesBottom ~= "Small Edge" and tilesBottom ~= "Small Edge Corner" then
        table.insert(sprites, getTile(baseTextures, entity, {
            i = 5, jVariation = true,
            x = flipX((slopeHeight * 16) - 8), y = flipY(slopeHeight * 8)
        }))
    end

    if tilesBottom == "Horizontal" then
        if gentle then
            if slopeHeight > 1 then
                table.insert(sprites, getTile(baseTextures, entity, {
                    i = 5, jVariation = true,
                    x = flipX((slopeHeight * 16) - 24), y = flipY(slopeHeight * 8)
                }))
                table.insert(sprites, getTile(baseTextures, entity, {
                    i = 5, jVariation = true,
                    x = flipX((slopeHeight * 16) - 16), y = flipY(slopeHeight * 8)
                }))
            end
            table.insert(sprites, getTile(baseTextures, entity, {
                i = 5, jVariation = true,
                x = flipX(slopeHeight * 16), y = flipY(slopeHeight * 8)
            }))
        else
            if slopeHeight > 1 then
                table.insert(sprites, getTile(baseTextures, entity, {
                    i = 5, jVariation = true,
                    x = flipX((slopeHeight * 8) - 16), y = flipY(slopeHeight * 8)
                }))
                table.insert(sprites, getTile(baseTextures, entity, {
                    i = 5, jVariation = true,
                    x = flipX((slopeHeight * 8) - 8), y = flipY(slopeHeight * 8)
                }))
            end
            table.insert(sprites, getTile(baseTextures, entity, {
                i = 5, jVariation = true,
                x = flipX(slopeHeight * 8), y = flipY(slopeHeight * 8)
            }))
        end
    end
    --#endregion

    --#region Slope
    if gentle then
        for i = 1, slopeHeight - 1, 1 do
            if i > 1 then
                table.insert(sprites, getTile(baseTextures, entity, {
                    i = 5, jVariationInner = true,
                    x = flipX(-24 + i * 16), y = flipY(i * 8)
                }))
                table.insert(sprites, getTile(baseTextures, entity, {
                    i = 5, jVariationInner = true,
                    x = flipX(-16 + i * 16), y = flipY(i * 8)
                }))
            end
            table.insert(sprites, getTile(baseTextures, entity, {
                i = 5, jVariationInner = true,
                x = flipX(-8 + i * 16), y = flipY(i * 8)
            }))
            table.insert(sprites, getTile(baseTextures, entity, {
                i = 5, jVariationInner = true,
                x = flipX(i * 16), y = flipY(i * 8)
            }))
        end
    else
        for i = 1, slopeHeight - 1, 1 do
            if i > 1 then
                if (tilesTop ~= "Small Edge") or (i > 2) then
                    table.insert(sprites, getTile(baseTextures, entity, {
                        i = 5, jVariationInner = true,
                        x = flipX(-16 + i * 8), y = flipY(i * 8)
                    }))
                end
                table.insert(sprites, getTile(baseTextures, entity, {
                    i = 5, jVariationInner = true,
                    x = flipX(-8 + i * 8), y = flipY(i * 8)
                }))
            end
            table.insert(sprites, getTile(baseTextures, entity, {
                i = 5, jVariationInner = true,
                x = flipX(i * 8), y = flipY(i * 8)
            }))
        end
    end

    for j = 0, slopeHeight - 1, 1 do
        local slopeSprites = getSlopeTile(slopeTextures, entity, {
            x = 8, y = 0, j = j,
            dir = dir, dirY = dirY,
            gentle = gentle,
        })
        for _, s in ipairs(slopeSprites) do
            table.insert(sprites, s)
        end
    end
    --#endregion

    --#region Bottom Tiles
    local tileDefsBot = InlineCondition(slopeHeight > 1, spriteDefsBot[upsideDown][side][gentle][tilesBottom],spriteDefsBotOneHeight[upsideDown][side][gentle][tilesBottom])
    if tileDefsBot then
        for _, tileDef in ipairs(tileDefsBot) do
            local newTexture = getTile(baseTextures, entity, tileDef)
            newTexture:addPosition((slopeHeight * (gentle and 16 or 8) * dir), (slopeHeight * 8 * dirY))
            table.insert(sprites, newTexture)
        end
    end
    --#endregion
end

local function getOverlaySprites(sprites, entity)
    local gentle = getGentle(entity)
    local side = getSide(entity)
    local type = "slope"
    local slopeHeight = getSlopeHeight(entity)
    local tilesTop = getTilesTop(entity)
    local upsideDown = getUpsideDown(entity)
    local value = 8
    local spriteYAdd = 4

    if gentle then
        type = "gentleSlope"
        value = 16
    end
    if side == "Right" then
        value = -value
    end

    if upsideDown then
        spriteYAdd = 12
    end

    local sprite = nil

    if tilesTop == "Vertical" then
        sprite = drawableSprite.fromTexture("util/XaphanHelper/" .. type .. side.. "Half", entity)
        sprite:addPosition(12, spriteYAdd)
    else
        sprite = drawableSprite.fromTexture("util/XaphanHelper/" .. type .. side, entity)
        sprite:addPosition(12, spriteYAdd)
    end

    if sprite then
        if upsideDown then
            sprite.scaleY = -1
        end
        table.insert(sprites, sprite)
    end

    for i = 1,(slopeHeight - 1) do
        sprite = drawableSprite.fromTexture("util/XaphanHelper/" .. type .. side, entity)
        sprite:addPosition(12 + i * value, spriteYAdd + i * InlineCondition(upsideDown, -8, 8))
        if sprite then
            if upsideDown then
                sprite.scaleY = -1
            end
            table.insert(sprites, sprite)
        end
    end
end

function Slope.sprite(room, entity)
    local sprites = {}
    local noRender = entity.noRender or false

    if noRender ~= true then
        if getSetting("slopes.drawTiles", true) then
            getTileSprites(sprites, entity)
        end
        if getSetting("slopes.drawOverlay", true) then
            getOverlaySprites(sprites, entity)
        end
    else
        getOverlaySprites(sprites, entity)
    end
    return sprites
end

function Slope.selection(room, entity)
    local gentle = entity.gentle or false
    local side = entity.side or "Left"
    local slopeHeight = entity.slopeHeight or 1
    local tilesTop = entity.tilesTop or "Horizontal"
    local tilesBottom = entity.tilesBottom or "Horizontal"
    local upsideDown = entity.upsideDown or false
    local offsetX = 0
    local offsetY = 0
    if side == "Right" then
        if gentle == false then
            offsetX = 8 - 8 * (slopeHeight - 1)
        end
        if gentle == true then
            offsetX = 0 - 16 * (slopeHeight - 1)
        end
    end
    if side == "Left" then
        if tilesTop == "Vertical" then
            offsetX = 8
        end
    end
    local width = 8 + 8 * slopeHeight
    if gentle then
        width = 8 + 16 * slopeHeight
    end
    if upsideDown then
        offsetY = 8 - 8 * (slopeHeight - 1)
        --[[ if tilesBottom ~= "Horizontal" then
            offsetY = offsetY - 8
        end ]]
    end
    return utils.rectangle(entity.x + offsetX, entity.y + offsetY, width + InlineCondition(tilesTop == "Vertical", -8, 0), 8 * slopeHeight --[[ + InlineCondition(tilesBottom ~= "Horizontal", 8, 0) ]])
end

-- initialize the settings immediately
-- this is so that they get stored into the .conf file without having to first place a slope
getSetting("slopes.drawTiles", true)
getSetting("slopes.drawOverlay", true)

return Slope