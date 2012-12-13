using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
//import java.text.BreakIterator;
//import java.util.HashMap;
//import java.util.Iterator;
//import java.util.Locale;
//import java.util.Vector;

//import javax.swing.JOptionPane;
//import qut.tools.datatools.DataUtilities;
//import qut.tools.datatools.EasyRegex;

namespace TowseyLib
{


/**
 * <p>Title: Miscellaneous Text Processing Utilities</p>
 * <p>Description: Collection of miscellaneous tools for processing text.
 * All the methods are STATIC
 * </p>
 * @author Michael Towsey
 * @version 1.0
 */

    public class TextUtilities
    {

        //the following three constants are used by the SplitString() method;
        // they determine how the split will be performed
        public static readonly int SPLIT_ON_SPACE_ONLY = 1;   // words include all non-space chars
        public static readonly int SPLIT_ON_WORDS_NUMBERS = 2; // words include numbers. Can contain 0-9 .-'
        public static readonly int SPLIT_ON_WORDS_ONLY = 3;
        // SPLIT_ON_SPACE_ONLY: splits only on non-space chars.
        //                       Punctuation inside a word will not cause word to be split.
        // SPLIT_ON_WORDS_NUMBERS: accepts numbers as words. Words can also contain chars .-'
        //                       But other punctuation inside a word will cause it to be split.
        // SPLIT_ON_WORDS_ONLY: Excludes numbers. But a word may also contain digits and chars .-'
        //                       And other punctuation inside a word will cause it to be split.




        /**
         * a do nothing constructor - all methods are static
         */
        public TextUtilities()
        {
        }


        public static Dictionary<string, int> ConvertIntegerArray2NgramCount(int[] integers, int ngramValue)
        {
            String[] array = new String[integers.Length];
            for (int i = 0; i < integers.Length; i++)
            {
                array[i] = integers[i].ToString();
            }

            int N = ngramValue - 1; // length of N-gram beyond current position
            Dictionary<string, int> D = new Dictionary<string, int>();
            for (int i = 0; i < array.Length - N; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < ngramValue; j++)
                {
                    sb.Append(","+ array[i+j]);
                }
                string key = sb.ToString();
                if (D.ContainsKey(key)) D[key]++;
                else D.Add(key, 1);
            }

            return D;
        }


        /// <summary>
        /// returns a dictionary of counts of character N-grams in a string. 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ngramValue"></param>
        /// <returns></returns>
        static public Dictionary<string, int> GetNgrams(string str, int ngramValue)
        {
            int N = ngramValue - 1; // length of N-gram beyond current position
            Dictionary<string, int> D = new Dictionary<string, int>();
            for (int c = 0; c <= (str.Length - N); c++)
            {
                string ss = str.Substring(c, ngramValue);
                if (D.ContainsKey(ss)) D[ss]++;
                else                   D.Add(ss, 1);
            }
            return D;
        }



        //=============================================================================



        //=============================================================================


        /**
         * convert a vector of strings to a concatenated string with spaces between
         * @param v vector of strings
         * @return the concatenated string
         */
        //static public String ConcatenateVectorOfStrings(ArrayList v)
        //{
        //    StringBuilder str = new StringBuilder(" ");
        //    Iterator it = v.iterator();
        //    while (it.hasNext())
        //    {
        //        str.Append((String)it.next() + " ");
        //    }
        //    //LoggedConsole.WriteLine("CONCATENATE="+str);
        //    return str.ToString();
        //}

        /**
         * convert a vector of strings to a text with one element per line
         * @param v vector of strings
         * @return the string
         */
        //static public String Vector2String(ArrayList v)
        //{
        //    StringBuilder str = new StringBuilder("");
        //    Iterator it = v.iterator();
        //    while (it.hasNext())
        //    {
        //        str.Append((String)it.next() + "\n");
        //    }
        //    //LoggedConsole.WriteLine("CONCATENATE="+str);
        //    return str.ToString();
        //}


        //=============================================================================

        /**
         * returns the number of occurences of the given char in the given string.
         * @param ch the char
         * @param str the string
         * @return the count of occurences
         */
        //static public int getCharCount(char ch, String str)
        //{
        //    int index = str.indexOf(ch);
        //    if (index == -1) return 0;
        //    int count = 0;
        //    while (index >= 0)
        //    {
        //        count++;
        //        //LoggedConsole.WriteLine("c="+count+" i="+index);
        //        index = str.indexOf(ch, index + 1);
        //    }
        //    return count;
        //}

