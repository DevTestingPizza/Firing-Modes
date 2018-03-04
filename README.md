# Firing Modes
This resource allows you to toggle between full automatic fire, burst fire (about 3 bullets each time) or single shot mode firing mode.
To switch between these modes, press L on your keyboard (there's currently no controller option for this).

There's also an implemented gun safety feature. Press K (also, keyboard only) to enable it and you won't be able to fire your weapon.

**Note, currently these firing modes and the safety mode only work on (S)MG's and Rifles. Soon™ there will be an option for you to choose your own weapons that have these features enabled.**


### Configuration (v1.1+)
There are 3 options available to change in the `__resource.lua` file.
1. `disableIcons 'false'`
Change this to 'true' if you want to hide/disable the icon in the top right corner of your screen.
2. `safetyToggleKey '7'`
Change the `'7'` to any valid control id if you want to change the "Toggle Safety Mode" button.
3. `switchFiringModeKey '311'`
Change the `'311'` to any valid control id if you want to change the "Switch Firing Mode" button.