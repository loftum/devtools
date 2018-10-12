using System;

namespace Shared.Commands
{
    public class Argument
    {
        public int Position { get; }
        public string Name { get; }
        public bool IsNamed => !string.IsNullOrWhiteSpace(Name);
        public string Value { get; }

        public bool Matches(int position, string name)
        {
            return IsNamed
                ? Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                : Position == position;
        }

        public Argument(int position, string raw)
        {
            Position = position;
            if (string.IsNullOrEmpty(raw))
            {
                return;
            }

            if (raw.StartsWith("-"))
            {
                var index = raw.IndexOf("=", StringComparison.InvariantCultureIgnoreCase);
                if (index > 0)
                {
                    Name = raw.Substring(1, index);
                    Value = raw.Substring(index + 1);
                }
                else
                {
                    Name = raw.Substring(1);
                }
            }
            else
            {
                Value = raw;
            }
        }
    }
}