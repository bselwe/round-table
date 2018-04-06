using System;
using System.Threading;
using static RoundTable.SleazyBar;

namespace RoundTable
{
    public class Waiter
    {
        private readonly ResourceType resource; // A waiter serves either cucumbers or wine
        private readonly SleazyBar bar;         // In this bar...
        private readonly Random random;

        public Waiter(ResourceType resource, SleazyBar bar)
        {
            this.resource = resource;
            this.bar = bar;
            this.random = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Run()
        {
            while (true)
            {
                var servingTime = GetServingTime();
                Thread.Sleep(servingTime);
                bar.AddResources(resource);
            }
        }

        private int GetServingTime()
        {
            return random.Next(Configuration.WaiterMinSleepTimeMs, Configuration.WaiterMaxSleepTimeMs);
        }
    }
}