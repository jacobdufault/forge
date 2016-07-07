**This project has been deprecated and no additional changes are planned.**

## Forge

`Forge` is a set of libraries that are designed to make writing a game easier. In some senses it is a game engine. The core library in `Forge` is `Forge.Entities`, which has a novel entity-component-system (ECS) implementation. There are a number of other libraries which are built around supporting `Forge.Entities` in a game, such as `Forge.Networking`.

Ultimately, `Forge`'s high level goals are as follows (in no particular order):

- Support for large projects
- Testing support for game logic
- Deterministic game-play
- Long-term preservation of game content
- High performance

## Quick Start

Open _Forge.sln_ in Visual Studio / MonoDevelop. Build the project called _AllLibraries_ to get DLLs that can be integrated directly into your project. Output DLLs will be in the top-level directory _Build_. `Forge` targets .NET 3.5 and .NET 4.0 Client; it can be used directly in Unity. DLLs are available directly in the `Build` folder.

Please see the [sample](https://github.com/jacobdufault/forge-sample) for an example of how to use `Forge`. More documentation is coming, but in general there are READMEs in directories that should prove useful for higher-level overviews. For more specific documentation, the code base is heavily commented, and the unit tests can potentially be useful (xUnit is used).

## Features

Forge is packed with many common features that are usable in all games, such as:

- Completely automated multithreading
- Deterministic game simulation (also allows for a minimal bandwidth networking model)
- Support for efficient queries of simulation state in the last frame (which means that rendering interpolation is a snap). This means that you'll never have to write another `PositionChangedEvent` class!
- Declarative support for listening to arbitrary modifications on entities (tell me when the `HealthData` has changed).
- 1-2 lines for saving and loading of games; backwards-compatibility easily supportable (data and systems need to serializable via `Json.NET`, however).
- Deterministic simulation *with* initialization order and update order independence
- A cross-platform networking module that builds on top of `Lidgren.Network`
- (soon) A Forge.Replays module that builds directly on top of the Forge.Entities API
- (soon) A module on the Unity asset store that gives a great Unity integration experience; currently being polished

## Concepts

`Forge` has a couple core concepts which allow the magic to happen. They are immutability, data, systems, and events.

In regards to immutability, and speaking very roughly, there are 3 game states in memory at all times that you game can work with; the previous state, the current state, and the future state. The previous and current states are read-only, while the future state is write-only. Systems can be notified that a data instance has been modified and then it can query the previous state to see what it's old value was. This is one of the ways in which initialization/update order are solved.

The game is composed of `IEntities` which contain `IData` instances (there are a variety of IData flavors, from ones which are Versioned (they support querying their previous states), to NonVersioned, which do not support querying, and Concurrent variants, which allow multiple writes in the same update). A data type can be something simple like your entities health or perhaps its position. Crucially, _data types contain no game logic_.

Game logic goes into `ISystems`, which are executed concurrently on multiple threads. `ISystems` can listen for a number of `Triggers`, such as when an entity has been added. Further, they can express interest in only certain types of entities, for example, a `MovementSystem` can request only entities which have `MovementData` and `PositionData`. Some of the triggers are pretty nifty, such as `Trigger.Modified`, which allows a system to be notified whenever an entity that it contains is modified (for example, the entity's position has changed). The immutability system then allows the system to query the old position even while systems are processing the entities new position.

`ISystems` do *not* deal with rendering any of the game state. Instead, the events API is used to notify the 3rd party renderer about how the simulation state has changed. Almost all of the busy work here is automated by the Unity/XNA integration packages.

Additionally, most games need to create new entities at runtime; `ITemplates` are preconfigured entities which can be instantiated at runtime. They are similar in nature to Unity's prefabs. `ITemplates` are the only way to create new `IEntity` instances at runtime.


## Engine Integration Status

A library is currently being developed to integrate `Forge` into Unity. There is a simple integration package for XNA/MonoGame in the [sample](https://github.com/jacobdufault/forge-sample) that will be developed further in the sample package.

## External Libraries

`Forge` builds on top of the fantastic work of other developers. The core 3rd party libraries used are `log4net`, `Lidgren.Network`, and `Newtonsoft.JSON`.
 
## License

Forge is freely available under the MIT license.
