using System;

namespace HazineCaseStudy
{
    class Program
    {
        static bool DEBUG_MODE_ENABLED = true;
        static int workerThreadCount = 5;
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                int.TryParse(args[1], out workerThreadCount);
            }

            FileReader fileReader = new FileReader("test-data/file2.txt");

            int count = 0;
            foreach (string sentence in fileReader.ReadSentences())
            {
                if (!String.IsNullOrEmpty(sentence))
                    Console.WriteLine("{0} - {1}", count++, sentence);
            }

        }
    }
}
