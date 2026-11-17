using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Levels.Packs
{
    public sealed class LevelPackService : ILevelSource
    {
        public const string RemoteKey = "level_pack_remote_v1";

        readonly LevelPackConfig _config;
        readonly IKeyValueStorage _storage;
        readonly IGameLogger _logger;
        readonly LevelPackJson _json = new();
        readonly LevelDefinitionValidator _validator;
        LevelPack _current;

        public LevelPackService(LevelPackConfig config, LevelRulesConfig rules, IKeyValueStorage storage, IGameLogger logger)
        {
            _config = config;
            _storage = storage;
            _logger = logger;
            _validator = new LevelDefinitionValidator(rules);
        }

        public LevelPack Current => _current ?? throw new InvalidOperationException("Load the level pack at boot before reading levels.");

        public bool IsRemote { get; private set; }

        public int LevelCount => Current.Levels.Count;

        public LevelData GetLevel(int index) => Current.Levels[index];

        public void Load()
        {
            if (!TryCheck(_config.BundledPack.text, out LevelPack bundled, out string problem))
                throw new InvalidOperationException($"The bundled level pack is broken: {problem}");
            _current = bundled;
            IsRemote = false;
            if (!_storage.TryGet(RemoteKey, out string stored))
                return;
            if (TryCheck(stored, out LevelPack remote, out problem) && remote.Revision >= bundled.Revision)
            {
                _current = remote;
                IsRemote = true;
                return;
            }

            _logger.LogWarning(problem != null
                ? $"Dropped the stored remote level pack: {problem}"
                : $"Dropped the stored remote level pack: revision {remote.Revision} is older than the bundled {bundled.Revision}.");
            _storage.Delete(RemoteKey);
            _storage.Flush();
        }

        public bool TryStoreRemote(string json, out string error)
        {
            if (!TryCheck(json, out LevelPack remote, out error))
                return false;
            if (!TryCheck(_config.BundledPack.text, out LevelPack bundled, out error))
                throw new InvalidOperationException($"The bundled level pack is broken: {error}");
            if (remote.Revision < bundled.Revision)
            {
                error = $"Revision {remote.Revision} is older than the bundled {bundled.Revision}.";
                return false;
            }

            _storage.Set(RemoteKey, json);
            _storage.Flush();
            return true;
        }

        bool TryCheck(string json, out LevelPack pack, out string problem)
        {
            if (!_json.TryRead(json, out pack, out problem))
                return false;
            for (int i = 0; i < pack.Levels.Count; i++)
            {
                IReadOnlyList<LevelIssue> issues = _validator.Validate(pack.Levels[i]);
                if (!_validator.HasErrors(issues))
                    continue;
                problem = $"Level {i + 1}: {FirstError(issues)}";
                pack = null;
                return false;
            }

            return true;
        }

        static string FirstError(IReadOnlyList<LevelIssue> issues)
        {
            foreach (LevelIssue issue in issues)
                if (issue.Severity == LevelIssueSeverity.Error)
                    return issue.Message;
            return null;
        }
    }
}
