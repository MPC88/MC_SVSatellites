# MC_SVSatellites  
  
Backup your save before using any mods.  
  
Uninstall any mods and attempt to replicate issues before reporting any suspected base game bugs on official channels.  
  
Function  
========  
Adds deployable satellites to serve as permanent map markers.  

**Satellite**  
A new item available from most stations.  Deploy these to add a new permanent marker to the sector map.  Satellites will reveal hidden debris fields in their radius.  Satellites will act as remote scanners, showing asteroids, ships, collectibles, etc.  Satellite scanners have a fixed power of 230 (configurable).  
  
**Articulate Arm**  
New equipment required for deploying satellites.  Provides a scaling scavenging loot bonus.  Activate to deploy a satellite (if you have then in your cargo).  Shift+Activate to destroy a satellite.  Note that satellite items are not recovered, the deployed satellite is just removed.  
  
Install  
=======  
1. Install BepInEx - https://docs.bepinex.dev/articles/user_guide/installation/index.html Stable version 5.4.21 x86.  
2. Run the game at least once to initialise BepInEx and quit.  
3. Download latest mod release.  
4. Place MC_SVSatellites.dll and mc_svsatellites in .\SteamLibrary\steamapps\common\Star Valor\BepInEx\plugins\  

Configuration  
===========  
After first run, a new file mc.starvalor.satellites.cfg will be created in .\Star Valor\BepInex\config.  Here you can set the scanner power for satellites.  