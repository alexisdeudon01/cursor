using System;
using System.Collections.Generic;
using UnityEngine;

namespace CursorServer
{
    /// <summary>
    /// Manages data-oriented operations with full authorization
    /// Handles all server-side data processing and synchronization
    /// </summary>
    public class AuthorizedDataManager : MonoBehaviour
    {
        private static AuthorizedDataManager instance;
        public static AuthorizedDataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AuthorizedDataManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AuthorizedDataManager");
                        instance = go.AddComponent<AuthorizedDataManager>();
                    }
                }
                return instance;
            }
        }

        [SerializeField] private ServerConfig serverConfig;

        private Dictionary<string, ServerData> serverDataStore = new Dictionary<string, ServerData>();
        private Queue<DataOperation> pendingOperations = new Queue<DataOperation>();
        private bool isProcessing = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (serverConfig != null && serverConfig.EnableDataOrientedProcessing)
            {
                InvokeRepeating(nameof(ProcessDataOperations), 0f, serverConfig.DataSyncIntervalMs / 1000f);
            }
        }

        public bool StoreData(string clientId, string key, object data)
        {
            if (!ServerAuthManager.Instance.IsAuthorized(clientId))
            {
                Debug.LogWarning($"Unauthorized data store attempt by client {clientId}");
                return false;
            }

            ServerData serverData = new ServerData
            {
                Key = key,
                Data = data,
                ClientId = clientId,
                Timestamp = DateTime.UtcNow,
                IsAuthorized = true
            };

            serverDataStore[key] = serverData;
            Debug.Log($"Data stored for key {key} by authorized client {clientId}");
            return true;
        }

        public object RetrieveData(string clientId, string key)
        {
            if (!ServerAuthManager.Instance.IsAuthorized(clientId))
            {
                Debug.LogWarning($"Unauthorized data retrieval attempt by client {clientId}");
                return null;
            }

            if (serverDataStore.TryGetValue(key, out ServerData data))
            {
                Debug.Log($"Data retrieved for key {key} by authorized client {clientId}");
                return data.Data;
            }

            return null;
        }

        public bool DeleteData(string clientId, string key)
        {
            if (!ServerAuthManager.Instance.IsAuthorized(clientId))
            {
                Debug.LogWarning($"Unauthorized data deletion attempt by client {clientId}");
                return false;
            }

            if (serverDataStore.Remove(key))
            {
                Debug.Log($"Data deleted for key {key} by authorized client {clientId}");
                return true;
            }

            return false;
        }

        public void QueueDataOperation(DataOperation operation)
        {
            if (ServerAuthManager.Instance.IsAuthorized(operation.ClientId))
            {
                pendingOperations.Enqueue(operation);
            }
        }

        private void ProcessDataOperations()
        {
            if (isProcessing || pendingOperations.Count == 0)
                return;

            isProcessing = true;

            int operationsToProcess = Mathf.Min(pendingOperations.Count, 10);
            for (int i = 0; i < operationsToProcess; i++)
            {
                DataOperation operation = pendingOperations.Dequeue();
                ExecuteOperation(operation);
            }

            isProcessing = false;
        }

        private void ExecuteOperation(DataOperation operation)
        {
            switch (operation.Type)
            {
                case OperationType.Store:
                    StoreData(operation.ClientId, operation.Key, operation.Data);
                    break;
                case OperationType.Retrieve:
                    RetrieveData(operation.ClientId, operation.Key);
                    break;
                case OperationType.Delete:
                    DeleteData(operation.ClientId, operation.Key);
                    break;
            }
        }

        [Serializable]
        public class ServerData
        {
            public string Key;
            public object Data;
            public string ClientId;
            public DateTime Timestamp;
            public bool IsAuthorized;
        }

        [Serializable]
        public class DataOperation
        {
            public OperationType Type;
            public string ClientId;
            public string Key;
            public object Data;
        }

        public enum OperationType
        {
            Store,
            Retrieve,
            Delete
        }
    }
}
