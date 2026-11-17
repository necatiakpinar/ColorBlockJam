namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// A block left the board and every frozen block counted it (T-34, DA-37); read each block's
    /// <c>BoardBlock.Ice</c> for what is left.
    /// </summary>
    public readonly struct IceCountedDownSignal
    {
    }
}
