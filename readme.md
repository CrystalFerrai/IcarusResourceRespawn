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

You will need to have the .NET Runtime 6.0 x64 installed. You can find the latest .NET 6 downloads [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0). Look for ".NET Runtime" or ".NET Desktop Runtime" (which includes .NET Runtime). Download and install the x64 version for your OS.

## How to Use

**BACKUP YOUR SAVE FILE BEFORE USING THIS PROGRAM.** If something goes wrong, there is no way to recover your save unless you have a backup.

### Prerequisite
You should have some familiarity with using command line programs or you may struggle to run this.

### Step 1: Locate your prospect save file
The normal location for these files is:
```
%localappdata%\Icarus\Saved\PlayerData\[your steam id]\Prospects
```

If you are running a dedicated server, the save file location will be different and may vary depending where your server is hosted. You will need to download the save file if it is hosted remotely, modify it, then reupload it. If your server is self hosted, the location for save files should be inside your server install directory at:
```
Icarus/Saved/PlayerData/DedicatedServer/Prospects
```

### Step 2: Backup your save files
Make copies of your prospect save files, ideally in some other location so that your game doesn't show duplicate saves.

### Step 3: Circumvent Steam Cloud
_This applies to local save files. If you are modifying a dedicated server save file, you can skip this step._

When you modify a save file, Steam cloud will often end up undoing your changes next time you run the game as it will download the copy from the cloud. There are two ways to circumvent this issue. You can either disable Steam cloud for Icarus from the Steam library or you can have the game running and sitting at the main menu while you modify the save files.

### Step 4: Run IcarusResourceRespawn
_Make sure the prospect is not currently loaded in your game or dedicated server before doing this step._

Open a command prompt (cmd) wherever you downloaded IcarusResourceRespawn and run the following command to modify your prospect. Substitute your save file location and file name.
```
IcarusResourceRespawn -f -t %localappdata%\Icarus\Saved\PlayerData\[your steam id]\Prospects\[your prospect file name].json
```

The above command respawns foliage (-f) and trees (-t). You can add more options for additional resource types.
```
-f Respawn bushes, berries, crops, etc.
-t Respawn trees
-v Respawn minable ores and rocks
-b Respawn obsidian, clay, scoria
-d Remove deep ore deposits so they respawn, possibly with different ore types (will disconnect existing drills)
-i Remove super cooled ice deposits so they respawn (will disconnect existing borers)
```

## How to Build

If you want to build, from source, follow these steps.
1. Clone the repo, including submodules.
    ```
    git clone --recursive https://github.com/CrystalFerrai/IcarusResourceRespawn.git
    ```
2. Open the file `IcarusResourceRespawn.sln` in Visual Studio.
3. Right click the solution in the Solution Explorer panel and select "Restore NuGet Dependencies".
4. Build the solution.

## Disclaimer

This program worked for me when I created it, but I only did limited testing. It may cause unintended side effects in your save, so back it up first and then verify no issues in game. This program may stop working if the prospect save format is updated in the future. Feel free to open an issue ticket on the Github repo to let me know if this happens.

## Support

This is just one of my many free time projects. No support or documentation is offered beyond this readme.
