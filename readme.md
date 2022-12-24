
## Project setup

1. Open [Env.props](Env.props) and set correct paths (`StationeersDir` and maybe others).


# Development environment setup

## VSCode

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
