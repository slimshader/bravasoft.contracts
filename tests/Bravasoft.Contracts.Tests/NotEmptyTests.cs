using System.Runtime.CompilerServices;
using Scores = Bravasoft.Contracts.NotEmpty<System.Collections.Generic.List<int>, int>;

namespace Bravasoft.Contracts.Tests;

public class NotEmptyTests
{
    // No guard in the body: indexing [0] is safe because the parameter type says so.
    private static int First(Scores xs) => ((List<int>)xs)[0];

    private static int Count(NotNull<List<int>> xs) => ((List<int>)xs).Count;

    [Fact]
    public void CallSiteReadsLikeAPlainArgument()
    {
        Assert.Equal(7, First(new List<int> { 7, 8 }));
    }

    [Fact]
    public void RejectsNull()
    {
        Assert.Throws<ContractViolationException>(() => First(null!));
    }

    [Fact]
    public void RejectsEmpty()
    {
        Assert.Throws<ContractViolationException>(() => First(new List<int>()));
    }

    [Fact]
    public void KeepsTheConcreteCollectionType()
    {
        var original = new List<int> { 1 };
        Scores wrapped = original;

        List<int> unwrapped = wrapped;

        Assert.Same(original, unwrapped);
    }

    [Fact]
    public void NotEmptyIsAcceptedWhereNotNullIsExpected()
    {
        Scores refined = new List<int> { 1, 2, 3 };

        Assert.Equal(3, Count(refined));
    }

    [Fact]
    public void NarrowingFromNotNullChecksTheRemainingContract()
    {
        NotNull<List<int>> empty = new List<int>();

        Assert.Throws<ContractViolationException>(() =>
        {
            Scores _ = empty;
        });
    }

    [Fact]
    public void DefaultInstanceThrowsInsteadOfYieldingNull()
    {
        Scores uninitialized = default;

        Assert.Throws<ContractViolationException>(() => uninitialized.Value);
    }

    [Fact]
    public void IsTheSizeOfTheReferenceItWraps()
    {
        Assert.Equal(Unsafe.SizeOf<List<int>>(), Unsafe.SizeOf<Scores>());
    }

    // Works for any reference collection, not just List<T>.
    [Fact]
    public void WorksForOtherReadOnlyCollections()
    {
        NotEmpty<int[], int> array = new[] { 1, 2 };
        NotEmpty<HashSet<string>, string> set = new HashSet<string> { "a" };

        Assert.Equal(2, array.Value.Length);
        Assert.Single(set.Value);
    }
}
