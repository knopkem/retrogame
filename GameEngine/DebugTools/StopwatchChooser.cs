using System;

namespace GameEngine.DebugTools
{
#if !WINDOWS
    class Stopwatch
    {
        public TimeSpan Elapsed;

        public static Stopwatch StartNew()
        {
            return new Stopwatch();
        }

        public void Reset()
        {
        }

        public void Start()
        {
        }
    }
#endif
}