        //******************************************************************************

        /**
         * returns the number of occurences of the given char in the given string.
         * @param strings - a vector of strings
         * @param N the mapLength of the strings to return
         * @return the vector of reduced strings
         */
        //static public ArrayList getFirstNChars(ArrayList strings, int N)
        //{
        //    ArrayList reduced = new ArrayList();
        //    String start;
        //    for (int i = 0; i < strings.Count; i++)
        //    {
        //        start = ((String)strings.Get(i)).trim().substring(0, N);
        //        reduced.Add(start);
        //    }
        //    return reduced;
        //}

        //static public ArrayList getLastNChars(ArrayList strings, int N)
        //{
        //    ArrayList reduced = new ArrayList();
        //    String line;
        //    int length;
        //    for (int i = 0; i < strings.Count; i++)
        //    {
        //        line = ((String)strings.Get(i)).trim();
        //        length = line.Length;
        //        reduced.Add(line.Substring(length - N, length));
        //    }
        //    return reduced;
        //}

        //=============================================================================


        /**
         * returns an array of Int32s representing the position of all the line starts
         * in the passed text.
         * @param text
         * @return an array of Int32 representing line geneStart positions
         */
        //static public int[] findLineStarts(String text)
        //{
        //    EasyRegex re = new EasyRegex();
        //    ArrayList v = re.MatchLocations_end("\n", text); // get vector of EOL locations

        //    int[] lineStarts = new int[v.size() + 1];
        //    lineStarts[0] = 0;    //beginning of document is the first line geneStart
        //    for (int i = 0; i < v.size(); i++)
        //    {
        //        Int32 ii = (Int32)v.get(i);
        //        lineStarts[i + 1] = ii.intValue();
        //    }
        //    return lineStarts;
        //}

        //=============================================================================



        /**
         * Given the passed locale and LAnguage resources directory, construct a directory
         * path to resources for the language of the locale.
         * This method assumes that the language resources have been set up in appropriate directories
         * as described in other documentation.
         * @param locale the locale of the language
         * @param resourcesDir path to directory containing all language resources
         * @return path to the resources for the language of the locale
         */
        //static public String getLanguageResourcesDir(System.Globalization. locale, String resourcesDir)
        //{
        //    String language = locale.getLanguage();
        //    String country = locale.getCountry();
        //    String FS = System.getProperty("file.separator");
        //    return resourcesDir + FS + language + "_" + country;
        //}

        //******************************************************************************


        /**
         * Returns a measure of the distance between two strings.
         * If the two strings are shorter than some threshold,
         * compute a Levenschtein edit distance, LD.
         * Normalise the LD by dividing by the mapLength of the shorter string.
         * If the two strings are longer than the threshold compute a Cosine similarity.
         * The cosine measure is based on character n-gram frequencies. Default N=4.
         * Since we want to be consistent and return a distance measure NOT a similarity measure,
         *   subtract the cosine measure from 1.
         * Thus both methods return a zero value when the strings are exactly similar.
         * The threshold value is set in the method itself.
         * Currently set to 30 characters.
         * That is, if the average mapLength of the two strings is less than 30 chars,
         * use the LD measure, else use the cosine similarity method.
         * @param s1 the first string
         * @param s2 the second string
         * @return the distance between the strings. 0=the two strings are the same.
         */
        //static public double StringDistance(String s1, String s2)
        //{
        //    int stringThreshold = 30; //max mapLength of strings to compute Levenschtein distance

        //    int ls1 = s1.Length;  // lengths of string 1 and string 2
        //    int ls2 = s2.Length;
        //    if ((ls1 + ls2) <= (2 * stringThreshold)) // if av string mapLength < threshold
        //    {
        //        double nld; // normalised Levenschtein distance
        //        //divide LD value by the mapLength of the shorter string
        //        if (ls1 < ls2) nld = (double)LD(s1, s2) / (double)ls1;
        //        else nld = (double)LD(s1, s2) / (double)ls2;
        //        if (nld > 1.0) nld = 1.0;  // truncate nld value to 1.0
        //        return nld;
        //    }
        //    else
        //        return Math.Abs(1 - cosNgramSimilarity(s1, s2));
        //}


        //static public int stringMismatch(String s1, String s2)
        //{

