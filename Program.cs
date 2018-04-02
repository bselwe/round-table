using System;

namespace RoundTable
{
    class Program
    {
        private static int numberOfKnights = Configuration.DefaultNumberOfKnights;

        private static void Main(string[] args)
        {
            HandleArguments(args);
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
    }
}
