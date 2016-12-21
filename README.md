CleanFarm
=========

On each new day this mod will remove all debris and other items from the farm that you specify in the config (see below).

**Warning: It may be a good idea to back up your save game as your farm is saved after the items have been removed.**


## Requirements

This mod works with the most recent version of Stardew Valley (last tested with 1.1).

This mod requires [SMAPI](https://github.com/ClxS/SMAPI) __1.0 or higher to run__.

For instructions on installing SMAPI view [this help page](http://canimod.com/guides/using-mods#installing-smapi).

## Installation

1. Download the latest release [here](https://github.com/tstaples/CleanFarm/releases).
2. Unzip the contents into your Mods folder which is located in the same directory as your Stardew Valley.exe and StardewValleyModdingAPI.exe.

To uninstall this mod you can simply delete the "CleanFarm" folder from your Mods directory.

## Configuration

In this mod's `config.json` file you will find options to enable/disable which objects are 'cleaned' from your farm. The options are:

```
{
  "RemoveGrass": false,
  "RemoveWeeds": true,
  "RemoveStones": true,
  "RemoveTwigs": true,
  "RemoveStumps": true,
  "RemoveSaplings": true,
  "MaxTreeGrowthStageToAllow": 5, // min 1, max 5
  "RemoveLargeLogs": true,
  "RemoveLargeRocks": true
}
```

The `MaxTreeGrowthStageToAllow` is only used if `RemoveSaplings` is true. It represents the max growth stage allowed, so any trees with a lower growth stage will be removed. This allows you to remove all seeds but keep existing saplings etc.
Only values between 1 and 5 are valid.

The growth stages are:
* 0 - Seed
* 1 - Sprout
* 2 - Sapling
* 3 - Bush
* 4 - Small Tree
* 5 - Tree

For more information on how to modify the config see [this page](http://canimod.com/guides/using-mods#configuring-mods).