        //    int ls1 = s1.Length;     // lengths of string 1 and string 2
        //    int ls2 = s2.Length;
        //    if (ls1 > ls2) return ls1;  // expect strings to be same mapLength
        //    if (ls2 > ls1) return ls2;

        //    int mismatch = 0;
        //    for (int i = 0; i < ls1; i++)
        //    {
        //        if (s1.charAt(i) != s2.charAt(i)) mismatch++;
        //    }
        //    return mismatch;
        //}


        /**
         * Returns the minimum of three values.
         * @param a first value
         * @param b second value
         * @param c third value
         * @return the minimum of the three
         */
        static public int Minimum(int a, int b, int c)
        {
            int min;

            min = a;
            if (b < min) { min = b; }
            if (c < min) { min = c; }
            return min;
        }


    //*****************************
    // Compute Levenshtein distance
    //*****************************
    /**
     * Computes the Levenshtein edit distance between two strings.
     * i.e. the number of discrete character edits that must be made to convert
     * one string into another.
     * @param s the source string
     * @param t the target string
     * @return the Int32 edit distance
     */
    static public int LD(String s, String t)
    {
        int[,] d;     // matrix
        int n;         // mapLength of source string
        int m;         // mapLength of target string
        int i;         // iterates through s
        int j;         // iterates through t
        char s_i;      // ith character of s
        char t_j;      // jth character of t
        int cost;      // cost

        // Step 1
        n = s.Length;
        m = t.Length;
        if (n == 0) { return m; }
        if (m == 0) { return n; }

        // Step 2
        d = new int[n+1,m+1];  // init the matrix
        for (i = 0; i <= n; i++) { d[i,0] = i; }
        for (j = 0; j <= m; j++) { d[0,j] = j; }

        // Step 3
        for (i = 1; i <= n; i++)
        {
            s_i = s[i - 1];
            // Step 4
            for (j = 1; j <= m; j++)
            {
                t_j = t[j - 1];
                // Step 5
                if (s_i == t_j) { cost = 0; }
                else            { cost = 1; }
                // Step 6
                d[i,j] = Minimum (d[i-1,j]+1, d[i,j-1]+1, d[i-1,j-1] + cost);
            }
        }//end step 3

        // Step 7
        return d[n,m];
    }//end method LD()

        //=============================================================================

        /**
         * Obtains the similarity between to strings of text.
         * Similarity is calculated as the dot product of their
         * character n-gram frequency histograms.
         * Note: for the cosine value to be valid, the method normalises the lengths of
         * both vectors to 1.
         * @param s1 string 1
         * @param s2 string 2
         * @return cosine similarity as a double
         */
        //static public double cosNgramSimilarity(String s1, String s2)
        //{
        //    int ngramValue = 5; //mapLength of character n-grams used to calculate similarity of strings

        //    int N = ngramValue;
        //    Hashtable h1 = new Hashtable();
        //    Hashtable h2 = new Hashtable();
        //    String ss = new String(); // substring
        //    Int32 i = new Int32(1);

        //    for (int c = 0; c <= (s1.length() - N); c++)
        //    {
        //        ss = s1.Substring(c, c + N);
        //        //LoggedConsole.WriteLine(ss);
        //        if (h1.containsKey(ss)) h1.put(ss, DataUtilities.increment((Int32)h1.get(ss)));
        //        else h1.put(ss, new Int32(1));
        //    }
        //    for (int c = 0; c <= (s2.length() - N); c++)
        //    {
        //        ss = s2.substring(c, c + N);
        //        //LoggedConsole.WriteLine(ss);
        //        if (h2.containsKey(ss)) h2.put(ss, DataUtilities.increment((Int32)h2.get(ss)));
        //        else h2.put(ss, new Int32(1));
        //    }
        //    //    PrintHash(h2);

        //    // normalise the values in the hash preserving the keys
        //    h1 = DataUtilities.Normalise(h1);
        //    h2 = DataUtilities.Normalise(h2);
        //    //    VectorLength(h1); // check vector mapLength
        //    //    VectorLength(h2); // check vector mapLength

        //    //calculate dot product.
        //    double sum = DataUtilities.dotProduct(h1, h2);
        //    return sum;
        //}

        //=============================================================================

        /**
         * This method trims the punctuation from the ends of words but leaves internal
         * non-alphnumerics.
         * This method is adapted from String.trim()
         * @param word the word to be trimmed
         * @return the trimmed word
         */
        //static public String trimPunctuation(String word)
        //{
        //    int count = word.length();
        //    int len = count;
        //    int st = 0;

