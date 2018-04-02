using System;
using System.Threading;

namespace RoundTable
{
    class Program
    {
        private static int numberOfKnights = Configuration.DefaultNumberOfKnights;

        private static void Main(string[] args)
        {
            HandleArguments(args);

            var rostrum = InitializeRostrum();
            InitializeKnights(rostrum);
        }

        private static void HandleArguments(string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Length != 1)
                    throw new ArgumentException("Invalid number of arguments. Arguments: n, n - knights.");
                
                if (!Int32.TryParse(args[0], out numberOfKnights))
                    throw new ArgumentException("All arguments must be integers. Arguments: n, n - knights.");
            }
        }

        private static Rostrum InitializeRostrum()
        {
            return new Rostrum(numberOfKnights);
        }

        private static void InitializeKnights(Rostrum rostrum)
        {
            for (int i = 0; i < numberOfKnights; i++)
            {
                var knight = new Knight(i, rostrum);
                new Thread(() => knight.Run()).Start();
            }
        }
    }
}
