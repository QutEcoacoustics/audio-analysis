using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace MarkovModels
{
    public class TrainingSet
    {

        public int Count { get { return sequences.Count; } }

        private Hashtable tagList = null;
        private List<string[]> sequences;

        public TrainingSet()
        {
        }

        public void AddSequence(string tag, string sequence)
        {
            if (sequences == null) sequences = new List<string[]>();
            if (tagList == null) tagList = new Hashtable();

            string[] data = new string[] { tag, sequence };
            sequences.Add(data);
            if (!tagList.ContainsKey(tag))
                tagList.Add(tag, 1);
        }

        public void AddSequences(string tag, string[] sequences)
        {
            for (int i = 0; i < sequences.Length; i++)
                AddSequence(tag, sequences[i]);
        }

        public string[] GetSequences(string label)
        {
            return sequences.Where(d => d[0] == label).Select(d => d[1]).ToArray();
        }

        public int GetSequenceCount(string label)
        {
            return sequences.Where(d => d[0] == label).Count();
        }

        public string[] GetSequences()
        {
            return sequences.Select(d => d[1]).ToArray();
        }

        public void WriteComposition()
        {
            Console.WriteLine("\tCOMPOSITION OF TRAINING DATA.");
            ICollection tags = tagList.Keys;
            foreach (string tag in tags)
            {
                int number = GetSequenceCount(tag);
                Console.WriteLine("\t Word=" + tag + "  Number of examples=" + number);
                string[] words = GetSequences(tag);
                for (int i = 0; i < words.Length; i++) Console.WriteLine("\t  " + words[i]);
            }
        }


        public static double AverageSequenceLength(string[] examples)
        {
            int examplecount = examples.Length;
            int totalLength = 0;

            for (int w = 0; w < examplecount; w++)
            {
                string word = examples[w];
                totalLength += word.Length;
            }
            return totalLength / (double)examplecount;
        }


    }//class TrainingSet

}
