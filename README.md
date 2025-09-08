This package adds QoL Attributes for unity developement.

## Setup
The installation is quite strait forward:
- `copy` the link of this repository
- in Unity, open the `Package Manager` window
- click on the top left `+`
- select `Add package from git URL...`
- a field should show up, `paste` the url in it
- press `Enter`

## How To Use
Refer to the official documentation about [C# Attributes](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/).

## List of Attributes

| Attributes | Use on | Description | Usage Example |
|---|---|---|---|
| DisableInInspector | `fields` | Makes inspector visible field readonly. | Disabling an ID field, you might want to see/copy it, but prevent its modification to avoid bugs. |
| SerializeAbstractField | `fields` | Makes it possible to modify an abstract field by instancing one of it's concrete children classes. Also works with Generic Classes. | Expose a `List<Enemy>` when Enemy is abstract. Maybe you'd want a `Goblin<T> where T : Element`, thus instancing `Goblin<Mind>` is possible. |


## Known Bugs
- Trying to instanciate a Generic Class with SerializeAbstractField only works if said class only have 1 Generic Type.

