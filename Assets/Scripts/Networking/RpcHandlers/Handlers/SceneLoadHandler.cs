using Core.StateSync;
using Networking.StateSync;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.RpcHandlers
{
    /// <summary>
    /// Handles scene loading and initialization for game sessions.
    /// Extracted from SessionRpcHub to reduce complexity.
    /// </summary>
    public class SceneLoadHandler : BaseRpcHandler
    {
        private enum ClientGameFlowState
        {
            Idle,
            LoadingScene,
            InitializingSystems,
            Running
        }

        private StateMachine<ClientGameFlowState> clientFlow;
        private string activeClientGameId;
        private string activeClientSessionName;
        private IGameDefinition activeClientGameDefinition;

        public override string GetHandlerName() => "SceneLoadHandler";

        protected override void OnInitialize()
        {
            clientFlow = new StateMachine<ClientGameFlowState>(ClientGameFlowState.Idle);
            clientFlow.OnStateChanged += (from, to) =>
            {
                Log($"ClientFlow: {from} -> {to} (gameId='{activeClientGameId ?? "?"}')");
            };
        }

        // ============================================================
        // LOAD GAME SCENE
        // ============================================================

        /// <summary>
        /// Load Game.unity scene additively and initialize game systems.
        /// </summary>
        public IEnumerator LoadGameSceneAndInitialize(string sessionName, string gameId, Vector3 worldOffset)
        {
            clientFlow?.TransitionTo(ClientGameFlowState.LoadingScene);

            Log($"Loading Game scene for session '{sessionName}'...");
            Scene previousScene = SceneManager.GetActiveScene();

            // Load Game scene additively
            var asyncLoad = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
            if (asyncLoad == null)
            {
                LogError("Failed to start loading Game scene!");
                yield break;
            }

            // Wait for scene to load
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            Log("Game scene loaded successfully");

            // Make Game the active scene and unload the previous lobby/menu scene to avoid the blue background camera.
            var gameScene = SceneManager.GetSceneByName(SceneNames.Game);
            if (gameScene.IsValid())
            {
                SceneManager.SetActiveScene(gameScene);
            }

            // Unload previous scene if it was the menu or client lobby.
            if (previousScene.IsValid() && previousScene.name != SceneNames.Game)
            {
                SceneManager.UnloadSceneAsync(previousScene);
            }
            // If Menu is still loaded separately, unload it too.
            var menuScene = SceneManager.GetSceneByName(SceneNames.Menu);
            if (menuScene.IsValid() && menuScene.isLoaded && menuScene.name != gameScene.name)
            {
                SceneManager.UnloadSceneAsync(menuScene);
            }

            // Initialize game systems
            clientFlow?.TransitionTo(ClientGameFlowState.InitializingSystems);
            InitializeGameSystems(sessionName, gameId, worldOffset);
            clientFlow?.TransitionTo(ClientGameFlowState.Running);
        }

        // ============================================================
        // LATE JOINER
        // ============================================================

        /// <summary>
        /// Setup game visuals for a late joiner.
        /// </summary>
        public void HandleLateJoiner(string sessionName, string gameId, Vector3 worldOffset, ulong targetClientId)
        {
            // Only process if this is for us
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClientId != targetClientId)
                return;

            Log($"Late joiner setup for game '{gameId}' in session '{sessionName}'");

            InitializeGameSystems(sessionName, gameId, worldOffset);

            // Fire event for UI to hide lobby
            SessionRpcHub.InvokeGameStart(sessionName, new System.Collections.Generic.List<ulong>(), null);
        }

        // ============================================================
        // PRIVATE METHODS
        // ============================================================

        private void InitializeGameSystems(string sessionName, string gameId, Vector3 worldOffset)
        {
            // Remember session for potential resync requests.
            activeClientSessionName = sessionName;
        
            // Ensure core singletons exist on the client.
            GameCommandClient.EnsureInstance();
            MapConfigSceneBuilder.EnsureInstance();
            EntityViewWorld.EnsureInstance();
        
            // Initialize game registry if needed.
            GameRegistry.Initialize();
        
            // Resolve the game definition (mode)
            var gameDef = GameRegistry.GetGame(gameId);
            if (gameDef != null)
            {
                if (activeClientGameDefinition != null && !ReferenceEquals(activeClientGameDefinition, gameDef))
                {
                    activeClientGameDefinition.CleanupGame();
                }
        
                activeClientGameDefinition = gameDef;
                activeClientGameId = gameId;
        
                // Client visuals are primarily driven by the replicated MapConfig command.
                // If a game wants to add extra visuals, it can do so once the MapConfig is applied.
                var config = GameCommandClient.Instance != null ? GameCommandClient.Instance.CurrentConfig : null;
                if (config != null)
                {
                    try
                    {
                        gameDef.SetupClientVisuals(config);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to setup client visuals for game '{gameId}': {ex.Message}");
                    }
                }
                else
                {
                    // One-shot subscribe; unsubscribes after first config arrives.
                    GameCommandClient.OnMapConfigApplied -= OnMapConfigApplied;
                    GameCommandClient.OnMapConfigApplied += OnMapConfigApplied;
                }
            }
            else
            {
                LogWarning($"Game '{gameId}' not found; client visuals not initialized");
            }
        
            // Store session info for input handling
            PlayerInputHandler.EnsureInstance();
            PlayerInputHandler.Instance.SetSession(sessionName);
        
            // If we haven't received the config within a short delay, request a resync.
            SceneLoadRunner.Run(RequestResyncIfMissing());
        
            // Initialize debug UI
            GameDebugUI.EnsureInstance();
            if (GameDebugUI.Instance != null)
            {
                GameDebugUI.Instance.SetSession(sessionName);
                if (NetworkManager.Singleton != null)
                    GameDebugUI.Instance.SetClientId(NetworkManager.Singleton.LocalClientId);
        
                // Keep debug overlay hidden by default so it doesn't cover gameplay.
                GameDebugUI.Instance.Hide();
            }
        
            Log($"Game systems initialized for session '{sessionName}'");
        }

        // (Visibility filtering and Netcode pawn migration removed in DTO-driven (Option 1) refactor.)


private void OnMapConfigApplied(MapConfigData config)
{
    // One-shot subscription; remove immediately.
    GameCommandClient.OnMapConfigApplied -= OnMapConfigApplied;

    if (activeClientGameDefinition == null)
        return;

    try
    {
        activeClientGameDefinition.SetupClientVisuals(config);
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to setup client visuals for game '{activeClientGameId}': {ex.Message}");
    }
}

        private IEnumerator RequestResyncIfMissing()
        {
            yield return new WaitForSeconds(0.5f);

            if (GameCommandClient.Instance == null)
            {
                yield break;
            }

            if (GameCommandClient.Instance.CurrentConfig != null)
            {
                yield break;
            }

            GameCommandClient.Instance.RequestResyncNow(activeClientSessionName, "MapConfig missing after scene load");
        }
    }
}
