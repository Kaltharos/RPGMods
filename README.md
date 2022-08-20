# RPGMods
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
Kill/Death will also be recorded, and a ladder board for the Top 10 K/D in the server.
### Toggle PvP Commnd
If enabled, players can toggle their pvp status on/off via the pvp command.\
If their pvp status is off, they are excluded from all PvP damage.\
Your server must be configured as a PvP server for the toggle to work,\
otherwise player will never be able to do any pvp damage despite toggling pvp status to be on.

PvP toggle will be overriden by `Hostility Mode` if honor system is active.
### Punishment System
Additionally there's a punishment system which can be used to punish player who kill lower level player,\
which is configurable in the config.\
Punishment will apply a debuff that reduce player combat effeciency.
- `-25%` Physical & spell power
- `-15` Physical, spell, holy, and fire resistance
- Gear level down (Overriden by EXP system if active)
### Honor System
All vampires start with `Neutral` honor rank.\
Killing a vampire with neutral or positive honor rank will deduct some honor points,\
while killing a vampire with negative honor rank will reward the player with some honor points.\
Other way to gain honor is by grinding mobs. Each mobs kill will give 1 honor points.\
There's a hard limit of `250p/hours` gain to prevent grind.

The honor title is added as a prefix to the player name.\
All chat commands which is included within RPGMods should still be used without the\
honor title prefix if a player name is required.\
Other stuff like whispering other player does require the title prefix to be used.

Honor title prefix is not automatically updated for chat messages,\
everything else like building ownership and hovering player name is automatically updated.

For all the mechanic to work correctly, please set your server settings to:
- `Game Mode: PvP`
- `Castle Damage Mode: Always`
- `Player Damage Mode: Always`
#### Hostility Mode
`[ON] Aggressive`\
Can damage any player.\
No reputation loss will be given to the aggressor when killed.

`[OFF] Passive`\
Cannot damage other players with positive reputation.
#### Castle Siege
`[ON] Sieging`\
Player castle(s) are now vulnerable, and can damage other sieging player castle(s).\
Aggressive state is enforced during siege time.\
Siege mode cannot be exited until 3 hours window has passed since activation.

`[OFF] Defensive`\
Exit castle siege mode.\
Castle(s) are now invulnerable.\
Player are able to enter passive state again.

`Global Siege`\
On global siege mode, all castle is vulnerable unless the player honor bonus says something else.\
Player aggressive state is not enforced during global siege.
#### All Honor Titles
- Glorious `[Req: 10000] [Reward: -1000]` `Castle is permanently invulnerable.`
- Noble `[Req: 5000] [Reward: -500]` `Castle receive -50% reduced damage.`
- Virtuous `[Req: 1500] [Reward: -100]` `+15% resource gathering`
- Reputable `[Req: 500] [Reward: -50]` `-25% durability loss`
- Neutral `[Req: 0] [Reward: -25]`
- Suspicious `[Req: -1] [Reward: 0]`
- Infamous `[Req: -1000] [Reward: 10]` `Enforced aggresive state.`
- Villainous `[Req: -3000] [Reward: 50]` `-20% damage taken from positive rep vampires.`
- Nefarious `[Req: -10000] [Reward: 100]` `+20% damage dealt to positive rep vampires.`
- Dreaded `[Req: -20000] [Reward: 150]` `Enforced castle siege participation`

## Command Permission & VIP Login Whitelist
Commands be configured to require a minimum level of permission for the user to be able to use them.\
When there's no minimum permission set in the `command_permission.json`, it will default to a minimum requirement of permission lv. 100.

VIP System when enabled, will enable the user with permission level higher or equal to the minimum requirement set in to config,\
to be able to bypass server capacity.

Permission level range from 0 to 100.\
With 0 as the default permission for user (lowest),\
and 100 as the highest permission (admin).

## Custom Ban System
You can now ban a player for the specified duration in days using the .ban/.unban command.\
`WARNING` if you remove RPGMods all the banned user via the chat command will no longer be banned!

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
<summary>VIP</summary>

- `Enable VIP System` [default `false`]\
Enable the VIP System.
- `Enable VIP Whitelist` [default `false`]\
Enable the VIP user to ignore server capacity limit.
- `Minimum VIP Permission` [default `10`]\
The minimum permission level required for the user to be considered as VIP.

<details>
<summary>-- VIP.InCombat Buff</summary>

- `Durability Loss Multiplier` [default `0.5`]\
Multiply durability loss when user is in combat. -1.0 to disable.\
Does not affect durability loss on death.
- `Garlic Resistance Multiplier` [default `-1.0`]\
Multiply garlic resistance when user is in combat. -1.0 to disable.
- `Silver Resistance Multiplier` [default `-1.0`]\
Multiply silver resistance when user is in combat. -1.0 to disable.
- `Move Speed Multiplier` [default `-1.0`]\
Multiply move speed when user is in combat. -1.0 to disable.
- `Resource Yield Multiplier` [default `2.0`]\
Multiply resource yield (not item drop) when user is in combat. -1.0 to disable.

