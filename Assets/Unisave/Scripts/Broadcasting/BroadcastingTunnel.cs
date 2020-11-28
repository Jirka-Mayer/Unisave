using System;
using LightJson;
using Unisave.Broadcasting.Sse;
using Unisave.Foundation;
using Unisave.Serialization;
using UnityEngine;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// The tunnel that transports events from the server to the client
    /// (all the channels combined with all the metadata)
    /// </summary>
    public class BroadcastingTunnel : IDisposable
    {
        /// <summary>
        /// Called when a message event arrives through the SSE tunnel
        /// </summary>
        public event Action<JsonObject> OnMessageEvent;
        
        /// <summary>
        /// Called when a subscription event arrives through the SSE tunnel
        /// </summary>
        public event Action<JsonObject> OnSubscriptionEvent;

        private ClientApplication app;

        /// <summary>
        /// The underlying SSE socket. Can be null when not needed.
        /// </summary>
        public SseSocket Socket { get; set; }

        public BroadcastingConnection ConnectionState
            => Socket == null
                ? BroadcastingConnection.Disconnected
                : Socket.connectionState;

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
            if (Socket == null)
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
            if (Socket != null)
                throw new InvalidOperationException("Socket already created");

            GameObject go = new GameObject(
                "UnisaveBroadcastingSseSocket",
                typeof(SseSocket)
            );
            UnityEngine.Object.DontDestroyOnLoad(go);

            if (!app.InEditMode)
                go.transform.parent = app.GameObject.transform;
            
            Socket = go.GetComponent<SseSocket>();
            Socket.Initialize(app);
            Socket.OnEventReceived += OnEventReceived;
        }

        private void DisposeSocket()
        {
            if (Socket == null)
                return;
            
            Socket.OnEventReceived -= OnEventReceived;
            
            UnityEngine.Object.Destroy(Socket.gameObject);
            Socket = null;
        }

        private void OnEventReceived(SseEvent @event)
        {
            switch (@event.@event)
            {
                case "message":
                    OnMessageEvent?.Invoke(@event.jsonData);
                    break;
                
                case "subscription":
                    OnSubscriptionEvent?.Invoke(@event.jsonData);
                    break;
                
                case "welcome":
                    // do nothing
                    break;
                
                default:
                    Debug.LogWarning(
                        "[Unisave] Unknown broadcasting event received: " +
                        @event.@event
                    );
                    break;
            }
        }

        public void Dispose()
        {
            DisposeSocket();
        }
    }
}