using System;
using System.Threading;

namespace RoundTable
{
    class Program
    {
        private static void Main(string[] args)
        {
            var rostrum = InitializeRostrum();
            var bar = InitializeSleazyBar();
            InitializeKnights(rostrum, bar);
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
    }
}
