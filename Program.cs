using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HazineCaseStudy
{
    class Program
    {
        static String FilePath = "";
        static int ThreadCount = 5;

        static ConcurrentDictionary<String, int> WORD_COUNTS = new ConcurrentDictionary<String, int>();
        static List<Task> threads = new List<Task>();
        static List<ThreadState> threadStates = new List<ThreadState>();

        private static Task DoSomethingAsync(int x)
        {
            return Task.Run(() => WorkerThread(x));
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
                FilePath = args[0];

            if (args.Length > 1)
                int.TryParse(args[1], out ThreadCount);

            FileReader fileReader = new FileReader(FilePath);
            int count = 0;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < ThreadCount; i++)
            {
                threadStates.Add(new ThreadState(i));
                // Action<int> action = (count) => WorkerThread(count);
                Task workerThread = DoSomethingAsync(i);
                threads.Add(workerThread);
            }

            int runOrder = 0;
            foreach (string sentence in fileReader.ReadSentences())
            {
                if (String.IsNullOrEmpty(sentence))
                    continue;
                count++;
                threadStates[runOrder].Queue.Enqueue(sentence);
                runOrder = (runOrder + 1) % ThreadCount;
            }

            System.Console.WriteLine("File reading elapsed time : {0} miliseconds", watch.ElapsedMilliseconds);

            foreach (var thread in threadStates)
            {
                thread.CanEnd = true;
            }
            Task.WaitAll(threads.ToArray());

            watch.Stop();
            System.Console.WriteLine("{0} Thread - Elapsed time : {1} miliseconds", ThreadCount, watch.ElapsedMilliseconds);

            int totalWord = threadStates.Sum(item => item.WordCount);

            System.Console.WriteLine("Sentence Count : {0}", count);
            System.Console.WriteLine("Word Count : {0}", totalWord);
            System.Console.WriteLine("Avg. Word Count : {0}", totalWord / count);

            foreach (ThreadState threadState in threadStates)
            {
                System.Console.WriteLine("ThreadId={0}, Count={1}", threadState.Id, threadState.SentenceCount);
            }

            var orderedResults = WORD_COUNTS.OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            foreach (var item in orderedResults)
            {
                System.Console.WriteLine("{0} {1}", item.Key, item.Value);
            }
        }

        static void WorkerThread(Object indexNum)
        {
            int index = (int)indexNum;
            // Console.WriteLine("Thread-{0} strted", id);

            while (true)
            {
                if (threadStates[index].CanEnd && threadStates[index].Queue.IsEmpty)
                    break;

                String sentence = "";
                if (threadStates[index].Queue.TryDequeue(out sentence))
                {
                    String[] words = sentence.Split(' ');

                    threadStates[index].SentenceCount = threadStates[index].SentenceCount + 1;
                    foreach (var word in words)
                    {
                        if (String.IsNullOrEmpty(word))
                            continue;

                        threadStates[index].WordCount = threadStates[index].WordCount + 1;
                        WORD_COUNTS.AddOrUpdate(word, 1, (key, oldVal) => oldVal + 1);
                    }
                }
                // else
                // {
                //     Thread.Sleep(1);
                // }
            }

            // Console.WriteLine("Thread-{0} finished", id);
        }

    }

}
