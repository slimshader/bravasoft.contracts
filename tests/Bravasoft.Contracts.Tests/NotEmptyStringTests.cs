using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts.Tests;

public class NotEmptyStringTests
{
    private static char First(NotEmptyString s) => ((string)s)[0];

    // A method that only needs the weaker contract accepts the stronger one unchanged.
    private static int Length(NotNull<string> s) => ((string)s).Length;

    [Fact]
    public void CallSiteReadsLikeAPlainArgument()
    {
        Assert.Equal('h', First("hello"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RejectsNullAndEmpty(string? bad)
    {
        Assert.Throws<ContractViolationException>(() => First(bad!));
    }

    [Fact]
    public void WhitespaceIsNotEmpty()
    {
        Assert.Equal(' ', First("  "));
    }

    [Fact]
    public void NotEmptyIsAcceptedWhereNotNullIsExpected()
    {
        NotEmptyString refined = "hello";

        Assert.Equal(5, Length(refined));
    }

    [Fact]
    public void NarrowingFromNotNullChecksTheRemainingContract()
    {
        NotNull<string> empty = string.Empty;

        Assert.Throws<ContractViolationException>(() =>
        {
            NotEmptyString _ = empty;
        });
    }

    [Fact]
    public void DefaultInstanceThrowsInsteadOfYieldingNull()
    {
        NotEmptyString uninitialized = default;

        Assert.Throws<ContractViolationException>(() => uninitialized.Value);
    }

    // Composing out of NotNull<string> rather than string must not cost a word.
    [Fact]
    public void IsTheSizeOfTheReferenceItWraps()
    {
        Assert.Equal(Unsafe.SizeOf<string>(), Unsafe.SizeOf<NotEmptyString>());
        Assert.Equal(Unsafe.SizeOf<string>(), Unsafe.SizeOf<NotNull<string>>());
    }

    // Widening is a field read, so a default instance widens without throwing - but the result is
    // itself a default NotNull, which still refuses to hand out a null.
    [Fact]
    public void WideningADefaultInstanceStillNeverYieldsNull()
    {
        NotNull<string> widened = default(NotEmptyString);

        Assert.Throws<ContractViolationException>(() => widened.Value);
    }
}
