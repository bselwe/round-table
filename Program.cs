using System;
using System.Threading;
using static RoundTable.SleazyBar;

namespace RoundTable
{
    class Program
    {
        private static void Main(string[] args)
        {
            var rostrum = InitializeRostrum();
            var bar = InitializeSleazyBar();
            InitializeKnights(rostrum, bar);
            InitializeWaiters(bar);
        }

        private static Rostrum InitializeRostrum()
        {
            return new Rostrum(Configuration.NumberOfKnights);
        }

        private static SleazyBar InitializeSleazyBar()
        {
            return new SleazyBar(Configuration.NumberOfKnights, Configuration.WineCapacity, Configuration.PlateCapacity);
        }

        private static void InitializeKnights(Rostrum rostrum, SleazyBar bar)
        {
            for (int i = 0; i < Configuration.NumberOfKnights; i++)
            {
                var knight = new Knight(i, rostrum, bar);
                new Thread(() => knight.Run()).Start();
            }
        }

        private static void InitializeWaiters(SleazyBar bar)
        {
            var wineWaiter = new Waiter(ResourceType.Wine, bar);
            var cucumbersWaiter = new Waiter(ResourceType.Cucumbers, bar);

            new Thread(() => wineWaiter.Run()).Start();
            new Thread(() => cucumbersWaiter.Run()).Start();
        }
    }
}
