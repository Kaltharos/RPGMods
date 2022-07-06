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
### Weapon Mastery
Mastering a weapon will now progressively give extra bonus to the character stats.\
Weapon mastery will increase when the weapon is used to kill a creature, and while in combat to a maximum of 60 seconds. (0.001%/Sec)\
Spell mastery can only increase and take effect when no weapon is equipped.
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

## PvP System
Configurable PvP kill serverwide announcement.\
Kill/Death will also be recorded, and a ladder board for the Top 5 K/D in the server.

Additionally there's a punishment system which can be used to punish player who kill lower level player,\
which is configurable in the config.\
Punishment will apply a debuff that reduce player combat effeciency.
- `-25%` Physical & spell power
- `-15` Physical, spell, holy, and fire resistance
- Gear level down (Overriden by EXP system if active)

## Config
<details>
<summary>Basic</summary>

- `Prefix` [default `.`]\
The prefix use for chat commands.
- `Command Delay` [default `5`]\
The number of seconds user need to wait out before sending another command.\
Admin will always bypass this.
- `DisabledCommands` [default `empty`]\
Enter command names to disable them. Seperated by commas.
- `WayPoint Limits` [default `3`]\
Set a waypoint limit per user.

</details>

<details>
<summary>PvP</summary>

- `Announce PvP Kills` [default `true`]\
Do I really need to explain this...?
- `Enable the PvP Ladder` [default `true`]\
Hmm... well it enables the ladder board in .pvp command
- `Enable PvP Punishment` [default `true`]\
Enables the punishment system for killing lower level player.
- `Punish Level Difference` [default `-10`]\
Only punish the killer if the victim level is this much lower.
- `Offense Limit` [default `3`]\
Killer must make this many offense before the punishment debuff is applied.
- `Offense Cooldown` [default `300`]\
Reset the offense counter after this many seconds has passed since last offense.
- `Debuff Duration` [default `1800`]\
Apply the punishment debuff for this amount of time.


</details>

<details>
<summary>HunterHunted</summary>

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

</details>

<details>
<summary>Experience</summary>

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

</details>

<details>
<summary>Mastery</summary>

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

</details>

## Permissions
You can only decide whether a command is admin only or not at this time.\
The permissions are saved in `BepInEx/config/RPGMods/permissions.json` and look like this:

<details>
<summary>Default Permission - Don't forget to copy!</summary>

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
  "punish": true,
  "autorespawn": true,
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

</details>

Removing a command from the list will automatically set it's value to `false`.

## Chat Commands

<details>
<summary>help</summary>

`help [<command>]`\
Shows a list of all commands.\
&ensp;&ensp;**Example:** `help experience`

</details>

<details>
<summary>kit</summary>

`kit <name>`\
Gives you a previously specified set of items.\
&ensp;&ensp;**Example:** `kit starterset`

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

</details>

<details>
<summary>blood</summary>

`blood <bloodtype> [<quality>] [<value>]`\
Sets your Blood type to the specified Type, Quality and Value.\
&ensp;&ensp;**Example:** `blood Scholar 100 100`

</details>

<details>
<summary>bloodpotion</summary>

`bloodpotion <bloodtype> [<quality>]`\
Creates a Potion with specified Blood Type, Quality and Value.\
&ensp;&ensp;**Example:** `bloodpotion Scholar 100`

</details>

<details>
<summary>waypoint</summary>

`waypoint <name|set|remove|list> [<name>] [global]`\
Teleports you to previously created waypoints.\
&ensp;&ensp;**Example:** `waypoint set home` <-- Creates a local waypoint just for you.\
&ensp;&ensp;**Example:** `waypoint set arena global` <-- Creates a global waypoint for everyone (Admin-Only).\
&ensp;&ensp;**Example:** `waypoint home` <-- Teleports you to your local waypoint.\
&ensp;&ensp;**Example:** `waypoint remove home` <-- Removes your local waypoint.\
&ensp;&ensp;**Example:** `waypoint list` <-- Shows a list of all to you accessible waypoints.

</details>

<details>
<summary>give</summary>

`give <itemname> [<amount>]`\
Adds the specified Item to your Inventory.\
&ensp;&ensp;**Example:** `give Stone Brick 17`

</details>

<details>
<summary>spawnnpc</summary>

`spawnnpc <prefabname> [<amount>] [<waypoint>]`\
Spawns a NPC. Optional: To a previously created waypoint.\
&ensp;&ensp;**Example:** `spawnnpc CHAR_Cursed_MountainBeast_VBlood 1 arena`

</details>

<details>
<summary>health</summary>

`health <percentage> [<playername>]`\
Sets your health to the specified percentage (0 will kill the player).\
&ensp;&ensp;**Example:** `health 100`\
&ensp;&ensp;**Example:** `health 0 LegendaryVampire`