</details>

<details>
<summary>-- VIP.OutCombat Buff</summary>

- `Durability Loss Multiplier` [default `0.5`]\
Multiply durability loss when user is out of combat. -1.0 to disable.\
Does not affect durability loss on death.
- `Garlic Resistance Multiplier` [default `2.0`]\
Multiply garlic resistance when user is out of combat. -1.0 to disable.
- `Silver Resistance Multiplier` [default `2.0`]\
Multiply silver resistance when user is out of combat. -1.0 to disable.
- `Move Speed Multiplier` [default `1.25`]\
Multiply move speed when user is out of combat. -1.0 to disable.
- `Resource Yield Multiplier` [default `2.0`]\
Multiply resource yield (not item drop) when user is out of combat. -1.0 to disable.

</details>

</details>

<details>
<summary>PvP</summary>

- `Announce PvP Kills` [default `true`]\
Make a server wide announcement for all PvP kills.
- `Enable Honor System` [default `false`]\
Enable the honor system.
- `Max Honor Gain/Hour` [default `250`]\
Maximum amount of honor points the player can gain per hour.
- `Enable Honor Benefit & Penalties` [default `true`]\
If disabled, the hostility state and custom siege system will be disabled.\
All other bonus is also not applied.
- `Custom Siege Duration` [default `180`]\
In minutes. Player will automatically exit siege mode after this many minutes has passed.\
Siege mode cannot be exited while duration has not passed.
- `Enable Hostile Glow` [default `true`]\
When set to true, hostile players will glow red.
- `Enable Proximity Hostile Glow` [default `true`]\
If enabled, hostile players will only glow when they are close to other online player.
If disabled, hostile players will always glow red.
- `Enable the PvP Ladder` [default `true`]\
Hmm... well it enables the ladder board in .pvp command.
- `Sort PvP Ladder by Honor` [default `true`]\
This will automatically be false if honor system is not enabled.
- `Enable PvP Toggle` [default `true`]\
Enable/disable the pvp toggle feature in the pvp command.
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
<summary>Siege</summary>

- `Buff Siege Golem` [default `false`]\
Enabling this will reduce all incoming physical and spell damage according to config.
- `Physical Damage Reduction` [default `0.5`]\
Reduce incoming damage by this much. Ex.: 0.25 -> 25%
- `Spell Damage Reduction` [default `0.5`]\
Reduce incoming spell damage by this much. Ex.: 0.75 -> 75%

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
- `Ambush Despawn Timer` [default `300`]\
Despawn the ambush squad after this many second if they are still alive. Ex.: -1 -> Never Despawn.

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
Commands permission uses permission level which start from 0 to 100.\
Permission level 0 means that it can be used by everyone.\
User designated as SuperAdmin in your server admin list will always bypass the permission requirement.\
Special commands params that require admin permission can also be adjusted here.

All abbreviation of the command are automatically included, you need only to put the primary command string.\
The permissions are saved in `BepInEx/config/RPGMods/command_permission.json`

<details>
<summary>Default Permission</summary>

```json
{
  "help": 0,
  "ping": 0,
  "myinfo": 0,
  "pvp": 0,
  "pvp_args": 100,
  "siege": 0,
  "siege_args": 100,
  "heat": 0,
  "heat_args": 100,
  "experience": 0,
  "experience_args": 100,
  "mastery": 0,
  "mastery_args": 100,
  "autorespawn": 100,
  "autorespawn_args": 100,
  "waypoint": 100,
  "waypoint_args": 100,
  "ban": 100,
  "bloodpotion": 100,
  "blood": 100,
  "customspawn": 100,
  "give": 100,
  "godmode": 100,
  "health": 100,
  "kick": 100,
  "kit": 100,
  "nocooldown": 100,
  "permission": 100,
  "playerinfo": 100,
  "punish": 100,
  "rename": 100,
  "adminrename": 100,
  "resetcooldown": 100,
  "save": 100,
  "shutdown": 100,
  "spawnnpc": 100,
  "speed": 100,
  "sunimmunity": 100,
  "teleport": 100
}
```

</details>

Removing a command from the list will automatically set it's permission requirement value to `100`.

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
<summary>-- How does kit work?</summary>

