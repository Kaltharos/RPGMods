# RPGMods
### [Original ChatCommands Repository](https://github.com/NopeyBoi/ChatCommands)
### Server Only Mod
Server only mod for RPG system which also include ChatCommands with bug fixes.\
Read the changelog for extra details.
#### [Video Demo of Experience & Mastery](https://streamable.com/k2p3bm)

## Experience System
Disable the VRising Gear Level system and replace it with a traditional RPG experience system,\
complete with exp sharing between clan members or other player designated as ally.

## Mastery System
### Weapon Mastery (NEW - Should be tested first!)
Mastering a weapon will now progressively give extra bonus to the character stats.
- Physical weapon (Sword, Spear, Fist, Schyte, Axes, Slashers, Mace, Fishing Pole) will give extra physical power. (100% -> 10 Power)
- Spell (Global/All Spells) will reduce the cooldown of all skills. (100% -> 50% cooldown reduction)

Weapon mastery will increase when the weapon is used to kill a creature, and while in combat to a maximum of 60 seconds. (0.001%/Sec)\
Spell mastery can only increase and take effect when no weapon is equipped.
### Defensive Mastery
TBA
### Mastery Decay
When the vampire goes to sleep (offline), all their mastery will continuously decay per minute passed while offline.\
This decay will keep on counting even while the server is offline.

## HunterHunted System
A new system where every NPC you killed contribute to a heat system,\
if you kill too many NPC from that faction, eventually your heat level will raise higher and higher.

The higher your heat level is, a more difficult squad of ambushers will be sent by that faction to kill you.\
Heat level will eventually cooldown the longer you went without killing NPCs from that faction,\
space your kills so you don't get hunter by an extremely elite group of assassins.

Otherwise, if you are dead for any reason at all, your heat/wanted level will reset back to anonymous.\
`-- Note` Ambush may only occur when the player is in combat.

## Kill Announcer & PvP Statistics
Every PvP kill will be announced server wide to all users.\
Kill/Death will also be recorded, and a ladder board for the Top 5 K/D in the server.

