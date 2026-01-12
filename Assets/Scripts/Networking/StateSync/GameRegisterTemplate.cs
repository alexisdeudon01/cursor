using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.StateSync
{
    [Serializable]
    public sealed class GameActionInstruction
    {
        public string actionName;
        public string payload;
    }

    [Serializable]
    public sealed class GameConfigData
    {
        public string gameTypeUid;
        public string gameId;
        public string displayName;
        public string description;
        public int minPlayers;
        public int maxPlayers;
        public string mapName;
        public Vector3 mapSize;
        public Vector3 worldOffset;
        public List<GameActionInstruction> actions = new List<GameActionInstruction>();
    }

    [Serializable]
    public sealed class GameRepresentation
    {
        public string gameTypeUid;
        public string gameId;
        public string displayName;
        public string description;
        public int minPlayers;
        public int maxPlayers;
        public string jsonConfig;
        public MapConfigData configData = new MapConfigData();
        public List<GameActionInstruction> actions = new List<GameActionInstruction>();

        public string BuildConfigJson()
        {
            var config = new GameConfigData
            {
                gameTypeUid = gameTypeUid,
                gameId = gameId,
                displayName = displayName,
                description = description ?? string.Empty,
                minPlayers = minPlayers,
                maxPlayers = maxPlayers,
                mapName = configData != null ? configData.mapName : string.Empty,
                mapSize = configData != null ? configData.mapSize : Vector3.zero,
                worldOffset = configData != null ? configData.worldOffset : Vector3.zero,
                actions = new List<GameActionInstruction>(actions)
            };

            jsonConfig = JsonUtility.ToJson(config);
            return jsonConfig;
        }

        public GameCommandDto BuildConfigCommand(string sessionUid, Vector3 worldOffset)
        {
            if (configData == null)
            {
                configData = new MapConfigData();
            }

            var config = new MapConfigData
            {
                mapName = configData.mapName,
	                shape = configData.shape,
                mapSize = configData.mapSize,
	                circleRadius = configData.circleRadius,
                gridWidth = configData.gridWidth,
                gridHeight = configData.gridHeight,
                cellSize = configData.cellSize,
	                seed = configData.seed,
                worldOffset = worldOffset
            };

            return GameCommandFactory.CreateMapConfig(sessionUid, config);
        }

        public static GameRepresentation FromDefinition(IGameDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var rep = new GameRepresentation
            {
                gameTypeUid = Guid.NewGuid().ToString("N"),
                gameId = definition.GameId,
                displayName = definition.DisplayName,
                description = definition.Description,
                minPlayers = definition.MinPlayers,
                maxPlayers = definition.MaxPlayers,
                jsonConfig = string.Empty,
                // Use the game's own config generator (at origin) so we capture shape/size settings.
                configData = definition.CreateMapConfig(Vector3.zero, 0) ?? new MapConfigData
                {
                    mapName = definition.DisplayName,
                    mapSize = new Vector3(20f, 0f, 20f),
                    worldOffset = Vector3.zero
                }
            };

            return rep;
        }
    }

    public sealed class GameRegisterTemplate
    {
        private readonly Dictionary<string, GameRepresentation> entries = new Dictionary<string, GameRepresentation>();
        private readonly Dictionary<string, string> byGameId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, GameRepresentation> Entries => entries;

        public void Register(GameRepresentation representation)
        {
            if (representation == null || string.IsNullOrEmpty(representation.gameTypeUid))
            {
                return;
            }

            if (representation.configData == null)
            {
                representation.configData = new MapConfigData();
            }

            if (string.IsNullOrEmpty(representation.configData.mapName))
            {
                representation.configData.mapName = representation.displayName;
            }

            representation.BuildConfigJson();

            entries[representation.gameTypeUid] = representation;

            if (!string.IsNullOrEmpty(representation.gameId))
            {
                byGameId[representation.gameId] = representation.gameTypeUid;
            }
        }

        public GameRepresentation GetByGameTypeUid(string gameTypeUid)
        {
            if (string.IsNullOrEmpty(gameTypeUid))
            {
                return null;
            }

            return entries.TryGetValue(gameTypeUid, out var rep) ? rep : null;
        }

        public GameRepresentation GetByGameId(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return null;
            }

            return byGameId.TryGetValue(gameId, out var uid) ? GetByGameTypeUid(uid) : null;
        }

        public void EnsureLoadedFromGameRegistry()
        {
            if (entries.Count > 0)
            {
                return;
            }

            GameRegistry.Initialize();
            foreach (var def in GameRegistry.AllGames)
            {
                var rep = GameRepresentation.FromDefinition(def);
                if (rep == null)
                {
                    continue;
                }

                Register(rep);
            }
        }
    }
}
