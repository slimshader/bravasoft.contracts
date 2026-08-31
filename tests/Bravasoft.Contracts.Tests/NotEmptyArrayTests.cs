using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts.Tests;

public class NotEmptyArrayTests
{
    private static int First(NotEmptyArray<int> xs) => xs.Value[0];

    private static int ViaGeneralForm(NotEmpty<int[], int> xs) => xs.Value.Length;

    private static int ViaNotNull(NotNull<int[]> xs) => xs.Value.Length;

    [Fact]
    public void CallSiteReadsLikeAPlainArgument()
    {
        Assert.Equal(7, First(new[] { 7, 8 }));
    }

    [Fact]
    public void RejectsNull()
    {
        Assert.Throws<ContractViolationException>(() => First(null!));
    }

    [Fact]
    public void RejectsEmpty()
    {
        Assert.Throws<ContractViolationException>(() => First(System.Array.Empty<int>()));
    }

    [Fact]
    public void IsAcceptedWhereAnyWeakerContractIsExpected()
    {
        NotEmptyArray<int> xs = new[] { 1, 2, 3 };

        Assert.Equal(3, ViaGeneralForm(xs));
        Assert.Equal(3, ViaNotNull(xs));
    }

    [Fact]
    public void DefaultInstanceThrowsInsteadOfYieldingNull()
    {
        NotEmptyArray<int> uninitialized = default;

        Assert.Throws<ContractViolationException>(() => uninitialized.Value);
    }

    [Fact]
    public void IsTheSizeOfTheReferenceItWraps()
    {
        Assert.Equal(Unsafe.SizeOf<int[]>(), Unsafe.SizeOf<NotEmptyArray<int>>());
    }

    // Reference element types go through the same array covariance the runtime already allows.
    [Fact]
    public void WorksForReferenceElementTypes()
    {
        NotEmptyArray<string> xs = new[] { "a", "b" };

        Assert.Equal("a", xs.Value[0]);
    }

    [Fact]
    public void UnwrapsImplicitlyToTheArray()
    {
        var original = new[] { 1, 2 };
        NotEmptyArray<int> wrapped = original;

        int[] unwrapped = wrapped;

        Assert.Same(original, unwrapped);
    }
}
