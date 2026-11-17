#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#elif UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace ColorBlockJam.Core.Haptics
{
    public sealed class MobileHapticService : IHapticService
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        static readonly int[] PredefinedEffects = { 2, 0, 5 };
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
