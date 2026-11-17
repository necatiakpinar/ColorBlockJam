#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#elif UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace ColorBlockJam.Core.Haptics
{
    /// <summary>
    /// Short taps through the platform's own haptics, with no third-party package: Android's
    /// <c>VibrationEffect</c> predefined effects (one-shot pulses before API 29) and iOS's
    /// <c>UIImpactFeedbackGenerator</c> (<c>Assets/Plugins/iOS/ColorBlockJamHaptics.mm</c>). Silent in the
    /// editor and on desktop. Android needs the VIBRATE permission
    /// (<c>Assets/Plugins/Android/ColorBlockJamHaptics.androidlib</c>).
    /// </summary>
    public sealed class MobileHapticService : IHapticService
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // android.os.VibrationEffect.EFFECT_TICK, EFFECT_CLICK, EFFECT_HEAVY_CLICK (API 29+).
        static readonly int[] PredefinedEffects = { 2, 0, 5 };
        // One-shot pulse lengths in milliseconds for the same three strengths, before API 29.
        static readonly long[] PulseMilliseconds = { 10L, 20L, 35L };
        const int DefaultAmplitude = -1;
        const int PredefinedSince = 29;
        const int OneShotSince = 26;

        readonly AndroidJavaObject _vibrator;
        readonly AndroidJavaObject[] _effects;
        readonly int _sdk;

        public MobileHapticService()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            _sdk = version.GetStatic<int>("SDK_INT");
            if (_vibrator == null || !_vibrator.Call<bool>("hasVibrator"))
            {
                _vibrator = null;
                return;
            }

            // Built once, so a tap allocates no Java objects of its own.
            if (_sdk >= PredefinedSince)
            {
                using var effect = new AndroidJavaClass("android.os.VibrationEffect");
                _effects = new AndroidJavaObject[PredefinedEffects.Length];
                for (int i = 0; i < _effects.Length; i++)
                    _effects[i] = effect.CallStatic<AndroidJavaObject>("createPredefined", PredefinedEffects[i]);
            }
            else if (_sdk >= OneShotSince)
            {
                using var effect = new AndroidJavaClass("android.os.VibrationEffect");
                _effects = new AndroidJavaObject[PulseMilliseconds.Length];
                for (int i = 0; i < _effects.Length; i++)
                    _effects[i] = effect.CallStatic<AndroidJavaObject>("createOneShot", PulseMilliseconds[i], DefaultAmplitude);
            }
        }

        public void Play(HapticImpact impact)
        {
            if (_vibrator == null)
                return;
            int index = IndexOf(impact);
            if (_effects != null)
                _vibrator.Call("vibrate", _effects[index]);
            else
                _vibrator.Call("vibrate", PulseMilliseconds[index]);
        }
#elif UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void ColorBlockJamHapticImpact(int style);

        // UIImpactFeedbackStyle: 0 light, 1 medium, 2 heavy.
        public void Play(HapticImpact impact) => ColorBlockJamHapticImpact(IndexOf(impact));
#else
        public void Play(HapticImpact impact)
        {
        }
#endif

        int IndexOf(HapticImpact impact) => impact switch
        {
            HapticImpact.Light => 0,
            HapticImpact.Medium => 1,
            _ => 2,
        };
    }
}
