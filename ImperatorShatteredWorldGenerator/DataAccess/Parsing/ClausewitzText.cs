using System.Collections.Generic;

namespace ImperatorShatteredWorldGenerator.DataAccess.Parsing
{
    internal static class ClausewitzText
    {
        internal static string StripComment(string line)
        {
            bool isInsideQuotes = false;

            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];

                if (character == '"')
                {
                    isInsideQuotes = !isInsideQuotes;

                    continue;
                }

                if (character == '#' && !isInsideQuotes)
                {
                    return line.Substring(0, index);
                }
            }

            return line;
        }

        internal static int GetBraceDelta(string line)
        {
            string content = StripComment(line);
            bool isInsideQuotes = false;
            int delta = 0;

            foreach (char character in content)
            {
                if (character == '"')
                {
                    isInsideQuotes = !isInsideQuotes;

                    continue;
                }

                if (isInsideQuotes)
                {
                    continue;
                }

                if (character == '{')
                {
                    delta += 1;
                }
                else if (character == '}')
                {
                    delta -= 1;
                }
            }

            return delta;
        }

        internal static int FindBlockEnd(IList<string> lines, int headerIndex, int depth)
        {
            for (int index = headerIndex + 1; index < lines.Count; index++)
            {
                depth += GetBraceDelta(lines[index]);

                if (depth <= 0)
                {
                    return index;
                }
            }

            return lines.Count - 1;
        }

        internal static IEnumerable<string> GetBlockContent(IList<string> lines, int headerIndex)
        {
            int endIndex = FindBlockEnd(lines, headerIndex, GetBraceDelta(lines[headerIndex]));

            for (int index = headerIndex + 1; index < endIndex; index++)
            {
                yield return lines[index];
            }
        }
    }
}