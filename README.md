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

## Engine Integration Status

A library is currently being developed to integrate `Forge` into Unity. There is a simple integration package for XNA/MonoGame in the [sample](https://github.com/jacobdufault/forge-sample) that will be developed further in the sample package.

## External Libraries

`Forge` builds on top of the fantastic work of other developers. The core 3rd party libraries used are `log4net`, `Lidgren.Network`, and `Newtonsoft.JSON`.
