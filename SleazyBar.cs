using System;
using System.Collections.Generic;
using System.Linq;

namespace RoundTable
{
    public class SleazyBar
    {
        private readonly object queueLock = new object();
        private readonly int knights; // Number of knights (including the king) - always even
        private readonly int wineCapacity; // The bottle can hold w units (cups) of wine
        private readonly int plateCapacity; // A plate has space for up to c cucumbers

        private Resource[] resources; // Wine and cucumbers
        private bool[] dishes; // Indicates whether plates or cups are taken
        private int wine;
        private bool wineAvailable => wine > 0;

        private bool[] waiting;
        private ConditionVariable[] qWaiting;

        public SleazyBar(int knights, int wineCapacity, int plateCapacity)
        {
            this.knights = knights;
            this.wineCapacity = wineCapacity;
            this.plateCapacity = plateCapacity;
            this.wine = wineCapacity;

            resources = new Resource[knights];
            dishes = new bool[knights];
            waiting = new bool[knights];
            qWaiting = new ConditionVariable[knights];

            for (int k = 0; k < knights; k++)
            {                                                                                      
                // For simplicity, let's assume that wine is always even - indexed.
                var type = k % 2 == 0 ? ResourceType.Wine : ResourceType.Cucumbers;

                resources[k] = new Resource(type);
                if (type == ResourceType.Cucumbers)
                    resources[k].Count = plateCapacity;

                qWaiting[k] = new ConditionVariable();
            }
        }

        public void TryToDrink(int knight)
        {
            lock (queueLock)
            {
                // Wait if resources are not available
                while (!AreResourcesAvailable(knight))
                    Wait(knight);

                Drink(knight);
            }
        }

        public void StopDrinking(int knight)
        {
            lock (queueLock)
            {
                SetDishesAvailability(knight, true);
                DistributeResources();
                Console.WriteLine($"[{knight}] Stopped drinking");
            }
        }

        public void AddResources(ResourceType type)
        {
            lock (queueLock)
            {
                if (type == ResourceType.Wine)
                {
                    wine = wineCapacity;
                }
                else if (type == ResourceType.Cucumbers)
                {
                    foreach (var resource in resources)
                        if (resource.Type == ResourceType.Cucumbers)
                            resource.Count = plateCapacity;
                }

                DistributeResources();
                Console.WriteLine($"{type} ADDED!");
            }
        }

        private void Wait(int k)
        {
            waiting[k] = true;
            Console.WriteLine($"[{k}] Waiting for drinking");
            qWaiting[k].Wait(queueLock);
            waiting[k] = false;
        }

        private void Drink(int k)
        {
            Console.WriteLine($"[{k}] Drinking");

            // Occupy dishes
            SetDishesAvailability(k, false);

            // Drink wine
            wine--;

            // Eat a cucumber
            GetCucumbers(k).Count--;
        }

        private void DistributeResources()
        {
            for (int k = 0; k < knights; k++)
                TryToDistributeResources(k);
        }

        private void TryToDistributeResources(int k)
        {
            if (waiting[k] && AreResourcesAvailable(k))
                qWaiting[k].Pulse();
        }

        private bool AreResourcesAvailable(int k)
        {
            return AreWineAndCucumbersAvailable(k) && AreDishesAvailable(k);
        }

        private bool AreWineAndCucumbersAvailable(int k)
        {
            return wineAvailable && GetCucumbers(k).Count > 0;
        }

        private bool AreDishesAvailable(int k)
        {
            return !dishes[k > 0 ? k - 1 : knights - 1] && !dishes[k];
        }

        private Resource GetCucumbers(int k)
        {
            return resources[k].Type == ResourceType.Cucumbers ? 
                resources[k] : resources[k > 0 ? k - 1 : knights - 1];

            // return k % 2 == 0 ? resources[k > 0 ? k - 1 : knights - 1] : resources[k];
        }

        private void SetDishesAvailability(int k, bool available)
        {
            dishes[k] = dishes[k > 0 ? k - 1 : knights - 1] = !available;
        }

        public class Resource
        {
            public ResourceType Type { get; private set; }
            public int Count { get; set; }

            public Resource(ResourceType type)
            {
                Type = type;
            }
        }

        public enum ResourceType
        {
            Wine,
            Cucumbers
        }
    }
}