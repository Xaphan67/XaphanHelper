# Xaphan Helper

This helper contains all the custom stuff used in **The Secret of Celeste Mountain**, as well as a few other entities.
It adds many elements useful for custom campaigns to the game that want a metroidvania feel, as well as some other more generic entities.

Download it [here!](https://gamebanana.com/mods/53729) or via **Olympus**.
Other mods requiring this helper as a dependency will prompt **Everest** to download it automatically for you.

This file describes every entity, trigger and effect added by this helper.
You can use this as a reference point when using all the new stuff to understand how it works.
All the descriptions for the attributes can also be seen in **Loenn** by hovering over an attribute name.

> **Notice:** Everything provided by this helper has been made with the aim of having as few bugs as possible. If you still find a bug somewhere, please report it to xaphan67 on Discord.

## Contents

- [Entities](#entities) (91)
- [Triggers](#triggers) (17)
- [Effects](#effects) (2)

---

## Entities

- [Air Bubbles](#air-bubbles)
- [Arrow Hole](#arrow-hole)
- [Arrow Trap](#arrow-trap)
- [Bomb Block](#bomb-block)
- [Bomb Switch](#bomb-switch)
- [Bouncy Bubble](#bouncy-bubble)
- [Break Block](#break-block)
- [Bubble Block](#bubble-block)
- [Bubble Door](#bubble-door)
- [Cell](#cell)
- [Cell Lock](#cell-lock)
- [Climbable Vine](#climbable-vine)
- [Collectable Door](#collectable-door)
- [Conveyor](#conveyor)
- [Crate](#crate)
- [Crates Spawner](#crates-spawner)
- [Custom Badeline Boss](#custom-badeline-boss)
- [Custom Bounce Block](#custom-bounce-block)
- [Custom Checkpoint](#custom-checkpoint)
- [Custom Collectable](#custom-collectable)
- [Custom Coverup Wall](#custom-coverup-wall)
- [Custom Crumble Block](#custom-crumble-block)
- [Custom Dash Block](#custom-dash-block)
- [Custom End Screen Controller](#custom-end-screen-controller)
- [Custom Exit Block](#custom-exit-block)
- [Custom Fake Wall](#custom-fake-wall)
- [Custom Falling Block](#custom-falling-block)
- [Custom Move Block](#custom-move-block)
- [Custom Moving Platform](#custom-moving-platform)
- [Custom Refill](#custom-refill)
- [Custom Temple Gate](#custom-temple-gate)
- [Custom Torch](#custom-torch)
- [Detonator](#detonator)
- [Dragon](#dragon)
- [Drone Gate](#drone-gate)
- [Drone Switch](#drone-switch)
- [Elevator](#elevator)
- [Elevator Barrier](#elevator-barrier)
- [Ethereal Block](#ethereal-block)
- [Explosive Boulder](#explosive-boulder)
- [FG Tiles Jumpthru](#fg-tiles-jumpthru)
- [Flag Block](#flag-block)
- [Flag Dash Switch](#flag-dash-switch)
- [Flag Strawberry](#flag-strawberry)
- [Flag Swap Block](#flag-swap-block)
- [Fuse](#fuse)
- [Fuse Break Block](#fuse-break-block)
- [Heat Controller](#heat-controller)
- [In-Game Map Controller](#in-game-map-controller)
- [In-Game Map Hint Controller](#in-game-map-hint-controller)
- [In-Game Map Room Adjust Controller](#in-game-map-room-adjust-controller)
- [In-Game Map Room Controller](#in-game-map-room-controller)
- [In-Game Map Sub Area Controller](#in-game-map-sub-area-controller)
- [In-Game Map Tiles Controller](#in-game-map-tiles-controller)
- [Jump Block](#jump-block)
- [Jump Block Flip Sound Controller](#jump-block-flip-sound-controller)
- [Laser Detector](#laser-detector)
- [Laser Emitter](#laser-emitter)
- [Leaf](#leaf)
- [Lever](#lever)
- [Light Orb](#light-orb)
- [Liquid](#liquid)
- [Magnetic Ceiling](#magnetic-ceiling)
- [Merge Chapters Controller](#merge-chapters-controller)
- [Powamp](#powamp)
- [Power Line](#power-line)
- [Pressure Plate](#pressure-plate)
- [Push Block](#push-block)
- [Push Block Track](#push-block-track)
- [Rail Block](#rail-block)
- [Sawblade](#sawblade)
- [Set Stats Flags Controller](#set-stats-flags-controller)
- [Shutter](#shutter)
- [Skultera](#skultera)
- [Slope](#slope)
- [Spike Trap](#spike-trap)
- [Teleport Strawberry](#teleport-strawberry)
- [Teleport To Other Side Portal](#teleport-to-other-side-portal)
- [Thorn Barrier](#thorn-barrier)
- [Timed Block](#timed-block)
- [Timed Dash Switch](#timed-dash-switch)
- [Timed Strawberry](#timed-strawberry)
- [Timed Temple Gate](#timed-temple-gate)
- [Timer Refill](#timer-refill)
- [Upgrade Collectable](#upgrade-collectable)
- [Upgrade Collectable (Resource)](#upgrade-collectable-resource)
- [Upgrade Controller](#upgrade-controller)
- [Vine Head](#vine-head)
- [Vine Path](#vine-path)
- [Warp Station](#warp-station)
- [Work Robot](#work-robot)

### Air Bubbles

Air bubbles emanating from the ground that will quickly regenerate the player's oxygen to its maximum value when touched. Intended to be used inside a Liquid entity with its type set to Water. The player will stay at max oxygen as long as they keep touching the bubbles.

_No configurable attribute._

### Arrow Hole

A hole intended to be placed on top of borders of solid entities or tiles. The hole prevents arrows from Arrow Traps from getting stuck on the wall.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Side** | The side the hole will face. |

### Arrow Trap

A trap that shoots arrows repeatedly or when triggered by the player. Arrows will get stuck to any solid they collide with for a short time.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Side** | The side the arrow trap will face. |
| **Mode** | How the arrow trap behaves.<br>Triggered: Shoot an arrow when the player is in front of the trap.<br>Automatic: Shoot arrows repeatedly. |
| **Initial Delay** | Time (in seconds) the trap will wait before it starts shooting arrows.<br>Only used if Mode is set to Automatic. |
| **Flag** | The flag that must be true for the trap to be able to shoot arrows.<br>Leave empty for no flag. |
| **Cooldown** | Time (in seconds) the trap will wait after shooting an arrow before shooting another one.<br>Only used if Mode is set to Automatic. |
| **Only Once** | If true, the trap will only try to shoot one arrow.<br>Only used if Mode is set to Triggered. |

### Bomb Block

A block similar to Zip Movers, but triggered when the player throws a bomb at one of its active sides.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Particle Color 1** | Hexadecimal code for the first color of the particles appearing when the block moves. |
| **Particle Color 2** | Hexadecimal code for the second color of the particles appearing when the block moves. |
| **Left Side** | Determine how bombs will interact with the block when hitting its left side.<br>Start Active: Bombs can activate the block multiple times.<br>Start Inactive: Bombs can activate the block multiple times once the side becomes active.<br>Active Only Once: Bombs can activate the block once.<br>None: Bombs cannot activate the block. |
| **Top Side** | Determine how bombs will interact with the block when hitting its top side.<br>Start Active: Bombs can activate the block multiple times.<br>Start Inactive: Bombs can activate the block multiple times once the side becomes active.<br>Active Only Once: Bombs can activate the block once.<br>None: Bombs cannot activate the block. |
| **Right Side** | Determine how bombs will interact with the block when hitting its right side.<br>Start Active: Bombs can activate the block multiple times.<br>Start Inactive: Bombs can activate the block multiple times once the side becomes active.<br>Active Only Once: Bombs can activate the block once.<br>None: Bombs cannot activate the block. |
| **Bottom Side** | Determine how bombs will interact with the block when hitting its bottom side.<br>Start Active: Bombs can activate the block multiple times.<br>Start Inactive: Bombs can activate the block multiple times once the side becomes active.<br>Active Only Once: Bombs can activate the block once.<br>None: Bombs cannot activate the block. |
| **Flag** | The flag that must be true for the block to be activated by bombs, regardless of the sides' configuration.<br>Leave empty for it to be always activatable by bombs. |
| **Speed** | The speed at which the block will move toward the end of its track when activated by a bomb. (Default: 360) |
| **Swap Active Sides** | If true, once one side of the block is activated by a bomb, inactive sides will become active and active ones will become inactive.<br>Sides usable only once will stay inactive if they have been activated once. |
| **Deactivate All Sides After Use** | If true, once the block gets activated from any side, all sides will become inactive regardless of whether they were already used or not. |
| **Toggle** | If true, the block will stay at the end of its track once activated. |

### Bomb Switch

An entity the player can throw a bomb into, that will set or unset a flag when the bomb explodes inside it.

| Attribute | Description |
|---|---|
| **Sprite** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Flag** | The flag that will be set or unset each time a bomb explodes while into the switch. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set/unset forever. |
| **Save Data Only After Checkpoint** | If true, the flag will be registered in the player Save file and will be set forever only after the player gets to a checkpoint in the current room or exits the room.<br>(Register In Save Data must be set to true) |

### Bouncy Bubble

A bubble that will bounce the player away when colliding with it, unless they dash into it to break it. Can be used in combination with Custom Refills to force the player to dash in order to collect the refill.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Respawn Time** | Time (in seconds) after the bubble is destroyed before it respawns.<br>Leave at 0 for no respawn. |

### Break Block

A solid block that only breaks if the player meets the specified condition or uses the correct ability.

| Attribute | Description |
|---|---|
| **Mode** | Block: The break block will not try to merge with nearby solid tiles.<br>Wall: The break block will try to merge with nearby solid tiles. |
| **Tiletype** | Change the visual appearance of the block. |
| **Type** | Determine what will trigger the break sequence.<br>Bomb: The player must use the Bomb upgrade close to this block.<br>Drone: The player must use the Remote Drone upgrade and shoot this block.<br>LightningDash: The player must collide with this block while using the Lightning Dash upgrade.<br>MegaBomb: The player must use the Mega Bomb upgrade close to this block.<br>RedBooster: The player must collide with this block while using a red booster.<br>ScrewAttack: The player must collide with this block while using the Screw Attack upgrade. |
| **Color** | Hexadecimal code of the tint color of the indicators. FFFFFF (white) means no color. |
| **Start Revealed** | If true, the indicators will be visible without the player needing to dash into the block first. |
| **Directory** | The directory from which the sprites are taken for the indicators, relative to Atlases/Gameplay/. |
| **Flag** | If this flag is set, the block will use the tileset set in Flag Tiletype when entering the room. |
| **Flag Tiletype** | Change the visual appearance of the block if the flag is set. |
| **Respawn** | Determine the way the block can respawn.<br> Always: The block always respawns when re-entering the room or if the player dies.<br> Until checkpoint: The block respawns if the player dies or leaves the room. If the player activates a Custom Checkpoint, enters a Stop Entity Respawn Trigger or enters a Change Respawn Trigger, the block will no longer respawn.<br> Never: The block never respawns once destroyed, even if the player leaves the room or dies. |
| **Destroy Static Movers** | If true, static movers (for example: Spikes) attached to the block will disappear when the block is destroyed. |

### Bubble Block

A solid block that destroys itself for a set amount of time every time the player dashes, then slowly reforms. If the player is inside the block when it is fully reformed, they will die.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |

### Bubble Door

Metroid-like doors that are intended to be placed at the edge of a room (both left/right and top/bottom) where the player will transition to another room. They open when the player comes nearby, and close if the player goes away. They can also only open once one or more flags are set. Comes in 5 colors (blue, red, green, yellow and grey).

| Attribute | Description |
|---|---|
| **Side** | The side of the screen the door will be attached to. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/.<br>This directory must contain a sub-directory for each color. (the name of each sub-directory is the name of the color) |
| **Color** | The color of the door. |
| **Flags** | The flags that must be set for the door to be active, separated by a comma (,). Leave blank to make the door always active. |
| **Open Sound** | Sound event that plays when the door opens. |
| **Close Sound** | Sound event that plays when the door closes. |
| **Unlock Sound** | Sound event that plays when the door unlocks. (when the flag switches from false to true while the player is in the same room) |
| **Lock Sound** | Sound event that plays when the door locks. (when the door closes behind the player but the flag is not set) |
| **Force Locked Flag** | If this flag is set, the door will be locked and use the Grey color, regardless of other settings. |
| **Only Need One Flag** | If true, the door will be active if any of the flags specified in "flags" is set. |

### Cell

A holdable entity that can be thrown into a Cell Lock.

| Attribute | Description |
|---|---|
| **Tutorial** | Whether the entity should show the usage tutorial. |
| **Flag** | If this flag is set, the Cell will not spawn. |
| **Sprite** | The directory from which the sprite is taken, relative to Atlases/Gameplay/. |
| **Drop When Dream Dash** | If true, the player will automatically drop the Cell if they try to dream dash while holding the Cell. |
| **Throw Force Multiplier** | Multiply the horizontal force when the player throws the Cell by this amount. |
| **Throw Up Force Multiplier** | Control the force at which the Cell will be going upward when the player throws it. |
| **Post Throw No Gravity Timer** | Time (in seconds) after the player throws the Cell that gravity will not be applied. |
| **Gravity Multiplier** | Multiply the gravity applied to the Cell by this amount. |
| **Friction Multiplier** | Multiply the friction applied to the Cell by this amount. |
| **Bounce Multiplier** | Control the speed that will be applied to the Cell when it bounces off walls or the ground. |
| **Kill Player On Death** | If true, when the Cell dies, the player will die as well. |
| **Death Color** | Hexadecimal code for the color of the death effect when the Cell dies. |
| **Emit Light** | If true, the Cell will emit a light around it. |
| **Light Color** | Hexadecimal code for the color of the light emitted by the Cell. (Only if Emit Light is true) |
| **Hit Sides Sound** | Sound event that plays when the Cell hits a wall. |
| **Hit Ground Sound** | Sound event that plays when the Cell hits the ground. |
| **Death Sound** | Sound event that plays when the Cell dies. |

### Cell Lock

Acts as a receptacle for a Cell. When a Cell is thrown inside, it can then be activated and will set a flag of your choice. Has an option to allow the player to leave the room before activating the Cell, and come back later with the Cell still slotted into the Cell Lock. The flag can be saved in the player save file to stay active forever.

| Attribute | Description |
|---|---|
| **Sprite** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Color** | The color of the lock. |
| **Flag** | The flag to set once the player activates the lock. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set forever. |
| **Sound** | The sound event that plays when the player activates the lock. |
| **Cell Inside** | If true, the lock will already have a Cell slotted in it and only needs to be activated. |
| **Keep Cell** | If true, the Cell will stay inside the lock even after the player dies or leaves the room and the corresponding Cell will no longer spawn. |
| **Slot Sound** | The sound event that plays when the player places a Cell into the lock. |
| **Instant** | If true, the flag will be set as soon as a cell is slotted without requiring the player to interact first. |
| **Type** | Determine the sprite that will be used from the directory set in the Sprite field. Available types: "Normal", "Floating". |
| **Only Cell Visible** | If true, the entity will be invisible in game, and only the cell sprites will appear when a cell is slotted in. |
| **Cell Offset X** | Number of pixels to move the horizontal location of the Cell when it is slotted relative to its default position.<br>Positive values move it to the right.<br>Negative values move it to the left.<br>This will also move the detection area that moves the Cell toward its final position. |
| **Cell Offset Y** | Number of pixels to move the vertical location of the Cell when it is slotted relative to its default position.<br>Positive values move it down.<br>Negative values move it up.<br>This will also move the detection area that moves the Cell toward its final position. |

### Climbable Vine

A vine that can be grabbed by the player. The player can also jump through the vine while grabbing it. The vine will switch from a safe state to a kill state based on the current Light Mode or a flag.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Flag** | The flag that must be set for the vine not to kill the player.<br>If empty, the vine will not kill the player if the Light mode set by a Light Mode trigger is set to Light instead, or if the player touches a Light orb. |

### Collectable Door

A fully customisable door like the one at the start of the core chapter in vanilla. You can change almost everything: size, sprites, colors (including particles), edges animation, sounds, speeds and, of course, what the door actually checks for to open. It can currently check the following: Total Hearts, Current chapter Heart, Current chapter Heart in the current session, Total Cassettes, Current chapter Cassette, Current chapter Cassette in the current session, Total Strawberries, Current chapter Strawberries, Current chapter Strawberries in the current session, Player has Golden Strawberry and Flags from a list of your choice. The door can also be horizontal instead of vertical. You can also register this door as opened in the player save file to have it stay open forever.

| Attribute | Description |
|---|---|
| **Orientation** | Orientation of the door. Vertical will check if the player is to the left or the right of the door. Horizontal will check if the player is above or below the door. |
| **Mode** | Determine what the door will check.<br>TotalHearts: Amount of hearts collected in this campaign.<br>CurrentChapterHeart: Is the heart in this chapter collected?<br>CurrentSessionHeart: Is the heart of this chapter collected during this session?<br>TotalCassettes: Amount of cassettes collected in this campaign.<br>CurrentChapterCassette: Is the cassette in this chapter collected?<br>CurrentSessionCassette: Is the cassette in this chapter collected during this session?<br>TotalStrawberries: Amount of strawberries collected in this campaign.<br>CurrentChapterStrawberries: Amount of strawberries collected in this chapter.<br>CurrentSessionStrawberries: Amount of strawberries collected in this chapter during this session.<br>GoldenStrawberry: Does the player currently have a golden strawberry?<br>Flags: Is the required amount of flags defined in the 'Flags' field met? |
| **Edges** | Determine the edges that will be visible. |
| **Edges Animation Mode** | Determine how the edges will animate.<br>Clockwise: The edges will rotate clockwise.<br>CounterClockwise: The edges will rotate counter-clockwise.<br>Static: The edges will not be animated. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/.<br>Leave empty to use default sprites based on the 'Mode' field. |
| **Map Icon** | Name of the sprite that will be displayed on the In-game map.<br>Must be in Atlases/Gui/maps/(yourname)/(yourcampaignname)/. |
| **Interior Color** | Hexadecimal code for the color of the interior of the door. |
| **Mist Color** | Hexadecimal code for the color of the mist inside of the door. |
| **Icons Color** | Hexadecimal code for the color of the icons of the door. |
| **Edges Color** | Hexadecimal code for the color of the edges of the door. |
| **Interior Particles Color** | Hexadecimal code for the color of the particles inside of the door. |
| **Opening Particles Color 1** | Hexadecimal code for the first color of the particles emitted when the door opens. |
| **Opening Particles Color 2** | Hexadecimal code for the second color of the particles emitted when the door opens. |
| **Slice Color** | Hexadecimal code for the color of the slice when the door opens. |
| **Slice Particles Color 1** | Hexadecimal code for the first color of the particles emitted by the slice. |
| **Slice Particles Color 2** | Hexadecimal code for the second color of the particles emitted by the slice. |
| **Requires** | Amount of collectables or flags required to open the door.<br>If 'Mode' is set to 'GoldenStrawberry', this is ignored. |
| **Flags** | Only if 'Mode' is set to 'Flags'. The flags the door will check, separated by a comma (,). |
| **Open Speed Multiplier** | Increases or decreases the speed at which the door opens. 1 = normal speed. |
| **Before Slice Delay** | The delay (in seconds) between the end of the collectables check and the start of the opening sequence. (slice) |
| **After Slice Delay** | The delay (in seconds) after the start of the opening sequence. (slice) |
| **Check Distance** | The maximum distance (in tiles) the player must be from the sides of the door for it to start checking the collectables. |
| **Check Display Speed** | The speed at which the door checks if the player has the required collectables. |
| **Unlock Sound** | The sound event that plays when the door opens. |
| **Fill Sound** | The sound event that plays each time a collectable check is met. |
| **Sound Index** | Sound that plays when the player lands or walks on the door. |
| **Register In Save Data** | If true, the door ID will be registered in the player save file and will stay open forever once the requirements are met and the player has opened the door. |
| **Level Set** | The level set the door will check when the mode is set to TotalHearts, TotalCassettes or TotalStrawberries. Leave empty to check this campaign level set. |

### Conveyor

A conveyor that can be either horizontal or vertical, and will move the player and various other entities when they grab it or stand on top of it.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Speed** | The speed of the conveyor (Default: 60) |
| **Direction** | The direction the conveyor will move actors standing on it or grabbing it. |
| **Active Flag** | The flag that must be set for the conveyor to be active.<br>Leave empty for it to be always active. |
| **Force Inactive Flag** | If this flag is set, the conveyor will not be active even if the Active Flag is set or if Active Flag is empty. |
| **Swap Flag** | If this flag is set, swap the direction of the conveyor. |
| **Vertical** | If true, makes the conveyor vertical instead of horizontal. |
| **Flip X** | If true, flip the conveyor to face the other way.<br>No effect if Vertical is not set. |

### Crate

A crate that can be used by the player to block lasers from Laser Emitters and activate other entities such as Touch Switches or Pressure Plates.

| Attribute | Description |
|---|---|
| **Type** | Type of the crate.<br>Wood: Fragile crates destroyed by spikes, missiles and sawblades.<br>Metal: Resistant crates immune to most hazards. |
| **No Spawn Flag** | If this flag is set, prevent the crate from spawning when the room loads. |

### Crates Spawner

An entity that will spawn Crates.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Max Crates** | The maximum amount of crates spawned by this spawner that can exist in the room at the same time.<br>Once this limit is reached, the spawner will stop spawning crates until at least one crate it spawned is destroyed. |
| **Type** | Type of crates spawned by the spawner.<br>Wood: Fragile crates destroyed by spikes, missiles and sawblades.<br>Metal: Resistant crates immune to most hazards. |
| **Cooldown** | Time (in seconds) between each spawn of a crate. |
| **Flag** | The flag that must be true to start spawning crates.<br>Leave empty for no flag. |
| **Force Inactive Flag** | If this flag is true, the spawner will not spawn crates even if the flag set in Flag is set or if it is empty. |

### Custom Badeline Boss

A Badeline boss that acts just as the vanilla one, except you can change the color of the particles it uses.

| Attribute | Description |
|---|---|
| **Start Hit** | Determines whether or not Badeline waits to attack until the first hit or upon room entry. |
| **Pattern Index** | Determines what pattern of attack Badeline uses.<br>0 - Badeline transforms into the boss after being hit once<br>1 - Shoots orbs<br>2 - Shoots laser followed by one orb<br>3 - Shoots five streams of two orbs<br>4 - Shoots five streams of two orbs followed by one laser<br>5 - Shoots laser followed by three streams of two orbs<br>6 - Shoots lasers<br>7 - Shoots orbs with less delay<br>8 - Shoots lasers with less delay<br>9 - Shoots streams of two orbs<br>10 - Does nothing<br>11 - Shoots orbs with more delay<br>12 - Does nothing<br>13 - Shoots orbs after getting hit once<br>14 - Shoots lasers with very short delay<br>15 - Shoots lasers with more delay |
| **Camera Lock Y** | Whether the camera should be locked vertically, functioning similarly to a killbox. (No effect if Camera Lock is not true) |
| **Hit Particle Color 1** | Hexadecimal code for the first color of the particles appearing when the boss is hit. |
| **Hit Particle Color 2** | Hexadecimal code for the second color of the particles appearing when the boss is hit. |
| **Shot Trail Particle Color 1** | Hexadecimal code for the first color of the particles appearing behind the boss shots. |
| **Shot Trail Particle Color 2** | Hexadecimal code for the second color of the particles appearing behind the boss shots. |
| **Move Particle Color 1** | Hexadecimal code for the first color of the particles appearing behind the boss when he moves. |
| **Move Particle Color 2** | Hexadecimal code for the second color of the particles appearing behind the boss when he moves. |
| **Trail Color** | Hexadecimal code for the color of the trail when the boss moves. |
| **Beam Dissipate Particle Color** | Hexadecimal code for the color of the particles appearing when the boss beam dissipates. |
| **Sprite Name** | The name of the variant for the sprite in Sprites.xml. |
| **Camera Lock** | If true, the boss will prevent the camera from moving freely. |
| **Draw Projectiles Outline** | If true, draw a black outline around the projectiles shot by the boss. |

### Custom Bounce Block

A custom version of the vanilla Bounce Block entity, with more customisation and an outline showing the time before it respawns.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Bounce Strength Multiplier** | Determine how strongly the block will push the player away.<br>1 is default vanilla value. |
| **Respawn Time** | Time (in seconds) for the block to respawn after use. |
| **Reform Time** | Time (in seconds) for the block to reform when it respawns. |
| **Not Core Mode** | If true, ignore current Core Mode and default to Ice mode. |

### Custom Checkpoint

When the player touches this entity, it plays a sound, plays a sprite, and the player's respawn point is set to the closest Player Spawn.

| Attribute | Description |
|---|---|
| **Sprite** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Activated Sprite X** | The X position for the sprite that appears when the checkpoint is activated. |
| **Activated Sprite Y** | The Y position for the sprite that appears when the checkpoint is activated. |
| **Remove Background When Active** | If true, the sprite of the inactive checkpoint is removed when the checkpoint is activated. |
| **Sound** | Sound event that plays when the checkpoint is activated. |
| **Emit Light** | If true, the checkpoint will emit light when activated. |
| **Light Color** | Hexadecimal code for the color of the light when the checkpoint is activated. |

### Custom Collectable

A collectable that sets a flag when collected by the player. This collectable can appear on the In-Game map, can change the music played or can complete the chapter after it's collected. The flag can be saved in the player save file to stay active forever.

| Attribute | Description |
|---|---|
| **Sprite** | The full path to the sprite, relative to Atlases/Gameplay/. |
| **Collect Sound** | Sound event that plays when the player collects this collectable. |
| **Change Music** | If true, the current music will stop while the collectSound plays when the collectable has been collected. |
| **New Music** | Only if changeMusic is set.<br>The music event that plays after the collectable has been collected. Leave blank to use the same music as the one that played before the collectable was collected. |
| **Flag** | The flag that will be set once the player collects the collectable. |
| **Map Icon** | Name of the sprite that will be displayed on the In-game map.<br>Must be in Atlases/Gui/maps/(yourname)/(yourcampaignname)/. |
| **Must Dash** | If true, the player must dash to collect the collectable. |
| **Collect Golden Strawberry** | If true, the golden strawberry will collect when the player collects the collectable. |
| **End Chapter** | If true, the chapter will end when the player collects the collectable. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set forever. |
| **Ignore Golden** | If true, the collectable will not re-appear when the player carries a golden berry if it has already been collected. |
| **Can Respawn** | If true, the collectable will respawn when the player re-enters the room it is in if the flag set by the collectable is not set. |
| **Loop Sound** | The sound event that will play each time the sprite finishes a loop. Leave empty for no sound. |
| **Loop Burst** | If true, the collectable will emit a small burst each time the sprite finishes a loop. |
| **Static Time** | Time in seconds between each sprite loop during which the sprite will pause on its first frame. 0 = no pause. |
| **Particles Color** | Hexadecimal code for the color of the particles. Leave blank for no particles. |
| **Wiggle** | If true, the collectable will slowly wiggle up and down. |
| **Animation Speed** | Delay between each frame of the animation. Default: 0.08. |
| **Poem** | If true, the collectable will show a poem on screen (like crystal hearts do) when collected. |
| **Poem Name** | Name of the poem that is displayed when the collectable is collected. No effect if Poem is not true. |
| **Poem Color** | Color of the name of the poem that is displayed when the collectable is collected. No effect if Poem is not true. |
| **Poem Particles Color** | Colors of the particles displayed when the collectable is collected. No effect if Poem is not true. |
| **Poem Sprite** | The full path to the sprite that is displayed behind the poem, relative to Atlases/Gui/. No effect if Poem is not true. |
| **Poem Sprite Animation Speed** | Delay between each frame of the animation of the sprite that is displayed behind the poem. No effect if Poem is not true. |
| **Poem Sprite Animation Pause** | The number of frames to wait between each loop of the sprite that is displayed behind the poem. No effect if Poem is not true. |

### Custom Coverup Wall

Same as the vanilla Coverup Wall, but this one can use a different tileset depending on the state of a flag.

| Attribute | Description |
|---|---|
| **Tiletype** | Change the visual appearance of the wall. |
| **Flag** | If this flag is set, the wall will use the tileset set in Flag Tiletype when entering the room. |
| **Flag Tiletype** | Change the visual appearance of the wall if the flag is set. |

### Custom Crumble Block

Same as the vanilla Crumble Platform, but with more customisation and an outline showing the time before it respawns.

| Attribute | Description |
|---|---|
| **Texture** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Rotation** | Determine by how many degrees the sprites will be rotated. |
| **One Use** | If true, the block will not respawn after being triggered. |
| **Group** | The group this block is part of.<br>All blocks that are part of the same group will be triggered when one of the blocks from the group is triggered.<br>Use -1 for no group. |
| **Respawn Time** | The time (in seconds) the block will wait before respawning.<br>If Group is not -1, this will be overwritten by the lowest value of this field of any adjacent block from the same group. |
| **Crumble Delay** | The time (in seconds) the block will wait before crumbling when the player lands on top of it.<br>There is always 0.2s added to this value.<br>If Group is not -1, this will be overwritten by the lowest value of this field of any adjacent block from the same group. |
| **Side Crumble Delay** | The time (in seconds) the block will wait before crumbling when the player grabs one of its sides.<br>There is always 0.2s added to this value.<br>If Group is not -1, this will be overwritten by the lowest value of this field of any adjacent block from the same group. |
| **Can Bypass Crumble Delay** | If true, immediately destroy the block if the player leaves the top of the block.<br>If Group is not -1, this will be true if any adjacent block from the same group also has this set to true. |
| **Can Bypass Side Crumble Delay** | If true, immediately destroy the block if the player leaves any sides of the block.<br>If Group is not -1, this will be true if any adjacent block from the same group also has this set to true. |
| **Light** | If true, the block will emit a small amount of light. |

### Custom Dash Block

Same as the vanilla Dash Block, but this one can use a different tileset depending on the state of a flag.

| Attribute | Description |
|---|---|
| **Can Dash** | Whether the player can break the block by dashing into it. |
| **Blend In** | Blends the block with the walls it touches, making it less apparent. |
| **Permanent** | Once broken, the block will remain that way, even if the player dies. |
| **Tiletype** | Change the visual appearance of the block. |
| **Flag** | If this flag is set, the block will use the tileset set in Flag Tiletype when entering the room. |
| **Flag Tiletype** | Change the visual appearance of the block if the flag is set. |

### Custom End Screen Controller

Allows you to have multiple end screens in your map, based on various conditions. You can have as many end screens as you want (use one controller per end screen). Conditions that can be checked currently include:
- Flags
- Time spent in the level
- Number of strawberries collected
- Percent of items (strawberries, hearts and cassettes) collected
- Percent of In-Game map explored

These conditions can check for the current chapter values or the whole campaign values.
If none of the end screens you have defined in these controllers has its conditions true, the normal end screen is displayed instead.
These end screens can also display additional information on the screen itself, including time, strawberries collected, items and map percents, as well as some custom text of your liking.

| Attribute | Description |
|---|---|
| **Atlas** | The full path to the images used by this End Screen, relative to Atlases/. |
| **Images** | The name of each image displayed, separated by a comma (,), in the order they will be drawn. |
| **Title** | Dialog ID for the title displayed at the top of the End Screen. Leave blank to use default titles based on the Side of this chapter. |
| **Show Title** | If true, the title will be visible. Set to false to hide it. |
| **Sub Text 1** | Dialog ID for the text that will appear at the bottom of the End Screen, above item and map percent if you use those. |
| **Sub Text 1 Color** | Hexadecimal code for the color of the text that will appear at the bottom of the End Screen, above item and map percent if you use those. |
| **Sub Text 2** | Dialog ID for the text that will appear at the bottom of the End Screen, above item and map percent if you use those. |
| **Sub Text 2 Color** | Hexadecimal code for the color of the text that will appear at the bottom of the End Screen, above item and map percent if you use those. |
| **Music** | The music event that plays when this End Screen appears. Leave blank to use default music based on the Side of this chapter. |
| **Hide Vanilla Timer** | If true, hide the vanilla timer at the bottom left of the End Screen. |
| **Required Time** | The maximum time allowed for this End Screen to appear, in seconds.<br>Leave to 0 for no time requirement. |
| **Show Time** | If true, the clear time will be displayed at the bottom of the End Screen. |
| **Required Strawberries** | The minimum amount of strawberries the player must have collected for this End Screen to appear. |
| **Show Strawberries** | If true, the amount of strawberries collected by the player will be displayed at the bottom of the End Screen. |
| **Strawberries Color** | Hexadecimal code for the color of the strawberries display, when not equal to the maximum number of strawberries. |
| **Strawberries Max Color** | Hexadecimal code for the color of the strawberries display, when equal to the maximum number of strawberries. |
| **Required Item Percent** | The minimum percentage of items the player must have collected for this End Screen to appear.<br>Items include Strawberries, Hearts and Cassettes. |
| **Show Item Percent** | If true, the percentage of items collected by the player will be displayed at the bottom of the End Screen. |
| **Item Percent Color** | Hexadecimal code for the color of the item percentage display, when below 100%. |
| **Item Percent Max Color** | Hexadecimal code for the color of the item percentage display, when at 100%. |
| **Required Map Percent** | The minimum percentage of map the player must have explored on the In-Game map for this End Screen to appear. |
| **Show Map Percent** | If true, the percentage of map explored by the player will be displayed at the bottom of the End Screen. |
| **Map Percent Color** | Hexadecimal code for the color of the map percentage display, when below 100%. |
| **Map Percent Max Color** | Hexadecimal code for the color of the map percentage display, when at 100%. |
| **Required Flags** | The flags that must be set for this End Screen to appear, separated by a comma (,). |
| **Requirements Check** | Chapter: The requirements (Time, Strawberries, Item Percent and Map Percent) will be calculated with values from this chapter only.<br>Campaign: The requirements (Time, Strawberries, Item Percent and Map Percent) will be calculated with values from the whole campaign.<br>This also determines if the values shown on the End Screen are the Chapter or Campaign values. |
| **Priority** | The order the game will check if conditions for this End Screen to appear are true.<br>The first End Screen found with all its conditions true will be displayed. |

### Custom Exit Block

Just like a vanilla Exit Block, except all the Custom Exit Blocks in the same room turn solid when the player is not inside any of them.

| Attribute | Description |
|---|---|
| **Tiletype** | Change the visual appearance of the block. |
| **Close Sound** | If true, the 'close behind' sound will play when the player is no longer inside any CustomExitBlock with the same group number as this one. |
| **Group** | All Custom Exit Blocks with the same group number will turn solid once the player is outside all of them. |
| **Mode** | Block: The exit block will not try to merge with nearby solid tiles.<br>Wall: The exit block will try to merge with nearby solid tiles. |
| **Flag** | If this flag is set, the block will use the tileset set in Flag Tiletype when entering the room. |
| **Flag Tiletype** | Change the visual appearance of the block if the flag is set. |

### Custom Fake Wall

Just like a vanilla Fake Wall, but you can link more than one together (using the same group) so they all reveal when the player enters one of them.

| Attribute | Description |
|---|---|
| **Mode** | Block: The fake wall will not try to merge with nearby solid tiles.<br>Wall: The fake wall will try to merge with nearby solid tiles. |
| **Tiletype** | Change the visual appearance of the wall. |
| **Play Transition Reveal** | If true, play the secret revealed sound if the player enters this fake wall during a screen transition. |
| **Group** | All Custom Fake Walls with the same group number will disappear once the player is inside one of them. |
| **Flag** | If this flag is set, the wall will use the tileset set in Flag Tiletype when entering the room. |
| **Flag Tiletype** | Change the visual appearance of the wall if the flag is set. |

### Custom Falling Block

Same as a vanilla Falling Block, except with more customisation (floating in Liquids, automatically falling if there is nothing solid above it, etc.).

| Attribute | Description |
|---|---|
| **Tiletype** | Change the visual appearance of the block. |
| **Climb Fall** | If true, the block will start falling when the player climbs on it. |
| **Behind** | If true, the block is rendered in the background. |
| **Fall If No Solid On Top** | If true, the block will fall if there are no solid entities or tiles directly on its top side. |
| **Magneting Ceilings Do Not Trigger** | If true, the block will not fall if the player grabs a Magnetic Ceiling attached to it. |
| **Can Float** | If true, the block will float when falling into a Liquid. |

### Custom Move Block

Same as the vanilla Move Block, with more customisation. Optimised for use with Magnetic Ceilings.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Direction** | The direction the block will start moving toward, when activated. |
| **Steer Sides** | The sides of the block that will have a button to allow the player to steer it.<br>Depending on the direction, only some values can be used:<br>Left or Right: TopOnly, BottomOnly, TopAndBottom.<br>Up or Down: LeftOnly, RightOnly, LeftAndRight.<br>Incorrect values for the chosen direction will result in no button added to the block. |
| **Idle Color** | Hexadecimal code of the color of the block when inactive. |
| **Move Color** | Hexadecimal code of the color of the block when it is moving. |
| **Break Color** | Hexadecimal code of the color of the block when it is about to break. |
| **Particles Color** | Hexadecimal code of the color of the particles emitted by the block. |
| **Speed** | The speed the block will move at. Default: 60. |
| **Acceleration** | The acceleration the block will have until it reaches the specified speed. Default: 300. |
| **Respawn Time** | Time (in seconds) after the block breaks before it respawns. |
| **One Use** | If true, the block will not respawn after being broken. |
| **Add Border** | If true, add a black border around the block. |
| **Glow** | If true, the colors of the block will have a glow effect. |
| **Button Ignore Colors** | If true, the buttons on the block will not be tinted with the colors defined in "Idle Color", "Move Color" and "Break Color". |
| **Button Pressed Offset** | Number of pixels the buttons will move when the player presses them. |
| **Magnetic Ceiling Offset** | Number of pixels the Magnetic Ceiling attached to this block will be moved downward.<br>Used to give the illusion that the magnetic ceiling is attached to the button below the block.<br>Ignored if Steer Sides is not set to "BottomOnly" or "TopAndBottom". |

### Custom Moving Platform

One or more moving platforms following a track, with different modes and reactions to flags.

| Attribute | Description |
|---|---|
| **Mode** | Determine how platforms will move on the track:<br>Restart: Once a platform reaches the end of the track, it reappears at the start.<br>Back And Forth: Once a platform reaches the end of the track, it goes back toward the beginning.<br>Back And Forth (All Platforms): Once a platform reaches the end of the track, ALL platforms go back toward the beginning. (Even ones that did not reach the end of the track)<br>Flag To Move: Platforms will not move unless the flag set in Move Flag is true. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Line Color A** | The hexadecimal code for the color of the edges of the track. |
| **Line Color B** | The hexadecimal code for the color of the center of the track. |
| **Particles Color A** | The hexadecimal code for the first color emitted by the platforms when moving. |
| **Particles Color B** | The hexadecimal code for the second color emitted by the platforms when moving. |
| **Orientation** | The side the platform will face toward. |
| **Amount** | Number of platforms that will be on the track. |
| **Length** | Length of the platforms. (in tiles) |
| **Speed** | The speed at which the platforms will move along the track. |
| **Start Offset** | The value (in percent, between 0 and 1) of the track length where the first platform will be placed on the track.<br>Example: use 0.5 to have a platform at the exact middle of the track. |
| **Spacing Offset** | The value (in percent, between 0 and 1) of the track length between each platform.<br>Platforms may behave incorrectly if this value multiplied by the Amount value is greater than 1. |
| **Attached Entity Platforms Indexes** | Determines which platforms will have entities attached to them, starting at index 1 for the first platform. (Separated by commas (,))<br>You need to place another entity next to the platform for this to work.<br>Leave empty to copy the attached entity to all platforms.<br>Compatible entities: Spikes, Spring, Magnetic Ceiling, Lever.<br><br>Example: If you have 3 platforms and want a spring attached to the 1st and last ones, place a spring attached to this platform, then change this to 1,3. |
| **Stop Flag** | If this flag is set, the platforms will stop moving no matter what. |
| **Swap Flag** | If this flag is set, the platforms will move in the opposite direction. |
| **Move Flag** | The flag that must be set to allow the platforms to move.<br>Only if Mode is set to Flag To Move. |
| **Force Inactive Flag** | If this flag is set, platforms will not move and stay idle. |
| **Draw Track** | If true, draw the track that the platforms will follow. |
| **Particles** | If true, the platforms will emit particles when they move. |

### Custom Refill

A custom version of dash refills that can refill only specific stuff (dash, jumps, oxygen, etc.) and uses an animation when respawning.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/.<br>Leave empty to use default sprites based on the type of the refill. |
| **Type** | The type of the refill. Determine what will be refilled.<br>All: Give back exactly one dash if the player does not already have the maximum number of dashes. Also restore jumps from the Space Jump ability if the dash is refilled.<br>All (Two Dashes): Always give two dashes even if the player's maximum number of dashes is currently only 1. Also restore jumps from the Space Jump ability if the dash is refilled.<br>One Dash: Give back exactly one dash if the player does not already have the maximum number of dashes.<br>Two Dashes: Always give two dashes even if the player's maximum number of dashes is currently only 1.<br>Max Jumps: Only restore jumps from the Space Jump ability.<br>Missiles: Restore the drone missiles to the current maximum amount of missiles.<br>Super Missiles: Restore the drone super missiles to the current amount of super missiles.<br>Oxygen: Fully restore the oxygen of the player.<br>Oxygen Small: Restore 1/3 of the oxygen of the player.<br>Oxygen and Oxygen Small will always make the refill usable only once. |
| **Respawn Time** | Time (in seconds) before the refill reappears after being used. (Ignored if the type is Oxygen or Oxygen Small) |
| **One Use** | If true, the refill will not respawn after being used. (Ignored if the type is Oxygen or Oxygen Small) |

### Custom Temple Gate

Just like a vanilla Temple Gate, but this one opens only if a specified flag is true. Compatible with vanilla Dash Switches.

| Attribute | Description |
|---|---|
| **Flag** | The flag that needs to be set for the gate to open.<br>If empty, the closest vanilla Dash Switch will affect this door. |
| **Sprite Name** | The name of the variant for the sprite in Sprites.xml. (minus templegate_) |
| **Open On Heart Collection** | If true, the gate will open when the heart is collected by the player in the current session (regardless of the flag state) |
| **Start Open** | If true, the gate will be open when the player enters the room.<br>If a flag is set in Flag, the door will close when the flag is true.<br>If not, the door will close when the closest vanilla Dash Switch is pressed. |
| **Horizontal** | If true, the gate will be horizontal instead of vertical. |
| **Attach Right** | If true, the gate will be attached to the right. (only if Horizontal is true) |
| **Silent** | If true, the gate will not emit any sound when it opens or closes. |

### Custom Torch

A torch-like entity that can have a custom sprite, light and particles color, and you can set a flag that must be true for the torch to activate on its own.

| Attribute | Description |
|---|---|
| **Play Lit Sound** | If true, play a sound when the torch is activated. |
| **Start Lit** | Change whether the light will start lit, or have the player's touch activate it. |
| **Flag** | The flag that must be set to activate the torch. |
| **Color** | Hexadecimal code for the color of the light and particles emitted when the torch is activated. |
| **Sprite** | The full path to the sprite, relative to Atlases/Gameplay/. |
| **Alpha** | Strength of the light. (minimum 0 (invisible), maximum 1) |
| **Start Fade** | Radius value where the light will start to fade. |
| **End Fade** | Radius value where the light will end. |
| **Sound** | Sound event that plays when the torch is activated (if Play Lit Sound is set). |
| **No Particles** | If true, no particles will be emitted when the torch is activated or deactivated. |
| **Off Frames** | List of frame indexes (separated by a comma) to play when the torch is deactivated. |
| **Turn On Frames** | List of frame indexes (separated by a comma) to play when the torch is activating. |
| **On Frames** | List of frame indexes (separated by a comma) to play when the torch is activated. |

### Detonator

A Detonator that will trigger a fuse adjacent to it. Can be triggered by the player or solid entities.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Flag** | The flag that will be true once the fuse connected to this detonator reaches one ending.<br>The ending of the fuse MUST collide with a Flag Block.<br>If the player enters the room and this flag is already set, the detonator will already be pressed. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set forever. |
| **Side** | The side the detonator will face. |
| **Speed** | The speed at which the fuse connected to this detonator will explode. (Must be between 0.01 and 0.3) |

### Dragon

An enemy that will move upward and shoot fireballs towards the player.

| Attribute | Description |
|---|---|
| **Shoot Direction** | Directions the dragon will try to shoot at the player. |
| **Shoot Strength** | Strength at which the dragon will shoot fireballs at the player. |
| **Initial Delay** | Time (in seconds) before the dragon starts shooting fireballs. |
| **Fireball Timer** | Time (in seconds) between each fireball. |
| **Fireballs** | Number of fireballs the dragon will shoot in a row before going in idle state. |
| **Idle Timer** | Time (in seconds) between each batch of fireballs. |

### Drone Gate

A small gate that only opens if the Remote Drone from the Remote Drone upgrade is nearby. You can use flags to activate/deactivate it.

| Attribute | Description |
|---|---|
| **Side** | The side the gate will face. |
| **Flag** | The flag that must be set for this gate to open when the drone is nearby. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Force Inactive Flag** | When this flag is true, the gate will always stay closed. |

### Drone Switch

A small switch that can only be triggered by the Remote Drone from the Remote Drone upgrade. The Remote Drone must shoot at it for it to either set or unset a flag. These switches attach to any adjacent solids.

| Attribute | Description |
|---|---|
| **Side** | The side the switch will face. |
| **Flag** | The flag that will be set/unset when the drone shoots this switch. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set forever when the drone shoots the switch.<br>If the drone shoots it again, it will be removed from the player save file. |
| **Save Data Only After Checkpoint** | If true, the flag will be registered in the player Save file and will be set forever only after the player gets to a checkpoint in the current room or exits the room.<br>(Register In Save Data must be set to true) |
| **Only Once** | If true, prevent the switch from being shot more than once. |
| **Reset On Death** | If true, allow the switch to be hit again after the player dies. No effect if Only Once is not set. |
| **No Reset On Death Flag** | The flag that will prevent the switch from being reset when the player dies. No effect if Reset On Death is not set. |

### Elevator

Use this elevator to transport the player to another room of the current chapter or even another chapter! Elevators can be a bit tricky to set up correctly. You can have a look at how it's done in SoCM to get an idea.

| Attribute | Description |
|---|---|
| **Sprite** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **End Position** | Number of pixels the elevator will travel when used.<br>Positive values move the elevator downward, and negative values move it upward. (1 tile in game is 8x8 pixels) |
| **Can Talk** | Can the player interact with the elevator to activate it? Must be set for the elevator to transport the player to another room or chapter. If not set, the elevator will activate as soon as the player steps on it. |
| **Timer** | Time to wait between the activation of the elevator and the moment it actually starts moving. |
| **To Chapter** | Which chapter this elevator will transport the player into. 0 = interlude.<br>Set this to the current chapter to transport the player to another room of the current chapter instead. |
| **Destination Room** | Name of the room the player will be transported into. If empty, the starting room of the corresponding chapter will be used. |
| **Spawn Room X** | X position of the spawn point where the player will appear in the destination room. |
| **Spawn Room Y** | Y position of the spawn point where the player will appear in the destination room. |
| **One Use** | If true, the player will not be able to use the elevator again after using it once. |
| **Flag** | The flag that must be set for this elevator to be usable by the player. Leave empty if you want the elevator to always be usable. |

### Elevator Barrier

Same as an invisible barrier, except the player can go through it when riding an elevator. Can be used to prevent the player from entering an elevator shaft, for example.

_No configurable attribute._

### Ethereal Block

A solid block that looks like a normal wall, but can be dashed through if the player has the Ethereal Dash ability.

| Attribute | Description |
|---|---|
| **Tiletype** | Change the visual appearance of the block. |

### Explosive Boulder

A boulder that must be dashed into to explode. Bombs can also be used to explode them. When dashed into, it launches the player in the same way vanilla Bumpers do.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Bounce Force** | How strongly the player will be pushed back when they dash into the boulder. |
| **Dash Cooldown** | Time (in seconds) the player cannot dash after they dash into the boulder. |
| **Gravity** | If true, the boulder will be affected by gravity and explode when it hits the ground after falling. |
| **Refill Jump** | If true, the player will get back their Space Jump when they dash into the boulder.<br>Only works with the Upgrade system. |

### FG Tiles Jumpthru

Just like a vanilla Jumpthru platform, but this one only visually attaches to foreground tiles. Used to make sure it will not visually attach to solid entities on top of it.

| Attribute | Description |
|---|---|
| **Surface Index** | The sound that will play when the player stands or walks on the platform. |
| **Texture** | Changes the appearance of the platform. |

### Flag Block

A solid block that destroys itself when a specific flag is true. If the player enters the room while the flag is already true, the block does not appear.

| Attribute | Description |
|---|---|
| **Tiletype** | Change the visual appearance of the block. |
| **Mode** | Block: The flag block will not try to merge with nearby solid tiles.<br>Wall: The flag block will try to merge with nearby solid tiles. |
| **Remove Flags** | If any of these flags are set, the block will not appear, or will break if it already appeared. Flags must be separated by a comma (,). |
| **Permanent** | If true, the block will not reappear if the player dies or re-enters the room. |
| **Flag** | If this flag is set, the block will use the tileset set in Flag Tiletype when entering the room. |
| **Flag Tiletype** | Change the visual appearance of the block if the flag is set. |

### Flag Dash Switch

A dash switch that sets a flag when the player dashes into it. The flag can be saved in the player save file to stay active forever.

| Attribute | Description |
|---|---|
| **Side** | The side the switch will face. |
| **Persistent** | Once turned on, it will stay activated even if the player dies. (No effect if Can Swap Flag is true) |
| **Sprite Name** | The name of the variant for the sprite in Sprites.xml. (minus dashSwitch_) |
| **Flag** | The flag that will be affected once the player dashes into the switch. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set forever. (No effect if Can Swap Flag is true and Mode is not set to SetTrue) |
| **Save Data Only After Checkpoint** | If true, the flag will be registered in the player Save file and will be set forever only after the player gets to a checkpoint in the current room or exits the room.<br>(Register In Save Data must be set to true. No effect if Can Swap Flag is true and Mode is not set to SetTrue) |
| **Can Swap Flag** | If true, the player will be able to dash into this switch multiple times, each time inverting the state of the flag. |
| **Particle Color 1** | Hexadecimal code of the first color of particles emitted when the player dashes into the switch. |
| **Particle Color 2** | Hexadecimal code of the second color of particles emitted when the player dashes into the switch. |
| **Mode** | SetTrue: The flag will be set to true when the player dashes into the switch.<br>SetFalse: The flag will be set to false when the player dashes into the switch.<br>SetInverted: The flag state will be inverted when the player dashes into the switch. |
| **In Wall** | If true, move the switch 4 pixels toward the wall. If Side is set to Left or Right, prevent the player from standing on it. If Side is set to Up or Down, prevent the player from grabbing it. |
| **Force Inactive Flag** | When this flag is true, the switch cannot be pressed. |
| **Allow Static Movers** | If true, allow static movers (i.e. Spikes) to attach to this switch. |

### Flag Strawberry

A Strawberry that sets a flag when collected.

| Attribute | Description |
|---|---|
| **Flag** | The flag that will be set once this strawberry is collected. |
| **Order** | Manually determine what order strawberries are visually placed in on the start menu during gameplay and level select. Overrides Everest's automatic berry IDs. (Default: -1) |
| **Checkpoint ID** | Manually determine what the checkpoint section strawberries are visually grouped up in, showing up on the start menu during gameplay and level select. Overrides Everest's automatic berry IDs. (Default: -1) |
| **Moon** | Makes the strawberry render as a space berry. |
| **Winged** | The strawberry attempts to vertically rise offscreen when the player dashes. |

### Flag Swap Block

Just like a vanilla Swap Block, but you can customise its speed and sprites, turn it into a Toggle Block (so the block does not come back to its original position on its own), change the particles colors and set a flag that must be true for the block to react to the player's dash.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Speed** | Speed of the block, going from the starting position to end position. |
| **Flag** | The flag that must be set once for the swap block to react to the player's dash. |
| **Toggle** | If true, the swap block will not return to the starting position on its own, and will do so only when the player dashes again. |
| **Particle Color 1** | Hexadecimal code of the first color of particles emitted when the block moves from one side to the other. |
| **Particle Color 2** | Hexadecimal code of the second color of particles emitted when the block moves from one side to the other. |
| **Render BG** | If true, render the background behind the block path. |

### Fuse

A fuse that must be adjacent to a Detonator and will determine the path the explosion from the detonator will follow.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |

### Fuse Break Block

A block that can be destroyed tile per tile by an explosion from a Detonator when it follows a fuse.

| Attribute | Description |
|---|---|
| **Texture** | Change the visual appearance of the block. |
| **Sound Index** | Sound that plays when the player grabs, lands or walks on the block. |

### Heat Controller

Place one in any room you want to set as 'heated'. The player will die after a set amount of time has passed once they enter a heated room. A thermometer-like visual will also show the player the time they have left before they die. If your map uses upgrades, the player is immune to that effect if they have collected the Varia Jacket upgrade. If your map uses the In-Game map, heated rooms appear in their own color (orange by default).

| Attribute | Description |
|---|---|
| **Max Duration** | Duration (in seconds) before the player dies due to overheating. |
| **Heat Effect** | If true, add a heat effect to the room. |
| **Inactive Flag** | While this flag is set, the controller will have no effect (the Heat Effect will still be shown if it's set). |
| **Reset Duration** | If true, immediately reset the duration to 0 instead of slowly decreasing it when the flag in Inactive Flag is true, allowing the player to have full duration immediately once it is set back to false. |

### In-Game Map Controller

Use this to add and customise an In-Game Map (Metroid inspired) to your map. You can change most of the colours used to make it look like you want. The In-Game Map tracks berries, hearts, cassettes, warps and upgrades collected by the player. Each time the player enters a new room, it is added to the map. You only need one In-Game Map Controller per chapter.

| Attribute | Description |
|---|---|
| **Explored Room Color** | Hexadecimal code for the color of rooms the player has explored. |
| **Unexplored Room Color** | Hexadecimal code for the color of rooms the player has not yet explored. |
| **Secret Room Color** | Hexadecimal code for the color of secret rooms. |
| **Heated Room Color** | Hexadecimal code for the color of heated rooms. |
| **Room Border Color** | Hexadecimal code for the color of the room borders. |
| **Elevator Color** | Hexadecimal code for the color of the elevator and elevator shaft in elevator rooms. |
| **Grid Color** | Hexadecimal code for the color of the grid. |
| **Map Name** | Dialog ID used to display the name at the top of the map. If empty, the chapter name will be displayed instead. |
| **Reveal Unexplored Rooms** | If true, unexplored rooms will be revealed immediately. |
| **Require Map Upgrade To Open** | If true, the player needs to collect the Map upgrade before being able to open the In-Game map. |
| **Show Progress** | Determine when the map will show the player progress.<br>Always: The player progress is always shown.<br>AfterChapterComplete: The player progress is shown once the player has completed the current chapter.<br>AfterCampaignComplete: The player progress is shown once the player has completed all chapters of the campaign.<br>Never: The player progress is never shown. |
| **Hide Map Progress** | If true, the player progress will not display the percentage of the map explored by the player. |
| **Hide Strawberry Progress** | If true, the player progress will not display the amount of strawberries collected by the player. |
| **Hide Moonberry Progress** | If true, the player progress will not display the amount of moonberries collected by the player. |
| **Hide Upgrade Progress** | If true, the player progress will not display the amount of upgrades collected by the player. |
| **Hide Heart Progress** | If true, the player progress will not display if the heart is collected by the player. |
| **Hide Cassette Progress** | If true, the player progress will not display if the cassette is collected by the player. |
| **Hide Icons In Unexplored Rooms** | If true, icons will not be displayed on the map for unexplored rooms. |
| **Custom Collectables Progress** | Custom collectables that should be shown in the player progress, separated by a comma (,).<br>Each Custom Collectable whose 'Map Icon' attribute value is listed here will be displayed.<br>The icon that will represent each Custom Collectable must be in Atlases/Gui/maps/(yourname)/(yourcampaignname)/keys/(Map Icon attribute)<br>The order in which you list the collectables here determines the order in which the player progress displays them. |
| **Secrets Custom Collectables Progress** | Custom collectables from the 'Custom Collectables Progress' attribute that should be considered as secret, separated by a comma (,).<br>For each collectable listed here, the player progress will only display the collectable once the player has collected at least one. |
| **Progress Color** | Hexadecimal code for the color of the player progress values. |
| **Progress Complete Color** | Hexadecimal code for the color of the player progress values when the player has collected every collectable of the same type in a Chapter or Sub Area. |

### In-Game Map Hint Controller

Use this controller to add hints on the In-Game map. Those hints can be used to help the player find the way forward, or point to important locations, based on flag states. Hints can either be a target or an arrow. Targets are best used to indicate a location the player should head toward, while arrows can be used to indicate a transition to another chapter through an Elevator or a Teleport To Chapter Trigger, for example. If there is at least one room in the current chapter with an In-Game Map Hint Controller, a button appears on the In-Game map to allow the player to toggle hints on or off.

| Attribute | Description |
|---|---|
| **Display Flags** | The flags that must be true for this hint to be visible, separated by a comma (,). |
| **Hide Flag** | The flag that must be true for this hint to no longer be visible. |
| **Tile Cords** | Coordinates of the tile where the hint will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Remove When Reached By Player** | If true, the hint will no longer appear once the player has reached the tile where the hint is drawn. |
| **Type** | Type of the hint. 'Target' can be used to indicate a position in a room, while 'Arrow' ideally indicates a transition to another chapter via an Elevator or a TeleportToChapter trigger. |
| **Direction** | Direction the arrow will point toward if type is set to Arrow. Ignored if type is set to Target. |

### In-Game Map Room Adjust Controller

Use this to adjust the position, size and entrances of a room if the way the code auto-generated them doesn't look good. You can also use this in combination with a Stop Adjust In-Game Map Trigger to make nice secrets.

| Attribute | Description |
|---|---|
| **Position X** | Moves the room horizontally on the map.<br>Positive values move the room to the right, and negative values move it to the left. (in tiles) |
| **Position Y** | Moves the room vertically on the map.<br>Positive values move the room downward, and negative values move it upward. (in tiles) |
| **Size X** | Increases or decreases the horizontal size of the room. (in tiles, only if the room does not contain an InGameMapTilesController) |
| **Size Y** | Increases or decreases the vertical size of the room. (in tiles, only if the room does not contain an InGameMapTilesController) |
| **Remove Entrance 0** | Hide this entrance on the map. |
| **Remove Entrance 1** | Hide this entrance on the map. |
| **Remove Entrance 2** | Hide this entrance on the map. |
| **Remove Entrance 3** | Hide this entrance on the map. |
| **Remove Entrance 4** | Hide this entrance on the map. |
| **Remove Entrance 5** | Hide this entrance on the map. |
| **Remove Entrance 6** | Hide this entrance on the map. |
| **Remove Entrance 7** | Hide this entrance on the map. |
| **Remove Entrance 8** | Hide this entrance on the map. |
| **Remove Entrance 9** | Hide this entrance on the map. |
| **Ignore Icons** | If true, icons used in this room will not move if the room is moved via this controller. |
| **Hidden Tiles** | Coordinates of tiles that won't be displayed when the player unlocks the map of this chapter or if 'revealUnexploredRooms' is set in the InGameMapController.<br>(Format: X-Y, separated by a comma. Example: 0-0,0-1,2-3 (0-0 is the top left tile)) |
| **Display** | Determine where this controller will take effect.<br>Always: Affect the area map and the world map.<br>Area map only: Affect the area map only.<br>World map only: Affect the world map only. |

### In-Game Map Room Controller

Use this in each room of your map to define where the entrances of the room are so they are shown correctly on the In-Game Map. You can also set if a room is visible even if the player hasn't entered it yet, or if it is a secret room.

| Attribute | Description |
|---|---|
| **Show Unexplored** | If true, this room will be visible when the player unlocks the map of this chapter or if 'revealUnexploredRooms' is set in the InGameMapController. |
| **Secret** | If true, this room will be treated as a secret room and use the secret room color. |
| **Entrance 0 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 0 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 1 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 1 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 2 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 2 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 3 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 3 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 4 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 4 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 5 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 5 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 6 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 6 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 7 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 7 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 8 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 8 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Entrance 9 Position** | The border of the tile where this entrance will be drawn. Can be set to Left, Right, Top or Bottom. None = no entrance. |
| **Entrance 9 Cords** | Coordinates of the tile where the entrance will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Map Shard Index** | Index of the MapShard upgrade the player must collect for this room to be revealed. (only if Show Unexplored is true) |
| **Sub Area Index** | Index of the Sub Area this room is part of. |

### In-Game Map Sub Area Controller

Use this controller to define custom parameters used by the In-Game map that will apply to a set of rooms. You can change the colors used to draw these rooms and the name of the sub-area that is displayed at the top of the map when the player is in one of these rooms.

| Attribute | Description |
|---|---|
| **Sub Area Name** | Dialog ID used to display the name at the top of the map when the player is in this Sub Area. If empty, the Sub Area name will not be shown. |
| **Sub Area Index** | Index of this Sub Area. All rooms that have the same Sub Area Index as this one in their In-Game Map Room Controller will be part of this Sub Area. |
| **Explored Room Color** | Hexadecimal code for the color of rooms the player has explored in this Sub Area. |
| **Unexplored Room Color** | Hexadecimal code for the color of rooms the player has not yet explored in this Sub Area. |
| **Secret Room Color** | Hexadecimal code for the color of secret rooms in this Sub Area. |
| **Heated Room Color** | Hexadecimal code for the color of heated rooms in this Sub Area. |
| **Room Border Color** | Hexadecimal code for the color of the room borders in this Sub Area. |
| **Elevator Color** | Hexadecimal code for the color of the elevator and elevator shaft in elevator rooms in this Sub Area. |

### In-Game Map Tiles Controller

Use this to customise which tiles are drawn for your room on the In-Game Map. This allows you to have rooms that aren't necessarily rectangles on the map, or have some special tiles for specific rooms such as elevator rooms! Rooms that use this controller also allow the In-Game Map to show exactly which tiles the player has visited when it enters them, instead of considering the whole room visited upon entering.

| Attribute | Description |
|---|---|
| **Tile 0** | Tile to draw. None = no tile. |
| **Tile 0 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 1** | Tile to draw. None = no tile. |
| **Tile 1 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 2** | Tile to draw. None = no tile. |
| **Tile 2 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 3** | Tile to draw. None = no tile. |
| **Tile 3 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 4** | Tile to draw. None = no tile. |
| **Tile 4 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 5** | Tile to draw. None = no tile. |
| **Tile 5 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 6** | Tile to draw. None = no tile. |
| **Tile 6 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 7** | Tile to draw. None = no tile. |
| **Tile 7 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 8** | Tile to draw. None = no tile. |
| **Tile 8 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Tile 9** | Tile to draw. None = no tile. |
| **Tile 9 Cords** | Coordinates of where the tile will be drawn.<br>(Format: X-Y. (0-0 is the top left tile)) |
| **Display** | Determine where to draw the tiles defined in this controller.<br>Always: Draw the tiles on the area map and the world map.<br>Area map only: Draw the tiles on the area map only.<br>World map only: Draw the tiles on the world map only. |

### Jump Block

A cassette-like block that switches from solid to air each time the player jumps.

| Attribute | Description |
|---|---|
| **Index** | Index of the block.<br>The blocks with the smallest index will start solid when the player enters the room.<br>Each time the player jumps, all blocks with the next index in chronological order will turn solid and other blocks will not be collidable anymore.<br>If all blocks in the room have the same index, they will switch from solid to air and vice versa each time the player jumps. |
| **Color** | Hexadecimal code for the color of the block. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Sound Index** | Sound that plays when the player grabs, lands or walks on the block. |
| **Switch Type** | Wiggle: The block will wiggle a bit when its state changes from solid to air and vice versa.<br>Fade: The block will become darker when changing from solid to air, and lighter again when switching from air to solid. |
| **Improved Tileset** | Use a tileset that allows for some randomness in the tiles instead of the default one. |
| **Only One Tileset** | If true, use only the 'solid' tileset, no matter the state of the block. |

### Jump Block Flip Sound Controller

Use this to specify the sounds that should play each time the jump blocks in the room switch from solid to air, or air to solid. If you do not use this, cassette blocks sounds are used.

| Attribute | Description |
|---|---|
| **On Sound** | Sound event that plays when the blocks switch from solid to air. |
| **Off Sound** | Sound event that plays when the blocks switch from air to solid. |

### Laser Detector

As long as a laser from a Laser Emitter collides with one side of this detector (you can specify the sides the detector will check), it sets a flag.

| Attribute | Description |
|---|---|
| **Flag** | The flag that will be set as long as a laser beam collides with this detector. |
| **Sides** | The sides of this detector that will check for collision with a laser beam. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |

### Laser Emitter

Fires a laser in the direction of your choice. It can fire 3 types of lasers:
- **Red**: kills the player on contact
- **Blue**: kills the player if they try to dash through it
- **Yellow**: kills the player if they do not dash through it

Laser emitters can attach to solid entities, and the laser itself dynamically collides with any solid entities or foreground tiles on its path as well.

| Attribute | Description |
|---|---|
| **Flag** | The flag that must be set for the laser to be active. If empty, the laser will always be active. |
| **Side** | The direction the emitter will shoot the laser. |
| **Type** | Kill: Kill the player when they touch the laser. If your map uses upgrades, the player can dash through it if they use the Screw Attack upgrade.<br>No Dash: Kill the player if they dash through the laser.<br>No Start Dash: Kill the player if they start dashing while on top of the laser.<br>Must Dash: Kill the player if they do not dash through the laser. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Inverted** | If true, the flag must not be set for the laser to be active. (No effect if the flag is empty) |
| **Base** | If true, add a base to the laser emitter. |
| **No Beam** | If true, this laser emitter will not shoot any laser. |
| **Laser Color** | Hexadecimal code for the color of the laser. Leave empty to use default color. |

### Leaf

A leaf working either like vanilla Clouds or vanilla Ice Bounce Blocks depending on the current Light Mode.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Flag** | The flag that must be set for the leaf to become bouncy.<br>If empty, the leaf will be bouncy if the Light mode set by a Light Mode trigger is set to Light instead, or if the player touches a Light orb. |
| **Respawn Time** | The time (in seconds) for the leaf to reappear after being used. |

### Lever

A lever that will set or unset a flag when the player touches it. Can be attached to solids.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Side** | The side of the lever. |
| **Group** | All levers with the same group will activate when the player touches one. Leave at 0 for no group. |
| **Flag** | The flag that will be true when the lever is activated. |
| **Active Sound** | The sound event that plays when the player active the lever. |
| **Can Swap Flag** | If true, the player can touch the lever multiple times, each time inverting the flag state. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set forever. (No effect if Can Swap Flag is true) |
| **Save Data Only After Checkpoint** | If true, the flag will be registered in the player Save file and will be set forever only after the player gets to a checkpoint in the current room or exits the room.<br>(Register In Save Data must be set to true. No effect if Can Swap Flag is true) |

### Light Orb

An orb that will change the Light Mode when the player touches it.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Time** | The time during which the light mode will switch.<br>No effect if Temporary is not set. |
| **Temporary** | If true, the light mode will revert to its previous state after a set time. |

### Liquid

An entity that can either be Acid, Lava, Quicksand or Water. Acid kills the player on contact, Lava emits bubble sounds and kills the player on contact. Quicksand sinks the player into it, heavily slows them down, reduces their jump height and prevents them from dashing or climbing when they are inside it. Water slows down the player when they are inside it. If your map uses upgrades, the player is immune to the slow effect if they have collected the Gravity Jacket upgrade. The liquids can be set to move up and down and use custom sprites and colors.

| Attribute | Description |
|---|---|
| **Low Position** | Number of pixels the liquid will wave down from its current position. (moving down, then up again, and so on...) 8 pixels = 1 game tile. |
| **Liquid Type** | Type of the liquid.<br>Acid and Acid_b: Liquid that kills the player on contact.<br>Lava: Bubbling liquid that emits sound and kills the player on contact.<br>Quicksand: Liquid in which the player sinks slowly, with their movement and jump height heavily slowed down. Also prevents the player from climbing and dashing. <br>Water: Liquid that slows down the player's movement while inside it, unless they have the GravityJacket upgrade. |
| **Frame Delay** | Delay between each frame of the animation of the liquid. |
| **Color** | Hexadecimal code for the color of the liquid. Leave blank to use the default liquid color. |
| **Transparency** | Transparency of the liquid. Values can be 0.01 to 1. The closer to 1, the more opaque the liquid will be.<br>Setting this to 0 will use the default value. |
| **Foreground** | If true, the liquid will render on top of foreground tiles as well. |
| **Rise Delay** | Time (in seconds) the liquid will wait before rising. |
| **Rise Distance** | Number of pixels this liquid will rise before stopping. Negative values will drain the liquid instead. 8 pixels = 1 game tile.<br>If left at 0, the liquid will neither rise nor drain and other 'Rise' attributes will be ignored. |
| **Rise Speed** | Speed at which the liquid will rise. |
| **Rise Shake** | If true, the screen will be shaking while the liquid is rising (and during the Rise Delay as well). |
| **Rise Flag** | The flag that must be set for the liquid to start rising. Leave blank for no flag. |
| **Rise End Flag** | If this flag is set when entering the room, the liquid will immediately be at the position it would reach once it stops rising, and it will not rise anymore. |
| **Rise Sound** | If true, a sound will play while the liquid is rising (and during the Rise Delay as well). |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/.<br>This directory must contain a sub-directory for the liquid type. (the name of the sub-directory is the name of the liquid type)<br>If empty, default sprites will be used. |
| **Surface Height** | Number of tiles in your custom sprites for the height of the surface of the liquid.<br>This corresponds to the number of tiles at the top of your custom sprites that will be drawn only once.<br>If not using custom sprites, leave this to 0 to use default values based on the liquid type. |
| **Visual Only** | If true, the liquid will have no effect on the player's physics and will not be able to kill them. |
| **Can Swim** | If true, the player will be able to swim in the liquid just like vanilla water. |
| **Appear Flags** | The liquid can appear only if all of these flags are set. Flags must be separated by a comma (,).<br>Leave blank to not check any flags. |
| **Remove Flags** | If any of these flags are set, the liquid will not appear. Flags must be separated by a comma (,).<br>Leave blank to not check any flags. |
| **Upside Down** | If true, the liquid will render upside down. |
| **Inside Transparency** | Transparency of the liquid when the player is completely inside. Values can be 0.01 to 1 and must be lower than or equal to the normal Transparency. The closer to 1, the more opaque the liquid will be.<br>Setting this to 0 will use the same value as the normal transparency value. |
| **Group** | ID of the group this liquid is part of.<br>All liquids in the same group will use the transparency set in "inside Transparency" when the player is inside any of them.<br>-1 = no group. |
| **Can Drown** | If true, the player will die if they stay inside the liquid for longer than the time specified in Air Timer. |
| **Air Timer** | Time (in seconds) that the player can stay in the liquid before dying, if Can Drown is true and Visual Only is false. |
| **Varia Prevent Dying** | If true, the player can walk in the liquid even if it is set to Lava or Acid, if the player has the Varia Jacket upgrade. |
| **Poisoned Color** | Hexadecimal code for the color of the liquid when it is poisoned.<br>When the liquid is purified, it will change to its normal color. |
| **Purify Flags** | Only if Liquid Type is set to Water.<br>The liquid will be poisoned and kill the player as long as all these flags are not set. Flags must be separated by a comma (,). |
| **Invert Purify Flags** | If true, the flags specified in Purify Flags must be set for the liquid to be poisoned. |
| **Purified Flag** | The flag that will be set as long as the liquid is purified.<br>This flag is automatically registered in the player save file when set. |

### Magnetic Ceiling

A ceiling that the player can grab, just like any vertical wall, by holding the Grab button while under it. While grabbed to the ceiling, the player can move left and right, but cannot dash or jump (unless you allow it). Staying still costs stamina, and moving costs even more stamina. If the magnetic ceiling is attached to any solid entity, that entity considers the player as on top of it when the player is grabbing the ceiling. This allows falling blocks to fall or zip movers to activate when the player grabs the ceiling, for example. If your map uses upgrades, the player needs to collect the Spider Magnet upgrade to be able to grab these ceilings.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Animation Speed** | Delay between each frame of the animation. |
| **Can Jump** | If true, the player can jump while grabbing the ceiling. |
| **No Stamina Drain** | If true, the player will not use stamina while grabbing the ceiling. |

### Merge Chapters Controller

Use this controller in the first chapter (or the prologue) of your campaign to create the illusion that all chapters from your campaign are only one big chapter. This removes the chapter selection screen and replaces it with a single panel displaying stats for all chapters combined (total strawberries, cassettes, hearts (A, B and C sides separated) and deaths (A, B and C sides separated)). The player starts at the beginning of the first chapter (or prologue) of your campaign the first time they start it, then they are loaded in a specific room based on the mode you set in this controller.

To progress to other chapters, you can use Elevators, Teleport To Chapter Triggers and Warp Stations.
To enter B and C sides, you can use Teleport To Other Side Portals.
This controller can also be used to do some things that are not normally possible, such as more than one heart/cassette in a chapter, or multiple B/C sides (by creating fake chapters and linking them to your real chapter).

| Attribute | Description |
|---|---|
| **Mode** | Classic: The player will start at the checkpoint they selected (same as vanilla).<br>Rooms: The player will start in the same room they were in when they left the campaign.<br>Warps: The player will start at the location of the last activated Warp Station they stood on. |
| **Keep Prologue Separated** | If true, the starting chapter of the campaign will not be merged.<br>No effect if the starting chapter of the campaign is not set as an interlude. |

### Powamp

An enemy that floats and only becomes deadly to the player a short time after they touch it.

| Attribute | Description |
|---|---|
| **Grown Delay** | Time (in seconds) before the powamp will become deadly after the player has triggered it. |
| **Grown Time** | Time (in seconds) it takes for the powamp to grow. |

### Power Line

A visual line used to indicate that an entity is connected to another with flags.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Flag** | The flag that must be set for this line to appear as powered. |
| **Inverted** | If true, the power line will be powered instead of unpowered when the flag in Flag is not set. |
| **Background** | If true, the power line will render in the background instead of the foreground. |

### Pressure Plate

An entity the player or an actor must stand on to set a flag. The flag is automatically unset if the player or the actor is no longer on it.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Flag** | The flag that will be true as long as the pressure plate is pressed. |

### Push Block

A block that moves based on the direction the player dashes into it. Must be on a Push Block Track that will define the area the block can move on.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Sound Index** | Sound that plays when the player lands or walks on the block. |
| **Particle Color 1** | Hexadecimal code used for the first color of the particles emitted by the block. |
| **Particle Color 2** | Hexadecimal code used for the second color of the particles emitted by the block. |
| **Must Dash** | If true, kill the player if they touch the block unless dashing into it to move it. |

### Push Block Track

An area Push Blocks can move on.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |

### Rail Block

A block that moves when the player dashes into it from the correct side, based on the track it is connected to. If left alone for long enough, it starts returning to its starting point.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Speed Mult** | Speed multiplier applied to the block when it moves.<br>1 = normal speed. Higher is faster, lower is slower.<br>Minimum 0.5, Maximum 1.5. |
| **Line Color A** | The hexadecimal code for the color of the edges of the track. |
| **Line Color B** | The hexadecimal code for the color of the center of the track. |
| **Particles Color A** | The hexadecimal code for the first color emitted by the block when it moves. |
| **Particles Color B** | The hexadecimal code for the second color emitted by the block when it moves. |
| **Can Dash** | If true, the block can move along the track when the player dashes into it from the correct side. |
| **Player Momentum** | If true, the block can move along the track when the player stands on it. |
| **No Return** | If true, prevent the block from going back to its starting position after a short time when the player stops interacting with it. |
| **Draw Track** | If true, draw the track that the block will follow. |
| **Particles** | If true, the block will emit particles when it moves. |

### Sawblade

A sawblade that follows a track, can react to flags, and kills the player or Crates. Metal Crates will be bounced away instead.

| Attribute | Description |
|---|---|
| **Mode** | Determine how sawblades will move on the track:<br>Restart: Once a sawblade reaches the end of the track, it reappears at the start.<br>Back And Forth: Once a sawblade reaches the end of the track, it goes back toward the beginning.<br>Back And Forth (All Sawblades): Once a sawblade reaches the end of the track, ALL sawblades go back toward the beginning. (Even ones that did not reach the end of the track)<br>Flag To Move: Sawblades will not move unless the flag set in Move Flag is true. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Line Color A** | The hexadecimal code for the color of the edges of the track. |
| **Line Color B** | The hexadecimal code for the color of the center of the track. |
| **Particles Color A** | The hexadecimal code for the first color emitted by the sawblades when moving. |
| **Particles Color B** | The hexadecimal code for the second color emitted by the sawblades when moving. |
| **Amount** | Number of sawblades that will be on the track. |
| **Radius** | Radius of the hitbox each sawblade will have. (in pixels) |
| **Speed** | The speed at which the sawblades will move along the track. |
| **Start Offset** | The value (in percent, between 0 and 1) of the track length where the first sawblade will be placed on the track.<br>Example: use 0.5 to have a sawblade at the exact middle of the track. |
| **Spacing Offset** | The value (in percent, between 0 and 1) of the track length between each sawblade.<br>Sawblades may behave incorrectly if this value multiplied by the Amount value is greater than 1. |
| **Stop Flag** | If this flag is set, the sawblades will stop moving no matter what. |
| **Swap Flag** | If this flag is set, the sawblades will move in the opposite direction. |
| **Move Flag** | The flag that must be set to allow the sawblades to move.<br>Only if Mode is set to Flag To Move. |
| **Force Inactive Flag** | If this flag is set, sawblades will not move and stay idle. |
| **Draw Track** | If true, draw the track that the sawblades will follow. |
| **Particles** | If true, the sawblades will emit particles when they move. |

### Set Stats Flags Controller

Use this controller in the first chapter (or the prologue) of your campaign to automatically gain access to some flags based on the player progression in your campaign. The controller enables the flags in every chapter of your campaign, even if the player is not in the one they correspond to. This allows you to use flags from the player progress of a different chapter than the one the player is currently in. You can also use these flags in combination with Achievement Helper to easily add some achievements to your campaign. The current conditions checked and the flags associated are:

| Condition | Flag |
|---|---|
| In-Game map of each chapter fully explored | `XaphanHelper_StatFlag_MapCh[ChapterIndex]` |
| Entered a sub-area on the In-Game map for the first time | `XaphanHelper_StatFlag_MapCh[ChapterIndex]-[SubArea]-Visited` |
| Sub-area on the In-Game map fully explored | `XaphanHelper_StatFlag_MapCh[ChapterIndex]-[SubArea]` |
| All strawberries of each chapter collected | `XaphanHelper_StatFlag_StrawberriesCh[ChapterIndex]` |
| All strawberries of each sub-area of each chapter collected | `XaphanHelper_StatFlag_StrawberriesCh[ChapterIndex]-[SubArea]` |
| All blue crystal hearts collected | `XaphanHelper_StatFlag_Hearts` |
| All cassettes collected | `XaphanHelper_StatFlag_Cassettes` |
| B-Side crystal heart of each chapter collected | `XaphanHelper_StatFlag_BSideCh[ChapterIndex]` |
| Golden strawberries collected | `XaphanHelper_StatFlag_GoldenCh[ChapterIndex]-[Mode]` |

- `[ChapterIndex]` is the number of the chapter displayed on the Chapter Panel. If your chapter is an interlude, `[ChapterIndex]` is 0.
- `[Mode]` is 0 for A-Sides, 1 for B-Sides and 2 for C-Sides.
- `[SubArea]` is the index of the sub-area specified in In-Game Map Sub Area Controllers.

_No configurable attribute._

### Shutter

A solid entity that acts similarly to a Temple Gate, but opens or closes at a custom speed and has a customisable length.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Direction** | The direction of the shutter. |
| **Speed** | The speed at which the shutter opens or closes. (Default: 30) |
| **Length** | The length (in tiles) of the shutter. |
| **Flag** | The flag that should be set for the shutter to open. |
| **Sound** | The sound event that plays when the shutter opens or closes. |
| **Start Open** | If true, the shutter will be open and will start closing when the flag set in Flag is set. |
| **Silent** | If true, the shutter will not make any sound when opening or closing. |

### Skultera

An enemy attached to a wall that sucks the player toward itself to kill them.

| Attribute | Description |
|---|---|
| **Direction** | The direction the skultera will face. |
| **Max Range** | The range (in tiles) in front of the skultera where the player will be affected. |
| **Suction Strength** | The strength at which the player will be pulled toward the skultera when in range. (Default: 150) |

### Slope

Slopes allow you to move the player up or down nicely. You can also allow the player to slide on them! Please note that Celeste was never developed with slopes in mind, so this is kind of a workaround.

| Attribute | Description |
|---|---|
| **Side** | Side of the slope. |
| **Gentle** | If true, the slope will take 2 tiles per tile up or down instead of one. |
| **Sound Index** | Sound that plays when the player lands or walks on the slope. |
| **Slope Height** | Height of the slope. This will also increase its width automatically. |
| **Tiles Top** | Tile pattern at the top of the slope. Use this to 'connect' the slope to nearby solid tiles. |
| **Tiles Bottom** | Tile pattern at the bottom of the slope. Use this to 'connect' the slope to nearby solid tiles. |
| **Custom Directory** | Leave the default value or empty if using vanilla textures. Specify here the directory where your custom texture is located, relative to Atlases/Gameplay/. |
| **Flag Custom Directory** | Same as the Custom Directory field. Used instead if the flag is set. |
| **Texture** | Texture of the slope. Type your custom texture name here if you have set a custom directory. |
| **Flag Texture** | Same as the Texture field. Used instead if the flag is set. |
| **Can Slide** | If true, the player can slide on the slope by holding down on it. |
| **Force Slide** | If true, the player will automatically slide on the slope. No effect if Can Slide is false. |
| **Upside Down** | Flip the slope so that it can be used on ceilings instead of the ground. |
| **No Render** | If true, the slope will be invisible. |
| **Sticky Dash** | If true, the player will stick to the slope while dashing away from it. |
| **Rainbow** | If true, the slope will have a rainbow effect. (No effect if No Render is true) |
| **Allow Light** | If true, prevent the slope from blocking light. |
| **Affect Player Speed** | If true, reduces the player's speed when going up the slope and increases it when going down. |
| **Flag** | If this flag is set, the slope will use the directory and texture set in Flag Custom Directory and Flag Texture when entering the room. |
| **Render Method** | Change the way the game draws the slope.<br>Depending on your tileset, one method may be better than the other.<br>(Effect only visible in game) |
| **Prevent Refill On Sliding** | If true, prevent the player from regaining their Dash or jumps<br>(when using the Space Jump ability) while the player slides on the slope.<br>No effect if Force Slide is false. |

### Spike Trap

A trap that acts like Trigger Spikes, but with a custom sound and sprite. Can be triggered more than once.

| Attribute | Description |
|---|---|
| **Sprite** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Direction** | Direction the spikes from the trap will be facing. |
| **Trigger Time** | Time (in seconds) after the player lands on the trap before the spikes get released. |
| **Retract** | If true, retract the spikes shortly after they have been triggered, allowing the trap to be triggered again. |

### Teleport Strawberry

A Strawberry that, when collected, teleports the player to another room.

| Attribute | Description |
|---|---|
| **Destination Room** | Name of the room the player will be transported into. |
| **Spawn Room X** | X position of the spawn point where the player will appear in the destination room. |
| **Spawn Room Y** | Y position of the spawn point where the player will appear in the destination room. |
| **Wipe Type** | Wipe effect to use during the teleportation. |
| **Wipe Duration** | Duration of the wipe effect (in seconds). |
| **Required Starwberries To Teleport** | Number of strawberries (including this one) that must have been collected in the current chapter to trigger the teleport. |
| **Order** | Manually determine what order strawberries are visually placed in on the start menu during gameplay and level select. Overrides Everest's automatic berry IDs. (Default: -1) |
| **Checkpoint ID** | Manually determine what the checkpoint section strawberries are visually grouped up in, showing up on the start menu during gameplay and level select. Overrides Everest's automatic berry IDs. (Default: -1) |
| **Moon** | Makes the strawberry render as a space berry. |
| **Winged** | The strawberry attempts to vertically rise offscreen when the player dashes. |

### Teleport To Other Side Portal

A portal that teleports the player to the B or C side of the current chapter. If the destination side is not unlocked yet for that chapter, it is unlocked after using the portal. This means this portal can be used in place of a cassette, for example. This also allows the player to enter the C side before unlocking the B side of a chapter. You can add some checks for the portal to appear:
- The cassette must be collected.
- The C Sides must be unlocked.
- One or more flags must be set.

You can also use this portal to teleport the player back to the A side of a chapter when in the B or C side.

| Attribute | Description |
|---|---|
| **Side** | The side of the current chapter the player will be teleported to, when entering the portal. |
| **Require Cassette Collected** | If true, the cassette of the current chapter must have been collected first before the portal can appear. |
| **Wipe Duration** | Duration of the wipe effect (in seconds). |
| **Wipe Type** | Wipe effect to use during the teleportation. |
| **Warp Sfx** | Sound event that plays when the player is teleported.<br>Leave empty to use the default sound. |
| **Teleport To Starting Spawn Of Chapter** | If true, the player will be teleported to the normal starting spawn instead of the closest spawn of the portal in the destination side if there is one. |
| **Require C Side Unlocked** | If true, the C-Side for the current chapter must be unlocked first before the portal can appear. |
| **Flags** | The flags that must be true before the portal can appear, separated by a comma (,). |
| **Register Current Side As Compelete** | If true, the side will be considered complete before teleporting the player.<br>Useful when ending a B or C side with a portal instead of a heart. |

### Thorn Barrier

A spinner-like entity that kills the player, but can be dashed through depending on the Light Mode or a flag.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Flag** | The flag that must be set for the barrier to kill the player.<br>If empty, the barrier will kill the player only if the Light mode set by a Light Mode trigger is set to Light instead, or if the player touches a Light orb. |

### Timed Block

A cassette-like block that switches from solid to air or air to solid when a timer is ticking.

| Attribute | Description |
|---|---|
| **Start Pressed** | If true, the block will not be solid when the player enters the room, and will turn solid when a Timed Dash Switch is pressed. |
| **Color** | Hexadecimal code for the color of the block. |
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Sound Index** | Sound that plays when the player grabs, lands or walks on the block. |

### Timed Dash Switch

A dash switch that starts a timer (duration of your choice) when the player dashes into it. While the timer is ticking, if the player dashes into another Timed Dash Switch, it can either extend the timer or set it to a specified value. A ticking sound also plays while the timer is ticking. This sound can either be on top of the current music, replace it, or not be there at all. Timed entities (Timed Block, Timed Strawberry, Timed Temple Gate, Timer Refill) react differently while the timer is ticking.

| Attribute | Description |
|---|---|
| **Side** | The side the switch will face.<br>Available sides: Up, Down, Left and Right. |
| **Sprite Name** | The name of the variant for the sprite in Sprites.xml. (minus dashSwitch_) |
| **Mode** | Add: If a Timed Dash Switch has already been pressed and the timer is still ticking, the timer of this switch will be added to the current time remaining, rounded to the nearest second.<br>Set: If a Timed Dash Switch has already been pressed and the timer is still ticking, the timer of this switch will replace the current time remaining. |
| **Timer** | Time (in seconds) of the timer that will start once this switch is pressed. Minimum 3 seconds. |
| **Ticking Type** | On Top: The ticking sound will play on top of the current music.<br>Tick only: The ticking sound will replace the current music.<br>None: No ticking sound will play. |
| **Particle Color 1** | Hexadecimal code of the first color of particles emitted when the player dashes into the switch. |
| **Particle Color 2** | Hexadecimal code of the second color of particles emitted when the player dashes into the switch. |
| **Flag** | The flag that will be set while the timer is ticking. |
| **In Wall** | If true, move the switch 4 pixels toward the wall. If Side is set to Left or Right, prevent the player from standing on it. If Side is set to Up or Down, prevent the player from grabbing it. |

### Timed Strawberry

A strawberry that only appears if a timer is ticking. If the timer runs out before the player collects the strawberry, it disappears and returns to its original position. You can allow the player to collect the strawberry even if the timer runs out, as long as the player has grabbed it.

| Attribute | Description |
|---|---|
| **Keep Even If Timer Run Out** | If true, the strawberry will not disappear if the player grabbed it before the timer ran out, but has not collected it. |
| **Order** | Manually determine what order strawberries are visually placed in on the start menu during gameplay and level select. Overrides Everest's automatic berry IDs. (Default: -1) |
| **Checkpoint ID** | Manually determine what the checkpoint section strawberries are visually grouped up in, showing up on the start menu during gameplay and level select. Overrides Everest's automatic berry IDs. (Default: -1) |
| **Moon** | Makes the strawberry render as a space berry. |

### Timed Temple Gate

Just like a vanilla Temple Gate, but this one opens only when a timer is ticking. You can set it to start opened, and it will close while the timer is ticking.

| Attribute | Description |
|---|---|
| **Start Open** | If true, the gate will be open when the player enters the room, and will close while a timer is ticking. |
| **Sprite Name** | The name of the variant for the sprite in Sprites.xml. (minus templegate_) |

### Timer Refill

A refill that appears only if a timer is ticking. If the player collects it, it can either extend the timer or set it to a specified value.

| Attribute | Description |
|---|---|
| **One Use** | If true, the refill disappears after the player has collected it. |
| **Timer** | Time (in seconds) that will be added to, or will replace, the current timer once this refill is collected, depending on the mode. Minimum 3 seconds. |
| **Mode** | Add: The timer of this refill will be added to the current time remaining, rounded to the nearest second.<br>Set: The timer of this refill will replace the current time remaining. |
| **Respawn Time** | Time (in seconds) after collecting this refill before it can be collected again. |

### Upgrade Collectable

A collectable that grants the player the ability of your choice when collected. Ability names and sprites can be customised.

| Attribute | Description |
|---|---|
| **Collect Sound** | Sound event that plays when the player collects this upgrade. |
| **New Music** | The music event that plays after the upgrade has been collected. Leave blank to use the same music as the one that played before the upgrade was collected. |
| **Upgrade** | The upgrade that will be granted to the player.<br>Map: Allows the player to open the In-Game map (if your chapter uses it) and display the mini-map.<br>MapShard: Reveals all rooms set as 'Show Unexplored' (and with the same MapShardIndex as the one defined in this upgrade's MapShardIndex) in their respective InGameMapRoomController on the In-Game map for the current chapter.<br>Binoculars: Allows the player to use binoculars to preview the room at any time.<br>Bombs: Allows the player to set bombs that can destroy some walls.<br>ClimbingKit: Allows the player to climb walls. (requires PowerGrip)<br>DashBoots: Allows the player to dash.<br>DroneTeleport: Allows the player to teleport to the location of the drone. (requires RemoteDrone)<br>EtherealDash: Allows the player to dash through Ethereal Blocks.<br>GoldenFeather: Allows the player to use a feather to glide and ride wind currents.<br>GravityJacket: Allows the player to move freely in water and lava. (Liquid entity only)<br>HoverJet: Allows the remote drone to stop its falling speed and float for a short time. (requires RemoteDrone)<br>JumpBoost: Allows the remote drone to jump higher. (requires RemoteDrone)<br>IceBeam: Allows the remote drone beam to freeze some entities. (requires RemoteDrone)<br>LightningDash: Allows the player to horizontally dash in the opposite direction of the wall they are grabbing until they hit a solid wall or entity.<br>LongBeam: Allows the remote drone beam to reach further targets. (requires RemoteDrone)<br>MegaBombs: Allows the player to set powerful bombs that can destroy some walls and activate some blocks.<br>MissilesModule: Allows the remote drone to shoot missiles. (requires RemoteDrone)<br>PortableStation: Allows the player to teleport to any unlocked Warp Station from anywhere.<br>PowerGrip: Allows the player to grab.<br>RemoteDrone: Allows the player to control a small drone.<br>ScrewAttack: Allows the player to destroy some walls, to increase their horizontal speed, and to go through some obstacles without dying while jumping.<br>SpaceJump: Allows the player to jump again once in mid-air.<br>SpiderMagnet: Allows the player to grab magnetic ceilings.<br>SuperMissilesModule: Allows the remote drone to shoot super missiles. (requires RemoteDrone)<br>VariaJacket: Allows the player to survive in heated rooms and in lava. (Liquid entity only).<br>WaveBeam: Allows the remote drone beam to go through solids. (requires RemoteDrone) |
| **Name Color** | Hexadecimal code for the color of the upgrade name displayed when the player collects it. |
| **Desc Color** | Hexadecimal code for the color of the upgrade description displayed when the player collects it. |
| **Particle Color** | Hexadecimal code for the color of the background particles when the player collects this upgrade. |
| **Custom Name** | Dialog ID to use for the name of this upgrade, instead of the default one. Leave blank to use the default name. |
| **Custom Sprite** | The directory from which the sprites are taken instead of the default ones, relative to Atlases/Gameplay/ and Atlases/Gui/.<br>You need 2 sprites:<br>-The one in Atlases/Gameplay/ is the one visible in game and should be 16x16 pixels.<br>-The one in Atlases/Gui/ is the one displayed in the background when the player collects this upgrade and in the Status screen and should be 480x480 pixels.<br>The sprite names should be the same as the default name of the upgrade you have selected (as seen in the Upgrade attribute).<br>Leave blank to use default sprites. |
| **Map Shard Index** | Only used if Upgrade is set to MapShard. The index of this MapShard that will determine which rooms it reveals. |

### Upgrade Collectable (Resource)

A collectable that gives the player some resources used by other upgrades.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/.<br>A subfolder with the name of the Type (without spaces) must be present in this directory. |
| **Type** | Type of the resource that will be given to the player.<br>Energy Tank: Increases the player's stamina by 5.<br>Fire Rate Module: Increases the drone's fire rate by 15%.<br>Missile: Increases the drone's maximum missile capacity by 2.<br>Super Missile: Increases the drone's maximum super missile capacity by 1. |

### Upgrade Controller

Use this controller to tell the game your map is using abilities. You can also set the abilities the player will be limited to, and the ones the player will start with when grabbing a golden berry. You only need one Upgrade Controller per chapter.

| Attribute | Description |
|---|---|
| **Start With Bombs** | If true, the player will start with the Bombs upgrade. |
| **Start With Climbing Kit** | If true, the player will start with the Climbing Kit upgrade. |
| **Start With Dash Boots** | If true, the player will start with the Dash Boots upgrade. |
| **Start With Drone Teleport** | If true, the player will start with the Drone Teleport upgrade. |
| **Start With Golden Feather** | If true, the player will start with the Golden Feather upgrade. |
| **Start With Binoculars** | If true, the player will start with the Binoculars upgrade. |
| **Start With Ethereal Dash** | If true, the player will start with the Ethereal Dash upgrade. |
| **Start With Gravity Jacket** | If true, the player will start with the Gravity Jacket upgrade. |
| **Start With Hover Jet** | If true, the player will start with the Hover Jet upgrade. |
| **Start With Lightning Dash** | If true, the player will start with the Lightning Dash upgrade. |
| **Start With Portable Station** | If true, the player will start with the Portable Station upgrade. |
| **Start With Power Grip** | If true, the player will start with the Power Grip upgrade. |
| **Start With Remote Drone** | If true, the player will start with the Remote Drone upgrade. |
| **Start With Screw Attack** | If true, the player will start with the Screw Attack upgrade. |
| **Start With Space Jump** | If true, the player will start with the Space Jump upgrade. |
| **Start With Spider Magnet** | If true, the player will start with the Spider Magnet upgrade. |
| **Start With Varia Jacket** | If true, the player will start with the Varia Jacket upgrade. |
| **Start With Mega Bombs** | If true, the player will start with the Mega Bombs upgrade. |
| **Start With Long Beam** | If true, the player will start with the Long Beam upgrade. |
| **Start With Ice Beam** | If true, the player will start with the Ice Beam upgrade. |
| **Start With Wave Beam** | If true, the player will start with the Wave Beam upgrade. |
| **Start With Missiles Module** | If true, the player will start with the Missiles Module upgrade. |
| **Start With Super Missiles Module** | If true, the player will start with the Super Missiles Module upgrade. |
| **Start With Jump Boost** | If true, the player will start with the Jump Boost upgrade. |
| **Golden Start With Bombs** | If true, the player will temporarily lose all upgrades except the Bombs when carrying the golden berry. |
| **Golden Start With Climbing Kit** | If true, the player will temporarily lose all upgrades except the Climbing Kit when carrying the golden berry. |
| **Golden Start With Dash Boots** | If true, the player will temporarily lose all upgrades except the Dash Boots when carrying the golden berry. |
| **Golden Start With Drone Teleport** | If true, the player will temporarily lose all upgrades except the Drone Teleport when carrying the golden berry. |
| **Golden Start With Golden Feather** | If true, the player will temporarily lose all upgrades except the Golden Feather when carrying the golden berry. |
| **Golden Start With Binoculars** | If true, the player will temporarily lose all upgrades except the Binoculars when carrying the golden berry. |
| **Golden Start With Ethereal Dash** | If true, the player will temporarily lose all upgrades except the Ethereal Dash when carrying the golden berry. |
| **Golden Start With Gravity Jacket** | If true, the player will temporarily lose all upgrades except the Gravity Jacket when carrying the golden berry. |
| **Golden Start With Hover Jet** | If true, the player will temporarily lose all upgrades except the Hover Jet when carrying the golden berry. |
| **Golden Start With Lightning Dash** | If true, the player will temporarily lose all upgrades except the Lightning Dash when carrying the golden berry. |
| **Golden Start With Portable Station** | If true, the player will temporarily lose all upgrades except the Portable Station when carrying the golden berry. |
| **Golden Start With Power Grip** | If true, the player will temporarily lose all upgrades except the Power Grip when carrying the golden berry. |
| **Golden Start With Remote Drone** | If true, the player will temporarily lose all upgrades except the Remote Drone when carrying the golden berry. |
| **Golden Start With Screw Attack** | If true, the player will temporarily lose all upgrades except the Screw Attack when carrying the golden berry. |
| **Golden Start With Space Jump** | If true, the player will temporarily lose all upgrades except the Space Jump when carrying the golden berry. |
| **Golden Start With Spider Magnet** | If true, the player will temporarily lose all upgrades except the Spider Magnet when carrying the golden berry. |
| **Golden Start With Varia Jacket** | If true, the player will temporarily lose all upgrades except the Varia Jacket when carrying the golden berry. |
| **Golden Start With Mega Bombs** | If true, the player will temporarily lose all upgrades except the Mega Bombs when carrying the golden berry. |
| **Golden Start With Long Beam** | If true, the player will temporarily lose all upgrades except the Long Beam when carrying the golden berry. |
| **Golden Start With Ice Beam** | If true, the player will temporarily lose all upgrades except the Ice Beam when carrying the golden berry. |
| **Golden Start With Wave Beam** | If true, the player will temporarily lose all upgrades except the Wave Beam when carrying the golden berry. |
| **Golden Start With Missiles Module** | If true, the player will temporarily lose all upgrades except the Missiles Module when carrying the golden berry. |
| **Golden Start With Super Missiles Module** | If true, the player will temporarily lose all upgrades except the Super Missiles Module when carrying the golden berry. |
| **Golden Start With Jump Boost** | If true, the player will temporarily lose all upgrades except the Jump Boost when carrying the golden berry. |
| **Only Allow Starting Upgrades** | If true, any upgrades except those defined here will be deactivated when entering the chapter. |
| **Upgrades Are Temporary** | If true, the player will lose all upgrades acquired in this chapter when they leave it, and upgrades will be collectable again when they re-enter the chapter. |
| **Disable Status Screen** | If true, the status screen cannot be opened. |
| **No Reset Upgrades** | If true, 'Golden Start With...' attributes will be ignored when collecting a Golden Strawberry. |
| **Jacket Priority** | Defines which Jacket upgrade visually overrides the other.<br>Expected syntax: Gravity>Varia.<br>Allowed values: Varia, Gravity. Anything else is ignored. |
| **Double Dash Flag** | If this flag is set, the player will have 2 dashes instead of one with the Dash Boots upgrade.<br> Only checked on room entry and respawn. |

### Vine Head

The head of a vine that will follow the path made with Vine Path entities depending on the Light Mode or a flag. Must be placed on top of a Vine Path entity at the starting point.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Grow Speed** | The speed at which the head will move across its path. |
| **Pause Time** | The time (in seconds) the head will stop once the player bounces on it before resuming movement. |
| **Flag** | The flag that must be set for the head to start moving.<br>If empty, the head will move if the Light mode set by a Light Mode trigger is set to Light instead, or if the player touches a Light orb. |

### Vine Path

The path a Vine Head will follow when moving.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |

### Warp Station

A station to add a fast travel system to your map. Each time the player activates a warp station (by standing on it), it is added to a list of possible destinations from all other warp stations. It is also possible to fast travel to another chapter of the same campaign. If your campaign uses the In-Game map, it is displayed when the player interacts with the warp station to show where the destination really is. The list of destinations is saved in the save file. If the player carries a golden strawberry, all warp stations are temporarily inactive again and have to be reactivated to prevent any cheese. You can customise the wipe that plays during the teleport process.

| Attribute | Description |
|---|---|
| **Beam Color** | Hexadecimal code for the color of the beam that appears when the warp is activated. |
| **Confirm Sfx** | Sound event that plays when the player is teleported to another Warp Station.<br>Leave empty to use the default sound |
| **Flag** | This flag must be set to allow the player to activate the warp.<br>Leave empty to make this warp always activatable. |
| **Index** | Leave at 0 if this is the only Warp Station in this room.<br>If more than one Warp Station is in this room, set this to anything other than 0 and to a different value for each Warp Station in this room. |
| **Sprite** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Wipe Duration** | Duration of the wipe effect (in seconds). |
| **Wipe Type** | Wipe effect to use during the teleportation. |
| **No Beam** | If true, the beam will not appear when the warp is activated. |
| **Only Current Chapter** | If true, this warp will only allow the player to travel to the other Warp Stations unlocked in the same chapter as this one. |

### Work Robot

A solid robot that walks back and forth on a solid surface when a flag is set. Can be pushed by the Remote Drone missiles.

| Attribute | Description |
|---|---|
| **Directory** | The directory from which the sprites are taken, relative to Atlases/Gameplay/. |
| **Size** | Size of the robot. This also affects its hitbox. |
| **Flag** | The flag that must be set for the robot to start walking. Leave empty for no flag. |
| **Speed** | The speed at which the robot moves (Default: 20) |
| **Start Walk Left** | If true, the robot will start walking toward the left instead of the right. |

---

## Triggers

- [Bag Display Tutorial Trigger](#bag-display-tutorial-trigger)
- [Drone Destroy Tutorial Trigger](#drone-destroy-tutorial-trigger)
- [Flag On Interact Trigger](#flag-on-interact-trigger)
- [Global Flag Trigger](#global-flag-trigger)
- [Hide MiniMap Trigger](#hide-minimap-trigger)
- [Light Mode Trigger](#light-mode-trigger)
- [Multi Music Trigger](#multi-music-trigger)
- [Poem Trigger](#poem-trigger)
- [Reset Flags Trigger](#reset-flags-trigger)
- [Start Countdown Trigger](#start-countdown-trigger)
- [Stop Adjust In-Game Map Trigger](#stop-adjust-in-game-map-trigger)
- [Stop Countdown Trigger](#stop-countdown-trigger)
- [Stop Entity Respawn Trigger](#stop-entity-respawn-trigger)
- [Sub-Area Name Trigger](#sub-area-name-trigger)
- [Teleport To Chapter Trigger](#teleport-to-chapter-trigger)
- [Unlock Warp Trigger](#unlock-warp-trigger)
- [Upgrade Trigger](#upgrade-trigger)

### Bag Display Tutorial Trigger

A trigger that shows how to switch upgrades below the Bag Display UI.

| Attribute | Description |
|---|---|
| **Slot** | Determine under which slot the tutorial will appear on the player UI. |
| **Keep UI Open On Leave** | If true, the tutorial will stay opened even if the player leaves the trigger. |
| **Only Once** | If true, the trigger will remove itself once the player leaves it. |

### Drone Destroy Tutorial Trigger

A trigger that shows above the player how to destroy the drone when the player enters it.

| Attribute | Description |
|---|---|
| **Flag** | The flag that must be true for this trigger to show the tutorial.<br>Leave empty for no flag. |
| **Only Once** | If true, the trigger will remove itself once the player leaves it. |

### Flag On Interact Trigger

When the specified flags are set, the player can interact with this trigger to set or unset a flag of your choice.

| Attribute | Description |
|---|---|
| **Req Flags** | The flags that must already be set to allow the player to interact with this trigger, separated by a comma (,). |
| **Set Flag** | The flag that will be affected when the player interacts with this trigger. |
| **State** | If true, the flag will be set when the player interacts with this trigger, otherwise the flag will be unset. |

### Global Flag Trigger

When the player enters this trigger, it sets or unsets a flag that is active for all chapters of the current campaign. Useful to set a flag in one chapter that affects something in another chapter. Flags set by this trigger are automatically prefixed by `Ch[chapterIndex]_` where `[chapterIndex]` is the chapter index of the chapter this trigger is in. If the chapter is set as an Interlude, `[chapterIndex]` is 0.

| Attribute | Description |
|---|---|
| **Flag** | The flag affected when the player enters this trigger.<br>This flag will be automatically prefixed by 'Ch[chapterIndex]_' where [chapterIndex] is the chapter index of the chapter this trigger is in.<br>If the chapter is set as an Interlude, [chapterIndex] will be 0.<br>Example: If you type 'Flag' here and this trigger is in the first chapter of this campaign, the flag 'Ch1_Flag' will be set for all chapters of this campaign. |
| **State** | If true, the flag will be set when the player enters the trigger, otherwise the flag will be unset. |
| **Switch Flag** | If true, the flag will switch between being set and not set each time the player enters the trigger. |
| **Level Set** | The levelSet this flag will affect. Leave blank to use the levelSet corresponding to the current chapter. |

### Hide MiniMap Trigger

When the player is inside this trigger, the mini map is not visible regardless of what the player has set in mod options.

_No configurable attribute._

### Light Mode Trigger

A trigger that will change the Light Mode used by other entities.

| Attribute | Description |
|---|---|
| **Mode** | The mode that will be set when the player enters the trigger. |

### Multi Music Trigger

When the player enters this trigger, it changes the music played based on the state of 2 flags:
- Play a specific music only if one of the flags is set
- Play a specific music only if the other flag is set
- Play a specific music only if both flags are set
- Play a specific music only if none of the flags are set

| Attribute | Description |
|---|---|
| **Flag A** | The first flag this trigger will check. If empty, the trigger will have no effect. |
| **Flag B** | The second flag this trigger will check. Ignored if Flag A is empty. |
| **Track A** | The music event that plays when the player enters this trigger and ONLY the flag in Flag A is set. |
| **Track B** | The music event that plays when the player enters this trigger and ONLY the flag in Flag B is set. |
| **Track Both** | The music event that plays when the player enters this trigger and both flags in Flag A AND Flag B are set. |
| **Track None** | The music event that plays when the player enters this trigger and both flags in Flag A AND Flag B are NOT set. |
| **Remove When Outside** | If true, the trigger is removed if the player spawns or moves outside of it. |

### Poem Trigger

When the player enters this trigger, it displays a poem on screen (same effect as collecting a crystal heart) that can be customised.

| Attribute | Description |
|---|---|
| **Trigger Sound** | Sound event that plays when the player enters the trigger. |
| **Change Music** | If true, the current music will stop while the triggerSound plays when the player enters the trigger. |
| **New Music** | Only if changeMusic is set.<br>The music event that plays after the player enters the trigger. Leave blank to use the same music as the one that played before the player entered the trigger. |
| **Flag** | The flag that will be set when the player enters the trigger. |
| **End Chapter** | If true, the chapter will end when the player enters the trigger. |
| **Register In Save Data** | If true, the flag will be registered in the player Save file and will be set forever. |
| **Only Once** | If true, the trigger can only be activated once. |
| **Poem Name** | Dialog ID of the poem that is displayed when the player enters the trigger. |
| **Poem Color** | Color of the name of the poem that is displayed when the player enters the trigger. |
| **Poem Particles Color** | Colors of the particles displayed when the player enters the trigger. |
| **Poem Sprite** | The full path to the sprite that is displayed behind the poem, relative to Atlases/Gui/. |
| **Poem Sprite Animation Speed** | Delay between each frame of the animation of the sprite that is displayed behind the poem. |
| **Poem Sprite Animation Pause** | The number of frames to wait between each loop of the sprite that is displayed behind the poem. |

### Reset Flags Trigger

When the player enters this trigger, it sets the specified flags to true or false. Flags can also be added or removed from the player save file using this trigger. (Note: this is different from the Global Flag Trigger: flags set/unset by this trigger only affect the chapter of your campaign this trigger is in.) You can set this trigger to remove itself if the player is not inside of it, allowing it to work only if the player spawns inside of it, or when the player enters a room from a specific entrance.

| Attribute | Description |
|---|---|
| **Set True Flags** | The flags that will be set when entering the trigger, separated by a comma (,). |
| **Set False Flags** | The flags that will be unset when entering the trigger, separated by a comma (,). |
| **Transition Update** | If true, the trigger will also work during room transitions. |
| **Remove When Outside** | If true, the trigger is removed if the player spawns or moves outside of it. |
| **Register In Save Data** | If true, the flags in "Set True Flags" will be registered in the player save file and will be true forever. The flags in "Set False Flags" will be removed from the player save file. |
| **Condition Flags** | The flags that must be set for this trigger to take effect, separated by a comma (,).<br>Leave empty if you want this trigger to always take effect. |
| **Inverted** | If true, the flags specified in "Condition Flags" must not be set for this trigger to take effect. |

### Start Countdown Trigger

When the player enters this trigger, it starts a global countdown that kills the player and sends them back to the room the countdown started in if it reaches 0 before the player reaches a Stop Countdown Trigger. This countdown can display a message on screen before it starts, set a flag, shake the screen continuously, display random explosions on screen and can continue from one chapter to another if using Elevators, Warp Stations or a Teleport To Chapter Trigger.

| Attribute | Description |
|---|---|
| **Time** | Time (in seconds) of the countdown that will start once the player enters the trigger. |
| **Start Flag** | This flag must be set for the countdown to start. Leave empty to not require any flag. |
| **Active Flag** | This flag will be set to true while the countdown is ticking. |
| **Shake** | If true, the screen will be shaking while the countdown is ticking. |
| **Explosions** | If true, explosions will appear randomly on the screen while the countdown is ticking. |
| **Cross Chapter** | If true, the countdown will transfer from chapter to chapter if the player uses a Warp Station, an Elevator or enters a Teleport to Chapter Trigger. |
| **Dialog ID** | Dialog ID of the message that will be drawn on screen before the countdown starts.<br>Leave empty for no message. |
| **Message Timer** | Time (in seconds) during which the message will be shown on screen (including the time needed to write the message).<br>The countdown will start 2.5 seconds before this time ends. |
| **Fast Message Display** | If true, the message will be drawn on screen faster. |
| **Message Color** | Hexadecimal code for the color of the message. |
| **Hide Flag** | If this flag is set, the countdown will not be displayed on screen but stay active. |
| **Events Flags** | The flags that must be set when the countdown reaches a specific time (in seconds), separated by a comma (,).<br>The syntax must be: time:flag - Example: 50:flag1,30:flag2 |

### Stop Adjust In-Game Map Trigger

When the player enters this trigger, the room they are in is no longer adjusted on the map. Can be used to reveal some entrances, or change the size of the room on the map, for example.

_No configurable attribute._

### Stop Countdown Trigger

When the player enters this trigger, it stops any current countdown.

_No configurable attribute._

### Stop Entity Respawn Trigger

When the player enters this trigger, specific entities in the room of your choice will no longer spawn. This currently works only with Break Blocks.

| Attribute | Description |
|---|---|
| **Room** | Entities in this room that are waiting to stop respawning will no longer spawn once the player enters the trigger.<br>Compatible entities: Break Block |

### Sub-Area Name Trigger

When the player enters this trigger, a text is displayed on screen for a short time (customisable). Used to display the name of a sub-area of your map. The game remembers which sub-area the player is in, and won't display the name if the player is already in the area the trigger is supposed to show.

| Attribute | Description |
|---|---|
| **Dialog ID** | Dialog ID used to display the name of the area. |
| **Timer** | How long the text stays on screen. |
| **Text Position X** | X position of the text. |
| **Text Position Y** | Y position of the text. (between 80 for the top of the screen and 1080 for the bottom of the screen) |

### Teleport To Chapter Trigger

When the player enters this trigger or interacts with it, they are teleported to the chapter of your choice, in the room of your choice. You can customise the wipe that plays during the teleport process.

| Attribute | Description |
|---|---|
| **Destination Chapter** | The path to the map bin the player will be teleported into.<br>Corresponds to the structure in your map folder (usually yourName/campaignName/mapName) without the .bin extension.<br>This also works if the map you try to teleport into is in another campaign. |
| **Destination Room** | Name of the room the player will be transported into. If empty, the starting room of the corresponding chapter will be used. |
| **Spawn Room X** | X position of the spawn point where the player will appear in the destination room. |
| **Spawn Room Y** | Y position of the spawn point where the player will appear in the destination room. |
| **To Chapter** | Which chapter the player will be teleported to. 0 = interlude. |
| **Wipe Duration** | Duration of the wipe effect (in seconds). |
| **Wipe Type** | Wipe effect to use during the teleportation. |
| **Register Current Chapter As Compelete** | If true, the chapter will be considered complete before teleporting the player. |
| **Can Interact** | If true, the player will need to interact with the trigger to get teleported instead of just entering the trigger. |

### Unlock Warp Trigger

When the player enters this trigger, it activates the specified warp. The player can then use Warp Stations to warp to this new location even if they did not have it activated before.

| Attribute | Description |
|---|---|
| **Chapter** | Chapter index of the warp to unlock. Use 0 for interludes. |
| **Room** | Name of the room containing the warp to unlock. |
| **Index** | Index of the warp to unlock in the corresponding room. Leave 0 if there is only one warp in that room. |

### Upgrade Trigger

When the player enters this trigger, they get the specified upgrade immediately. Refer to the Upgrade Collectable section for a list of all upgrades available. This trigger can also remove the specified upgrade from the player instead.

| Attribute | Description |
|---|---|
| **Upgrade** | The upgrade that will be granted to the player. Select "All" to give all upgrades.<br>Binoculars: Allows the player to use binoculars to preview the room at any time.<br>Bombs: Allows the player to set bombs that can destroy some walls.<br>ClimbingKit: Allows the player to climb walls. (requires PowerGrip)<br>DashBoots: Allows the player to dash.<br>DroneTeleport: Allows the player to teleport to the location of the drone. (requires RemoteDrone)<br>EtherealDash: Allows the player to dash through Ethereal Blocks.<br>GoldenFeather: Allows the player to use a feather to glide and ride wind currents.<br>GravityJacket: Allows the player to move freely in water and lava. (Liquid entity only)<br>HoverJet: Allows the remote drone to stop its falling speed and float for a short time. (requires RemoteDrone)<br>JumpBoost: Allows the remote drone to jump higher. (requires RemoteDrone)<br>IceBeam: Allows the remote drone beam to freeze some entities. (requires RemoteDrone)<br>LightningDash: Allows the player to horizontally dash in the opposite direction of the wall they are grabbing until they hit a solid wall or entity.<br>LongBeam: Allows the remote drone beam to reach further targets. (requires RemoteDrone)<br>MegaBombs: Allows the player to set powerful bombs that can destroy some walls and activate some blocks.<br>MissilesModule: Allows the remote drone to shoot missiles. (requires RemoteDrone)<br>PortableStation: Allows the player to teleport to any unlocked Warp Station from anywhere.<br>PowerGrip: Allows the player to grab.<br>RemoteDrone: Allows the player to control a small drone.<br>ScrewAttack: Allows the player to destroy some walls, to increase their horizontal speed, and to go through some obstacles without dying while jumping.<br>SpaceJump: Allows the player to jump again once in mid-air.<br>SpiderMagnet: Allows the player to grab magnetic ceilings.<br>SuperMissilesModule: Allows the remote drone to shoot super missiles. (requires RemoteDrone)<br>VariaJacket: Allows the player to survive in heated rooms and in lava. (Liquid entity only).<br>WaveBeam: Allows the remote drone beam to go through solids. (requires RemoteDrone) |
| **Disable** | If true, disable the specified upgrade if the player has it. (Or all upgrades if Upgrade is set to "All") |
| **Only Once** | If true, the trigger can only be activated once. |

---

## Effects

### Heat Particles

Same effect as the vanilla Heatwave effect, but without the screen distortion and the Hot colorgrade. You can set the amount of particles, their colors and if you want the mist to show or not.

| Attribute | Description |
|---|---|
| **Particles Colors** | The two hexadecimal codes for the colors of the particles, separated by a comma (,). |
| **Particles Amount** | Number of particles to display. |

### Glow

An effect intended to be used primarily for backgrounds, darkening and then lightening them repeatedly to give the illusion that they are glowing. The color can be changed to allow usages for other purposes.

| Attribute | Description |
|---|---|
| **Color** | Color of the glow. |
