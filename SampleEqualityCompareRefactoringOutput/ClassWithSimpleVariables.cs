using System;

#nullable enable

namespace SampleJsonConverterCode
{
    public class SimpleVariables : IEquatable<SimpleVariables>
    {
        public string Name { get; }
        public int SomeInt { get; }
        public DateTime ADateTime { get; }
        public string? MaybeAName { get; }
        public int? MaybeAnInt { get; }
        public DateTime? MaybeADateTime { get; }

        public SimpleVariables(string name, int someInt, DateTime aDateTime, string? maybeAName, int? maybeAnInt, DateTime? maybeADateTime)
        {
            Name = name;
            SomeInt = someInt;
            ADateTime = aDateTime;
            MaybeAName = maybeAName;
            MaybeAnInt = maybeAnInt;
            MaybeADateTime = maybeADateTime;
        }

        public bool Equals(SimpleVariables? other)
        {
            if (this is null) return other is null;
            return other is SimpleVariables &&
            Name == other.Name &&
            SomeInt == other.SomeInt &&
            ADateTime == other.ADateTime &&
            MaybeAName == other.MaybeAName &&
            MaybeAnInt == other.MaybeAnInt &&
            MaybeADateTime == other.MaybeADateTime;
        }

        public override bool Equals(object? other)
        {
            if (this is null) return other is null;
            return other is SimpleVariables variables && Equals(variables);
        }

        public static bool operator==(SimpleVariables? a, SimpleVariables? b)
        {
            if (a is null) return b is null;
            return b is object && a.Equals(b);
        }

        public static bool operator!=(SimpleVariables? a, SimpleVariables? b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, SomeInt, ADateTime, MaybeAName, MaybeAnInt, MaybeADateTime);
        }
    }
}
