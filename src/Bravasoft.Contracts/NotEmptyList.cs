using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts
{
    /// <summary>
    /// A <see cref="List{T}"/> that is neither <see langword="null"/> nor empty - the one-parameter
    /// convenience spelling of <c>NotEmpty&lt;List&lt;T&gt;, T&gt;</c>, in the same spirit as
    /// <see cref="NotEmptyString"/>. Use it as a parameter type - <c>int Sum(NotEmptyList&lt;int&gt; xs)</c>
    /// - and the call site still reads as <c>Sum(xs)</c>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <remarks>
    /// <para>
    /// Convenience wrappers like this one can only be written over a concrete class. There is no
    /// <c>NotEmptyIList&lt;T&gt;</c> and there cannot be: C# forbids user-defined conversions to or
    /// from an interface type (CS0552), so such a type could neither accept an
    /// <c>IList&lt;T&gt;</c> implicitly nor hand one back - it would lose both halves of the
    /// ergonomics that are the entire point. Interface-typed collections are reached through the
    /// two-parameter <see cref="NotEmpty{TList, TItem}"/>, which sidesteps the rule by taking the
    /// collection as a type parameter rather than as an interface.
    /// </para>
    /// <para>
    /// Built out of a <see cref="NotEmpty{TList, TItem}"/>, so it is a relabelling rather than a
    /// second implementation: widening to <c>NotEmpty&lt;List&lt;T&gt;, T&gt;</c> or
    /// <c>NotNull&lt;List&lt;T&gt;&gt;</c> re-tests nothing and costs nothing.
    /// </para>
    /// </remarks>
    public readonly struct NotEmptyList<T>
    {
        private readonly NotEmpty<List<T>, T> _value;

        /// <summary>
        /// Wraps <paramref name="value"/>, which must be neither <see langword="null"/> nor empty.
        /// </summary>
        /// <param name="value">The list to wrap.</param>
        /// <exception cref="ContractViolationException">
        /// <paramref name="value"/> is <see langword="null"/> or empty.
        /// </exception>
        public NotEmptyList(List<T> value) => _value = new NotEmpty<List<T>, T>(value);

        /// <summary>The wrapped list, never <see langword="null"/> and never empty.</summary>
        /// <exception cref="ContractViolationException">
        /// This is <c>default(NotEmptyList&lt;T&gt;)</c>.
        /// </exception>
        public List<T> Value
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

        /// <summary>Unwraps to the underlying list.</summary>
        /// <param name="value">The wrapper to unwrap.</param>
        public static implicit operator List<T>(NotEmptyList<T> value) => value.Value;

        /// <summary>Relabels as the general form. Tests nothing.</summary>
        /// <param name="value">The wrapper to widen.</param>
        public static implicit operator NotEmpty<List<T>, T>(NotEmptyList<T> value) => value._value;

        /// <summary>Widens to the weaker contract. Tests nothing.</summary>
        /// <param name="value">The wrapper to widen.</param>
        public static implicit operator NotNull<List<T>>(NotEmptyList<T> value) =>
            (NotNull<List<T>>)value._value;

        /// <summary>Wraps a list, which must be neither <see langword="null"/> nor empty.</summary>
        /// <param name="value">The list to wrap.</param>
        public static implicit operator NotEmptyList<T>(List<T> value) => new NotEmptyList<T>(value);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowUninitialized() => throw new ContractViolationException(
            $"default(NotEmptyList<{typeof(T)}>) does not satisfy the non-empty contract.");
    }
}
