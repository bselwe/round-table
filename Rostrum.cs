using System;
using System.Collections.Generic;
using System.Linq;

namespace RoundTable
{
    public class Rostrum
    {
        private readonly object queueLock = new object();
        private readonly int knights; // Number of knights (including the king) - always even
        private readonly int king = 0; // For simplicity, let's assume that the king is always 0 - indexed

        private RostrumState[] states;
        private bool[] waiting, listening;
        private ConditionVariable[] qWaiting, qListening;

        public Rostrum(int knights)
        {
            this.knights = knights;

            states = new RostrumState[knights];
            waiting = new bool[knights];
            listening = new bool[knights];
            qWaiting = new ConditionVariable[knights];
            qListening = new ConditionVariable[knights];

            for (int k = 0; k < knights; k++)
            {
                states[k] = RostrumState.NotTalking;
                qWaiting[k] = new ConditionVariable();
                qListening[k] = new ConditionVariable();
            }
        }

        public void TryToTalk(int knight)
        {
            lock (queueLock)
            {
                // Wait if the king is currently talking
                while (!IsKing(knight) && states[knight] == RostrumState.Listening)
                    Listen(knight);

                // Wait if any neighbor is talking
                while (GetNeighbors(knight).Any(n => states[n] == RostrumState.Talking))
                    Wait(knight);

                states[knight] = RostrumState.Talking;
                Console.WriteLine($"[{knight}] Talking");

                // The king has started talking, set knights's state to listening
                if (IsKing(knight))
                    states = states.Select(s => s != RostrumState.Talking ? RostrumState.Listening : s).ToArray();
            }
        }

        public void StopTalking(int knight)
        {
            lock (queueLock)
            {
                states[knight] = RostrumState.NotTalking;
                Console.WriteLine($"[{knight}] Stopped talking");

                if (IsKing(knight))
                {
                    // The king has stopped talking
                    // Try to wake up all listening & waiting knights

                    for (int k = 0; k < knights; k++)
                    {
                        if (!IsKing(k) && states[k] == RostrumState.Listening)
                        {
                            states[k] = RostrumState.NotTalking;
                            TryToWakeUpListening(k);
                            TryToWakeUpWaiting(k);
                        }
                    }   
                }
                else
                {
                    // The knight has stopped talking
                    // Try to wake up his waiting neighbors
                    
                    // Ignore when the king is talking
                    if (states[king] == RostrumState.Talking)
                    {
                        states[knight] = RostrumState.Listening;
                        return;
                    }

                    foreach (var k in GetNeighbors(knight))
                        TryToWakeUpWaiting(k);
                }
            }
        }

         private void Listen(int k)
        {
            listening[k] = true;
            Console.WriteLine($"[{k}] Listening to the king");
            qListening[k].Wait(queueLock);
            listening[k] = false;
        }

        private void Wait(int k)
        {
            waiting[k] = true;
            Console.WriteLine($"[{k}] Waiting for neighbors to start talking");
            qWaiting[k].Wait(queueLock);
            waiting[k] = false;
        }

        private void TryToWakeUpListening(int k)
        {
            if (listening[k])
            {
                qListening[k].Pulse();
                Console.WriteLine($"[{k}] Waking up from listening ...");
            }
        }

        private void TryToWakeUpWaiting(int k)
        {
            if (waiting[k] && AreNotTalking(GetNeighbors(k)))
            {
                qWaiting[k].Pulse();
                Console.WriteLine($"[{k}] Waking up from waiting ...");
            }
        }

        private IEnumerable<int> GetNeighbors(int k)
        {   
            if (k > 0)
                yield return k - 1;
            if (k == 0)
                yield return knights - 1;
            if (k < knights - 1)
                yield return k + 1;
            if (k == knights - 1)
                yield return 0;
        }

        private bool AreNotTalking(IEnumerable<int> knights)
        {
            return knights.All(k => states[k] != RostrumState.Talking);
        }

        private bool IsKing(int k)
        {
            return k == king;
        }

        public enum RostrumState
        {
            Talking, // The knight is currently talking
            NotTalking, // The knight is not talking
            Listening // The knight is listening to the king
        }
    }
}