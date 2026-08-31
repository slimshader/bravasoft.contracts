using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts.Tests;

public class NotEmptyListTests
{
    private static int First(NotEmptyList<int> xs) => xs.Value[0];

    private static int ViaGeneralForm(NotEmpty<List<int>, int> xs) => xs.Value.Count;

    private static int ViaNotNull(NotNull<List<int>> xs) => xs.Value.Count;

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

    // The convenience spelling satisfies every contract it refines.
    [Fact]
    public void IsAcceptedWhereAnyWeakerContractIsExpected()
    {
        NotEmptyList<int> xs = new List<int> { 1, 2, 3 };

        Assert.Equal(3, ViaGeneralForm(xs));
        Assert.Equal(3, ViaNotNull(xs));
    }

    [Fact]
    public void DefaultInstanceThrowsInsteadOfYieldingNull()
    {
        NotEmptyList<int> uninitialized = default;

        Assert.Throws<ContractViolationException>(() => uninitialized.Value);
    }

    [Fact]
    public void IsTheSizeOfTheReferenceItWraps()
    {
        Assert.Equal(Unsafe.SizeOf<List<int>>(), Unsafe.SizeOf<NotEmptyList<int>>());
    }

    [Fact]
    public void UnwrapsImplicitlyToTheList()
    {
        var original = new List<int> { 1 };
        NotEmptyList<int> wrapped = original;

        List<int> unwrapped = wrapped;

        Assert.Same(original, unwrapped);
    }
}
