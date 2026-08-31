# Bravasoft.Contracts

Types that move invariants into the type system, so a precondition is checked where it is
actually broken - at the call site - rather than inside the function that was handed bad input.

## Upholding a contract is the caller's job

A precondition is a promise the *caller* makes. When it is broken, the bug is in the caller: it
computed, loaded, or forgot to check something and then passed it on. The called function is the
victim, not the culprit.

Traditional guard clauses get this backwards. The check lives inside the callee, so that is where
the exception is raised, and that is what the stack trace accuses:

```csharp
int Length(string s)
{
    if (s is null) throw new ArgumentNullException(nameof(s));   // wrong place to find out
    return s.Length;
}
```

```
ArgumentNullException: Value cannot be null. (Parameter 's')
   at Program.Traditional(String s)          <- accused, but blameless
   at Program.BuggyCaller(Boolean traditional)
   at Program.Main()
```

The top frame points at `Length`, a function that is behaving perfectly. The real defect is one
frame down, and every reader of that trace has to work that out for themselves - every time.

Make the contract part of the signature and the check moves to where the mistake was made. The
implicit conversion runs in the caller's frame, before control ever reaches the callee:

```csharp
int Length(NotNull<string> s) => ((string)s).Length;            // no guard; there cannot be one
```

```
ContractViolationException: Value of type 'System.String' must not be null.
   at Bravasoft.Contracts.NotNull`1.ThrowNull()
   at Bravasoft.Contracts.NotNull`1..ctor(T value)
   at Bravasoft.Contracts.NotNull`1.op_Implicit(T value)
   at Program.BuggyCaller(Boolean traditional)                  <- the actual bug
   at Program.Main()
```

`Length` does not appear in that trace at all, because it was never entered. The deepest frame of
your own code is the line that broke the promise. Nothing is left to interpret.

The call site is unchanged - `Length(name)` still compiles and reads the same. What changed is
who is accountable, and where you land in the debugger.

## The contract belongs in the signature

Written as a guard clause, a precondition is an implementation detail. It lives in the body, and
the only ways to discover it are to read the source or to hit it at runtime. `int Length(string s)`
says nothing about null; the requirement is real but invisible, so every caller either guesses,
trusts a comment, or defensively checks again.

`int Length(NotNull<string> s)` states it. IntelliSense shows it, the generated XML docs carry it,
a decompiled reference carries it, and it shows up in the diff when it changes - because it is
part of the type, not part of the implementation.

And because it is a type, it works in the other direction too. A return type is a promise the
*callee* makes:

```csharp
NotNull<string> LookUp(string key) => ...
```

Callers never null-check the result of `LookUp`, and no comment has to tell them not to. If the
promise is broken, the conversion runs at the `return`, inside the callee:

```
ContractViolationException: Value of type 'System.String' must not be null.
   at Bravasoft.Contracts.NotNull`1.ThrowNull()
   at Bravasoft.Contracts.NotNull`1..ctor(T value)
   at Bravasoft.Contracts.NotNull`1.op_Implicit(T value)
   at Program.LookUp(String key)                               <- the actual bug
   at Program.InnocentCaller()
   at Program.Main()
```

So the party at fault lands in the deepest frame either way. Parameters are the caller's
obligation and are checked in the caller's frame; return values are the callee's obligation and
are checked in the callee's. In both cases the code that broke the promise is the code you are
looking at when the debugger stops.

## What this buys you

- **Blame lands on the party at fault.** The stack trace names the code with the bug, not its
  first victim.
- **The body has nothing to check.** No guard clause, no `!`, no defensive branch - the parameter
  type already ruled the bad case out. Fewer lines, and no dead paths to test.
- **The compiler propagates it.** Anything already holding a `NotNull<T>` can pass it along
  without re-checking, so the check happens once, at the edge, instead of at every layer.
- **The requirement is visible without reading the implementation**, in both directions, and it
  cannot go stale - a comment saying "must not be null" does not compile.

## The types

| Type | Guarantees | Refines |
| --- | --- | --- |
| `NotNull<T>` where `T : class` | not null | |
| `NotEmptyString` | not null, `Length > 0` | `NotNull<string>` |
| `NotEmpty<TList, TItem>` where `TList : class, IReadOnlyCollection<TItem>` | not null, `Count > 0` | `NotNull<TList>` |
| `NotEmptyList<T>` | over `List<T>` | `NotEmpty<List<T>, T>` |
| `NotEmptyArray<T>` | over `T[]` | `NotEmpty<T[], T>` |

A stronger contract implies every weaker one, and converts to it implicitly, so a method taking
`NotNull<string>` accepts a `NotEmptyString` unchanged. Widening re-tests nothing.

## Cost

Each type is a `readonly struct` holding a single reference, so it is exactly the size of the
thing it wraps and allocates nothing. The check happens once, on the way in; the throw lives in a
separate non-inlined method so what remains on the hot path is a never-taken branch.

## The one hole

C# has no way to intercept `default(SomeStruct)` - no parameterless constructor runs for
`default(T)`, array elements, or uninitialized fields, and `record struct` does not change that.
So a default instance can exist without ever passing a check. Reading `Value` on one throws
rather than handing back a `null`, which keeps the invariant true at the only point where it is
observable.

## Requirements

`netstandard2.1`, C# 9 - consumable from Unity 2021.3 and later, including IL2CPP.

## Licence

MIT.
