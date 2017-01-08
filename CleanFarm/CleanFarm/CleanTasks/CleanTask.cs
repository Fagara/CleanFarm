using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewModdingAPI;

namespace CleanFarm.CleanTasks
{
    /// <summary>The abstract base class for a CleanTask.</summary>
    /// <typeparam name="ItemType">The type of item the task is meant to clean up.</typeparam>
    abstract class CleanTask<ItemType> : ICleanTask
    {
        /// <summary>The config for this mod.</summary>
        protected ModConfig Config { get; private set; }

        /// <summary>The items that have been removed by this task when Run is called. Maps GetItemName to the number of instances that were removed.</summary>
        protected Dictionary<string, int> RemovedItems { get; private set; }


        /// <summary>Creats an instance of the clean task.</summary>
        /// <param name="config">The config object for this mod.</param>
        public CleanTask(ModConfig config)
        {
            this.Config = config;
            this.RemovedItems = new Dictionary<string, int>();
        }

        /// <summary>Can this task be run. Usually checks the config settings to see if it's enabled.</summary>
        public abstract bool CanRun();

        /// <summary>Runs the clean task.</summary>
        /// <param name="farm">The farm to be cleaned.</param>
        public abstract void Run(Farm farm);

        /// <summary>Prints to the console all the items that were removed in a nice format. Only runs if the config option is enabled.</summary>
        /// <param name="monitor">The monitor interface to access the log method from.</param>
        public void ReportRemovedItems(IMonitor monitor)
        {
            if (this.Config.ReportRemovedItemsToConsole)
            {
                monitor.Log($"Running task {this.GetType().Name}");

                foreach (var removed in this.RemovedItems)
                {
                    // Pluralize if needed
                    string removedName = removed.Value > 1 && !removed.Key.EndsWith("s") 
                        ? removed.Key + "s" 
                        : removed.Key;
                    monitor.Log($"Removed {removed.Value} {removedName}");
                }

                if (this.RemovedItems.Count == 0)
                    monitor.Log("No items to remove.");

                // Clear the items once they have been reported so they aren't reported again.
                this.RemovedItems.Clear();
            }
        }

        /// <summary>Checks if an item meets the criteria to be removed from the farm. This is passed to a 'Where' query in RemoveAndRecordItems.</summary>
        /// <param name="item">The item in question.</param>
        protected abstract bool ShouldRemoveItem(ItemType item);

        /// <summary>Gets the human readable name of an item. Used for reporting the item.</summary>
        /// <param name="item">The item whose name to get.</param>
        protected abstract string GetItemName(ItemType item);

        /// <summary>Collects, removes and tracks items from a native collection.</summary>
        /// <typeparam name="T">The type of the collection. The collection may be a dictionary in which case it would not match ItemType.</typeparam>
        /// <param name="collection">The collection to remove items from.</param>
        /// <param name="extractItemFunc">A method to extract an ItemType from an element of the collection.</param>
        protected void RemoveAndRecordItems<T>(ICollection<T> collection, Func<T, ItemType> extractItemFunc)
        {
            // Find all the items that should be removed.
            ICollection<T> toRemove = collection
                .Where(item => ShouldRemoveItem(extractItemFunc(item)))
                .Select(item => item)
                .ToList();

            // Make sure the list is empty first.
            this.RemovedItems.Clear();

            foreach (var item in toRemove)
            {
                if (this.Config.ReportRemovedItemsToConsole)
                {
                    // Track each item that is removed, incrementing the count each time an item with a matching name is found.
                    ItemType extracted = extractItemFunc(item);
                    var itemName = GetItemName(extracted);
                    this.RemovedItems[itemName] = this.RemovedItems.ContainsKey(itemName)
                        ? this.RemovedItems[itemName] + 1
                        : 1;
                }

                // Remove the item from the native collection.
                collection.Remove(item);
            }
        }
    }
}
