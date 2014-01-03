## Forge

`Forge` is a set of libraries that are designed to make writing a game easier. In some senses it is a game engine. The core library in `Forge` is `Forge.Entities`, which has a novel entity-component-system (ECS) implementation. There are a number of other libraries which are built around supporting `Forge.Entities` in a game, such as `Forge.Networking`.

## Quick Start

Open _Forge.sln_ in Visual Studio / MonoDevelop. Build the project called _AllLibraries_ to get DLLs that can be integrated directly into your project. Output DLLs will be in the top-level directory _Build_. `Forge` targets .NET 3.5; it can be used directly in Unity.

`Forge` is currently pre-release software. There is a lack of documentation, but there unit tests (written in xUnit) if you want to get a view of how to use the engine. A component that neatly ties `Forge` into Unity is currently under development.

## Engine Integration

A library is currently being developed to integrate `Forge` into Unity. There are plans to do a demo application for MonoGame/XNA.

## External Libraries

`Forge` builds on top of the fantastic work of other developers. The core 3rd party libraries used are `log4net`, `Lidgren.Network`, and `Newtonsoft.JSON`.
