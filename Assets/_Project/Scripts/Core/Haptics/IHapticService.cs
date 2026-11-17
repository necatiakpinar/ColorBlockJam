namespace ColorBlockJam.Core.Haptics
{
    /// <summary>Plays a short haptic tap on the device; does nothing where the device has none.</summary>
    public interface IHapticService
    {
        void Play(HapticImpact impact);
    }
}
