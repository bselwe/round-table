using System;
using System.Threading;

namespace RoundTable
{
    public class Knight
    {
        private readonly Rostrum rostrum;
        private readonly Random random;

        private readonly int id;

        public Knight(int id, Rostrum rostrum)
        {
            this.rostrum = rostrum;
            this.random = new Random(Guid.NewGuid().GetHashCode());
            this.id = id;
        }

        public void Run()
        {
            while (true)
            {
                var activity = GetRandomActivity();
                var activityTime = GetActivityTime();

                if (activity == KnightActivity.Talking)
                    rostrum.TryToTalk(id);

                Thread.Sleep(activityTime);

                if (activity == KnightActivity.Talking)
                    rostrum.StopTalking(id);
            }
        }

        private KnightActivity GetRandomActivity()
        {
            var activites = Enum.GetValues(typeof(KnightActivity));
            return (KnightActivity) activites.GetValue(random.Next(activites.Length));
        }

        private int GetActivityTime()
        {
            return random.Next(Configuration.KnightMinSleepTimeMs, Configuration.KnightMaxSleepTimeMs);
        }

        public enum KnightActivity
        {
            Sleeping,
            Talking
        }
    }
}