using System;
using LightJson;
using Unisave.Broadcasting.Sse;
using Unisave.Foundation;
using Unisave.Serialization;
using UnityEngine;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// The tunnel that transports messages from the server to the client
    /// (all the channels combined with all the metadata)
    /// </summary>
    public class BroadcastingTunnel : IDisposable
    {
        /// <summary>
        /// Called when a new event arrives through the tunnel
        /// </summary>
        public event Action<JsonObject> OnEventReceived;

        private ClientApplication app;

        private SseSocket sseSocket;

        private int lastReceivedMessageId = 0;

        public BroadcastingConnection ConnectionState
            => sseSocket == null
                ? BroadcastingConnection.Disconnected
                : sseSocket.connectionState;

        public BroadcastingTunnel(ClientApplication app)
        {
            this.app = app;
        }

        /// <summary>
        /// Called just before the tunnel becomes needed
        /// (idempotent)
        /// </summary>
        public void IsNeeded()
        {
            if (sseSocket == null)
                CreateSocket();
        }

        /// <summary>
        /// Called right after the tunnel stops being needed
        /// (idempotent)
        /// </summary>
        public void IsNotNeeded()
        {
            DisposeSocket();
        }

        private void CreateSocket()
        {
            if (sseSocket != null)
                throw new InvalidOperationException("Socket already created");

            GameObject go = new GameObject(
                "UnisaveBroadcastingSseSocket",
                typeof(SseSocket)
            );
            UnityEngine.Object.DontDestroyOnLoad(go);

            if (!app.InEditMode)
                go.transform.parent = app.GameObject.transform;
            
            sseSocket = go.GetComponent<SseSocket>();
            sseSocket.Initialize(app, lastReceivedMessageId);
            sseSocket.OnMessageReceived += OnSseMessageReceived;
        }

        private void DisposeSocket()
        {
            if (sseSocket == null)
                return;
            
            sseSocket.OnMessageReceived -= OnSseMessageReceived;
            
            UnityEngine.Object.Destroy(sseSocket.gameObject);
            sseSocket = null;
        }

        private void OnSseMessageReceived(SseMessage message)
        {
            if (message.id != null)
                lastReceivedMessageId = (int) message.id;
            
            // TODO: dummy implementation
            
            var m = new Unisave.Examples.ChatDoodle.Backend.ChatMessage {
                message = message.data,
                nickname = "NOPE"
            };
            
            OnEventReceived?.Invoke(new JsonObject {
                ["type"] = "message",
                ["message"] = Serializer.ToJson<BroadcastingMessage>(m)
            });
        }

        public void Dispose()
        {
            DisposeSocket();
        }
    }
}