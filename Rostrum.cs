using System;
using System.Collections.Generic;
using System.Linq;

namespace RoundTable
{
    public class Rostrum
    {
        private object queueLock = new object();

        private RostrumState[] states;
        private bool[] waiting, listening;
        private ConditionVariable[] qWaiting, qListening;

        public int Knights { get; private set; } // Number of knights (including the king)
        public int King { get; private set; } = 0; // For simplicity, let's assume that the king is always 0 - indexed

        public Rostrum(int knights)
        {
            Knights = knights;

            states = new RostrumState[Knights];
            waiting = new bool[Knights];
            listening = new bool[Knights];
            qWaiting = new ConditionVariable[Knights];
            qListening = new ConditionVariable[Knights];

            for (int k = 0; k < Knights; k++)
            {
                states[k] = RostrumState.NotTalking;
                qWaiting[k] = new ConditionVariable();
                qListening[k] = new ConditionVariable();
            }
        }

        public void TryToTalk(int i)
        {
            lock (queueLock)
            {
                // Wait if the king is currently talking
                while (!IsKing(i) && states[i] == RostrumState.Listening)
                    Listen(i);

                // Wait if any neighbor is talking
                while (GetNeighbors(i).Any(n => states[n] == RostrumState.Talking))
                    Wait(i);

                states[i] = RostrumState.Talking;
                Console.WriteLine($"[{i}] Talking");

                // The king has started talking, set knights's state to listening
                if (IsKing(i))
                    states = states.Select(s => s != RostrumState.Talking ? RostrumState.Listening : s).ToArray();
            }
        }

        public void StopTalking(int i)
        {
            lock (queueLock)
            {
                states[i] = RostrumState.NotTalking;
                Console.WriteLine($"[{i}] Stopped talking");

                if (IsKing(i))
                {
                    // The king has stopped talking
                    // Try to wake up all listening & waiting knights

                    for (int k = 0; k < Knights; k++)
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
                    if (states[King] == RostrumState.Talking)
                    {
                        states[i] = RostrumState.Listening;
                        return;
                    }

                    foreach (var k in GetNeighbors(i))
                        TryToWakeUpWaiting(k);
                }
            }
        }

        private void Listen(int i)
        {
            listening[i] = true;
            Console.WriteLine($"[{i}] Listening to the king");
            qListening[i].Wait(queueLock);
            listening[i] = false;
        }

        private void Wait(int i)
        {
            waiting[i] = true;
            Console.WriteLine($"[{i}] Waiting for neighbors to start talking");
            qWaiting[i].Wait(queueLock);
            waiting[i] = false;
        }

        private void TryToWakeUpListening(int i)
        {
            if (listening[i])
            {
                qListening[i].Pulse();
                Console.WriteLine($"[{i}] Waking up from listening ...");
            }
        }

        private bool TryToWakeUpWaiting(int i)
        {
            if (waiting[i] && AreNotTalking(GetNeighbors(i)))
            {
                qWaiting[i].Pulse();
                Console.WriteLine($"[{i}] Waking up from waiting ...");
                return true;
            }
            return false;
        }

        private IEnumerable<int> GetNeighbors(int i)
        {   
            if (i > 0)
                yield return i - 1;
            if (i == 0)
                yield return Knights - 1;
            if (i < Knights - 1)
                yield return i + 1;
            if (i == Knights - 1)
                yield return 0;
        }

        private bool AreNotTalking(IEnumerable<int> knights)
        {
            return knights.All(k => states[k] != RostrumState.Talking);
        }

        private bool IsKing(int i)
        {
            return i == King;
        }

        public enum RostrumState
        {
            Talking, // The knight is currently talking
            NotTalking, // The knight is not talking
            Listening // The knight is listening to the king
        }
    }
}