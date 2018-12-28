using System;
using System.Diagnostics;

namespace Industry.Utilities
{
    public class Timer
    {
        public enum Units
        {
            Seconds, Milliseconds, NanoSeconds
        }

        public Timer()
        {
            stopwatch = new Stopwatch();
        }

        private Stopwatch stopwatch;

        public Timer Start()
        {
            if (!stopwatch.IsRunning)
                stopwatch.Start();

            return this;
        }
        
        public double ElapsedTime(Units time)
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();
            else return 0;

            /*
            long eMillis = stopwatch.ElapsedMilliseconds;

            if (time == Units.NanoSeconds)
                return 1000000 * eMillis;
            else if (time == Units.Milliseconds)
                return eMillis;
            else
                return eMillis / 1000;
            */
            
            double ticks = stopwatch.ElapsedTicks;
            ticks /= Stopwatch.Frequency;
            stopwatch.Reset();

            if (time == Units.NanoSeconds)
                return 1000000000.0 * ticks;
            else if (time == Units.Milliseconds)
                return 1000.0 * ticks;
            else
                return ticks;
            
        }        
    }
}
