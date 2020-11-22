using System;
using System.Collections.Concurrent;

namespace HazineCaseStudy
{
    class ThreadState
    {
        public int Id { get; }
        public int SentenceCount { get; set; }
        public int WordCount { get; set; }
        public ConcurrentQueue<String> Queue { get; }
        public bool CanEnd { get; set; }

        public ThreadState(int id)
        {
            Id = id;
            Queue = new ConcurrentQueue<string>();
            SentenceCount = 0;
            CanEnd = false;
        }
    }
}
