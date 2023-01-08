
## Project setup

It is expected that:
- you already know how to create at least a simple hello-worldish console C# application;
- you read the `Stationeers.Addons` documentation and know how to create your own mod with custom C# scripts;

1. Open [Env.props](Env.props) and set correct paths (`StationeersDir` and maybe others).

1. Start `dotnet build` command and check that it was successful.

1. Project folders:

|||
| :--- | :--- |
| mod25243 | Tiny mod example to check your development environment setup |
| mod1332  | Augmented Reality Mod |
| test  | Unit tests |

4. There is a second repository with binary files (e.g., Unity assets bundles), `stationeers-cynofield-mods-binaries`. You may need to download it as well.


# Development environment setup

## C# SDK

The version of .Net your project using must be compatible with the one used inside Unity.
Currenly it is `.NET Framework 4.8` aka `TargetFramework=net48`.

If you are setting up a development environment for the first time, start with `mod25243`.
This is the tiny project you may use to check that everything is fine with your development setup.
Try the following commands inside `mod25243` folder and check that all of them succeeded:
```
dotnet restore
dotnet clean
dotnet build
```

Then copy mod files as described in [this readme](mod25243/ModTemplate/readme.md) and [that readme](mod25243/Scripts/readme.md). Start the game and check that you see what [ModScriptExample.cs](mod25243/Scripts/ModScriptExample.cs) did.

The versions of C# SDK I use:
```
> dotnet --version
7.0.101

> dotnet msbuild --version
MSBuild version 17.4.0+18d5aef85 for .NET 
17.4.0.51802

> dotnet nuget --version
NuGet Command Line
6.4.0.117
```

## VSCode

If you already have fully functional MS Visual Studio, you may skip this chapter.

Otherwise consider using MS VSCode - a lightweight IDE that will save you from 
installing a full version of Visual Studio.

You will need to restart VSCode several times when making changes below.
Sometimes soft restart is enough - `Ctrl-Shift-P` - `Developer: Reload Window`.

### Extensions

First of all you need `ms-dotnettools.csharp`.

See the full list of recommended extensions:
```
ms-dotnettools.csharp
jmrog.vscode-nuget-package-manager
fernandoescolar.vscode-solution-explorer
redhat.vscode-xml
Fudge.auto-using
FelschR.extbundles-csharp
k--kato.docomment
tintoy.msbuild-project-tools
pflannery.vscode-versionlens
yzhang.markdown-all-in-one
```

### Global omnisharp settings

Go into `%USERPROFILE%\.omnisharp\omnisharp.json`.
If this file/dir doesn't exist yet - create it.
The file must contain the following, you will have to print the value of %USERPROFILE% variable:
```
{
    "RoslynExtensionsOptions": {
        "enableAnalyzersSupport": true,
	"LocationPaths": ["C:/Users/______YOUR USER NAME HERE______/.omnisharp/nuget/Microsoft.Unity.Analyzers/analyzers/dotnet/cs"]
    }
}
```

Then download [Microsoft.Unity.Analyzers package](https://www.nuget.org/packages/Microsoft.Unity.Analyzers/)
by clicking [Download package](https://www.nuget.org/api/v2/package/Microsoft.Unity.Analyzers/1.15.0).

Then unzip the downloaded .nupkg file, so the LocationPaths in omnisharp.json will match directories on disk.


### Global settings

Global VSCode settings file is located here `%USERPROFILE%\AppData\Roaming\Code\User\settings.json`.

Make sure it contains the following options:
```
{
    "omnisharp.enableEditorConfigSupport": false,
    "omnisharp.enableRoslynAnalyzers": true,
    "omnisharp.useModernNet": false,
    "csharp.unitTestDebuggingOptions": {
        "type": "clr",
        "console": "integratedTerminal"
    },
}
```

### Other important things to know

For correct support of net48 (and other older .Net frameworks) debug, 
launch configuration types must be `clr`, not `coreclr` as VSCode usually sets by default.

It applies to `csharp.unitTestDebuggingOptions` (above), and to launch configurations
in `.code-workspace` and `.vscode/launch.json` files.

You will not be able to debug Unity process from VSCode because omnisharp doesn't support
`Mono` debug. `Mono` is the version of .net execution environment used inside Unity.

But you will be able to debug unit tests if you use them.

There were mentions about the means to debug `Mono` from VSCode by replacing Unity DLLs
with patched ones, which enables remote debugging. But I didn't manage to make it working.
