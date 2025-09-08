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
Simply write in your C# scripts `[TheAttributeYouNeed]` above a class, field, property, or method to add Editor/Runtime functionalities.

## List of Attributes

| Attributes | Description | Compatible with |
|---|---|---|
| DisableInInspector | Makes inspector visible field readonly. Useful for debuging. For instance disabling an ID field, you might want to see it, but prevent its modification to not create bugs. | `fields` |
