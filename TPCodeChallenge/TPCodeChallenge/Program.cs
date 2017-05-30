using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;

namespace TPCodeChallenge
{
    class Program
    {
        #region Global variables.
        /// <summary>
        /// The weighted string version of the checkPrase.
        /// </summary>
        private static WeighedString checkPhrase = new WeighedString(ConfigurationManager.AppSettings.Get("checkPhrase"));
        /// <summary>
        /// The easy md5 hash to test the final phrases with to determine the answer.
        /// </summary>
        private static string checkHashEasy = ConfigurationManager.AppSettings.Get("checkHashEasy");
        /// <summary>
        /// The medium md5 hash to test the final phrases with to determine the answer.
        /// </summary>
        private static string checkHashMedium = ConfigurationManager.AppSettings.Get("checkHashMedium");
        /// <summary>
        /// The hard md5 hash to test the final phrases with to determine the answer.
        /// </summary>
        private static string checkHashHard = ConfigurationManager.AppSettings.Get("checkHashHard");
        /// <summary>
        /// The number of answers found currently by the application.
        /// </summary>
        private static int answersFound = 0;
        #endregion

        /// <summary>
        /// Reads from the configuration file a phrase, a words list and three MD5 hash values.
        /// Then produces all possible anagrams of that phrase.
        /// Finally, it displays the phrases that correspond to the given MD5 hash values (if any).
        /// </summary>
        static void Main()
        {
            string wordListFileLocation = ConfigurationManager.AppSettings.Get("wordListLocation");
            List<string> wordList = File.ReadAllLines(wordListFileLocation).ToList();

            Console.WriteLine("The initial word list contains " + wordList.Count.ToString() + " words.");
            wordList = RemoveInvalidAndDuplicateWords(wordList, checkPhrase);
            Console.WriteLine("The word list after removing invalid words contains " + wordList.Count.ToString() + " words.");
            // Default assumption of a maximum of 4 word anagrams. (next region enables user defined depth searches)
            int noOfWords = 0;
            int.TryParse(ConfigurationManager.AppSettings.Get("defaultNoOfWords"), out noOfWords);

            #region User specified words search.
            //// The user may choose the maximum depth of recursion in order to improve performance if he knows the number of words
            //// contained in the actual solution.
            //string userInput = "";
            //do
            //{
            //    Console.WriteLine("");
            //    Console.WriteLine("Please choose a maximum number of words for the searched anagrams.");
            //    Console.WriteLine("WARNING: Any number of words over 3 will take considerably more time to complete.");
            //    Console.WriteLine("");
            //    Console.WriteLine("Number of words (leave empty if none specified): ");
            //    userInput = Console.ReadLine();
            //    if (string.IsNullOrEmpty(userInput))
            //    {
            //        noOfWords = checkPhrase.Phrase.Replace(" ", "").Length;  // When the user does not specify a number of words then choose
            //        userInput = noOfWords.ToString();                        // the length of the initial phrase assuming that the solution could be
            //    }                                                            // a combination of 1 character words.
            //    if (!int.TryParse(userInput, out noOfWords))
            //    {
            //        Console.WriteLine("");
            //        Console.WriteLine("The number of words must be an integer number greater than zero.");
            //    }
            //} while (noOfWords < 1);
            #endregion

            // The anagram search takes place.
            Console.WriteLine("");
            Console.WriteLine("Processing started...." + DateTime.Now.ToShortTimeString());
            CheckWordList(wordList, checkPhrase, noOfWords);
            Console.WriteLine("Processing ended...." + DateTime.Now.ToShortTimeString());
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Removes all words from a list that could not be parts of the specified character weight
        /// and limits appearances of words to one per word.
        /// </summary>
        /// <param name="sourceList">The list that is going to be filtered.</param>
        /// <param name="sourceWeight">The character weight to compare the list to.</param>
        /// <returns></returns>
        private static List<string> RemoveInvalidAndDuplicateWords(List<string> sourceList, WeighedString sourceWeighedString)
        {
            List<string> result = new List<string>();

            foreach (string word in sourceList)
            {
                WeighedString weighedWord = word.ToWeighedString();
                if (weighedWord.IsSubSetOf(sourceWeighedString)
                    && !result.Contains(word))
                    result.Add(word);
            }
            return result;
        }

        /// <summary>
        /// Searches all possible anagrams of the specified wordList (of the specified number of words) that have
        /// the same character weight as that of the checkString. For those that satisfy the criteria it makes an md5 hash check against
        /// the three possible hashes specified in the application settings.
        /// </summary>
        /// <param name="wordList">The input wordList to check for possible solutions.</param>
        /// <param name="checkString">The string to base the comparison on.</param>
        /// <param name="numberOfWords">The number of words in the anagrams.</param>
        private static void CheckWordList(List<string> wordList, WeighedString checkString, int numberOfWords)
        {
            // After each iteration all anagrams containing the checked word have been processed.
            // The wordListCopy helps us remove processed words from it without affecting the iteration.

            wordList = wordList.OrderByDescending(x => x.Length).ToList();
            List<string> wordListCopy = new List<string>(wordList);
            foreach (string word in wordList)
            {
                if (answersFound > 2)  // All answers have been found.
                    return;
                wordListCopy.Remove(word);
                CheckAnagramsBasedOnWord(word, wordListCopy, checkString, numberOfWords);
            }
        }

        /// <summary>
        /// Used internally to produce and check for a solution on all possible anagramms that contain the baseWord.
        /// </summary>
        /// <param name="baseWord">The word (or phrase) that is going to be checked for solutions.</param>
        /// <param name="wordList">The rest of the word list to make combinations with.</param>
        /// <param name="checkString">The comparison phrase.</param>
        /// <param name="numberOfWords">The maximum number on which the recursive search will take place.</param>
        private static void CheckAnagramsBasedOnWord(string baseWord, List<string> wordList, WeighedString checkString, int numberOfWords)
        {
            if (answersFound > 2) // All answers have been found.
                return;

            // The function always attempts to hash check word combinations to the baseWord that have the same character weight to the
            // checkString.
            List<string> checkWords = wordList.FindAll(x => (baseWord.ToWeighedString() + x.ToWeighedString()) == checkString);
            foreach (string word in checkWords)
                HashCheck(baseWord + " " + word);

            // Then it adds words to the current base word to explore anagramms of the combination (subsets) using recursion.
            if (baseWord.Count(x => x == ' ') < (numberOfWords - 2))  // Max recursion limitation to improve performance.
            {
                List<string> eligibleWords = wordList.FindAll(x => (baseWord.ToWeighedString() + x.ToWeighedString()).IsSubSetOf(checkString));
                List<string> eligibleWordsCopy = new List<string>(eligibleWords);
                eligibleWords = eligibleWords.OrderBy(x => x.Length).ToList();
                foreach (string word in eligibleWords)
                {
                    if (word.Length > (checkString.Phrase.Length - baseWord.Length + 1))  // Since the list is sorted the remaining word combinations
                        return;                                                           // will always produce larger phrases than the checkString.
                    eligibleWordsCopy.Remove(word);
                    CheckAnagramsBasedOnWord(baseWord + " " + word, eligibleWordsCopy, checkString, numberOfWords);
                }
            }
        }

        /// <summary>
        /// Compares the MD5 hash of the given checkPhrase and all of its space based
        /// permutations to the ones found in the application settings.
        /// For each answer found a notification message is shown on the screen.
        /// </summary>
        /// <param name="checkWord">The word to be hash checked.</param>
        private static void HashCheck(string checkWord)
        {
            List<string> checkPerms = checkWord.GetAllPermutations(' ');
            foreach (string word in checkPerms)
            {
                string wordHash = GenerateMd5Hash(word);
                if (wordHash.ToLower() == checkHashEasy)
                {
                    Console.WriteLine("The easy answer is: " + word);
                    answersFound++;
                }
                if (wordHash.ToLower() == checkHashMedium.ToLower())
                {
                    Console.WriteLine("The medium answer is: " + word);
                    answersFound++;
                }
                if (wordHash.ToLower() == checkHashHard.ToLower())
                {
                    Console.WriteLine("The hard answer is: " + word);
                    answersFound++;
                }
            }
        }

        /// <summary>
        /// Generates the MD5 Hash code of the inputText.
        /// </summary>
        /// <param name="inputText">The text to be processed.</param>
        private static string GenerateMd5Hash(string inputText)
        {
            // Calculate the MD5 has from the inputText.
            MD5 md5 = MD5.Create();
            byte[] inputTextBytes = Encoding.ASCII.GetBytes(inputText);
            byte[] hash = md5.ComputeHash(inputTextBytes);

            // Convert the byte array to a hex string.
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
