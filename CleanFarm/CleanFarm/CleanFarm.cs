using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework.Input;
using CleanFarm.CleanTasks;
using System;

namespace CleanFarm
{
    public class CleanFarm : Mod
    {
        /// <summary>The config for this mod.</summary>
        private ModConfig Config;

        /// <summary>The clean tasks to run.</summary>
        private List<ICleanTask> CleanTasks;


        /// <summary>Mod entry point.</summary>
        /// <param name="helper">Mod helper interface.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            this.CleanTasks = new List<ICleanTask>()
            {
                new ObjectCleanTask(this.Config),
                new ResourceClumpCleanTask(this.Config),
                new TerrainFeatureCleanTask(this.Config)
            };

            TimeEvents.OnNewDay += OnNewDay;

        #if DEBUG
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        #endif
        }

        /// <summary>Used for manually running the clean tasks for debugging.</summary>
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
        #if DEBUG
            if (e.KeyPressed == Keys.V)
            {
                Clean();
            }
        #endif
        }

        /// <summary>Callback for the OnNewDay event. Runs the clean once the day has finished transitioning.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewDay(object sender, EventArgsNewDay e)
        {
            // Do it once the day has finished transitioning.
            if (!e.IsNewDay)
                Clean();
        }

        /// <summary>Runs the clean tasks.</summary>
        private void Clean()
        {
            var farm = Game1.locations.Find(loc => loc is Farm) as Farm;
            if (farm == null)
                return;

            this.Monitor.Log("Cleaning up the farm...");

            foreach (ICleanTask cleanTask in this.CleanTasks)
            {
                if (!cleanTask.CanRun())
                    continue;
                try
                {
                    cleanTask.Run(farm);
                    cleanTask.ReportRemovedItems(this.Monitor);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{cleanTask.ToString()} failed to run: {ex}", LogLevel.Error);
                }
            }

            this.Monitor.Log("Cleanup complete!");
        }
    }
}
