using System.Collections.Generic;
using System.Linq;

namespace TPCodeChallenge
{
    /// <summary>
    /// Represents a string acompanied by its character weight.
    /// Character weights are sets of each character present in the current string acompanied by the number
    /// of times it appears in it. Spaces are not included when weighing a string.
    /// </summary>
    public class WeighedString
    {
        /// <summary>
        /// The string representation of the weighted string.
        /// </summary>
        public string Phrase { get; set; }

        /// <summary>
        /// The character weight of the weighted string.
        /// </summary>
        public Dictionary<char, int> Weight { get; }

        /// <summary>
        /// The number of spaces present in the Phrase property.
        /// </summary>
        public int SpaceCount { get; }

        /// <summary>
        /// Initializes a new instance of the WeighedString class setting it's Phrase property.
        /// </summary>
        /// <param name="phrase">The Phrase property of this instance.</param>
        public WeighedString(string phrase)
        {
            Phrase = phrase;
            Weight = GetCharWeight(phrase);
            SpaceCount = Phrase.Count(x => x == ' ');
        }

        /// <summary>
        /// Returns the set of characters in the specified text and how many times each one appears in it.
        /// Note: Spaces are not included in the returned set.
        /// </summary>
        /// <param name="inputText">The text for which the character weight will be returned.</param>
        /// <returns></returns>
        private static Dictionary<char, int> GetCharWeight(string inputText)
        {
            Dictionary<char, int> charWeight = new Dictionary<char, int>();
            inputText = inputText.Replace(" ", "");
            foreach (char character in inputText)
            {
                if (charWeight.ContainsKey(character))
                {
                    int weight = 0;
                    charWeight.TryGetValue(character, out weight);
                    weight++;
                    charWeight[character] = weight;
                }
                else
                    charWeight.Add(character, 1);
            }
            return charWeight;
        }

        /// <summary>
        /// Determines if the current WeighedString's Weight is a subset of the target WeighedString's Weight.
        /// The WeighedString is a subset even when it has the exact same Weight as the one being compared to.
        /// </summary>
        /// <param name="compareTo">The WeighedString to compare this instance to.</param>
        /// <returns></returns>
        public bool IsSubSetOf(WeighedString compareTo)
        {
            if (Weight.Count > compareTo.Weight.Count) return false;

            // If the current WeighedString contains a character not found in the compareTo or
            // if it contains a character more times then it's not a subset.
            foreach (char currWeightKey in Weight.Keys)
            {
                if (!compareTo.Weight.ContainsKey(currWeightKey)
                    || Weight[currWeightKey] > compareTo.Weight[currWeightKey]) return false;
            }
            return true;
        }

        /// <summary>
        /// Two weighed strings are considered equal when their weights match.
        /// </summary>
        /// <param name="a">The first item of the comparison.</param>
        /// <param name="b">The second item of the comparison.</param>
        public static bool operator ==(WeighedString a, WeighedString b)
        {
            // Evaluate the variety of characters.
            if (a.Weight.Count != b.Weight.Count) return false;

            foreach (char currWeightKey in a.Weight.Keys)
            {
                if (!b.Weight.ContainsKey(currWeightKey)
                    || a.Weight[currWeightKey] != b.Weight[currWeightKey]) return false;
            }
            return true;
        }

        public static bool operator !=(WeighedString a, WeighedString b)
        {
            return !(a == b);
        }

        public static WeighedString operator +(WeighedString a, WeighedString b)
        {
            return new WeighedString(a.Phrase + " " + b.Phrase);
        }
    }

    public static class TypeExtensions
    {
        /// <summary>
        /// Converts the current string to a WeighedString.
        /// </summary>
        /// <param name="sourceString">The string to be converted.</param>
        /// <returns></returns>
        public static WeighedString ToWeighedString(this string sourceString)
        {
            return new WeighedString(sourceString);
        }

        #region GetAllPermutations.
        /// <summary>
        /// Produces a list with all possible permutations of the current string based on the
        /// specified character.
        /// </summary>
        /// <param name="input">The string to be permuted.</param>
        /// <param name="separator">The separator the permutations will be based on.</param>
        public static List<string> GetAllPermutations(this string input, char separator)
        {
            List<string> perms = input.Split(separator).ToList<string>();
            return Permute(perms);
        }

        /// <summary>
        /// Used internally by GetAllPermutations to initiate recursion and produce all permutations of the specified list.
        /// </summary>
        /// <param name="input">A list with words that will be permuted.</param>
        private static List<string> Permute(List<string> input)
        {
            // The recursive permute begins with the input list and two index numbers:
            // that of the first and the last element of the list.
            int lastIndex = input.Count - 1;
            return Permute(input, 0, lastIndex);
        }

        /// <summary>
        /// Used internally by Permute to carry recursion and populate the list with all possible permutations of the input list.
        /// </summary>
        /// <param name="input">The list to be permuted.</param>
        /// <param name="startIndex">The index to start the current instance of the permutation process.</param>
        /// <param name="endIndex">The index to end the current instance of the permucation process.</param>
        private static List<string> Permute(List<string> input, int startIndex, int endIndex)
        {
            List<string> result = new List<string>();
            
            // When recursion reaches the point where the last word is examined
            // it returns the combination of words in the order that it was produced by the current iteration.
            if (startIndex == endIndex)
            {
                string currentPermutation = "";
                foreach (string word in input)
                {
                    currentPermutation += " " + word;
                }
                result.Add(currentPermutation.Trim());
            }
            else  // When iterating we move from start to end swapping the first element with that of the iteration.
                for (int i = startIndex; i <= endIndex; i++)
                {
                    input.Swap(startIndex, i);
                    result.AddRange(Permute(input, startIndex + 1, endIndex));  // Recursion is used to repeat the process for all elements after
                    input.Swap(startIndex, i);                                  // that of the current iteration.
                }
            return result;
        }
        #endregion

        /// <summary>
        /// Swaps two elements of the current List.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="list">The current list.</param>
        /// <param name="indexA">The index of the first element to be swapped.</param>
        /// <param name="indexB">The index of the second element to be swapped.</param>
        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }
    }
}
