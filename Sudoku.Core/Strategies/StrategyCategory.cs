namespace Sudoku.Core.Strategies
{
    /// <summary>
    /// Categories of solving strategies based on difficulty.
    /// Based on SudokuWiki.org classification system.
    /// </summary>
    public enum StrategyCategory
    {
        /// <summary>
        /// Basic strategies (Score: 1-50).
        /// Includes: Naked/Hidden Singles, Pairs, Triples, Quads, Pointing Pairs, Box/Line Reduction.
        /// </summary>
        Basic,

        /// <summary>
        /// Tough strategies (Score: 60-150).
        /// Includes: X-Wing, Y-Wing, Swordfish, Simple Colouring, XYZ-Wing, BUG.
        /// </summary>
        Tough,

        /// <summary>
        /// Diabolical strategies (Score: 160-300).
        /// Includes: X-Cycles, XY-Chains, Jellyfish, Unique Rectangles, 3D Medusa.
        /// </summary>
        Diabolical,

        /// <summary>
        /// Extreme strategies (Score: 300+).
        /// Includes: Finned Fish, AIC, Forcing Chains, Exocet, Death Blossom.
        /// </summary>
        Extreme
    }
}
