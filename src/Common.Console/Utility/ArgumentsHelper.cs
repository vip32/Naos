namespace Naos.Core.Common.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ArgumentsHelper
    {
        public static string[] Split(string line)
        {
            if (line.IsNullOrEmpty())
            {
                return Enumerable.Empty<string>().ToArray();
            }

            line = line.Replace("\0", string.Empty);
            var result = new List<string>();
            var currentArgument = new StringBuilder();
            char currentQuote = char.MinValue;

            void reset()
            {
                result.Add(currentArgument.ToString());
                currentArgument = new StringBuilder();
                currentQuote = char.MinValue;
            }

            foreach (char c in line)
            {
                if (currentQuote == char.MinValue)
                {
                    if (c == ' ')
                    {
                        reset();
                    }
                    else if (c == '\'')
                    {
                        reset();
                        currentQuote = '\'';
                    }
                    else if (c == '"')
                    {
                        reset();
                        currentQuote = '"';
                    }
                    else
                    {
                        currentArgument.Append(c);
                    }
                }
                else
                {
                    if (c == currentQuote)
                    {
                        reset();
                    }
                    else
                    {
                        currentArgument.Append(c);
                    }
                }
            }

            reset();
            return result.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
        }
    }
}