&ensp;&ensp;You will get a new config file located in `BepInEx/config/RPGMods/kits.json`
```json
[
  {
    "Name": "Kit1",
    "PrefabGUIDs": {
      "820932258": 50,
      "2106123809": 20
    }
  },
  {
    "Name": "Kit2",
    "PrefabGUIDs": {
      "820932258": 50,
      "2106123809": 20
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

`waypoint <name|set|remove|list> [<name>]`\
Teleports you to previously created waypoints.\
&ensp;&ensp;**Example:** `waypoint set home` <-- Creates a local waypoint just for you.\
&ensp;&ensp;**Example:** `waypoint home` <-- Teleport you to your local waypoint.\
&ensp;&ensp;**Example:** `waypoint remove home` <-- Remove your local waypoint.\
&ensp;&ensp;**Example:** `waypoint list` <-- Shows a list of all to you accessible waypoints.

&ensp;&ensp;**Special Params -> `<name|set|remove|list> [<name>] [global]`** ` Creates a global waypoint usable by everyone.`\
&ensp;&ensp;**Example:** `waypoint set arena global` <-- Creates a global waypoint for everyone (Special Params).\
&ensp;&ensp;**Example:** `waypoint remove arena global` <-- Remove a global waypoint for everyone (Special Params).

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
<summary>customspawn</summary>

`customspawn <Prefab Name> [<BloodType> <BloodQuality> <BloodConsumeable("true/false")> <Duration>]`\
Spawns a modified NPC at your current position.\
&ensp;&ensp;**Example:** `customspawn CHAR_Bandit_Thug creature 100 true -1` -> Spawn Bandit Thug with unlimited lifespan.\
&ensp;&ensp;**Example:** `customspawn CHAR_Bandit_Thug creature 100 true 5` -> Spawn Bandit Thug with a lifespan of 5 seconds.

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
&ensp;&ensp;**Special Params -> `[<all>|<playername>]`** `Toggle the auto respawn for specified player or server wide.`\
&ensp;&ensp;**Example:** `autorespawn all`\
&ensp;&ensp;**Example:** `autorespawn LegendaryVampire`

</details>

<details>
<summary>heat</summary>

`heat`\
Checks your heat/wanted level by the factions.\
&ensp;&ensp;**Special Params -> `[<debug>|<value> <value> [<PlayerName>]]`** `Display numeric heat or set your or the specified player heat.`\
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

`pvp [<on>|<off>|<top>]`\
Display your PvP statistics or toggle PvP state.\
&ensp;&ensp;**Example:** `pvp`\
&ensp;&ensp;**Example:** `pvp top`\
&ensp;&ensp;**Example:** `pvp on`\
&ensp;&ensp;**Example:** `pvp off`

&ensp;&ensp;**Special Params -> `<on>|<off> <playername>`** `Toggles PvP state for the specified player.`\
&ensp;&ensp;**Special Params -> `<rep> <ammount> <playername>`** `Set the specified player reputation points.`\
&ensp;&ensp;**Example:** `pvp on LegendaryVampire`\
&ensp;&ensp;**Example:** `pvp off LegendaryVampire`\
&ensp;&ensp;**Example:** `pvp rep 1000 LegendaryVampire`


</details>

<details>
<summary>siege</summary>

`siege [<on>|<off>]`\
Display all players currently in siege mode, or engage siege mode.\
&ensp;&ensp;**Example:** `siege`\
&ensp;&ensp;**Example:** `siege on`\
&ensp;&ensp;**Example:** `siege off`

&ensp;&ensp;**Special Params -> `<global>`** `Toggles server-wide siege mode on or off).`

</details>

<details>
<summary>experience</summary>

`experience [<log> <on>|<off>]`\
Diplays your current exp and progression to the next level, or toggle the exp gain notification.\
&ensp;&ensp;**Example:** `experience`\
&ensp;&ensp;**Example:** `experience log off`

&ensp;&ensp;**Special Params -> `[<set> <value> [<PlayerName>]]`** `Set your or the specified player experience value.`\
&ensp;&ensp;**Example:** `experience set 1000`\
&ensp;&ensp;**Example:** `experience set 2000 LegendaryVampire`

</details>

<details>
<summary>mastery</summary>

`mastery [<log> <on>|<off>]`\
Display your current mastery progression, or toggle the mastery gain notification.\
&ensp;&ensp;**Example:** `mastery`\
&ensp;&ensp;**Example:** `mastery log off`

&ensp;&ensp;**Special Params -> `[<set> <type> <value> [<PlayerName>]]`** `Set your or the specified player mastery value.`\
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

<details>
<summary>permission</summary>

`permission <list>|<save>|<reload>|<set> <0-100> <playername>|<steamid>`\
Manage commands and user permissions level.\
&ensp;&ensp;**Example:** `permission list` -> List all users with special permission.\
&ensp;&ensp;**Example:** `permission save` -> Save the most recent user permission list.\
&ensp;&ensp;**Example:** `permission reload` -> Directly reload user permission and command permission from the JSON file.\
&ensp;&ensp;**Example:** `permission set 100 LegendaryVampire`\
&ensp;&ensp;**Example:** `permission set 0 LegendaryVampire`

</details>

<details>
<summary>ban/unban</summary>

`ban <playername> [<days> <reason>]`\
Check the status of specified player, or ban them. 0 days will translate to permanently banned.

`unban <playername>`\
Remove the specified player from the ban list.

</details>

<details>
<summary>kick</summary>

`kick <playername>`\
Kick the specified player from the server.

</details>

<details>
<summary>shutdown</summary>

`shutdown`\
Trigger the exit signal & shutdown the server.

</details>

<details>
<summary>rename</summary>

`rename <player name> <new name>`\
Rename the specified player.

</details>

<details>
<summary>adminrename</summary>

`adminrename <player name> <new name>`\
Rename the specified player. Careful, the new name isn't verified.\
This means it's possible for names to use color tags or symbols.\
Adding a color tag to the player name may make it hard for you and other user to `/whisper` or find the player with commands.

</details>

<details>
<summary>playerinfo</summary>

`playerinfo <player name>`\
Display the player information details.

</details>

<details>
<summary>myinfo</summary>

`myinfo`\
Display your user info and location.

</details>

## More Information
<details>
<summary>Changelog</summary>

`1.1.0`
- Added duration option for customspawn command.
- Added honor system and a ton of other mechanics it entails.
- Added siege command.
- Added rename & adminrename commands.
- Added playerinfo & myinfo commands to help server admins with some debugging.
- Fixed hunter hunted not spawning anything on low heat level.
- Give command will now refuse to run if no arguments is given.
- SpawnNPC on waypoint now properly accept the spawn counts.
- Implemented allies caching for better performance.
- Bug fix with the exp gain for killing lower level mobs.
- HunterHunted ambush group are now part of vampire hunters faction.

`1.0.2`
- Added customspawn command.
- Added property to compile with wetstone or not.
- Added shutdown command.
- Bugfix for on defeat message.
- Added kits json save/load log message.
- Minor adjustments.

`1.0.1`
- Added optional wetstone dependency.
- Added compabilities with wetstone reload function.

`1.0.0`
- Removed wetstone dependency

`0.3.2`
- Adjustment in EXP System so player don't get more exp if they've reached the max level cap.
- Adjustment in EXP gain for killing lower level mobs.
- Adjusted bonus EXP gain for killing higher level mob to be capped at 10 level higher max.
- User with sufficient permission for waypoint_args will no longer be restricted by the waypoint limit.
- Reduced the required heat for ambush to occur.
- Fixed PvP Debuff punishment not applying correctly.

`0.3.1`
- Added configurable permission for special params that previously only usable by admins.
- Added VIP system that can give a passive buff to VIP players.

`0.3.0`
- Changed command permission to use permission level instead of just checking for admin/not.
- Permission and disabled commands config now automatically include abbreviation.
- Added whitelist/vip system which should be able to bypass max connected user config.
- Added ban command.
- Added kick command.

`0.2.5`
- Emergency patch release to fix that losing exp on death is still active even when the exp system is disabled.

`0.2.4`
- Modified the save command to also force the server to save game.
- Modified the level up chat notification to be on/off according to the .xp log command.
- Fixed the waypoint bug, admin will ignore the limit properly now, and config for waypoint is properly set.
- Fixed bug with .mastery set command not being able to set other player mastery.
- Moved EXP lost on death to downed event to avoid people suiciding after pvp and losing exp.
- Attempt to fix rare broken string that is totally unknown how it can occurs.
- Commands `.help [<command>]` will no longer show details if the user doesn't have sufficient priviledge.

`0.2.3`
- Added level up effect & notification for the experience system.
- Added config to disable PvP toggling in the pvp command.
- Changed the default exp & mastery feed to be off instead of on.

`0.2.2`
- Fixed some stats being bugged in the mastery system.
- Reduced movement speed bonus from Slashers and None mastery.
- Fixed mastery set command output only saying the first char of the weapon type.
- Added Siege Golem buff options in configs.
- Added ambush unit despawn timer in configs.

`0.2.1`
- Renamed the list in mastery to be in accordance with the mastery in game types.

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
<summary>Developer & Contributors</summary>

### [Discord](https://discord.gg/XY5bNtNm4w)
### Main Developer
- Kaltharos#0001

### Contributors
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
- Punishment debuff lower the player gear level, which will be overriden by the experience system if the exp system is active.

</details>

<details>
<summary>Planned Features</summary>

- More optimization! It never hurts to optimize!
- Average reputation of clan members.
- More dynamic events.
- Bloodline. (On hold)
- Kits Option: Limited Uses. (On hold)
- Explore team/alliance in VRising. (On hold)
- Need a better name tagging sytem. (On hold)

</details>