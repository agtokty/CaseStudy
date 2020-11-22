using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            Queue = new ConcurrentQueue<string>();
            SentenceCount = 0;
            CanEnd = false;
        }
    }
    class Program
    {
        static ConcurrentDictionary<String, int> WORD_COUNTS = new ConcurrentDictionary<String, int>();
        static int ThreadCount = 5;

        static List<Thread> threads = new List<Thread>();
        static List<ThreadState> threadStates = new List<ThreadState>();

        static void Main(string[] args)
        {
            String filePath = "";

            if (args.Length > 0)
            {
                filePath = args[0];
            }

            if (args.Length > 1)
                int.TryParse(args[1], out ThreadCount);

            FileReader fileReader = new FileReader(filePath);
            int count = 0;


            for (int i = 0; i < ThreadCount; i++)
            {
                threadStates.Add(new ThreadState(i));
                Thread workerThread = new Thread(new ParameterizedThreadStart(WorkerThread));
                workerThread.Start(i);
                threads.Add(workerThread);
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            int order = 0;
            foreach (string sentence in fileReader.ReadSentences())
            {
                if (String.IsNullOrEmpty(sentence))
                    continue;

                // if (count % 100 == 0)
                //     Console.WriteLine("{0} - {1}", count, sentence);

                count++;
                threadStates[order].Queue.Enqueue(sentence);
                order = (order + 1) % ThreadCount;
            }

            foreach (ThreadState threadState in threadStates)
            {
                threadState.CanEnd = true;
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            watch.Stop();
            System.Console.WriteLine("{0} Thread - Elapsed time : {1} miliseconds", ThreadCount, watch.ElapsedMilliseconds);

            int totalWord = threadStates.Sum(item => item.WordCount);

            System.Console.WriteLine("Sentence Count : {0}", count);
            System.Console.WriteLine("Avg. Word Count : {0}", totalWord / count);

            foreach (ThreadState threadState in threadStates)
            {
                System.Console.WriteLine("ThreadId={0}, Count={1}", threadState.Id, threadState.SentenceCount);
            }

            // Console.ReadLine();
        }

        static void WorkerThread(Object idNum)
        {
            int id = (int)idNum;
            // Console.WriteLine("Thread-{0} strted", id);

            // while (!threadStates[id].CanEnd && !threadStates[id].Queue.IsEmpty)
            while (true)
            {
                // Console.WriteLine("Thread-{0} working", id);
                // continue;
                if (threadStates[id].CanEnd && threadStates[id].Queue.IsEmpty)
                    break;

                String sentence = "";
                if (!threadStates[id].Queue.IsEmpty && threadStates[id].Queue.TryDequeue(out sentence))
                {
                    String[] words = sentence.Split(' ');

                    threadStates[id].SentenceCount = threadStates[id].SentenceCount + 1;
                    threadStates[id].WordCount = threadStates[id].WordCount + words.Length;
                    foreach (var word in words)
                    {
                        if (String.IsNullOrEmpty(word))
                            continue;
                        // threadStates[id].WordCount = threadStates[id].WordCount + 1;
                        // WORD_COUNTS.AddOrUpdate(word, 1, (key, oldVal) => oldVal + 1);
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            // Console.WriteLine("Thread-{0} finished", id);
        }
    }
}
