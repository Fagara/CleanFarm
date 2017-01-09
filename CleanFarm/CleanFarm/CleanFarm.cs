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

        /// <summary>Gets the farm location.</summary>
        private Farm PlayerFarm => Game1.locations.Find(loc => loc is Farm) as Farm;


        /// <summary>Mod entry point.</summary>
        /// <param name="helper">Mod helper interface.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            InitTasks(this.Config);

            TimeEvents.OnNewDay += OnNewDay;

            InitDebugCommands(helper);
        }

        /// <summary>Creates the clean tasks. This is just it isn't duplicated for the debug command.</summary>
        /// <param name="config">Mod config object.</param>
        private void InitTasks(ModConfig config)
        {
            this.CleanTasks = new List<ICleanTask>()
            {
                new ObjectCleanTask(config),
                new ResourceClumpCleanTask(config),
                new TerrainFeatureCleanTask(config)
            };
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
            if (this.PlayerFarm == null)
            {
                this.Monitor.Log("Cannot clean farm: farm location is invalid.", LogLevel.Warn);
                return;
            }

            this.Monitor.Log("Cleaning up the farm...");

            // Run the tasks
            foreach (ICleanTask cleanTask in this.CleanTasks)
            {
                if (!cleanTask.CanRun())
                    continue;
                try
                {
                    cleanTask.Run(this.PlayerFarm);
                    cleanTask.ReportRemovedItems(this.Monitor);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{cleanTask.ToString()} failed to run: {ex}", LogLevel.Error);
                }
            }

            this.Monitor.Log("Cleanup complete!");
        }

        #region DebugCommands
        private void InitDebugCommands(IModHelper helper)
        {
        #if DEBUG
            // Manually run the clean
            ControlEvents.KeyPressed += (sender, e) =>
            {
                if (e.KeyPressed == Keys.V)
                    Clean();
            };

            // Convenience for testing only with command line
            Command.RegisterCommand("cf_clean", "Manually runs the clean.").CommandFired += (sender, e) => Clean();

            Command.RegisterCommand("cf_restore", "Restores the items removed from the farm.").CommandFired += (sender, e) =>
            {
                this.Monitor.Log("Restoring removed items", LogLevel.Trace);
                if (this.PlayerFarm == null)
                {
                    this.Monitor.Log("Farm is invalid", LogLevel.Error);
                    return;
                }

                try
                {
                    foreach (var task in this.CleanTasks)
                        task.RestoreRemovedItems(this.PlayerFarm);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error while trying to restore items: {ex}", LogLevel.Error);
                }
            };

            // Reloads config and re-creates tasks. Used for quickly testing different config settings without restarting.
            Command.RegisterCommand("cf_reload", "Reloads the config.").CommandFired += (sender, e) =>
            {
                this.Monitor.Log("Reloading config", LogLevel.Trace);
                this.Config = helper.ReadConfig<ModConfig>();
                InitTasks(this.Config);
            };
        #endif // DEBUG
        }
        #endregion DebugCommands
    }
}
