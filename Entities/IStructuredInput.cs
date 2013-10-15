using Neon.Utility;

namespace Neon.Entities {
    /// <summary>
    /// User input that is structured / of some input format.
    /// </summary>
    public interface IStructuredInput {
    }

    // Client code should do something like this:
    /*
    [Serializable]
    public class GameEntityCommand : IStructuredCommand {
    }

    [Serializable]
    public class PlaceTowerEntityCommand : GameEntityCommand {
        /// <summary>
        /// The grid node to place the tower at
        /// </summary>
        public Vector2r GridNodePosition;

        /// <summary>
        /// The tower to create
        /// </summary>
        public ContentTower TowerPrefab;

        public override string ToString() {
            return string.Format("PlaceTowerEntityCommand [Position={0}, TowerPrefab={1}]",
                GridNodePosition, TowerPrefab);
        }
    }
    */

}