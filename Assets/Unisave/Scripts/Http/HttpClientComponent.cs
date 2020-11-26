using System;
using System.Collections.Generic;
using System.Net.Mime;
using LightJson;
using Unisave.Http.Client;
using UnityEngine;
using UnityEngine.Networking;

namespace Unisave.Http
{
    public class HttpClientComponent : MonoBehaviour
    {
        // TODO: display debug log, like SSE socket has

        /// <summary>
        /// Should the game object be destroyed after one performed request?
        /// (used for requests in edit mode)
        /// </summary>
        public bool DestroyImmediateAfterOneRequest { get; set; } = false;
        
        public void SendRequest(
            string method,
            string url,
            Dictionary<string, string> headers,
            JsonObject payload,
            Action<Response> callback
        )
        {
            void HandleRequestResponse(
                UnityWebRequest request,
                DownloadHandlerBuffer downloadHandler
            )
            {
                var contentType = request.GetResponseHeader("Content-Type")
                    ?? "text/plain";

                var response = Response.Create(
                    downloadHandler.text,
                    new ContentType(contentType).Name,
                    (int) request.responseCode
                );

                callback?.Invoke(response);

                if (DestroyImmediateAfterOneRequest)
                    DestroyImmediate(gameObject);
            }

            StartCoroutine(
                AssetHttpClient.TheRequestCoroutine(
                    method, url, headers, payload, HandleRequestResponse
                )
            );
        }
    }
}