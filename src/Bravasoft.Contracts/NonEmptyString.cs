using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts
{
    /// <summary>
    /// A <see cref="string"/> that is neither <see langword="null"/> nor <c>""</c>. Use it as a
    /// parameter type - <c>char First(NonEmptyString s)</c> - and both checks happen on the way in,
    /// at the call site, which still reads as <c>First(s)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a refinement of <see cref="NotNull{T}"/>: non-empty implies non-null. It is built
    /// out of one rather than beside one - the field is a <c>NotNull&lt;string&gt;</c> - so the
    /// null half of the contract has a single definition, widening is a field read that re-tests
    /// nothing, and narrowing checks only emptiness, the part not already guaranteed. The wrapper
    /// holds one reference and nothing else, so it is the size of a <see cref="string"/>.
    /// </para>
    /// <para>
    /// "Empty" means zero length. A string of spaces satisfies this contract - whitespace is a
    /// separate, stricter refinement and belongs in its own type rather than being folded in here.
    /// </para>
    /// <para>
    /// Like <see cref="NotNull{T}"/> this is a struct, so <c>default(NonEmptyString)</c> exists and
    /// skips the constructor. Reading it throws rather than yielding <see langword="null"/>.
    /// </para>
    /// </remarks>
    public readonly struct NonEmptyString
    {
        private readonly NotNull<string> _value;

        /// <summary>
        /// Wraps <paramref name="value"/>, which must be neither <see langword="null"/> nor empty.
        /// </summary>
        /// <param name="value">The string to wrap.</param>
        /// <exception cref="ContractViolationException">
        /// <paramref name="value"/> is <see langword="null"/> or <c>""</c>.
        /// </exception>
        public NonEmptyString(string value)
        {
            // The null half of the contract is NotNull's to enforce and report.
            _value = new NotNull<string>(value);

            if (value.Length == 0)
            {
                ThrowEmpty();
            }
        }

        /// <summary>The wrapped string, never <see langword="null"/> and never empty.</summary>
        /// <exception cref="ContractViolationException">This is <c>default(NonEmptyString)</c>.</exception>
        public string Value
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

        /// <summary>Unwraps to the underlying string.</summary>
        /// <param name="value">The wrapper to unwrap.</param>
        public static implicit operator string(NonEmptyString value) => value.Value;

        /// <summary>
        /// Widens to the weaker contract. Non-empty already implies non-null, so this only discards
        /// information.
        /// </summary>
        /// <param name="value">The wrapper to widen.</param>
        public static implicit operator NotNull<string>(NonEmptyString value) => value._value;

        /// <summary>Wraps a string, which must be neither <see langword="null"/> nor empty.</summary>
        /// <param name="value">The string to wrap.</param>
        public static implicit operator NonEmptyString(string value) => new NonEmptyString(value);

        /// <summary>
        /// Narrows to the stronger contract, checking the part that is not already guaranteed.
        /// </summary>
        /// <param name="value">The wrapper to narrow.</param>
        public static implicit operator NonEmptyString(NotNull<string> value) =>
            new NonEmptyString(value.Value);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowEmpty() => throw new ContractViolationException(
            "Value of type 'System.String' must not be empty.");

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowUninitialized() => throw new ContractViolationException(
            "default(NonEmptyString) does not satisfy the non-empty contract.");
    }
}
