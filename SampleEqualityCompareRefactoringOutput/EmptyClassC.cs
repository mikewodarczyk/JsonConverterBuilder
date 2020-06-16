using System;

#nullable enable

namespace SampleJsonConverterCode
{
    public class EmptyClassC : IEquatable<EmptyClassC>
    {
        public bool Equals(EmptyClassC? other)
        {
            if (this is null) return other is null;
            return other is EmptyClassC && true;
        }

        public override bool Equals(object? other)
        {
            if (this is null) return other is null;
            return other is EmptyClassC variables && Equals(variables);
        }

        public static bool operator==(EmptyClassC? a, EmptyClassC? b)
        {
            if (a is null) return b is null;
            return b is object && a.Equals(b);
        }

        public static bool operator!=(EmptyClassC? a, EmptyClassC? b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
