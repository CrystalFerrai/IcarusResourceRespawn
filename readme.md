# Icarus Resource Respawner

A command line program that modifies an Icarus game save to respawn all resources.

## Features

This prgram modifies a prospect save file and respawns the following types of resources:
* Voxel nodes - includes ores and rocks. May include cave entrance blockers, haven't checked.
* Trees
* Foliage - includes things like food plants and bushes

This should work for any type of prospect. It doesn't matter if it is a mission or an open world.

## Releases

Releases can be found [here](https://github.com/CrystalFerrai/IcarusResourceRespawn/releases). There is no installer, just unzip the contents to a location on your hard drive.

## How to Use

**BACKUP YOUR SAVE FILE BEFORE USING THIS PROGRAM.** If something goes wrong, there is no way to recover your save unless you have a backup.

### Prerequisite
You should have some familiarity with using command line programs or you may struggle to run this.

### Step 1: Locate your prospect save file
The normal location for these files is:
```
%localappdata%\Icarus\Saved\PlayerData\[your steam id]]\Prospects
```

If you are running a dedicated server, the save file location may be different, depending on how the server was installed. If it is not in the above location, then it is probably within your dedicated server install directory under:
```
Icarus/Saved/PlayerData/DedicatedServer/Prospects
```

### Step 2: Backup your save files
Make copies of your prospect save files, ideally in some other location so that your game doesn't show duplicate saves.

### Step 3: Run IcarusResourceRespawn
_Make sure the prospect is not currently loaded in your game or dedicated server before doing this step._

Open a command prompt (cmd) wherever you downloaded IcarusResourceRespawn and run the following command to modify your prospect. Substitute your save file location and file name.
```
IcarusResourceRespawn %localappdata%\Icarus\Saved\PlayerData\[your steam id]]\Prospects\[your prospect file name].json
```

_or_

Instead of running from a command prompt, you can drag your prospect save file and drop it onto IcarusResourceRespawn.exe. However, the window won't stay open when it's done running to show whether any errors happened if you do it this way.

## How to Build

If you want to build, from source, follow these steps.
1. Clone the repo, including submodules.
    ```
    git clone --recursive https://github.com/CrystalFerrai/IcarusSaveLib.git
    ```
2. Open the file `IcarusResourceRespawn.sln` in Visual Studio.
3. Right click the solution in the Solution Explorer panel and select "Restore NuGet Dependencies".
4. Build the solution.

## Disclaimer

This program worked for me when I created it, but I only did limited testing. It may cause unintended side effects in your save, so back it up first and then verify no issues in game. This program may stop working if the prospect save format is updated in the future. Feel free to open an issue ticket on the Github repo to let me know if this happens.

## Support

This is just one of my many free time projects. No support or documentation is offered beyond this readme.
