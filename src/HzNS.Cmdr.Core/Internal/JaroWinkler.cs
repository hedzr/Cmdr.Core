using System;
using System.Text;

namespace HzNS.Cmdr.Internal
{
    internal static class JaroWinkler
    {
        private const double DefaultMismatchScore = 0.0;
        private const double DefaultMatchScore = 1.0;

        /// <summary>
        /// Gets the similarity between two strings by using the Jaro-Winkler algorithm.
        /// A value of 1 means perfect match. A value of zero represents an absolute no match
        /// </summary>
        /// <param name="firstWord"></param>
        /// <param name="secondWord"></param>
        /// <returns>a value between 0-1 of the similarity</returns>
        /// 
        public static double RateSimilarity(string? firstWord, string? secondWord)
        {
            if (firstWord == null || secondWord == null) return DefaultMismatchScore;

            // Converting to lower case is not part of the original Jaro-Winkler implementation
            // But we don't really care about case sensitivity in DIAMOND and wouldn't decrease security names similarity rate just because
            // of Case sensitivity
            firstWord = firstWord.ToLower();
            secondWord = secondWord.ToLower();

            if (firstWord == secondWord)
                //return (SqlDouble)defaultMatchScore;
                return DefaultMatchScore;


            {
                // Get half the length of the string rounded up - (this is the distance used for acceptable transpositions)
                var halfLength = Math.Min(firstWord.Length, secondWord.Length) / 2 + 1;

                // Get common characters
                var common1 = GetCommonCharacters(firstWord, secondWord, halfLength);
                var commonMatches = common1?.Length ?? 0;

                // Check for zero in common
                if (commonMatches == 0)
                    //return (SqlDouble)defaultMismatchScore;
                    return DefaultMismatchScore;

                var common2 = GetCommonCharacters(secondWord, firstWord, halfLength);

                // Check for same length common strings returning 0 if is not the same
                if (commonMatches != common2?.Length)
                    //return (SqlDouble)defaultMismatchScore;
                    return DefaultMismatchScore;

                // Get the number of transpositions
                int transpositions = 0;
                for (int i = 0; i < commonMatches; i++)
                {
                    if (common1 != null && common1[i] != common2[i])
                        transpositions++;
                }

                // ReSharper disable once NotAccessedVariable
                int j = 0;
                // ReSharper disable once RedundantAssignment
                j += 1;

                // Calculate Jaro metric
                transpositions /= 2;
                double jaroMetric = commonMatches / (3.0 * firstWord.Length) +
                                    commonMatches / (3.0 * secondWord.Length) +
                                    (commonMatches - transpositions) / (3.0 * commonMatches);
                //return (SqlDouble)jaroMetric;
                return jaroMetric;
            }

            //return (SqlDouble)defaultMismatchScore;
        }

        /// <summary>
        /// Returns a string buffer of characters from string1 within string2 if they are of a given
        /// distance seperation from the position in string1.
        /// </summary>
        /// <param name="firstWord">string one</param>
        /// <param name="secondWord">string two</param>
        /// <param name="separationDistance">separation distance</param>
        /// <returns>A string buffer of characters from string1 within string2 if they are of a given
        /// distance seperation from the position in string1</returns>
        private static StringBuilder? GetCommonCharacters(string firstWord, string secondWord, int separationDistance)
        {
            if ((firstWord != null) && (secondWord != null))
            {
                var returnCommons = new StringBuilder(20);
                var copy = new StringBuilder(secondWord);
                var firstWordLength = firstWord.Length;
                var secondWordLength = secondWord.Length;

                for (int i = 0; i < firstWordLength; i++)
                {
                    char character = firstWord[i];
                    bool found = false;

                    for (int j = Math.Max(0, i - separationDistance);
                        !found && j < Math.Min(i + separationDistance, secondWordLength);
                        j++)
                    {
                        if (copy[j] == character)
                        {
                            found = true;
                            returnCommons.Append(character);
                            copy[j] = '#';
                        }
                    }
                }

                return returnCommons;
            }

            return null;
        }
    }
}