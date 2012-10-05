﻿// Copyright 2011-2012 Katana contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.HttpListener.Owin
{
    internal class DisconnectHandler
    {
        private readonly ConcurrentDictionary<ulong, Lazy<CancellationToken>> _connectionCancellationTokens;
        private readonly System.Net.HttpListener _listener;
        private CriticalHandle _requestQueueHandle;
        private FieldInfo _connectionIdField;

        /// <summary>
        /// Initializes a new instance of <see cref="DisconnectHandler"/>.
        /// </summary>
        /// <param name="listener">The <see cref="Server"/>'s HttpListener</param>
        internal DisconnectHandler(System.Net.HttpListener listener)
        {
            _connectionCancellationTokens = new ConcurrentDictionary<ulong, Lazy<CancellationToken>>();
            _listener = listener;
        }

        /// <summary>
        /// Initializes the Request Queue Handler.  Meant to be called once the servers <see cref="HttpListener"/> has been started.
        /// </summary>
        internal void Initialize()
        {
            // Get the request queue handle so we can register for disconnect
            var requestQueueHandleField = typeof(System.Net.HttpListener).GetField("m_RequestQueueHandle", BindingFlags.Instance | BindingFlags.NonPublic);

            // Get the connection id field info from the request object
            _connectionIdField = typeof(HttpListenerRequest).GetField("m_ConnectionId", BindingFlags.Instance | BindingFlags.NonPublic);

            if (requestQueueHandleField != null)
            {
                _requestQueueHandle = (CriticalHandle)requestQueueHandleField.GetValue(_listener);
            }
        }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> associated with the <paramref name="context"/>.  
        /// If the <see cref="CancellationToken"/> does not exist for the given <paramref name="context"/> then <see cref="CreateToken"/> is called.
        /// </summary>
        /// <param name="context">The context for the current connection.</param>
        /// <returns>A cancellation token that is registered for disconnect for the current connection.</returns>
        internal CancellationToken GetDisconnectToken(HttpListenerContext context)
        {
            if (_connectionIdField == null || _requestQueueHandle == null)
            {
                Debug.WriteLine("Server: Unable to resolve requestQueue handle. Disconnect notifications will be ignored");
                return CancellationToken.None;
            }

            var connectionId = (ulong)_connectionIdField.GetValue(context.Request);
            return _connectionCancellationTokens.GetOrAdd(connectionId, key => new Lazy<CancellationToken>(() => CreateToken(key))).Value;
        }

        private unsafe CancellationToken CreateToken(ulong connectionId)
        {
            Debug.WriteLine("Server: Registering connection for disconnect for connection ID: " + connectionId);

            // Create a nativeOverlapped callback so we can register for disconnect callback
            var overlapped = new Overlapped();
            var cts = new CancellationTokenSource();

            var nativeOverlapped = overlapped.UnsafePack(
                (errorCode, numBytes, overlappedPtr) =>
                {
                    Debug.WriteLine("Server: http.sys disconnect callback fired for connection ID: " + connectionId);

                    // Free the overlapped
                    Overlapped.Free(overlappedPtr);

                    // Pull the token out of the list and Cancel it.
                    Lazy<CancellationToken> token;
                    _connectionCancellationTokens.TryRemove(connectionId, out token);
                    try
                    {
                        cts.Cancel();
                    }
                    catch (AggregateException)
                    {
                    }

                    cts.Dispose();
                },
                null);

            uint hr = NativeMethods.HttpWaitForDisconnect(_requestQueueHandle, connectionId, nativeOverlapped);

            if (hr != NativeMethods.HttpErrors.ERROR_IO_PENDING &&
                hr != NativeMethods.HttpErrors.NO_ERROR)
            {
                // We got an unknown result so return a None
                Debug.WriteLine("Unable to register disconnect callback: " + hr);
                return CancellationToken.None;
            }

            return cts.Token;
        }
    }
}
