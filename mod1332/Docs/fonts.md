
# Goal

Goal: solve the problem of blurred 2D text in your Unity game.

## TL;DR

The result of the described technique, as well as the presentation of fonts included into the mod,
are on [this screenshot](screenshots/fonts2d.png).

Open it with an image editor, not with an image viewer that will make everything blurry.

Zoom in to see every pixel and enjoy the perfect rendering.

Compare pixel perfect fonts with the game standard fonts below and with the game interface to see the difference.


# Links

Best article about 2D pixel perfect font rendering in Unity:
https://medium.com/@dan.liberatore/pixel-perfect-text-and-ui-in-unity-2021-56d60ba9370f

Others:

https://forum.unity.com/threads/pixel-art-font-sizing-issues.635422/#post-5693926

https://pavcreations.com/pixel-perfect-fonts-in-unity-the-practical-guide/

Excellent collection of free fonts from Style-7:

https://www.dafont.com/chess-7-chess-7.d1833


# Finding the right font

Fonts could be bitmap aka raster aka pixel fonts or vector aka outline.
To achieve the pixel perfect rendering, we need the first type.
Download **bitmap** font from, e.g. dafont.com. 
Write down font size, it is very important. 
The size is written among the other font information on dafont.com.
The other option is to open the font in the editor and count pixels height of the largest character.
Also, ensure upfront that font contains all characters you need.

E.g., Font SGK100 by NewMoleFont, height = 16px, https://www.dafont.com/sgk100.font

# Unity Editor

You will need Unity Editor compatible with the game. 
Very different versions probably will generate an incompatible asset bundle.

1. Unity Editor, Project, Assets - create Resources/Fonts folder.

2. Import New Asset, choose font `.ttf` file.

3. Inspector, Import Settings, set:
- Font size - set exactly the font size you noted when downloading the font, e.g. `16`;
- Rendering mode - `Hinted Raster`;
- Character - `Unicode`;
- Ascent Calculation Mode - not important?;
- Use Legacy Bounds - off;
- Should Round Advance Value - on;

Press `Apply` in Inspector window

4. Window, Text Mesh Pro, Font Asset Creator, set:
- Source Font File - font `.ttf` file from previous steps;
- Sampling Point Size - Custom Size, set exactly the font size you noted when downloading the font, e.g. `16`;
- Padding - depends on how different character sizes are, 2..6 are ok;
- Packing Method - Fast (not important?);
- Atlas Resolution - size of generated bitmap, depends on the amount of characters in the font and font size, 
e.g. 512x512 or 1024x256 are probably ok;
- Character Set - Unicode Range (Hex), set `000-4ff,500-52f`, this contains ASCII, Cyrillic and other usual characters;
- Render Mode - `RASTER`;
- Get Kerning Pairs - on;

Press `Generate Font Atlas`, then `Save` inside Font Asset Creator window.
Choose `.asset` file location inside Project/Assets.

5. Inspector, TMP_FontAsset asset file from previous step, set:
- Line Height - small value like 10, otherwise rendered text will have too big line spacing;
- AssetBundle (checkbox at the bottom of Inspector) - set the name of Asset Bundle, it will be the filename on the following step;

6. (Needed once) Create Project, Assets, Editor folder.
In the project window press Create -> C# Script.
Paste script name: `CreateAssetBundles`.
Paste script text:
```
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles2")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.UncompressedAssetBundle,
                                        BuildTarget.StandaloneWindows);
    }
}
```
Exact parameters values are important:
- `BuildAssetBundleOptions.UncompressedAssetBundle`;
- `BuildTarget.StandaloneWindows`;

If everything is ok, new menu `Assets - Build AssetBundles2` will appear.
Be carefull with this script text, Unity Editor will not help you with error diagnostics, 
it will just don't work if incorrect.
For more details see https://docs.unity3d.com/2022.2/Documentation/Manual/AssetBundles-Workflow.html

7. Run `Assets`, `Build AssetBundles2` menu created in the previous step.

If everything is ok, the asset bundle file Project/Assets/AssetBundles/`name from step 5` will be created.

Copy it into the folder, known by the game. Any other folder will be ok, because you may use absolute file names in the game. The main point is to copy it somewhere, otherwise the game and Unity Editor may conflict over this file lock (typical problem on MS Windows filesystem).

# Game scripts

1. Loading font from asset.

Load bundle and review its contents (`file` may be an absolute path):
```
UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromFile(file);
UnityEngine.Debug.Log(string.Join("\n", bundle.GetAllAssetNames()));
```

Unload bundle before loading it again:
```
UnityEngine.AssetBundle.Unload(true);
```

2. Load font and set it into your TextMeshProUGUI.

Font size **must be** divisible to font natural height you know from previous steps. 
So the good `fontSize` values are 16, 32, 48, etc. Otherwise text will be blurry.
Font name may be just a filename, full path inside asset bundle is not required.

```
TMP_FontAsset font = bundle.LoadAsset<TMP_FontAsset>("sgk100.asset");

TextMeshProUGUI text;
...
text.font = font;
text.fontSize = 16;
```

3. Create layout tuned for pixel perfect rendering.

Create layout like Canvas -> VerticalLayoutGroup -> TextMeshProUGUI.

For Canvas, set:
```
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
canvas.pixelPerfect = true;
canvas.scaleFactor = 1;
```

For VerticalLayoutGroup, set:
```
layout.childAlignment = TextAnchor.UpperLeft;
layout.childControlWidth = false;
layout.childControlHeight = false;
layout.childForceExpandWidth = false;
layout.childForceExpandHeight = false;
layout.childScaleWidth = false;
layout.childScaleHeight = false;
layout.spacing = 10;
layout.padding = new RectOffset(5, 5, 5, 5);
```

For TextMeshProUGUI, set (in addition to font and fontSize):
```
text.rectTransform.sizeDelta = new Vector2(300f, 300f);
text.alignment = TextAlignmentOptions.TopLeft;
text.text = "HELLO, perfectly rendered text!";
```

If everything is done right, every pixel of the font will land exactly into physical pixel on your monitor.
