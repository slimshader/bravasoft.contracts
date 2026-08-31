using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts
{
    /// <summary>
    /// An array that is neither <see langword="null"/> nor empty - the one-parameter convenience
    /// spelling of <c>NotEmpty&lt;T[], T&gt;</c>. Use it as a parameter type -
    /// <c>T Pick&lt;T&gt;(NotEmptyArray&lt;T&gt; xs)</c> - and the call site still reads as
    /// <c>Pick(xs)</c>, with <c>[0]</c> safe in the body.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <remarks>
    /// Built out of a <see cref="NotEmpty{TList, TItem}"/>, so it is a relabelling rather than a
    /// second implementation: widening to <c>NotEmpty&lt;T[], T&gt;</c> or <c>NotNull&lt;T[]&gt;</c>
    /// re-tests nothing and costs nothing. Arrays reach this type because <c>T[]</c> implements
    /// <see cref="IReadOnlyCollection{T}"/> and is a reference type; see
    /// <see cref="NotEmptyList{T}"/> for why no interface-typed equivalent can exist.
    /// </remarks>
    public readonly struct NotEmptyArray<T>
    {
        private readonly NotEmpty<T[], T> _value;

        /// <summary>
        /// Wraps <paramref name="value"/>, which must be neither <see langword="null"/> nor empty.
        /// </summary>
        /// <param name="value">The array to wrap.</param>
        /// <exception cref="ContractViolationException">
        /// <paramref name="value"/> is <see langword="null"/> or has no elements.
        /// </exception>
        public NotEmptyArray(T[] value) => _value = new NotEmpty<T[], T>(value);

        /// <summary>The wrapped array, never <see langword="null"/> and never empty.</summary>
        /// <exception cref="ContractViolationException">
        /// This is <c>default(NotEmptyArray&lt;T&gt;)</c>.
        /// </exception>
        public T[] Value
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

        /// <summary>Unwraps to the underlying array.</summary>
        /// <param name="value">The wrapper to unwrap.</param>
        public static implicit operator T[](NotEmptyArray<T> value) => value.Value;

        /// <summary>Relabels as the general form. Tests nothing.</summary>
        /// <param name="value">The wrapper to widen.</param>
        public static implicit operator NotEmpty<T[], T>(NotEmptyArray<T> value) => value._value;

        /// <summary>Widens to the weaker contract. Tests nothing.</summary>
        /// <param name="value">The wrapper to widen.</param>
        public static implicit operator NotNull<T[]>(NotEmptyArray<T> value) =>
            (NotNull<T[]>)value._value;

        /// <summary>Wraps an array, which must be neither <see langword="null"/> nor empty.</summary>
        /// <param name="value">The array to wrap.</param>
        public static implicit operator NotEmptyArray<T>(T[] value) => new NotEmptyArray<T>(value);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowUninitialized() => throw new ContractViolationException(
            $"default(NotEmptyArray<{typeof(T)}>) does not satisfy the non-empty contract.");
    }
}
