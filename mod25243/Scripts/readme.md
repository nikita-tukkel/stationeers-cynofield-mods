This is the `Scripts` folder of the mod, as required by `Stationeers.Addons`.
For the comfort of development, hardlink this folder into your mod folder inside Stationeers mod directory.
After that, start Stationeers as `rocketstation.exe --live-reload`, and press `Ctrl-R` during the game
to recompile and reload mod scripts.

The correct folders structure will be:
```
Documents\my games\Stationeers\mods
  mod25243
    About
    GameData
    Scripts
```

If the mod logs something using UnityEngine.Debug.Log(), it will goes into the file
`%USERPROFILE%\AppData\LocalLow\Rocketwerkz\rocketstation\Player.log`.