</details>

<details>
<summary>speed</summary>

`speed`\
Toggles speed buff.

</details>

<details>
<summary>sunimmunity</summary>

`sunimmunity`\
Toggles sun immunity.

</details>

<details>
<summary>nocooldown</summary>

`nocooldown`\
Toggles all skills & abilities to have no cooldown.

</details>

<details>
<summary>resetcooldown</summary>

`resetcooldown [<playername>]`\
Reset all skills & abilities cooldown for you or the specified player.\
&ensp;&ensp;**Example:** `resetcooldown`\
&ensp;&ensp;**Example:** `resetcooldown LegendaryVampire`

</details>

<details>
<summary>teleport</summary>

`teleport <playername>`\
Teleport to another online player within your clan.\
&ensp;&ensp;**Example:** `teleport LegendaryVampire`

</details>

<details>
<summary>godmode</summary>

`godmode`\
Toggles god mode for you.

</details>

<details>
<summary>autorespawn</summary>

`autorespawn`\
Toggles auto respawn on same position on death.\
&ensp;&ensp;**Admin Only Params -> `[<all>|<playername>]`** `Toggle the auto respawn for specified player or server wide.`\
&ensp;&ensp;**Example:** `autorespawn all`\
&ensp;&ensp;**Example:** `autorespawn LegendaryVampire`

</details>

<details>
<summary>heat</summary>

`heat`\
Checks your heat/wanted level by the factions.\
&ensp;&ensp;**Admin Only Params -> `[<debug>|<value> <value> [<PlayerName>]]`** `Display numeric heat or set your or the specified player heat.`\
&ensp;&ensp;**Example:** `heat 500 500`\
&ensp;&ensp;**Example:** `heat 500 500 LegendaryVampire`

</details>

<details>
<summary>ping</summary>

`ping`\
Show you your latency to the server.

</details>

<details>
<summary>pvp</summary>

`pvp [<on>|<off>]`\
Toggles PvP or display your PvP statistics & the current leaders in the ladder.\
&ensp;&ensp;**Example:** `pvp`\
&ensp;&ensp;**Example:** `pvp off`

</details>

<details>
<summary>experience</summary>

`experience [<log> <on>|<off>]`\
Diplays your current exp and progression to the next level, or toggle the exp gain notification.\
&ensp;&ensp;**Example:** `experience`\
&ensp;&ensp;**Example:** `experience log off`

&ensp;&ensp;**Admin Only Params -> `[<set> <value> [<PlayerName>]]`** `Set your or the specified player experience value.`\
&ensp;&ensp;**Example:** `experience set 1000`\
&ensp;&ensp;**Example:** `experience set 2000 LegendaryVampire`

</details>

<details>
<summary>mastery</summary>

`mastery [<log> <on>|<off>]`\
Display your current mastery progression, or toggle the mastery gain notification.\
&ensp;&ensp;**Example:** `mastery`\
&ensp;&ensp;**Example:** `mastery log off`

&ensp;&ensp;**Admin Only Params -> `[<set> <type> <value> [<PlayerName>]]`** `Set your or the specified player mastery value.`\
&ensp;&ensp;**Example:** `mastery set sword 100000`\
&ensp;&ensp;**Example:** `mastery set spear 2000 LegendaryVampire`

</details>

<details>
<summary>save</summary>

`save`\
Trigger the database saving manually.

</details>

<details>
<summary>punish</summary>

`punish <playername> [<remove>]`\
Manually punish someone or lift their debuff.\
This command may still be used even when punishment system is disabled.\
&ensp;&ensp;**Example:** `punish LegendaryVampire`\
&ensp;&ensp;**Example:** `punish LegendaryVampire remove`

</details>

## More Information
<details>
<summary>Changelog</summary>

`0.2.0`
- Fixed typo in mastery commands for setting Schyte mastery.
- Added PvP punishment system.
- Changed PvP system to hook from downed player instead of killed player.
- Fixed bug in mastery decay not being disabled when mastery system is not enabled.
- Fixed bug in mastery command that still report mastery status even when the system is disabled.

`0.1.6`
- Commands & permission are no longer case sensitive. F*ck...

`0.1.5`
- Introduced a mechanic to randomize mastery gain from creature kills.
- Fixed issue on mastery gain on player death.
- Fleshed out the weapon mastery bonus.

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

### PvP System
- Punishment debuff lower the player gear level, which will be override by the experience system if the exp system is active.

</details>

<details>
<summary>Planned Features</summary>

- Chat permission roles.
- Kits Option: Limited Uses.
- More optimization! It never hurts to optimize!
- Add ban command with duration.
- Explore team/alliance in VRising.
- Hook into whatever system possible to add a tag to player names.

</details>