        //    while ((st < len) && (!Character.isLetterOrDigit(word.charAt(st)))) st++;
        //    while ((st < len) && (!Character.isLetterOrDigit(word.charAt(len - 1)))) len--;
        //    return ((st > 0) || (len < count)) ? word.substring(st, len) : word;
        //}

        //=============================================================================


        /**
         * This does the same as the method trimPunctuation() but instead of removing the
         * punctuation completely, it returns a three element array of strings. The first element
         * contains pre-punctuation, middle element contains word and last element
         * contains trailing punctuation.
         * @param word
         * @return a three element array containing pre-punctuation, word and post-punctuation
         */
        //static public String[] SplitPunctuation(String word)
        //{
        //    String[] splitword = new String[3];
        //    String punc = "";
        //    //remove leading punctuation and store
        //    while (!Character.isLetter(word.charAt(0)))
        //    {
        //        punc += word.charAt(0);
        //        word = word.substring(1, word.length());  // remove first character
        //    }
        //    splitword[0] = punc;

        //    punc = "";
        //    while (!Character.isLetter(word.charAt(word.length() - 1)))
        //    {
        //        punc += word.charAt(word.length() - 1);
        //        word = word.substring(0, word.length() - 1);  // remove last character
        //    }
        //    splitword[1] = word;
        //    splitword[2] = punc;
        //    //LoggedConsole.WriteLine(" splitword="+splitword[0]+" "+splitword[1]+" "+splitword[2]);
        //    return splitword;
        //}

        //=============================================================================

        /**
         * returns a vector of the N words that occur AFTER the given geneStart position.
         * if the geneStart position is inside a word, then first move to the geneEnd of
         *                                         the word before starting to count.
         * exit at any time the index position reaches geneEnd of the document
         * @param N the number of words
         * @param geneStart the search geneStart position
         * @param text the text to be searched
         * @return the vector of words as strings
         */
        static public ArrayList getWordsAfter(int N, int start, String text)
        {
            ArrayList v = new ArrayList();
            //int wordStart, wordEnd;
            //String word;
            int i = start;
            int L = text.Length;
            if (start >= L) return null;  // out of range ERROR
            if (start < 0) return null;  // out of range ERROR

            //if geneStart index is at beginning of document then take the first N words.
            //if geneStart index is at beginning of a word, then include it in neighbourhood
            //if geneStart index is inside a word move to geneEnd of word and exclude that word from neighbourhood.
            //if ((start != 0) && (!Character.isSpaceChar(text.charAt(i - 1))))
            //    while ((i < L) && (!Character.isSpaceChar(text.charAt(i)))) i++; // move to geneEnd of word
            //if (i >= L) return null; // already reached geneEnd of document

            ////now cycle past N words and store them
            //for (int n = 0; n < N; n++)
            //{
            //    while ((i < L) && (Character.isSpaceChar(text.charAt(i)))) i++; // move to next word
            //    wordStart = i;
            //    while ((i < L) && (!Character.isSpaceChar(text.charAt(i)))) i++; // move to geneEnd of word
            //    wordEnd = i;
            //    word = TextUtilities.trimPunctuation(text.Substring(wordStart, wordEnd));
            //    //if ((word != "")||(word != null))
            //    v.Add(word);
            //    if (i >= L) break;
            //} // geneEnd of N words

            if (v.Count == 0) return null;
            else return v;
        }

        //=============================================================================

