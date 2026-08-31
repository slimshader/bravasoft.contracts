using Bravasoft.Contracts;

namespace Bravasoft.Contracts.Tests;

public class NotNullTests
{
    // The shape the type exists for: no precondition check in the body.
    private static int Length(NotNull<string> s) => s.Value.Length;

    [Fact]
    public void CallSiteReadsLikeAPlainArgument()
    {
        Assert.Equal(5, Length("hello"));
    }

    [Fact]
    public void CallSiteWithNullThrowsBeforeEnteringTheMethod()
    {
        string? missing = null;

        Assert.Throws<ContractViolationException>(() => Length(missing!));
    }

    [Fact]
    public void ConstructorRejectsNull()
    {
        Assert.Throws<ContractViolationException>(() => new NotNull<string>(null!));
    }

    [Fact]
    public void ConvertsBackToTheUnderlyingReference()
    {
        var original = new Box();
        NotNull<Box> wrapped = original;

        Box unwrapped = wrapped;

        Assert.Same(original, unwrapped);
        Assert.Same(original, wrapped.Value);
    }

    // NotNull<object> is the one case where the implicit unwrap loses: converting to object is a
    // boxing conversion, which the compiler prefers over the user-defined operator. Value still works.
    [Fact]
    public void ConvertingNotNullOfObjectToObjectBoxesInsteadOfUnwrapping()
    {
        var original = new object();
        NotNull<object> wrapped = original;

        object boxed = wrapped;

        Assert.IsType<NotNull<object>>(boxed);
        Assert.Same(original, wrapped.Value);
    }

    [Fact]
    public void DefaultInstanceThrowsInsteadOfYieldingNull()
    {
        NotNull<string> uninitialized = default;

        Assert.Throws<ContractViolationException>(() => uninitialized.Value);
    }

    private sealed class Box
    {
    }
}
