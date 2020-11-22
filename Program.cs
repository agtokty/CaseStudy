using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HazineCaseStudy
{
    class Program
    {
        static String TextFilePath = "";
        static int ThreadCount = 5;

        static ConcurrentDictionary<String, int> WORD_COUNTS_RESULTS = new ConcurrentDictionary<String, int>();
        static int TOTAL_SENTENCE_COUNT = 0;

        static List<Thread> threads = new List<Thread>();
        static List<ThreadState> threadStates = new List<ThreadState>();

        static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                TextFilePath = args[0];
            }
            else
            {
                Console.WriteLine("Please provide a text file path");
                return 0xA0;//ERROR_BAD_ARGUMENTS
            }

            if (args.Length > 1)
                int.TryParse(args[1], out ThreadCount);

            FileReader fileReader = new FileReader(TextFilePath);

            //var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < ThreadCount; i++)
            {
                threadStates.Add(new ThreadState(i));
                Thread workerThread = new Thread(new ParameterizedThreadStart(WorkerThread));
                workerThread.Start(i);
                threads.Add(workerThread);
            }

            int runOrder = 0;
            foreach (string sentence in fileReader.ReadSentences())
            {
                if (String.IsNullOrEmpty(sentence))
                    continue;
                TOTAL_SENTENCE_COUNT++;
                threadStates[runOrder].Queue.Enqueue(sentence);
                runOrder = (runOrder + 1) % ThreadCount;
            }
            // System.Console.WriteLine("File reading elapsed time : {0} miliseconds", watch.ElapsedMilliseconds);

            foreach (ThreadState thread in threadStates)
            {
                thread.CanEnd = true;
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            //watch.Stop();
            //System.Console.WriteLine("{0} Thread - Elapsed time : {1} miliseconds", ThreadCount, watch.ElapsedMilliseconds);

            PrintResults2Console();

            return 0x0;//ERROR_SUCCESS
        }

        static void PrintResults2Console()
        {
            int totalWordCount = threadStates.Sum(item => item.WordCount);

            System.Console.WriteLine("Sentence Count : {0}", TOTAL_SENTENCE_COUNT);
            // System.Console.WriteLine("Word Count : {0}", totalWordCount);
            System.Console.WriteLine("Avg. Word Count : {0}", totalWordCount / TOTAL_SENTENCE_COUNT);

            foreach (ThreadState threadState in threadStates)
            {
                System.Console.WriteLine("ThreadId={0}, Count={1}", threadState.Id, threadState.SentenceCount);
            }

            var orderedResults = WORD_COUNTS_RESULTS.OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            foreach (var item in orderedResults)
            {
                System.Console.WriteLine("{0} {1}", item.Key, item.Value);
            }
        }

        /// <summary>
        /// Common word counting method that use the state of a particular thread by its index
        /// </summary>
        /// <param name="indexNum">index of the worker thread</param>
        static void WorkerThread(Object indexNum)
        {
            int index = (int)indexNum;
            // Console.WriteLine("Thread-{0} strted", id);

            while (true)//Work constantly
            {
                //Until its marked as done and its queue is empty
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
                        WORD_COUNTS_RESULTS.AddOrUpdate(word, 1, (key, oldVal) => oldVal + 1);
                    }
                }
                // else //sleep a bit before loop again
                // {
                //     Thread.Sleep(1);
                // }
            }
            // Console.WriteLine("Thread-{0} finished", id);
        }

    }

}
