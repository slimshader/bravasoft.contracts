using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts
{
    /// <summary>
    /// A collection that is neither <see langword="null"/> nor empty. Use it as a parameter type -
    /// <c>int Sum(NotEmpty&lt;List&lt;int&gt;, int&gt; xs)</c> - and both checks happen on the way
    /// in, at the call site, which still reads as <c>Sum(xs)</c>. The body may index <c>[0]</c> or
    /// call <c>Max()</c> without guarding.
    /// </summary>
    /// <typeparam name="TList">
    /// The concrete collection type. It stays concrete through the wrapper, so
    /// <see cref="Value"/> hands back a <c>List&lt;int&gt;</c>, not an interface.
    /// </typeparam>
    /// <typeparam name="TItem">The collection's element type.</typeparam>
    /// <remarks>
    /// <para>
    /// Naming the element type as well as the collection is unfortunate but forced:
    /// <see cref="IReadOnlyCollection{T}"/> is generic, and C# forbids user-defined conversions to
    /// or from an interface type (CS0552), so the wrapper cannot simply hold an
    /// <c>IReadOnlyCollection&lt;TItem&gt;</c> - the implicit conversions that make the whole
    /// pattern work would not compile. A type parameter constrained to the interface is allowed,
    /// which is why the constraint is written this way. A <c>using</c> alias keeps call sites short:
    /// <c>using Scores = Bravasoft.Contracts.NotEmpty&lt;System.Collections.Generic.List&lt;int&gt;, int&gt;;</c>
    /// </para>
    /// <para>
    /// This refines <see cref="NotNull{T}"/> the same way <see cref="NotEmptyString"/> does, and is
    /// built out of one, so the null half of the contract has a single definition and widening to
    /// <c>NotNull&lt;TList&gt;</c> re-tests nothing.
    /// </para>
    /// <para>
    /// <typeparamref name="TList"/> is constrained to reference types, which is what makes
    /// <c>default(NotEmpty&lt;,&gt;)</c> detectable. That rules out struct collections such as
    /// <c>ImmutableArray&lt;T&gt;</c>, whose own default is already a trap this type could not see.
    /// </para>
    /// </remarks>
    public readonly struct NotEmpty<TList, TItem>
        where TList : class, IReadOnlyCollection<TItem>
    {
        private readonly NotNull<TList> _value;

        /// <summary>
        /// Wraps <paramref name="value"/>, which must be neither <see langword="null"/> nor empty.
        /// </summary>
        /// <param name="value">The collection to wrap.</param>
        /// <exception cref="ContractViolationException">
        /// <paramref name="value"/> is <see langword="null"/> or has a <c>Count</c> of zero.
        /// </exception>
        public NotEmpty(TList value)
        {
            // The null half of the contract is NotNull's to enforce and report.
            _value = new NotNull<TList>(value);

            if (value.Count == 0)
            {
                ThrowEmpty();
            }
        }

        /// <summary>The wrapped collection, never <see langword="null"/> and never empty.</summary>
        /// <exception cref="ContractViolationException">
        /// This is <c>default(NotEmpty&lt;TList, TItem&gt;)</c>.
        /// </exception>
        public TList Value
        {
            get
            {
                var value = _value.ValueOrNull;

                if (value is null)
                {
                    ThrowUninitialized();
                }

                return value;
            }
        }

        /// <summary>
        /// The wrapped collection without the initialization check, or <see langword="null"/> for
        /// a default instance. Lets a convenience wrapper over a fixed <typeparamref name="TList"/>
        /// report its own contract in the failure message.
        /// </summary>
        internal TList? ValueOrNull => _value.ValueOrNull;

        /// <summary>Unwraps to the underlying collection.</summary>
        /// <param name="value">The wrapper to unwrap.</param>
        public static implicit operator TList(NotEmpty<TList, TItem> value) => value.Value;

        /// <summary>
        /// Widens to the weaker contract. Non-empty already implies non-null, so this is a field
        /// read that tests nothing.
        /// </summary>
        /// <param name="value">The wrapper to widen.</param>
        public static implicit operator NotNull<TList>(NotEmpty<TList, TItem> value) => value._value;

        /// <summary>
        /// Wraps a collection, which must be neither <see langword="null"/> nor empty.
        /// </summary>
        /// <param name="value">The collection to wrap.</param>
        public static implicit operator NotEmpty<TList, TItem>(TList value) =>
            new NotEmpty<TList, TItem>(value);

        /// <summary>
        /// Narrows to the stronger contract, checking only emptiness - the part not already
        /// guaranteed.
        /// </summary>
        /// <param name="value">The wrapper to narrow.</param>
        public static implicit operator NotEmpty<TList, TItem>(NotNull<TList> value) =>
            new NotEmpty<TList, TItem>(value.Value);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowEmpty() => throw new ContractViolationException(
            $"Collection of type '{typeof(TList)}' must not be empty.");

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowUninitialized() => throw new ContractViolationException(
            $"default(NotEmpty<{typeof(TList)}, {typeof(TItem)}>) does not satisfy the non-empty contract.");
    }
}
