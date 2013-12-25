## Neon.Entities

`Neon.Entities` is an advanced entity-component-system implementation that is inspired by immutability. It is designed to scale to large games and be rendering engine agnostic, and there is first-class support for Unity.

Here's the short list:

* Automatic multithreading
* Determinstic execution (perfect for multiplayer RTS/tower-defense/simulation)
* Super simple save/load game support (replay support coming soon)
* No more initialization and update order races

By focusing on immutability, `Neon.Entities` is able to do some pretty awesome magic. It gives you automatic multithreading and deterministic execution (great for RTS, tower-defense, and simulation games!). Further, `Neon.Entities` removes the dreaded updated order and initialization order problems that are common to so many frameworks. Do you ever recall working with a `PositionChangedEvent`? That's completely removed in `Neon.Entities`; you can listen for data changes/additions/removals *and* be able to see the previous state of the data, before the modification.

`Neon.Entities` has first class support for saving and loading of games (replay support is simple to implement and will be eventually added in a `Neon.Replays` module); to save a game, all you have to do is add a few lines of annotations to objects which are used in the game simulation (`Json.NET` is used for serialization).

To aid in game design, `Neon.Entities` has two modes of operation; one for the game designer, where you can edit game state, and one for the gamer, where the game is actually simulated. The game designer mode is perfect for creating a level in the `Unity` editor, and then you switch to the engine mode to play it! Simple.

## Implementation Overview
So, you want to dive into the details? Be warned, this code is complicated.

In this folder, there are two subfolders of interest: `API` and `Implementation`. `API` contains the public type definitions that users of the library use. Code here is extremely stable and changes are almost always backwards compatible. You build your app on top of `API`. `Implementation` contains the actual implementation for the API.

If you go into the `Implementation` folder, you'll find four more folders.

* `Content`: Implements the API for the game designer.
* `ContextObjects`: An implementation detail for the serialization process. This will likely get moved into `Shared/ContextObjects`.
* `Runtime`: Implements the API for the player; ie, actually implements the simulation.
* `Shared`: Shared code between `Content` and `Runtime`.

If you're looking to see how `Neon.Entities` does its magic, you're best bet is to look into `Runtime`. That has almost all of the interesting code.

