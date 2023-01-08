
If you are not familiar with typical file locations on your PC, this is a little guide for you.

# System folders

| Reference | Example | Purpose |
| :--- | :--- | :--- |
| %USERPROFILE% | C:\Users\your_name | Standard Windows "user home" folder |
| %Documents% | %USERPROFILE%\Documents | Standard Windows "my documents" folder. May be moved elsewhere if a user customized the standard setup |
| %APPDATA% | %USERPROFILE%\AppData | Standard Windows folder usually used by games and other software. Typical sub-folders are: Local, Roaming, LocalLow |
| %steam% | D:\games\steam\ | This is the place where you install Steam. Contains `common` folder with games and `workshop` folder with Steam mods |

# Stationeers folders and files

| Folder | Purpose |
| :--- | :--- |
| %steam%\steamapps\common\Stationeers | Main game files |
| %steam%\steamapps\common\Stationeers\AddonManager | Stationeers.Addons installation folders. Contains `AddonsCacheTemp` and `AddonsCache` you may need to clear sometimes |
| %steam%\steamapps\common\Stationeers\rocketstation_Data\StreamingAssets\version.ini| This file contains the game version |
| %Documents%\my games\Stationeers\saves | Game save files |
| %Documents%\my games\Stationeers\mods | Game local mods - when you need mods not from Steam |
| %APPDATA%\LocalLow\Rocketwerkz\rocketstation\Player.log | Game log file | 

# Mod files and folders

The correct files and folders structure of any mod is:
```
%Documents%\my games\Stationeers\mods    <--- Game local mods folder
  mod25243                               <--- Mod folder
    About 
      About.xml
    GameData
    Scripts
      CustomScript.cs
```
