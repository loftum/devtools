using System;

namespace Cert.Commands
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
                var index = raw.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
                Name = index > 0 ? raw.Substring(1, index - 1) : raw.Substring(1);
                Value = index > 0 ? raw.Substring(index + 1) : null;
            }
            else
            {
                Value = raw;
            }
        }

        public override string ToString()
        {
            return IsNamed ? $"-{Name}:{Value}" : Value;
        }
    }
}