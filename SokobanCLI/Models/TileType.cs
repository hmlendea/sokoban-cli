namespace SokobanCLI.Models
{
    /// <summary>
    /// Tile identifiers used by the level files.
    /// </summary>
    public enum TileType
    {
        Empty = 0,
        Wall = 1,
        Box = 2,
        Goal = 3,
        Player = 4,
        BoxOnGoal = 5,
        PlayerOnGoal = 6
    }
}