## Config
### Basic
- `Prefix` [default `.`]\
The prefix use for chat commands.
- `Command Delay` [default `5`]\
The number of seconds user need to wait out before sending another command.\
Admin will always bypass this.
- `DisabledCommands` [default `empty`]\
Enter command names to disable them. Seperated by commas.
- `WayPoint Limits` [default `3`]\
Set a waypoint limit per user.
### PvP
- `Announce PvP Kills` [default `true`]\
Do I really need to explain this...?
- `Enable the PvP Ladder` [default `true`]\
Hmm... well it enables the ladder board in .pvp command
### Hunter Hunted
- `Enable` [default `true`]\
Enable/disable the HunterHunted system.
- `Heat Cooldown Value` [default `35`]\
Set the reduction value for player heat for every cooldown interval.
- `Bandit Heat Cooldown Value` [default `35`]\
Set the reduction value for player heat from the bandits faction for every cooldown interval.
- `Cooldown Interval` [default `60`]\
Set every how many seconds should the cooldown interval trigger.
- `Ambush Interval` [default `300`]\
Set how many seconds player can be ambushed again since last ambush.
- `Ambush Chance` [default `50`]\
Set the percentage that an ambush may occur for every cooldown interval.
### Experience
- `Enable` [default `true`]\
Enable/disable the Experience system.
- `Max Level` [default `80`]\
Configure the experience system max level..
- `Multiplier` [default `1`]\
Multiply the experience gained by the player.
- `VBlood Multiplier` [default `15`]\
Multiply the experience gained from VBlood kills.
- `EXP Lost / Death` [default `0.10`]\
Percentage of experience the player lost for every death by NPC, no EXP is lost for PvP.
- `Constant` [default `0.2`]\
Increase or decrease the required EXP to level up.\
[EXP Table & Formula](https://bit.ly/3npqdJw)
- `Group Modifier` [default `0.75`]\
Set the modifier for EXP gained for each ally(player) in vicinity.\
Example if you have 2 ally nearby, EXPGained = ((EXPGained * Modifier)*Modifier)
- `Ally Max Distance` [default `50`]\
Set the maximum distance an ally(player) has to be from the player for them to share EXP with the player
### Mastery
- `Enable Weapon Mastery` [default `true`]\
Enable/disable the weapon mastery system.
- `Enable Mastery Decay` [default `true`]\
Enable/disable the decay of weapon mastery when the user is offline.
- `Max Mastery Value` [default `100000`]\
Configure the maximum mastery the user can atain. (100000 is 100%)
- `Mastery Value/Combat Ticks` [default `5`]\
Configure the amount of mastery gained per combat ticks. (5 -> 0.005%)
- `Max Combat Ticks` [default `12`]\
Mastery will no longer increase after this many ticks is reached in combat. (1 tick = 5 seconds)
- `Mastery Multiplier` [default `1`]\
Multiply the gained mastery value by this amount.
- `VBlood Mastery Multiplier` [default `15`]\
Multiply Mastery gained from VBlood kill.
- `Decay Interval` [default `60`]\
Every amount of seconds the user is offline by the configured value will translate as 1 decay tick.
- `Decay Value` [default `1`]\
Mastery will decay by this amount for every decay tick. (1 -> 0.001%)


## Permissions
You can only decide whether a command is admin only or not at this time.\
The permissions are saved in `BepInEx/config/RPGMods/permissions.json` and look like this:
```json
{
  "help": false,
  "speed": true,
  "kit": true,
  "blood": true,
  "heat": false,
  "ping": false,
  "pvp": false,
  "save": true,
  "autorespawn": false,
  "waypoint": false,
  "wp": false,
  "health": true,
  "hp": true,
  "give": true,
  "g": true,
  "bloodpotion": true,
  "bp": true,
  "sunimmunity": true,
  "sun": true,
  "spawnnpc": true,
  "snp": true,
  "nocooldown": true,
  "nocd": true,
  "resetcooldown": true,
  "cd": true,
  "teleport": false,
  "tp": false,
  "godmode": true,
  "god": true,
  "experience": false,
  "exp": false,
  "xp": false,
  "mastery": false,
  "m": false
}
```
Removing a command from the list will automatically set it's value to `false`.

## Chat Commands
`help [<Command>]`: Shows a list of all commands.\
`kit <Name>`: Gives you a previously specified set of items.
<details>
<summary>How does kit work?</summary>

&ensp;&ensp;You will get a new config file located in `BepInEx/config/RPGMods/kits.json`
```json
[
  {
    "Name": "Example1",
    "PrefabGUIDs": {
      "820932258": 50, <-- 50 Gem Dust
      "2106123809": 20 <-- 20 Ghost Yarn
    }
  },
  {
    "Name": "Example2",
    "PrefabGUIDs": {
      "x1": y1,
      "x2": y2
    }
  }
]
```

</details>

`blood <BloodType> [<Quality>] [<Value>]`: Sets your Blood type to the specified Type, Quality and Value.\
&ensp;&ensp;**Example:** `blood Scholar 100 100`

`bloodpotion <BloodType> [<Quality>]`: Creates a Potion with specified Blood Type, Quality and Value.\
&ensp;&ensp;**Example:** `bloodpotion Scholar 100`

`waypoint <Name|Set|Remove|List> [<Name>] [global]`: Teleports you to previously created waypoints.\
&ensp;&ensp;**Example:** `waypoint set home` <-- Creates a local waypoint just for you.\
&ensp;&ensp;**Example:** `waypoint set arena global` <-- Creates a global waypoint for everyone (Admin-Only).\
&ensp;&ensp;**Example:** `waypoint home` <-- Teleports you to your local waypoint.\
&ensp;&ensp;**Example:** `waypoint remove home` <-- Removes your local waypoint.\
&ensp;&ensp;**Example:** `waypoint list` <-- Shows a list of all to you accessible waypoints.

`give <Item Name> [<Amount>]`: Adds the specified Item to your Inventory.\
&ensp;&ensp;**Example:** `give Stone Brick 17`

`spawnnpc <Prefab Name> [<Amount>] [<Waypoint>]`: Spawns a NPC. Optional: To a previously created waypoint.\
&ensp;&ensp;**Example:** `spawnnpc CHAR_Cursed_MountainBeast_VBlood 1 arena`

`health <Amount>`: Sets your health to the specified amount.\
`speed`: Toggles speed buff.\
`sunimmunity`: Toggles sun immunity.\
`nocooldown`: Toggles all skills & abilities to have no cooldown.\
`resetcooldown [<PlayerName>]`: Reset all skills & abilities cooldown for you or the specified player.\
`teleport <PlayerName>`: Teleport to another online player within your clan.\
`godmode`: Toggles god mode for you.\
`autorespawn [<All>|<PlayerName>]`: Toggles auto respawn on same position on death.`\
&ensp;&ensp;**Admin Only Params -> `[<All>|<PlayerName>]`** `Toggle the auto respawn for specified player or server wide.

`heat`: Checks your heat/wanted level by the factions.\
&ensp;&ensp;**Admin Only Params -> `[<debug>|<value> <value> [<PlayerName>]]`** `Display current configuration or set your or the specified player heat value.`\
&ensp;&ensp;**Example:** `heat 500 500`\
&ensp;&ensp;**Example:** `heat 500 500 LegendaryVampire`

`ping`: Show you your latency to the server.\
`pvp [<on>|<off>]`: Toggles PvP or display your PvP statistics & the current leaders in the ladder.\
`experience [<log> <on>|<off>]`: Diplays your current exp and progression to the next level, or toggle the exp gain notification.\
&ensp;&ensp;**Admin Only Params -> `[<set>] [<value>] [<PlayerName>]`** `Set your or the specified player experience value.`\
&ensp;&ensp;**Example:** `experience set 1000`\
&ensp;&ensp;**Example:** `experience set 2000 LegendaryVampire`

`mastery [<log> <on>|<off>]`: Display your current mastery progression, or toggle the mastery gain notification.\
&ensp;&ensp;**Admin Only Params -> `[<set>] [<type>] [<value>] [<PlayerName>]`** `Set your or the specified player mastery value.`\
&ensp;&ensp;**Example:** `mastery set sword 100000`\
&ensp;&ensp;**Example:** `mastery set spear 2000 LegendaryVampire`

`save`: Trigger the database saving manually.

## More Information
<details>
<summary>Changelog</summary>

`0.1.4`
- Added Weapon Mastery system.
- Disabled EXP/Mastery gain from summoned creatures.
- Added EXP & Mastery gain logs for players.
- Changed some 'notification' type of message into Lore chat type.
- Added capabilities to change other player heat values.
- Added mastery command.
- Added a new abreviation for experience command. (exp)

`0.0.3`
- Fixed bug with chat cooldown being applied twice the value of the config
- Fixed bug with waypoint limits.
- Fixed bug with PvPStats recording.
- Fixed bug with teleport command.
- PvPKD should display decimals properly now.

`0.0.2`
- Fixed bug on allies checking when it was called if plugin was never reloaded with Wetstone.

`0.0.1`
- Added command delay timer
- Integrated the data saving into the GameServer autosave & shutdown
- All saved data will now use SteamID as key for compability with character name changes
- Added Experience system
- Changed SunImmunity behavior, there's no more persistent sun immunity with this
- Added GodMode command
- Added HunterHunted (Wanted Level) system
- Added PvP stats & leaderboard system for it
- Added PvP kill serverwide announcement
- Added ping command to check for latency against the server
- Added autorespawn command
- Added nocooldown command
- Added resetcooldown command
- Fixed blood command to apply the bloodtype buff and avoid BloodHunger HUD bug
- Optimized NPC spawn system, it will not lag the server anymore
- Modified NPC spawn command to accept amount to spawn
- Fixed NPC spawn command to be able to spawn normal units
- Hide commands from user that do not have sufficient priviledge to use the command
- Disabled waypoint command for user in combat
- Modified waypoint command to "instance" the waypoint name
- Admin ignore waypoint limit
- Modified health command to be able to affect specified player or kill them by setting their HP to 0
- Some other thing that i may not be able to remember

</details>

<details>
<summary>Contributor</summary>

### [Discord](https://discord.gg/XY5bNtNm4w)
#### Without these people, this project will just be a dream. (In no particular order)
- Dimentox#1154
- Nopey#1337
- syllabicat#0692
- errox#7604

</details>

<details>
<summary>Known Issues</summary>

### General
- Resetcooldown command does not refresh skills that has charges.
- Blood command cannot apply "fragile" blood type.

### Experience System
- Some blood buff give a gear level to the character, which would be fixed once they kill something or re-equip accessory.

### HunterHunted System
- There's no known issue yet. Heat level does get reset if you reload the plugin/restart server, this is an intended behaviour.

</details>

<details>
<summary>Planned Features</summary>

- Chat permission roles.
- Kits Option: Limited Uses.
- More optimization! It never hurts to optimize!
- Add ban command with duration.
- Explore team/alliance in VRising.
- Hook into whatever system possible to add a tag to player names.
- Other defensive mastery system.

</details>