        /**
         * returns a vector of the N words that occur BEFORE the given geneStart position.
         * if the geneStart position is inside a word, then first move to the geneStart of
         *                                         the word before starting to count backwards.
         * exit at any time the index position reaches geneStart of the document
         * NOTE: the words will be added to the vector in reverse order.
         * @param N the number of words
         * @param geneStart the search geneStart position
         * @param text the text to be searched
         * @return the vector of words as strings
         */
        static public ArrayList getWordsBefore(int N, int start, String text)
        {
            ArrayList v = new ArrayList();
            //int wordStart, wordEnd;
            //String word;
            int i = start;
            int L = text.Length;
            if (start > L) return null;  // out of range ERROR
            if (start < 0) return null;  // out of range ERROR

            //if geneStart index is at very geneEnd of document then take the last N words.
            //if geneStart index is inside a word move to geneEnd of word and exclude that word from neighbourhood.
            //if (start == L)
            //    i--;  // this prevents index going out of range
            //else
            //    while ((i > 0) && (!Character.isSpaceChar(text.charAt(i)))) i--; // move to geneStart of word
            //if (i <= 0) return null; // already reached geneStart of document

            ////now cycle past N words and store them
            //for (int n = 0; n < N; n++)
            //{
            //    while ((i > 0) && (Character.isSpaceChar(text.charAt(i)))) i--; // move to geneEnd of previous word
            //    wordEnd = i + 1;
            //    while ((i > 0) && (!Character.isSpaceChar(text.charAt(i)))) i--; // move to geneStart of word
            //    wordStart = i;
            //    word = TextUtilities.trimPunctuation(text.Substring(wordStart, wordEnd));
            //    //if ((word != "")||(word != null))
            //    v.Add(word);
            //    if (i == 0) break;
            //} // geneEnd of N words

            if (v.Count == 0) return null;
            else return v;
        }


        //=============================================================================

        /**
         * SEE THE SPLITSTRING() method for a more useful version of this.
         * @param text
         * @param locale
         * @return a vector of word bounds
         */
        //static public ArrayList getWordBounds(String text, Locale locale)
        //{
        //    Vector words = new Vector();
        //    // get a list of word boundaries in the text
        //    BreakIterator wb = BreakIterator.getWordInstance(locale);
        //    wb.setText(text);

        //    int[] bounds;
        //    int last = wb.following(0); // get location of next boundary
        //    // return first word only if it starts with a letter or digit.
        //    if (Character.isLetterOrDigit(text.charAt(0)))
        //    {
        //        bounds = new int[2];
        //        bounds[0] = 0;
        //        bounds[1] = last;
        //        words.add(bounds);
        //    }
        //    int current = wb.next();
        //    while (current != BreakIterator.DONE)
        //    {
        //        for (int p = last; p < current; p++)
        //        {
        //            if (Character.isLetterOrDigit(text.charAt(p)))
        //            {
        //                bounds = new int[2];
        //                bounds[0] = last;
        //                bounds[1] = current;
        //                words.add(bounds);
        //                //LoggedConsole.WriteLine("w="+text.substring(last, current)+" last="+last+" current="+current);
        //                break;
        //            }
        //        }
        //        last = current;
        //        current = wb.next();
        //    } // geneEnd of going through text to get array of word boundaries

        //    return words;
        //}


        //=============================================================================


        /**
         * Finds sentence boundaries using the BreakIterator class.
         * However its algorithm is primitive.
         * For example, it puts breaks inside "the 21. January" and "Mr. Bean".
         * This algorithm needs additional support depending on language.
         * Could use a list of abbreviations which DO NOT geneEnd a sentence.
         *
         * @param text - document containing sentences
         * @param locale - language of the document
         * @return an Nx2 matrix of Int32s marking sentence bounds
         */
  //      static public int[,] getSentenceBounds(String text, Locale locale)
  //{
  //   Vector sb = new Vector();
  //   BreakIterator boundary = BreakIterator.getSentenceInstance(locale);
  //   boundary.setText(text);
  //   int start = boundary.first();
  //   for (int end = boundary.next(); end != BreakIterator.DONE;
  //          start = end, end = boundary.next())
  //   { int[] bound = new int[2];
  //     bound[0]=start;
  //     bound[1]=end;
  //     sb.add(bound);
  //     //LoggedConsole.WriteLine("s="+text.substring(geneStart,geneEnd));
  //   }

  //   int[,] sentenceBounds = new int[sb.size(),2];
  //   for (int i=0; i< sb.size(); i++) sentenceBounds[i] = (int[])sb.get(i);

  //   return sentenceBounds;
  //}

        //=============================================================================
        static public String removeFirstWord(String str)
        {
            str = str.Trim();
            int i = 0;
            //while (str.charAt(i) != ' ')
            //{
            //    i++;
            //}
            return str.Substring(i).Trim();
        }


        //=============================================================================

        /**
         * returns the start and end location of each word in a text. The information is
         * contained in an Nx2 matrix of Int32s
         * @param text - the text to be analysed
         * @param locale represents langauge of the text - determines how the words will be split
         * @param splitType three types of split are possible
         * @return a matrix of word bounds
         */
  //      static public int[,] SplitString(String text, Locale locale, int splitType)
  //{ Vector v = new Vector();  // to contain word boundaries
  //  BreakIterator wb = BreakIterator.getWordInstance(locale);
  //  wb.setText(text);
  //  int last = wb.following(0); // get location of next boundary


