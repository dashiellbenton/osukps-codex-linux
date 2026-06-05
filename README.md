## osukps
A little program that shows keystates, keys per second and total keys for rythm games (osu!, stepmania etc).

## Preview
![preview](/preview.gif?raw=true)  
Extra buttons can be added or removed (minimum 1, maximum 10).  
Button/text/kps colors can be customized.

## Linux support
osukps now runs on Linux through Mono WinForms while keeping the original compact overlay, right-click menu, configurable buttons, KPS colors, counters and recording/playback behavior.

### Arch Linux
Install the runtime/build dependencies:

```sh
sudo pacman -S mono libx11
```

Build and run:

```sh
xbuild osukps.sln /p:Configuration=Release
mono osukps/bin/osukps.exe
```

### Other distros
Install Mono, WinForms support and X11 client libraries from your distro packages (package names are commonly `mono-complete` or `mono-devel`, plus `libx11`). Then build with `xbuild` or `msbuild` and run the generated executable with `mono`.

### Display server note
Global key polling on Linux uses X11 (`XQueryKeymap`). It works in X11 sessions and compatibility environments that expose the keyboard through X11. Native Wayland compositors intentionally block this kind of global key-state polling, so run osukps in an X11 session if keys do not register.

## Download
See [Releases](https://github.com/yugecin/osukps/releases)

## Usage
Right click to access a menu that does stuff (add/remove buttons, exit).  
Click a button (blue square) to assign a key to it.

## License
[MIT License](/LICENSE)
