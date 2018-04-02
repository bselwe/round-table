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

        private int knights; // Number of knights (including the king)
        // For simplicity, let's assume that the king is always 0 - indexed.

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
            }
        }

        public void TryToTalk(int i)
        {
            lock (queueLock)
            {
                // The king is currently talking
                if (!IsKing(i) && states[i] == RostrumState.Listening)
                    Listen(i);

                // Any neighbor is talking
                if (GetNeighbors(i).Any(n => states[n] == RostrumState.Talking))
                    Wait(i);

                states[i] = RostrumState.Talking;
                Console.WriteLine($"[{i}] Talking");

                // The king has started talking, others are listening
                if (IsKing(i))
                    states.Apply(s => s != RostrumState.Talking ? RostrumState.Listening : s);
            }
        }

        public void StopTalking(int i)
        {
            lock (queueLock)
            {
                states[i] = RostrumState.NotTalking;

                if (IsKing(i))
                {
                    // The king stopped talking
                    // Trying to wake up all listening & waiting knights

                    for (int k = 1; k < knights; k++)
                    {
                        if (states[k] == RostrumState.Listening)
                        {
                            states[k] = RostrumState.NotTalking;
                            
                            if (listening[k])
                                qListening[k].Pulse();

                            if (waiting[k] && AreNotTalking(GetNeighbors(k)))
                                qWaiting[k].Pulse();
                        }
                    }   
                }
                else
                {
                    // The knight stopped talking
                    // Trying to wake up his waiting neighbors
                    // (only when the king is not talking)

                    if (states[0] == RostrumState.Talking)
                    {
                        states[i] = RostrumState.Listening;
                        return;
                    }

                    foreach (var k in GetNeighbors(i))
                    {
                        if (waiting[k] && AreNotTalking(GetNeighbors(k)))
                            qWaiting[k].Pulse();
                    }
                }
            }
        }

        private void Listen(int i)
        {
            listening[i] = true;
            Console.WriteLine($"[{i}] Listening");
            qListening[i].Wait(queueLock);
            listening[i] = false;
        }

        private void Wait(int i)
        {
            waiting[i] = true;
            Console.WriteLine($"[{i}] Waiting");
            qWaiting[i].Wait(queueLock);
            waiting[i] = false;
        }

        private IEnumerable<int> GetNeighbors(int i)
        {   
            yield return (i - 1) % knights;
            yield return (i + 1) % knights;
        }

        private bool AreNotTalking(IEnumerable<int> knights)
        {
            return knights.All(k => states[k] != RostrumState.Talking);
        }

        private bool IsKing(int i)
        {
            return i == 0;
        }

        public enum RostrumState
        {
            Talking,
            NotTalking,
            Listening // Listening to the king
        }
    }
}