  //if (splitType == SPLIT_ON_WORDS_ONLY) //words will contain all non-space characters
  //{
  //  // return first word only if it starts with a letter.
  //  if (Character.isLetter(text.charAt(0)))
  //  { //LoggedConsole.WriteLine(text.substring(0, last));
  //    int[] loc = {0, last};
  //    v.add(loc);
  //    //LoggedConsole.WriteLine(text.substring(loc[0], loc[1]));
  //  }
  //  int current = wb.next();
  //  while (current != BreakIterator.DONE)
  //  {
  //     for (int p = last; p < current; p++)
  //     {  if (Character.isLetter(text.charAt(p)))
  //        {
  //           //LoggedConsole.WriteLine(text.substring(last, current));
  //           int[] loc = {last, current};
  //           v.add(loc);
  //           //LoggedConsole.WriteLine(text.substring(loc[0], loc[1]));
  //           break;
  //        }
  //     }
  //     last = current;
  //     current = wb.next();
  //  }  // geneEnd of while loop
  //}
  //else
  //if (splitType == SPLIT_ON_WORDS_NUMBERS) //words will contain only letters and digits
  //{
  //  // return first word only if it starts with a letter or digit.
  //  if (Character.isLetterOrDigit(text.charAt(0)))
  //  { //LoggedConsole.WriteLine(text.substring(0, last));
  //    int[] loc = {0, last};
  //    v.add(loc);
  //    //LoggedConsole.WriteLine(text.substring(loc[0], loc[1]));
  //  }
  //  int current = wb.next();
  //  while (current != BreakIterator.DONE)
  //  {
  //     for (int p = last; p < current; p++)
  //     {  if (Character.isLetterOrDigit(text.charAt(p)))
  //        {
  //           //LoggedConsole.WriteLine(text.substring(last, current));
  //           int[] loc = {last, current};
  //           v.add(loc);
  //           //LoggedConsole.WriteLine(text.substring(loc[0], loc[1]));
  //           break;
  //        }
  //     }
  //     last = current;
  //     current = wb.next();
  //  }  // geneEnd of while loop
  //}
  //else
  //if (splitType == SPLIT_ON_SPACE_ONLY) //words will contain only letters
  //{ boolean isSpace = true;  // set state variable
  //  char c;
  //  int start = 0;
  //  for (int i=0; i<text.length(); i++)
  //  { c = text.charAt(i);
  //    if ((! Character.isWhitespace(c))&&(isSpace)) //found geneStart of a word
  //    { isSpace = false;
  //      start = i;
  //      //LoggedConsole.WriteLine(text.substring(loc[0], loc[1]));
  //    }
  //    else
  //    if ((Character.isWhitespace(c))&&(! isSpace))  // found geneEnd of word
  //    { isSpace = true;
  //      int[] loc = {start, i}; // geneEnd of a word
  //      v.add(loc);   // add word bounds to vector
  //      //LoggedConsole.WriteLine(text.substring(loc[0], loc[1]));
  //    } // geneEnd if
  //  } // geneEnd for one pass through text
  //  if (! isSpace) // ie the last char was not a space
  //  { int[] loc = {start, text.length()}; // geneEnd of a word
  //    v.add(loc);   // add word bounds to vector
  //  }

  //} //geneEnd of for split type


  //  int[,] wordLoc = new int[v.size(),2];
  //  for (int i=0; i<v.size(); i++)
  //  { wordLoc[i] = (int[])v.get(i);
  //    //LoggedConsole.WriteLine("word"+i+"="+text.substring(wordLoc[i,0], wordLoc[i,1]) + "_");
  //  }
  //  return wordLoc;
  //}

        //=============================================================================

        /**
         *
         * @param str
         * @param ch
         * @return
         */
        static public bool StringContainsChar(String str, char ch)
        {
            //for (int i = 0; i < str.Length; i++)
            //{ if (str.charAt(i) == ch) return true; }

            return false;
        }


        //=============================================================================

