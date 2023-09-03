# MC_SVSatellites  
  
Backup your save before using any mods.  
  
Uninstall any mods and attempt to replicate issues before reporting any suspected base game bugs on official channels.  

WARNING: Until the game is changed, uninstalling any equipment or item mod such as this will break any save game played while the mod was installed.  
  
Function  
========  
Adds deployable satellites to serve as permanent map markers.  

**Satellite**  
A new item available from most stations.  Deploy these to add a new permanent marker to the sector map.  Satellites will reveal hidden debris fields in their radius.  Satellites will act as remote scanners, showing asteroids, ships, collectibles, etc.  Satellite scanners have a fixed power of 230 (configurable).  
  
**Articulated Arm**  
New equipment required for deploying satellites.  Provides a scaling scavenging loot bonus.  Activate to deploy a satellite (if you have then in your cargo).  Shift+Activate to destroy the nearest satellite.  Note that satellite items are not recovered, the deployed satellite is just removed.  
  
Install  
=======  
1. Install BepInEx - https://docs.bepinex.dev/articles/user_guide/installation/index.html Stable version 5.4.21 x86.  
2. Run the game at least once to initialise BepInEx and quit.  
3. Download latest mod release.  
4. Place MC_SVSatellites.dll and mc_svsatellites in .\SteamLibrary\steamapps\common\Star Valor\BepInEx\plugins\  

Configuration  
===========  
After first run, a new file mc.starvalor.satellites.cfg will be created in .\Star Valor\BepInex\config.  Here you can set the scanner power for satellites.  

Mod Info
======
Satellite item ID = 30000  
Articulated Arm equipment ID = 30000

Credit  
=======  
Satellite model is a modified (reduced poly, several details removed, textures edits) version of: https://sketchfab.com/3d-models/communication-satellite-9a7ad3344edb4e598de848a5badb7416 by Harri Snellman 
