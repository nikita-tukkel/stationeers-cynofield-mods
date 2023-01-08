
# Augmented Reality Mod

The mod adds Augmented Reality into the game when your character wears Sensor Lenses, including:
- Device Details panel;
- 3D device Annotations;
- Watch panel to monitor specified devices;
- Notifications Log panel and 3D Announcements to monitor important devices;

# Installation

Download the release from here: https://github.com/nikita-tukkel/stationeers-cynofield-mods/releases

Unzip into local mods folder (`%Documents%\my games\Stationeers\mods`).

If you are not familiar with typical locations on your PC, see [this guide](Docs/folders.md).

Mod requires [Stationeers.Addons](https://github.com/Erdroy/Stationeers.Addons) to work. To install `Stationeers.Addons`, unzip it into `%steam%\steamapps\common\Stationeers\AddonManager`.

When updating `Stationeers.Addons`, first delete the whole `%steam%\steamapps\common\Stationeers\AddonManager` folder.

## Installation problems

Both `Stationeers.Addons` and the mod highly depends on the game internals, which are changed
on the course of game developers improving the game.

See the [Compatibility guide](Docs/compatibility.md) for details and the list of compatible versions.

# Usage

The mod was created for people who like the idea of bringing more programming into Stationeers gameplay.
Instead of creating an universal UI that fit everybody, I was trying to make the mod 
extensible so anyone with a thirst for programming will have lesser trouble creating the UI of personal preferences. Basic C# knowledge is required.

When you see you want something extra from the mod, you are welcome to change files under Scripts folder.

The guide below contains the references to the code, which will help you get started.


## Turning On

You will see the mod is installed successfully by visiting the game Main Menu - Workshop.
If you open the game console (`F3`), you will notice a message like:
```
#I 19:18:27.747 cynofield.mods.AugmentedRealityEntry started successfully
```

Check the game log file `%APPDATA%\LocalLow\Rocketwerkz\rocketstation\Player.log` for a detailed log.

The mod is attached to `Sensor Lenses` inventory item. To turn on the mod, you must wear and turn on those glasses.
You may build them with Tier Two Tool Manufactory - Sensor Lenses.
Or spawn it with Creative Mode dynamic panel, `ItemSensorLenses`.

Wear `Sensor Lenses`, insert Battery and turn them On. Sensor Unit is not required.
If everything is ok, you will see the Augmented Reality interface.

Check the [AugmentedRealityEntry.cs](Scripts/AugmentedRealityEntry.cs).

There are some mod configuration parameters on the top of it.
Scroll down to `ArStateManager` class. You may change it if you want to detach the mod from `Sensor Lenses`.


## Device Details

This is the panel on the right side of the HUD, displaying the detailed live information about the device you are looking at.

Every device has (or will have someday) its own UI. To change the existing detail panel, or create one of your own,
check [ThingsUi.cs](Scripts/ui/ThingsUi.cs) and files inside `Scripts/ui/things`.

The entry point to the Device Details panel is here [AugmentedDisplayRight.cs](Scripts/ui/AugmentedDisplayRight.cs) and the complete call hierarchy from the game to the specific details panel goes through:
- [AugmentedRealityPatcher.cs](Scripts/AugmentedRealityPatcher.cs);
- [AugmentedRealityEntry.cs](Scripts/AugmentedRealityEntry.cs);
- [AugmentedUiManager.cs](Scripts/ui/AugmentedUiManager.cs);
- [AugmentedDisplayRight.cs](Scripts/ui/AugmentedDisplayRight.cs);
- [ThingsUi.cs](Scripts/ui/ThingsUi.cs);
- and then into a concrete UI implementation, e.g., [CableUi.cs](Scripts/ui/things/CableUi.cs);


## 3D Device Annotations

This is another means of displaying device details - on the projection in the 3D world, somewhere near the device.
Usually, annotation UI is a simplified variant of Device Details UI.

There are two types of Annotations: dynamic and static.

You create dynamic annotations by looking at the device and pressing Shift.
Keep holding Shift and move around to reposition the dynamic annotation.

Press Ctrl to make all dynamic annotations disappear.

There could be a total of 3 dynamic annotations at the same time. If you want to change this count 
or change the hotkeys, check [AugmentedDisplayInWorld.cs](Scripts/ui/AugmentedDisplayInWorld.cs);

You create static annotations by adding "#ar" tag to the device name (use Labeller tool).
Such annotations are glued to the device and can't be moved. They are independent from static annotations and their maximum count is somewhat unlimited.

There are several color schemes for annotations.
By default they are applied in a loop, but you may specify one explicitly via tag parameter: 
"#ar0" will give you light text on dark background, "#ar1" will give you dark text on light background.
Check [AugmentedRealityEntry.cs](Scripts/AugmentedRealityEntry.cs) and `Scripts/ui/styles` for more information.


## Watches

Add "#w" tag to the device name. It will make a watch for this device to be displayed on the upper left side of the HUD. Watches will work from any distance, so it will be your favorite tool to know what going on the opposite side of your Station.

Like the Details UI, every watch is a live UI panel, just smaller and more specialized.

Unlike the Details UI, you may implement different watches for a single type of device.
To reference a specific watch, use tag parameters, e.g. "#w-BAT".

Check `RenderWatch` in [UiDefault.cs](Scripts/ui/things/UiDefault.cs) for a simple example of a watch.
Check [BatteryUi.cs](Scripts/ui/things/BatteryUi.cs) for a couple of much more advanced examples.


## Notifications Log

This is a lower left panel, used to display short lived notifications.
Each notification disappears after 60 seconds.
Only 10 last notifications are displayed.

Like the Watches and the Details UI, every Notification is a live UI panel. You may keep updating it with live data if you want to.

Check [BatteryUi.cs](Scripts/ui/things/BatteryUi.cs) for an example of Watch producing a notification.


## Announcements

Announcements are 3D text messages designed to be displayed in front of your character's eyes.
They are used in conjunction with Notifications when you need to emphasis on some event.

Check [WeatherStationUi.cs](Scripts/ui/things/WeatherStationUi.cs) for an example of how a Watch outputs an Announcement.
The `BatteryUi`, mentioned above, outputs both a Notification and an Announcement.


# Customizing the mod

If you are already familiar with C# and `Stationeers.Addons`, you are welcome to carve your own specifics into the mod code.

If you are new to one or another:
- start with any tutorial on how to create a console "Hello world" with C#;
- check the [root readme](../readme.md) for information on how to setup your development environment;
- try the `../mod25243`, it is a simple mod to help you get started;


## Productivity tips

### Use Stationeers.Addons live reload

Add `--live-reload` to the game start parameters, so it will be `rocketstation.exe --live-reload`.
During the game, press `Ctrl-R`. The `Stationeers.Addons` will re-compile and reload your code.
Sometimes it is not stable, but anyway it many times faster than doing a full game restart.

### Use hardlinks / folder junctions

Instead of copying every changed file from the folder where you edit them with your IDE,
into the `mods/modname/Scripts` folder, create a hardlink (or directory junction) for `Scripts` folder.


# Mod limitations

There is already a good lot of effort put into this mod.
But nevertheless it is still the earliest version and some deficiencies are expected.

## Screen resolution

All UIs are designed and tested for Full HD (1920x1080) resolution.
Adjusting it to other popular resolutions is one of the nearest things to do.


## Multiplayer

The mod is created only with single-player usage in mind.
I don't plan to try it in a multiplayer or do any multiplayer related improvements.


## Game versions

As it was mentioned earlier, the mod relies on `Stationeers.Addons`.
So if your game version doesn't work with `Stationeers.Addons`, neither the mod do.
Someday I will look into BepInEx integration, maybe it will improve the situation.


# Future plans

Task #0: for the god's sake stop developing the mod and play the game again :-)

## Different screen resolutions

Try other resolutions and see what minimum could be done to make the mod look fine.
Still, 1920x1080 will remain the primary supported resolution.

## UIs almost for all devices

Review every device in the game and make a reasonable UI for it.
Implement watches and notifications for devices that typically need them.
Improve the boilerplate code for UI layouts and decrease the amount of copy-paste.
Delegate more visual preferences into `BaseSkin`.

## Try-catch them all

Add `try-catch` in every place where aggregating code calls the specific implementations.
So the failures in specifics are easier to fix and don't break other parts of the mod.

## Runtime metrics

Implement metrics "engine" to measure how many CPU/RAM resources are eaten by the mod.
This is needed to prevent performance-related bugs from leaking into the release.

## Different static Annotations behaviour

At this moment, static Annotation automatically rotates to the side of the device visible by the player.
I will disable this rotation for devices that have an evident front panel interface, like consoles or atmospherics.

## 2D plotting

Add the ability to display live 2D plots in the 2D and 3D UIs.

## PDA Holo protector

PDA will project its screen into the environment. Placing PDA on the ground will be an alternative way of creating a dynamic Annotations.

## PDA applications

Some basic stuff for making the creation of interactive PDA applications easier.

## Remote desktop

Annotation may be "connected" to the specified device far away and display related information.
This will unite PDA related mod features and Annotations.

## Mouse gestures

Every beltalowda knows this hand gestures language for a reason.

## HER OS character

HER OS `['heroz]` will infrequently send contextual motivational messages and hints.


# Contacts

If you are not reading it on my Github already, here is the [single point of truth for this mod](https://github.com/nikita-tukkel/stationeers-cynofield-mods).

To contact the author, join (Stationeers.Addons Discord)[https://discord.gg/aUwsxMe8].
