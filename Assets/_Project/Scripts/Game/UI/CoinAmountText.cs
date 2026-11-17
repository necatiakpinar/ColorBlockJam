using TMPro;
using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>A coin balance as the counters show it: the number up to 999, then thousands ("1.28k").</summary>
    public sealed class CoinAmountText : MonoBehaviour
    {
        const int ThousandsFrom = 1000;

        [SerializeField]
        TMP_Text _text;

        public string Text => _text.text;

        public void Show(int coins)
        {
            if (coins < ThousandsFrom)
                _text.SetText("{0}", coins);
            else
                _text.SetText("{0:2}k", coins / (float)ThousandsFrom);
        }
    }
}
