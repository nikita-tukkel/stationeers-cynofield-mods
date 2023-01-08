# Augmented Reality Mod Compatibility Guide

The mod depends on [Stationeers.Addons](https://github.com/Erdroy/Stationeers.Addons).

Both Stationeers.Addons and the mod highly depends on the game internals, which are changed
on the course of game developers improving the game.

It is great that our beloved game being evolved, but sometimes internal changes are unpredictable.
We learned how to live with that and keep the game in a playable state.

This requires the following knowledge:

1. Obtaining the older version of the game.

2. Preventing the game from auto-updating.

3. Getting the required versions of mods.


## Obtaining the older version of the game

In short, you need to know how to use Steam Console and `download_depot` command,
and you need to know Manifest ID of the specific game version.

After that, you may open Steam Console (`win+R: steam://open/console`) and execute the command that will
download the required version for you, e.g.:
```
download_depot 544550 544551 5085088832022169075
```

Manifest ID is the Steam internal ID you may use to download older game versions.
To find the Manifest ID, use this page:
https://steamdb.info/depot/544551/manifests/

or find it at the very bottom of the patch notes for the version you need here:
https://steamdb.info/app/544550/patchnotes/

Check out the more detailed guide:
https://www.makeuseof.com/how-to-downgrade-steam-games/


## Preventing the game from auto-updating

In the Steam Library properties of the game, select Updates - Only update this game when I launch it.
After that, you have to switch Steam to Offline Mode before starting the game.

The easier way is just to make a copy of the game and launch it from that location.


## Getting the required versions of mods

The rule here is simple: if the game and all mods are working fine, backup the game, steam mods, local mods and save files. You can't download older versions of mods from Steam, so having a backup will save you from digging the whole Internet for the right version of every mod.

Folders you need:

| Purpose | Path |
| :--- | :--- |
| Save files | %Documents%\my games\Stationeers\saves |
| Game local mods | %Documents%\my games\Stationeers\mods | 
| Steam mods | %steam%\steamapps\workshop\content\544550 |
| Game main files  | %steam%\steamapps\common\Stationeers |

Where `%Documents%` is standard Windows Documents folder and `%steam%` is where your Steam installation is.


## Compatible Versions

The Game version is taken from this file: 'steam\steamapps\common\Stationeers\rocketstation_Data\StreamingAssets\version.ini`

| Mod    | Stationeers.Addons | The Stationeers Game | Manifest ID |
| :---   | :---               | :---                 | :--- |
| v1.0.0 | v0.4.2             | 0.2.3719.18087       | 5085088832022169075 |
