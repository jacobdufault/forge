## Forge.Network

`Forge.Network` is a set of APIs that are designed to solve many common problems with networking. It provides a `Chat` system, a `Lobby` system (with map downloads), and a `Game` system, that while generic, is designed to integrate cleanly with `Forge.Entities`.

We think that you'll love this library. It's built on top of the fantastic `Lidgren.Network` library, so it has awesome performance and awesome cross-platform support.

## Implementation Structure

Feel free to dive into the code, but here's a rough outline of what's what.

* `Core`: Core networking primitives that abstract `Lidgren.Network`. All other networking components build on top of `Core`.
* `Chat`: The chat subsystem.
* `Lobby`: The lobby subsystem (joining games, download maps, etc).
* `Pausing`: Supports pausing and unpausing a game.
* `AutomaticTurnGame`: The subsystem designed to support a running game.