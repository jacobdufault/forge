// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Newtonsoft.Json;
using System;

namespace Forge.Networking.Core {
    /// <summary>
    /// A network player is an abstraction over a network connection.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Player {
        /// <summary>
        /// The name that the player gave themselves.
        /// </summary>
        [JsonProperty("Name")]
        public string Name;

        /// <summary>
        /// The GUID that uniquely identifies this player. This GUID can be per-session and does not
        /// need to be permanent.
        /// </summary>
        [JsonProperty("Guid")]
        public Guid Guid;

        /// <summary>
        /// Determines whether the specified see cref="System.Object" }, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this
        /// instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this
        /// instance; otherwise, /c>.</returns>
        public override bool Equals(object obj) {
            if (obj is Player == false) {
                return false;
            }

            Player player = (Player)obj;
            return Guid == player.Guid;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.</returns>
        public override int GetHashCode() {
            return Guid.GetHashCode();
        }

        public override string ToString() {
            return string.Format("Player [Name={0}, GUID={1}]", Name, Guid);
        }
    }
}