# TowerCore

This is **NOT** a Stardew Valley mod, but it is part of some of them. This is
a shared code library containing source files that I reuse across my mods by
importing them directly.

If you are building any of my mods from source, you should put this project
directory at the same level as the ones you are compiling. e.g.:

```
├── HatMouseLacey
│   └── ...
├── SecretNoteFramework
│   └── ...
└── TowerCore
    ├── lib
    │   └── ...
    ├── LICENSE
    ├── README.md (this file)
    └── TowerCore.msbuild
```

Each mod is set up to find TowerCore in this way, and should build without
issue once this is done.


## How to Use

If you are me in the future, or if you are someone else who wants to use this
library for your own mods, here's how it works.

First, you need to import the .msbuild file to add its instructions to your
build process. Change the path in the `Project` attribute to match your setup:

```
<Import Project="..\TowerCore\TowerCore.msbuild" Label="Shared" />
```

Then, in your source files, you can use it by declaring

```cs
using ichortower.TowerCore;
```

... or `using Log = ichortower.TowerCore.Log;` or similar, as desired.

Make sure that the `<Import ...>` directive in your .csproj follows after the
necessary feature flag properties (see below), since the .msbuild file you are
importing will check their values.


### Feature Flags

Use these properties in your .csproj to set flags for TowerCore to check.
These control conditional compilation (and references, where applicable) of
certain features which you may not need.

`<EnableHarmony>true</EnableHarmony>`: this is a SMAPI flag, but TowerCore
checks for it. If set, TowerCore will include a small toolkit for declaring
Harmony patches.

`<Tower_EnableCrossConfig>true</Tower_EnableCrossConfig>`: this flag tells
TowerCore to include code to read other mods' config files. This requires
access to Newtonsoft's json parser, so that reference will be included
automatically.
