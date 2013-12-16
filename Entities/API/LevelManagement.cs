using Neon.Entities.Implementation.Shared;
using Neon.FileSaving;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// Facilitates the loading and saving of IContentDatabases (levels).
    /// </summary>
    public static class LevelManager {
        /// <summary>
        /// Returns a new empty level.
        /// </summary>
        public static ISavedLevel CreateLevel() {
            return new SavedLevel();
        }
    }

    /// <summary>
    /// Stores an IGameInput instance along with the update that the input was issued.
    /// </summary>
    public class IssuedInput {
        /// <summary>
        /// The update number that this input was issued at.
        /// </summary>
        public readonly int UpdateNumber;

        /// <summary>
        /// The input instance itself.
        /// </summary>
        public readonly IGameInput Input;

        /// <summary>
        /// Creates a new IssuedInput instance.
        /// </summary>
        public IssuedInput(int updateNumber, IGameInput input) {
            UpdateNumber = updateNumber;
            Input = input;
        }
    }

    /// <summary>
    /// Represents a level that has been saved.
    /// </summary>
    public interface ISavedLevel {
        /// <summary>
        /// Paths of assemblies that should be loaded into the AppDomain.
        /// </summary>
        List<string> AssemblyInjectionPaths {
            get;
        }

        /// <summary>
        /// Types of the system providers that should be used to get instances of ISystems.
        /// </summary>
        List<Type> SystemProviderTypes {
            get;
        }

        /// <summary>
        /// The current state of the game.
        /// </summary>
        IGameSnapshot CurrentState {
            get;
        }

        /// <summary>
        /// The original state of the game before any updates occurred.
        /// </summary>
        IGameSnapshot OriginalState {
            get;
        }

        /// <summary>
        /// The templates that are used in the level.
        /// </summary>
        List<ITemplate> Templates {
            get;
        }

        /// <summary>
        /// The input that can be used to transform the original state into the current state.
        /// </summary>
        List<IssuedInput> Input {
            get;
        }
    }
}