using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class DateTimeTests : CodeRefactoringProviderTestFixture
    {

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new CreateJsonConverterCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;
    

        [Fact]
        public void DateTimeTest()
        {
            string initialCode =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrBronners.PricingChangeSchema
{
    public class [|BasePriceChange|] : IEquatable<BasePriceChange>
    {
        public BasePriceChange(DateTime date, string sku, decimal? newPrice, string createdByUser)
        {
            Date = date;
            Sku = sku;
            NewPrice = newPrice;
            CreatedByUser = createdByUser;
        }

        public DateTime Date { get; set; }
        public string Sku { get; set; }
        public decimal? NewPrice { get; set; }
        public string CreatedByUser { get; set; }

        public override string ToString()
        {
            return $""{{Date:{Date},Sku:{Sku},NewPrice:{NewPrice}}}"";
        }

        public BasePriceChange DeepCopy()
        {
            BasePriceChange ch = new BasePriceChange(
              date: Date,
              sku: Sku,
              newPrice: NewPrice,
              createdByUser: """"
            );
            return ch;
        }

        public bool Equals(BasePriceChange other)
        {
            return Date == other.Date &&
                Sku == other.Sku &&
                NewPrice == other.NewPrice &&
                CreatedByUser == other.CreatedByUser;
        }

        public static bool operator ==(BasePriceChange? a, BasePriceChange? b)
        {
            if (a is null) return b is null;
            return b is object && a.Equals(b);
        }

        public static bool operator !=(BasePriceChange? a, BasePriceChange? b)
        {
            if (a is null) return b is object;
            return b is null || !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Date, Sku, NewPrice, CreatedByUser);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return obj is BasePriceChange && Equals((BasePriceChange)obj);
        }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("DateTimeSampleCode.cs");

            Test(initialCode, expectedCode);
        }

    }
}
