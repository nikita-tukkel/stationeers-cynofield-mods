
The mod adds Augmented Reality into the game when your character wear Sensor Lenses, including:
- Device Details panel;
- 3D device Annotations;
- Watch panel to monitor specified devices;
- Notifications Log panel and 3D Announcements to monitor important devices;

# Installation

1. Install [Stationeers.Addons](https://github.com/Erdroy/Stationeers.Addons) if you don't have it already.
2. Download mod archive and extract it into Stationeers local mods directory.
3. If everything is ok, you will see the mod inside the game Main Menu - Workshop.

What is "Stationeers local mods directory"?

This is where your save files are. It is located under Windows standard `Documents` folder, which is inside
user home directory aka `%USERPROFILE%`. Check via Explorer to make sure.
The path inside `Documents` folder is `\my games\Stationeers\mods`. You need to create `mods` directory if you don't have one. The correct folder structure will be:

```
Documents\my games\Stationeers\mods
  mod1332
    About
    Content
    GameData
    Scripts
```

# Usage

Obtain a `Sensor Lenses`. It is an advanced item for the glasses inventory slot.
You may build it with Tier Two Tool Manufactory - Sensor Lenses.
Or spawn it with Creative Mode dynamic panel, `ItemSensorLenses`.

Wear `Sensor Lenses`, insert Battery and turn them On. Sensor Unit is not required.
If everything is ok, you will see the Augmented Reality interface.

Aim at a device and press Shift to create 3D annotation for supported devices.

Hold Shift and move around to change annotation position.

Press Ctrl to dismiss annotations.

With Labeller, add "#ar" to the device name, to create a static annotation that will stick to the device.

With Labeller, add "#w" to the device name, to create a Watch.


# Troubleshooting

If mod doesn't work, usually it is a compatibility problem between the game, Stationeers.Addons and the mod.
The game is in active development, so the compatibility is a common problem which influences any mod.
The developers of the game are improving and updating it for our mutual good, so we are used to
deal with compatibility issues.

Typical steps to fix the compatibiliy problem include:

1. Delete AddonsCacheTemp and AddonsCache folders inside `steam\steamapps\common\Stationeers\AddonManager` and try again.
2. Check if your version of the game is compatible with your version of Stationeers.Addons. Upgrade/downgrade both of them if needed.
3. Read the Compatibility chapter of the mod guide (see link below).


# Customizing the mod

The mod was created for people who like the idea of bringing more programming into Stationeers gameplay.
Instead of creating an universal UI that should fit anyone, I was trying to make the mod 
extensible so anyone with a thirst for programming will have lesser trouble creating the UI of personal preferences. Basic C# knowledge is required.

When you see that you want something extra from the mod, you are welcome to modify files under Scripts folder.
Read the mod guide for details (see link below).


# Contacts

1. Mod guide: https://github.com/nikita-tukkel/stationeers-cynofield-mods/tree/master/mod1332
2. Mod sources, latest version, and the single point of truth: https://github.com/nikita-tukkel/stationeers-cynofield-mods/
3. To contact the author join (Stationeers.Addons Discord)[https://discord.gg/aUwsxMe8].

