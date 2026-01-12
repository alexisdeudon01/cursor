using Core.Games;
using Core.Utilities;
using Networking.StateSync;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Networking.RpcHandlers
{
    /// <summary>
    /// Handles game start validation and initiation.
    /// Extracted from SessionRpcHub to reduce complexity.
    /// </summary>
    public class GameStartHandler : BaseRpcHandler
    {
        private GameStartValidator validator;

        public override string GetHandlerName() => "GameStartHandler";

        protected override void OnInitialize()
        {
            validator = new GameStartValidator();
        }

        protected override void OnCleanup()
        {
            validator = null;
        }

        // ============================================================
        // START GAME
        // ============================================================

        public void HandleStartGame(string sessionName, ulong clientId)
        {
            if (!CheckInitialized())
                return;

            NetworkLogger.LogRpc("StartGame", clientId, $"session='{sessionName}'");

            if (GameSessionManager.Instance == null)
            {
                NetworkLogger.Error("GameStart", "GameSessionManager not available on server");
                return;
            }

            GameRegistry.Initialize();

            // Validate game start with detailed feedback
            var validation = validator.ValidateGameStart(sessionName, clientId);

            if (!validation.IsValid)
            {
                NetworkLogger.Warning("GameStart", $"Validation failed for '{sessionName}': {validation.ErrorMessage}");
                SendGameStartFailed(clientId, validation.ErrorMessage, validation.Reason);
                return;
            }

            NetworkLogger.Info("GameStart", $"All conditions met for '{sessionName}' - proceeding");

            var players = GameSessionManager.Instance.GetPlayers(sessionName);
            string gameId = GameSessionManager.Instance.GetSelectedGameId(sessionName);
            if (string.IsNullOrEmpty(gameId))
            {
                gameId = "square-game";
            }

            var container = GameSessionManager.Instance.GetSecureContainer(sessionName);
            if (container == null)
            {
                NetworkLogger.Error("GameStart", $"Session container not found for '{sessionName}'");
                SendGameStartFailed(clientId, "Session introuvable", GameStartFailureReason.SessionNotFound);
                return;
            }

            var gameDef = GameRegistry.GetGame(gameId);
            if (gameDef == null)
            {
                NetworkLogger.Warning("GameStart", $"Game definition not found for '{gameId}', falling back to square-game");
                gameId = "square-game";
                gameDef = GameRegistry.GetGame(gameId);
            }

            if (!container.StartGame(gameId, gameDef))
            {
                NetworkLogger.Warning("GameStart", $"StartGame rejected for '{sessionName}'");
                SendGameStartFailed(clientId, "Impossible de demarrer la partie", GameStartFailureReason.ServerError);
                return;
            }

            StartGameForPlayers(sessionName, players, gameId, container);
        }

        // ============================================================
        // SET GAME TYPE
        // ============================================================

        public void HandleSetGameType(string sessionName, string gameId, ulong clientId)
        {
            if (!CheckInitialized())
                return;

            if (GameSessionManager.Instance == null)
            {
                NetworkLogger.Error("GameStart", "GameSessionManager not available");
                return;
            }

            NetworkLogger.LogRpc("SetGameType", clientId, $"session='{sessionName}', gameId='{gameId}'");
            GameSessionManager.Instance.SetGameType(clientId, sessionName, gameId);
        }

        // ============================================================
        // PRIVATE METHODS
        // ============================================================

        private void StartGameForPlayers(string sessionName, List<ulong> players, string gameId, SessionContainer container)
        {
            NetworkLogger.Info("GameStart", $"Starting game: session='{sessionName}', gameId='{gameId}', players={players.Count}");
            NetworkBootstrap.LogGame("STARTED", sessionName, players.Count);

            // Ensure GameInstanceManager exists
            if (GameInstanceManager.Instance == null)
            {
                var go = new GameObject("GameInstanceManager");
                go.AddComponent<GameInstanceManager>();
            }

            // Build player data with names (no DefaultPlayer required)
            var playerData = new List<(ulong, string)>();
            foreach (var clientId in players)
            {
                string playerName = null;
                if (container != null)
                {
                // (Option1) SessionContainer no longer stores per-player data; resolve name via NetworkPlayerResolver.
                }

                if (string.IsNullOrWhiteSpace(playerName))
                {
                    playerName = NetworkPlayerResolver.GetPlayerName(clientId);
                }

                playerData.Add((clientId, playerName));
                NetworkLogger.DebugLog("GameStart", $"Player {clientId}: {playerName}");
            }

            // Determine isolated world offset from container
            Vector3 worldOffset = Vector3.zero;
            if (container != null)
            {
                worldOffset = container.WorldOffset;
            }

            // Resolve the authoritative session UID (used by the replicated command protocol)
            var registryHub = GlobalRegistryHub.Instance ?? GlobalRegistryHub.EnsureInstance();
            string sessionUid = sessionName;
            if (registryHub != null)
            {
                var sessionEntry = registryHub.SessionRegistry.GetByName(sessionName);
                if (sessionEntry != null && !string.IsNullOrEmpty(sessionEntry.SessionUid))
                {
                    sessionUid = sessionEntry.SessionUid;
                }

                // Keep game type uid for UI/clients (optional)
                registryHub.GameRegisterTemplate.EnsureLoadedFromGameRegistry();
                var representation = registryHub.GameRegisterTemplate.GetByGameId(gameId);
                if (sessionEntry != null && representation != null)
                {
                    sessionEntry.GameTypeUid = representation.gameTypeUid;
                }
            }

            // Create the game instance (server authoritative sim world)
            if (!GameInstanceManager.Instance.CreateGame(sessionName, sessionUid, gameId, playerData, worldOffset))
            {
                NetworkLogger.Error("GameStart", $"Failed to create game for session '{sessionName}'");
                return;
            }

            container?.SetGameRunning();

            // Notify clients to load the Game scene
            if (!GameInstanceManager.Instance.TryGetWorldOffset(sessionName, out var clientOffset))
            {
                clientOffset = worldOffset;
            }

            Hub.StartGameClientRpc(sessionName, gameId, clientOffset, BuildClientRpcParams(players));

            // Send initial replicated snapshot (map config + all spawns)
            GameInstanceManager.Instance.SendFullSnapshotToClients(sessionName, players, includeMapConfig: true);

            // Fire event for other systems
            SessionRpcHub.InvokeGameStart(sessionName, new List<ulong>(players), null);
        }

        private void SendGameStartFailed(ulong clientId, string errorMessage, GameStartFailureReason reason)
        {
            if (Hub != null && Hub.IsSpawned)
            {
                Hub.SendGameStartFailedClientRpc(errorMessage, reason, BuildClientRpcParams(clientId));
            }
        }
    }

    /// <summary>
    /// Validator for game start conditions.
    /// </summary>
    public class GameStartValidator : BaseValidator
    {
        public override ValidationResult ValidateAccess(ulong clientId, string sessionName, string actionName)
        {
            return ValidateClientInSession(clientId, sessionName);
        }

        /// <summary>
        /// Comprehensive game start validation.
        /// </summary>
        public GameStartValidation ValidateGameStart(string sessionName, ulong clientId)
        {
            sessionName = sessionName?.Trim();
            if (string.IsNullOrEmpty(sessionName))
            {
                return GameStartValidation.Failure(
                    "Nom de session invalide",
                    GameStartFailureReason.SessionNotFound
                );
            }

            // Check if session exists
            var details = SessionManager.BuildDetails(sessionName);
            if (!details.HasValue)
            {
                return GameStartValidation.Failure(
                    $"La session '{sessionName}' n'existe plus",
                    GameStartFailureReason.SessionNotFound
                );
            }

            // Check if requester is host
            if (details.Value.session.creator != clientId)
            {
                return GameStartValidation.Failure(
                    "Seul l'hôte peut démarrer la partie",
                    GameStartFailureReason.NotSessionHost
                );
            }

            // Check players
            var players = SessionManager.GetPlayers(sessionName);
            if (players == null || players.Count == 0)
            {
                return GameStartValidation.Failure(
                    "Aucun joueur dans la session",
                    GameStartFailureReason.NotEnoughPlayers
                );
            }

            // Check game type
            string gameId = SessionManager.GetSelectedGameId(sessionName);
            if (string.IsNullOrEmpty(gameId))
            {
                gameId = "square-game";
            }

            var gameDef = GameRegistry.GetGame(gameId);
            if (gameDef == null)
            {
                return GameStartValidation.Failure(
                    $"Type de jeu invalide: {gameId}",
                    GameStartFailureReason.InvalidGameType
                );
            }

            // Check minimum players
            int minPlayers = Mathf.Max(1, gameDef.MinPlayers);
            if (details.Value.session.playerCount < minPlayers)
            {
                return GameStartValidation.Failure(
                    $"Pas assez de joueurs ({details.Value.session.playerCount}/{minPlayers} requis)",
                    GameStartFailureReason.NotEnoughPlayers
                );
            }

            // Check all players ready
            if (details.Value.session.readyCount < details.Value.session.playerCount)
            {
                return GameStartValidation.Failure(
                    $"Tous les joueurs doivent être prêts ({details.Value.session.readyCount}/{details.Value.session.playerCount})",
                    GameStartFailureReason.NotAllPlayersReady
                );
            }

            // All checks passed
            return GameStartValidation.Success();
        }
    }
}
