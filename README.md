# Bravasoft.Contracts

Types that move invariants into the type system - `NotEmptyString` in place of a `string` that
every function has to check for itself. Both conversions are implicit, so adopting one is a
signature change and nothing else.

## Only the signature changes

The usual constructor, with the checks written out:

```csharp
public Player(string name, List<Item> inventory)
{
    if (string.IsNullOrEmpty(name))
        throw new ArgumentException("Name must not be empty.", nameof(name));
    if (inventory is null || inventory.Count == 0)
        throw new ArgumentException("Inventory must not be empty.", nameof(inventory));

    _name = name;
    _inventory = inventory;
}
```

The same constructor with the requirements in its signature:

```csharp
private readonly string _name;                // still a plain string
private readonly List<Item> _inventory;       // still a plain List<Item>

public Player(NotEmptyString name, NotEmptyList<Item> inventory)
{
    _name = name;                             // implicit NotEmptyString  -> string
    _inventory = inventory;                   // implicit NotEmptyList<T> -> List<T>
}
```

No guards, no `.Value`, no casts, and the fields keep their ordinary types - so every method that
already reads `_name` is untouched. Callers do not change either, which is where this starts to
pay:

```csharp
static Player Load(SaveData save)
{
    var items = new List<Item>();

    if (save.Version >= 2)                        // bug: a v1 save never fills the list
    {
        foreach (var id in save.ItemIds)
            items.Add(Catalogue.Find(id));
    }

    return new Player(save.Name, items);          // throws HERE, in Load, on a v1 save
}
```

`items` is an ordinary `List<Item>` and is passed as one. The conversion runs at the call, so the
empty list is caught on this line - not two frames later, inside a constructor that did nothing
wrong.

## The bug is in the caller

That distinction is the whole point, and the stack traces show it. Guard clauses put the check
inside the callee, so that is what gets accused:

```
ArgumentException: Inventory must not be empty. (Parameter 'inventory')
   at Traditional.Player..ctor(String name, List`1 inventory)   <- accused, but blameless
   at Traditional.Loader.Load(SaveData save)
   at Program.Main()
```

With the contract in the signature, `Player` is never entered, so it cannot be blamed:

```
ContractViolationException: Collection of type 'List`1[Item]' must not be empty.
   at Bravasoft.Contracts.NotEmpty`2.ThrowEmpty()
   at Bravasoft.Contracts.NotEmpty`2..ctor(TList value)
   at Bravasoft.Contracts.NotEmptyList`1..ctor(List`1 value)
   at Bravasoft.Contracts.NotEmptyList`1.op_Implicit(List`1 value)
   at Contracted.Loader.Load(SaveData save)                     <- the actual bug
   at Program.Main()
```

The deepest frame of your own code is the line that broke the promise. Nothing is left to
interpret.

It works on return values too. `NotNull<string> LookUp(string key)` is a promise the *callee*
makes: callers never null-check the result, and a broken promise is caught at the `return`,
inside `LookUp`. Parameters are the caller's obligation and fail in the caller's frame; return
values are the callee's and fail in the callee's.

## Reaching the value

Member access does not see through a user-defined conversion, so `name.Length` will not compile
(`CS1061`). Use `.Value`:

```csharp
int length = name.Value.Length;               // or ((string)name).Length
```

Prefer `.Value` over the cast. A cast reads as though something is being forced, which is the
opposite of what is happening - the value has already been checked, and `.Value` simply names it.

## Why not nullable reference types, or `[NotNull]`?

Both are erased before anything runs, and both warn rather than fail.

In Unity that gap is the default state: nullable reference types are off, with no project setting
to turn them on - the Editor regenerates the `.csproj` files, so the switch has to live in a
`csc.rsp` or as `#nullable enable` per file. Engine and package APIs carry no annotations either,
so even a project that opts in gets nothing at the boundaries where outside values arrive.

`[NotNull]` adds a harder limit: it is one check. There is no `[NotEmpty]`, no `[Positive]`, no
`[InRange(1, 10)]`, and you cannot write them - the vocabulary belongs to whoever wrote the
analyser. A contract expressed as a type is ordinary code you can add to.

## The types

| Type | Guarantees | Refines |
| --- | --- | --- |
| `NotNull<T>` where `T : class` | not null | |
| `NotEmptyString` | not null, `Length > 0` | `NotNull<string>` |
| `NotEmpty<TList, TItem>` where `TList : class, IReadOnlyCollection<TItem>` | not null, `Count > 0` | `NotNull<TList>` |
| `NotEmptyList<T>` | over `List<T>` | `NotEmpty<List<T>, T>` |
| `NotEmptyArray<T>` | over `T[]` | `NotEmpty<T[], T>` |

A stronger contract converts implicitly to every weaker one, so a method taking `NotNull<string>`
accepts a `NotEmptyString` unchanged. Widening re-tests nothing.

## Cost

Each type is a `readonly struct` holding a single reference: the size of the thing it wraps, and
no allocation. The check happens once, on the way in, and the throw sits in a separate non-inlined
method, so what is left on the hot path is a never-taken branch.

## The `default` hole

C# cannot intercept `default(SomeStruct)` - no constructor runs for `default(T)`, array elements,
or uninitialised fields, and `record struct` does not change that. So a default instance can exist
without ever passing a check. Reading `Value` on one throws rather than returning `null`, which
keeps the invariant true at the only point where it is observable.

## Requirements

`netstandard2.1`, C# 9 - consumable from Unity 2021.3 and later, including IL2CPP.

## Licence

MIT.
