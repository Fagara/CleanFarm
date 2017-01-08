using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace CleanFarm.CleanTasks
{
    /// <summary>CleanTask for cleaning TerrainFeatures from the farm (saplings, grass).</summary>
    class TerrainFeatureCleanTask : CleanTask<TerrainFeature>
    {
        /// <summary>The max growth stage of trees to allow. All trees below this stage are removed.</summary>
        private int MaxGrowthStage = Tree.saplingStage;

        /// <summary>Creats an instance of the clean task.</summary>
        /// <param name="config">The config object for this mod.</param>
        public TerrainFeatureCleanTask(ModConfig config)
            : base(config)
        {
            this.MaxGrowthStage = (int)MathHelper.Clamp(config.MaxTreeGrowthStageToAllow, Tree.sproutStage, Tree.treeStage);
        }

        /// <summary>Can this task be run. Usually checks the config settings to see if it's enabled.</summary>
        public override bool CanRun()
        {
            return this.Config.RemoveSaplings || this.Config.RemoveGrass;
        }

        /// <summary>Runs the clean task.</summary>
        /// <param name="farm">The farm to be cleaned.</param>
        public override void Run(Farm farm)
        {
            RemoveAndRecordItems(farm.terrainFeatures, pair => pair.Value);
        }

        /// <summary>Gets the human readable name of an item. Used for reporting the item.</summary>
        /// <param name="item">The item whose name to get.</param>
        protected override string GetItemName(TerrainFeature item)
        {
            return item is Tree
                ? "Tree"
                : item.ToString();
        }

        /// <summary>Checks if an item meets the criteria to be removed from the farm. This is passed to a 'Where' query in RemoveAndRecordItems.</summary>
        /// <param name="item">The item in question.</param>
        protected override bool ShouldRemoveItem(TerrainFeature item)
        {
            if (item is Tree && this.Config.RemoveSaplings)
            {
                var tree = item as Tree;
                return (tree.growthStage < this.MaxGrowthStage ||
                        (tree.stump && this.Config.RemoveStumps));
            }
            return (item is Grass && this.Config.RemoveGrass);
        }
    }
}