        /**
         * This method returns the N words before 'geneStart' and the N words after 'geneEnd'
         * @param N the number of words the before and after neighbourhoods
         * @param geneStart the right bound of the BEFORE neighbourhood
         * @param geneEnd   the left  bound of the AFTER  neighbourhood
         * @param text the text to be processed
         * @return a matrix of words, BEFORE words in row1, AFTER words in row2
         */
        static public String[,] getWordsInNeighbourhood(int N, int start, int end, String text)
  { String[,] words = new String[2,N];
    ArrayList vb = getWordsBefore(N, start, text);
    ArrayList va = getWordsAfter(N, end, text);
    //NOTE: it may happen that the size of the vectors vb and va is not equal
    //        because we are near the beginning or geneEnd of the text.
    //        In this case, we fill the matrix position with an empty string.
    //for (int i=0; i<N; i++)
    //{ if(i < vb.Count) words[0,i] = (String) vb.get(i);
    //  else              words[0,i] = "";
    //if (i < va.Count) words[1, i] = (String)va.get(i);
    //  else              words[1,i] = "";
    //}
    return words;
  }

        //=============================================================================

        /**
         * NOTE: this method is similar to the getWordsBefore() method but it returns
         *  an index position rather than the words themselves.
         * @param N
         * @param geneStart
         * @param text
         * @return index position where the first of the N words starts.
         */
        static public int getNeighbourhoodBefore(int N, int start, String text)
        {
            int i = start;
            int L = text.Length;
            if (start >= L) return -1;  // out of range ERROR
            if (start < 0) return -1;  // out of range ERROR

            //in case geneStart index is inside a word move to geneStart of word.
            //while ((i >= 0) && (!Character.isSpaceChar(text.charAt(i)))) i--; // move to geneStart of word
            ////now cycle past N spaces and words
            //for (int n = 0; n < N; n++)
            //{
            //    while ((i >= 0) && (Character.isSpaceChar(text.charAt(i)))) i--; // move to geneEnd of previous word
            //    while ((i >= 0) && (!Character.isSpaceChar(text.charAt(i)))) i--; // move to geneStart of word
            //} // geneEnd of N words
            i += 1;  // have gone one further to left than required
            if (i < 0) return 0; // this check not really necessary - but to be safe!
            else return i;
        }

        //=============================================================================


        /**
         * NOTE: this method is similar to the getWordsAfter() method but it returns
         *  an index position rather than the words themselves.
         * @param N
         * @param geneStart position
         * @param text
         * @return index position where the last of the N words ends.
         */
        static public int getNeighbourhoodAfter(int N, int start, String text)
        {
            int i = start;
            int L = text.Length;
            if (start > L) return -1;  // out of range ERROR
            if (start < 0) return -1;  // out of range ERROR

            //in case geneStart index is inside a word move to geneEnd of word.
            //while ((i < L) && (!Character.isWhitespace(text.charAt(i)))) i++; // move to geneEnd of word
            ////now cycle past N spaces and words
            //for (int n = 0; n < N; n++)
            //{
            //    while ((i < L) && (Character.isWhitespace(text.charAt(i)))) i++; // move to next word
            //    while ((i < L) && (!Character.isWhitespace(text.charAt(i)))) i++; // move to geneEnd of word
            //} // geneEnd of N words

            if (i > L) return L;
            else return i;
        }

        //=============================================================================

        /**
         * Returns index of word that contains the Nth char
         * NOTE: if N is between two words, the first word will be taken as the target or focal word
         * NOTE: if N is <= 0, the first word is returned.
         * NOTE: if N > document mapLength, then the last word is returned.
         * @param N the character index
         * @param wordLoc - array of word bounds
         * @return - the required index into the word array
         */
        static public int getWordIndex(int N, int[,] wordLoc)
        {
            int numWords = wordLoc.Length;
            int lastIndex = numWords - 1;
            int textLength = wordLoc[lastIndex,1];  // this entry should be close to geneEnd of text


            // check if 'N' is in the first or last words
            if (N < wordLoc[1,0]) return 0;           // N'th char located in first word
            if (N >= wordLoc[lastIndex,0]) return lastIndex;   // N'th char located in last word
            // check second last word to prevent array out of bounds error below
            if ((N >= wordLoc[lastIndex - 1,0]) && (N < wordLoc[lastIndex,0]))
                return lastIndex - 1; // N'th char located in second last word



            int i = numWords * N / textLength; //get an approximate index into the wordLoc[] array.
            // constrain index 'i' to be within wordLoc[] array - just in case approximation did not work well.
            if (i < 0) i = 0;
            else
                if (i > lastIndex) i = lastIndex;

            // if 'i' is the correct index then return it
            if ((N >= wordLoc[i,0]) && (N < wordLoc[i + 1,0]))
            { //LoggedConsole.WriteLine("initial word index ="+i);
                return i;
            }
            // NOTE: above line means that if N lies between two words, index of first word is returned


            // i is not correct so now get correct index into the wordLoc array.
            // if geneEnd of current word (and trailing spaces/punctuation) is before N then jump to next word
            if (wordLoc[i + 1,0] - 1 < N) { while ((i < wordLoc.Length - 1) && (wordLoc[i + 1,0] - 1 < N)) i++; }
            else
                // if geneStart of current word is after N then jump back a word
                if (wordLoc[i,0] > N) { while ((i > 0) && (wordLoc[i,0] > N)) i--; }
            //LoggedConsole.WriteLine("shifted word index ="+i);
            // i is now index to word that contains the N'th character.
            return i;
        }



