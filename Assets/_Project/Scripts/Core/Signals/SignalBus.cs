using System;
using System.Collections.Generic;

namespace ColorBlockJam.Core.Signals
{
    public sealed class SignalBus
    {
        readonly Dictionary<Type, object> _channels = new();

        public void Subscribe<TSignal>(Action<TSignal> handler) where TSignal : struct
        {
            GetOrCreateChannel<TSignal>().Add(handler);
        }

        public void Unsubscribe<TSignal>(Action<TSignal> handler) where TSignal : struct
        {
            if (_channels.TryGetValue(typeof(TSignal), out object channel))
                ((SignalChannel<TSignal>)channel).Remove(handler);
        }

        public void Fire<TSignal>(TSignal signal) where TSignal : struct
        {
            if (_channels.TryGetValue(typeof(TSignal), out object channel))
                ((SignalChannel<TSignal>)channel).Fire(signal);
        }

        SignalChannel<TSignal> GetOrCreateChannel<TSignal>() where TSignal : struct
        {
            if (_channels.TryGetValue(typeof(TSignal), out object channel))
                return (SignalChannel<TSignal>)channel;

            var created = new SignalChannel<TSignal>();
            _channels.Add(typeof(TSignal), created);
            return created;
        }

        sealed class SignalChannel<TSignal> where TSignal : struct
        {
            readonly List<Action<TSignal>> _handlers = new();
            int _fireDepth;
            bool _hasPendingRemovals;

            public void Add(Action<TSignal> handler)
            {
                if (handler == null)
                    throw new ArgumentNullException(nameof(handler));
                if (_handlers.Contains(handler))
                    throw new InvalidOperationException(
                        $"Handler {handler.Method.Name} is already subscribed to {typeof(TSignal).Name}.");

                _handlers.Add(handler);
            }

            public void Remove(Action<TSignal> handler)
            {
                int index = _handlers.IndexOf(handler);
                if (index < 0)
                    return;

                if (_fireDepth > 0)
                {
                    _handlers[index] = null;
                    _hasPendingRemovals = true;
                    return;
                }

                _handlers.RemoveAt(index);
            }

            public void Fire(TSignal signal)
            {
                _fireDepth++;
                try
                {
                    int count = _handlers.Count;
                    for (int i = 0; i < count; i++)
                        _handlers[i]?.Invoke(signal);
                }
                finally
                {
                    _fireDepth--;
                    if (_fireDepth == 0 && _hasPendingRemovals)
                    {
                        _handlers.RemoveAll(handler => handler == null);
                        _hasPendingRemovals = false;
                    }
                }
            }
        }
    }
}
