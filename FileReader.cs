using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HazineCaseStudy
{

    /// <summary>
    /// 
    /// </summary>
    public class FileReader
    {

        private char[] EndIndicators = new char[] { '.', '?' };
        private static string FilePath = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sentenceEndIndicators"></param>
        public FileReader(string filePath, char[] sentenceEndIndicators = null)
        {
            if (sentenceEndIndicators != null && sentenceEndIndicators.Length > 0)
                this.EndIndicators = sentenceEndIndicators;

            if (String.IsNullOrEmpty(filePath))
                throw new Exception("File path can not be null or empty!");
            if (!File.Exists(filePath))
                throw new Exception("File is not exist : " + filePath);

            FilePath = filePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadSentences()
        {
            using (FileStream fs = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                string lastRemain = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    string[] sentences = line.Split(EndIndicators);

                    if (!string.IsNullOrEmpty(lastRemain))
                    {
                        sentences[0] = lastRemain + sentences[0];
                        lastRemain = "";
                    }

                    if (sentences.Length > 1)
                    {
                        for (int i = 0; i < sentences.Length - 1; i++)
                        {
                            yield return sentences[i];
                        }

                        if (EndIndicators.Contains(line[line.Length - 1]))
                        {
                            yield return sentences[sentences.Length - 1];
                        }
                        else
                        {
                            lastRemain = sentences[sentences.Length - 1] + " ";
                        }
                    }
                    else
                    {
                        if (EndIndicators.Contains(line[line.Length - 1]))
                        {
                            yield return sentences[0];
                        }
                        else
                        {
                            lastRemain = lastRemain + sentences[0] + " ";
                        }
                    }
                }
            }
        }
    }

}
