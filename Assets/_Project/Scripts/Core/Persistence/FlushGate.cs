using System;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class FlushGate
    {
        readonly Action _flush;
        int _depth;
        bool _pending;

        public FlushGate(Action flush)
        {
            _flush = flush;
        }

        public IDisposable Defer()
        {
            _depth++;
            return new Scope(this);
        }

        public void Request()
        {
            if (_depth > 0)
            {
                _pending = true;
                return;
            }

            _flush();
        }

        void Release()
        {
            _depth--;
            if (_depth > 0 || !_pending)
                return;
            _pending = false;
            _flush();
        }

        sealed class Scope : IDisposable
        {
            FlushGate _gate;

            public Scope(FlushGate gate)
            {
                _gate = gate;
            }

            public void Dispose()
            {
                FlushGate gate = _gate;
                _gate = null;
                gate?.Release();
            }
        }
    }
}
