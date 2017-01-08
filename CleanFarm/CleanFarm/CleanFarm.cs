using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CleanFarm
{
    public class CleanFarm : Mod
    {
        private ModConfig Config;
        private List<string> ObjectsToRemove;
        private int MaxGrowthStage = Tree.saplingStage;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            // Build a list of items to look for.
            this.ObjectsToRemove = new List<string>();
            if (this.Config.RemoveStones) this.ObjectsToRemove.Add("stone");
            if (this.Config.RemoveTwigs) this.ObjectsToRemove.Add("twig");
            if (this.Config.RemoveWeeds) this.ObjectsToRemove.Add("weeds");

            // Validate the growth stage
            this.MaxGrowthStage = (int)MathHelper.Clamp(this.Config.MaxTreeGrowthStageToAllow, Tree.sproutStage, Tree.treeStage);

            TimeEvents.OnNewDay += OnNewDay;

        #if DEBUG
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        #endif
        }

        // Debug
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
        #if DEBUG
            if (e.KeyPressed == Keys.V)
            {
                Clean();
            }
        #endif
        }

        private void OnNewDay(object sender, EventArgsNewDay e)
        {
            // Do it once the day has finished transitioning.
            if (!e.IsNewDay)
            {
                Clean();
            }
        }

        private void Clean()
        {
            var farm = Game1.locations.Find(loc => loc is Farm) as Farm;
            if (farm == null)
                return;

            this.Monitor.Log("Cleaning up the farm!");

            // Used to store the unique item name and the amount of it that was removed.
            var removedItems = new Dictionary<string, int>();

            // TODO: create a class for each of these types to remove to reduce duplication.
            // Objects to remove
            if (this.ObjectsToRemove.Count > 0)
            {
                // Remove objects we don't want
                var objectsToRemove = farm.objects
                    .Where(pair => ShouldRemoveItem(pair.Value.Name))
                    .Select(pair => pair)
                    .ToList();

                RemoveAndRecordItem(farm.objects, objectsToRemove, removedItems, item => item.Value.Name);
            }

            // Resource clumps
            if (this.Config.RemoveLargeLogs || this.Config.RemoveLargeRocks)
            {
                var clumps = farm.resourceClumps
                    .Where(clump => ShouldRemoveResourceClump(clump))
                    .Select(clump => clump)
                    .ToList();

                RemoveAndRecordItem(farm.resourceClumps, clumps, removedItems, GetResourceClumpName);
            }

            // Saplings
            if (this.Config.RemoveSaplings)
            {
                var terrainToRemove = farm.terrainFeatures
                    .Where(pair => ShouldRemoveTerrain(pair.Value))
                    .Select(pair => pair)
                    .ToList();

                RemoveAndRecordItem(farm.terrainFeatures, terrainToRemove, removedItems, item => GetTerrainFeatureName(item.Value));
            }

            // Report what was removed
            if (this.Config.ReportRemovedItemsToConsole)
            {
                foreach (var removed in removedItems)
                {
                    // Pluralize if needed
                    this.Monitor.Log($"Removed {removed.Value} {(removed.Value > 1 && !removed.Key.EndsWith("s") ? removed.Key + "s" : removed.Key)}");
                }
            }
            this.Monitor.Log(removedItems.Count > 0 ? "Cleanup complete!" : "Nothing to remove.");
        }

        private bool ShouldRemoveItem(string name)
        {
            return this.ObjectsToRemove.Contains(name.ToLower());
        }

        private bool ShouldRemoveTerrain(TerrainFeature feature)
        {
            if (feature is Tree && this.Config.RemoveSaplings)
            {
                var tree = feature as Tree;
                return (tree.growthStage < this.MaxGrowthStage ||
                        (tree.stump && this.Config.RemoveStumps));
            }
            return (feature is Grass && this.Config.RemoveGrass);
        }

        private bool ShouldRemoveResourceClump(ResourceClump clump)
        {
            int type = clump.parentSheetIndex;
            return ((this.Config.RemoveLargeRocks && type == ResourceClump.boulderIndex) ||
                    this.Config.RemoveLargeLogs && (type == ResourceClump.hollowLogIndex || type == ResourceClump.stumpIndex));
        }

        // TODO: maybe create GetName extension methods for all these types?
        private string GetResourceClumpName(ResourceClump clump)
        {
            if (clump.parentSheetIndex == ResourceClump.boulderIndex) return "Boulder";
            if (clump.parentSheetIndex == ResourceClump.hollowLogIndex) return "Hollow Log";
            if (clump.parentSheetIndex == ResourceClump.stumpIndex) return "Stump";
            if (clump.parentSheetIndex == ResourceClump.meteoriteIndex) return "Meteorite";
            return clump.GetType().ToString();
        }

        private string GetTerrainFeatureName(TerrainFeature feature)
        {
            return feature is Tree 
                ? "Tree" 
                : feature.ToString();
        }

        private void RemoveAndRecordItem<T>(ICollection<T> collection, ICollection<T> toRemove, Dictionary<string, int> removedItems, Func<T, string> getNameFunc)
        {
            foreach (var item in toRemove)
            {
                var itemName = getNameFunc(item);
                removedItems[itemName] = removedItems.ContainsKey(itemName) ? removedItems[itemName] + 1 : 1;
                collection.Remove(item);
            }
        }
    }
}
