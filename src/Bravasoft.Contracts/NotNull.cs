using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bravasoft.Contracts
{
    /// <summary>
    /// A reference of type <typeparamref name="T"/> that cannot be <see langword="null"/>.
    /// Use it as a parameter type - <c>int Length(NotNull&lt;string&gt; s)</c> - and the null check
    /// happens on the way in, at the call site, which still reads as <c>Length(s)</c>.
    /// </summary>
    /// <typeparam name="T">The referenced type. Value types cannot be null, so they are excluded.</typeparam>
    /// <remarks>
    /// Both conversions are implicit, so callers pass a <typeparamref name="T"/> and the body uses a
    /// <typeparamref name="T"/>; only the boundary mentions <see cref="NotNull{T}"/>.
    /// As a struct it costs no allocation, at the price of <c>default(NotNull&lt;T&gt;)</c>, which
    /// skips the constructor - reading it throws rather than yielding a null.
    /// One caveat: for <c>NotNull&lt;object&gt;</c> specifically, assigning to an <see cref="object"/>
    /// boxes the struct instead of unwrapping it, because the compiler prefers a boxing conversion
    /// over a user-defined one. Use <see cref="Value"/> there.
    /// </remarks>
    public readonly struct NotNull<T>
        where T : class
    {
        private readonly T? _value;

        /// <summary>Wraps <paramref name="value"/>, which must not be <see langword="null"/>.</summary>
        /// <param name="value">The reference to wrap.</param>
        /// <exception cref="ContractViolationException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public NotNull(T value)
        {
            if (value is null)
            {
                ThrowNull();
            }

            _value = value;
        }

        /// <summary>The wrapped reference, never <see langword="null"/>.</summary>
        /// <exception cref="ContractViolationException">This is <c>default(NotNull&lt;T&gt;)</c>.</exception>
        /// <remarks>
        /// The null test cannot be removed: <c>default(NotNull&lt;T&gt;)</c> is produced without
        /// running any constructor and C# offers no way to intercept that. It is kept as cheap as
        /// possible instead - the throw lives in a separate non-inlined method, so this property
        /// compiles to a load, a never-taken branch, and a return, and stays small enough to inline.
        /// </remarks>
        public T Value
        {
            get
            {
                if (_value is null)
                {
                    ThrowUninitialized();
                }

                return _value;
            }
        }

        /// <summary>
        /// The wrapped reference without the initialization check, or <see langword="null"/> for
        /// <c>default(NotNull&lt;T&gt;)</c>. Lets a refinement that holds a
        /// <see cref="NotNull{T}"/> field report its own contract in the failure message.
        /// </summary>
        internal T? ValueOrNull => _value;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNull() => throw new ContractViolationException(
            $"Value of type '{typeof(T)}' must not be null.");

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowUninitialized() => throw new ContractViolationException(
            $"default(NotNull<{typeof(T)}>) does not satisfy the not-null contract.");

        /// <summary>Unwraps to the underlying reference.</summary>
        /// <param name="notNull">The wrapper to unwrap.</param>
        public static implicit operator T(NotNull<T> notNull) => notNull.Value;

        /// <summary>Wraps a reference, which must not be <see langword="null"/>.</summary>
        /// <param name="value">The reference to wrap.</param>
        public static implicit operator NotNull<T>(T value) => new NotNull<T>(value);
    }
}
