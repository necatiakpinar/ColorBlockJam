using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Animations;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    public sealed class CoinFlightView : MonoBehaviour, ICoinFlight
    {
        [Tooltip("The HUD's coin counter, held and counted up here.")]
        [SerializeField]
        GameplayHudView _hud;

        [Tooltip("The counter's coin icon: where the coins land and what swells.")]
        [SerializeField]
        RectTransform _target;

        [Tooltip("Full-screen and invisible; on only while coins fly, so no tap reaches the HUD or the board.")]
        [SerializeField]
        GameObject _inputBlocker;

        [SerializeField]
        RectTransform _coinPrefab;

        [SerializeField]
        SparkleBurstView _sparklePrefab;

        [Tooltip("Coins made up front: one per pile coin of the win popup.")]
        [SerializeField, Min(1)]
        int _coinCount = 9;

        [Tooltip("Bursts made up front; landings close together play on separate ones.")]
        [SerializeField, Min(1)]
        int _sparkleCount = 3;

        readonly List<RectTransform> _coins = new();
        readonly List<SparkleBurstView> _sparkles = new();
        readonly List<Vector3> _from = new();
        CoinFlightSettings _settings;
        Sequence _flight;
        Sequence _punch;
        int _nextSparkle;
        int _coinsToAdd;
        int _landings;

        [Inject]
        public void Construct(GameplayConfig config)
        {
            _settings = config.CoinFlight;
            for (int i = 0; i < _coinCount; i++)
            {
                RectTransform coin = Instantiate(_coinPrefab, transform);
                coin.gameObject.SetActive(false);
                _coins.Add(coin);
            }

            for (int i = 0; i < _sparkleCount; i++)
            {
                SparkleBurstView sparkle = Instantiate(_sparklePrefab, transform);
                sparkle.gameObject.SetActive(false);
                _sparkles.Add(sparkle);
            }

            _inputBlocker.SetActive(false);
        }

        public void HoldCounter(int shownCoins) => _hud.HoldCoins(shownCoins);

        public async UniTask FlyAsync(IReadOnlyList<RectTransform> sources, int coins, CancellationToken cancellation)
        {
            _from.Clear();
            for (int i = 0; i < sources.Count && i < _coins.Count; i++)
                _from.Add(sources[i].position);
            if (_from.Count == 0)
            {
                _hud.AddHeldCoins(coins);
                return;
            }

            _coinsToAdd = coins;
            _landings = _from.Count;
            _inputBlocker.SetActive(true);
            for (int i = 0; i < _from.Count; i++)
                _coins[i].gameObject.SetActive(true);
            _flight?.Kill();
            _flight = default(CoinFlightAnimation).Play(new CoinFlightAnimation.Config(_coins, _from, _target.position,
                transform.lossyScale.x, _settings, Land, gameObject));
            try
            {
                await _flight.ToUniTask(cancellationToken: cancellation);
            }
            catch (OperationCanceledException)
            {
                _inputBlocker.SetActive(false);
                for (int i = 0; i < _coins.Count; i++)
                    _coins[i].gameObject.SetActive(false);
                throw;
            }
        }

        void Land(int index)
        {
            _coins[index].gameObject.SetActive(false);
            int share = _landings == 1 ? _coinsToAdd : _coinsToAdd / _landings;
            _coinsToAdd -= share;
            _landings--;
            _hud.AddHeldCoins(share);

            _punch?.Kill();
            _punch = default(PunchScaleAnimation).Play(new PunchScaleAnimation.Config(_target, _settings.Punch, _settings.PunchDuration));
            _sparkles[_nextSparkle].Play(_target.position, _settings.SparkleDistance, _settings.SparkleDuration);
            _nextSparkle = (_nextSparkle + 1) % _sparkles.Count;
        }

        void OnDestroy()
        {
            _flight?.Kill();
            _punch?.Kill();
        }
    }
}
