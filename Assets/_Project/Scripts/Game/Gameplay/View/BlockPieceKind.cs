namespace RollicCase.Game.Gameplay.View
{
    /// <summary>The supplied modular block meshes (art-pipeline.md §4).</summary>
    public enum BlockPieceKind
    {
        /// <summary>Quarter cell, top cap only: both neighbours toward its corner belong to the block.</summary>
        Center = 1000,

        /// <summary>Quarter cell with one side wall, authored on its +z side.</summary>
        Edge = 2000,

        /// <summary>Quarter cell with a rounded outer corner, authored at +x +z.</summary>
        OuterCorner = 3000,

        /// <summary>Whole-cell piece around a grid vertex with a concave notch, authored at +x +z.</summary>
        InnerCorner = 4000,
    }
}