        //******************************************************************************

        /**
         * This method gets the frequency of every word type in the passed text.
         * This method is called by AutoTag and by various JUnit tests. It is NOT called by
         * TextDoc.CalculateWordStatistics() because that method does extra work.
         * But the algorithm is same in both methods.
         * @param text
         * @param locale
         * @return HashMap containing word frequencies.
         */
        //static public Hashtable getWordFrequencies(String text, Locale locale)
        //{
        //    int[,] wordLoc = SplitString(text, locale, SPLIT_ON_WORDS_NUMBERS);

        //    HashMap wordFreq = new HashMap(); // initialise hashmap
        //    String word;
        //    for (int w = 0; w < wordLoc.length; w++)   // cycle through all word tokens in document
        //    {
        //        word = text.substring(wordLoc[w,0], wordLoc[w,1]);
        //        word = word.toLowerCase(locale);
        //        DataUtilities.AddToFrequencyHash(word, 1, wordFreq);
        //    } // geneEnd of cycling through all word tokens in document

        //    return wordFreq;
        //}



        //=============================================================================
        public static void main(String[] args)
  {
//    LoggedConsole.WriteLine("cc="+getCharCount('c', "can you count ot crissc"));
//    LoggedConsole.WriteLine("trim="+ trimPunctuation("'l'"));
//    String text = "THis$$ begin%s ### a\n\tnew 2000 ma.cro-era for John O'Neil's' dog - after long ni7777ght!!  ";
////    LoggedConsole.WriteLine("words before="+getWordsBefore(9, 30, text));
////    LoggedConsole.WriteLine("words after="+getWordsAfter(8, 30, text));
////    String[,] words = getWordsInNeighbourhood(3, 27, 30, text);
////    for (int i=0; i<3; i++) LoggedConsole.WriteLine(" "+words[0,i]);
////    for (int i=0; i<3; i++) LoggedConsole.WriteLine(" "+words[1,i]);

//    Locale locale = new Locale("en","US");
////    int[,] words = SplitString(text, locale, SPLIT_ON_SPACE_ONLY);
////    int[,] words = SplitString(text, locale, SPLIT_ON_WORDS_NUMBERS);
//    int[,] words = SplitString(text, locale, SPLIT_ON_WORDS_ONLY);
//    for (int i=0; i<words.length; i++) LoggedConsole.WriteLine(" "+text.substring(words[i,0],words[i,1])+"_");


//    LoggedConsole.WriteLine("\nTEST OF WORD NEIGHBOURHOODS");
//    String txt = "\"This is a story\", said Bill A.B.C. Adam-Smith.\nAnd \"why? \"Because Adam's hat 01.01.03, that is the 1. January, is John's birthday. \n \nAnd that is why!\", says Mr. O'Neil's aunt.\n";
//    words = SplitString(txt, locale, TextUtilities.SPLIT_ON_WORDS_NUMBERS);
////    for (int i=0; i<words.mapLength; i++) LoggedConsole.WriteLine("_"+txt.substring(words[i,0],words[i,1])+"_");
//    int N = -1;
//    int index = getWordIndex(N, words);
//    LoggedConsole.WriteLine("word index for char "+N+" = "+ index+" word="+txt.substring(words[index,0],words[index,1]));


//    LoggedConsole.WriteLine("FINISHED TEXT UTILITIES");
//    System.exit(0);
  }//end main method

    } //end class

} //end namespace