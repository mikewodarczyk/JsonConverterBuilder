using System;

#nullable enable

namespace SampleJsonConverterCode
{
    public class /*[|*/SimpleVariables/*|]*/
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
    }
}
