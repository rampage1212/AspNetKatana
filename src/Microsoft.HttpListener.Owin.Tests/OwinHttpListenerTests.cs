﻿//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.HttpListener.Owin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using System.IO;

    // TODO: Convert to XUnit?
    // These tests measure that the core HttpListener wrapper functions as expected in normal and exceptional scenarios.
    // NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    [TestClass]
    public class OwinHttpListenerTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http://*:8080/BaseAddress/" };
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private static readonly string[] HttpsServerAddress = new string[] { "https://*:9090/BaseAddress/" };
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        private AppFunc notImplemented = env => { throw new NotImplementedException(); };

        [TestMethod]
        public void OwinHttpListener_CreatedStartedStoppedDisposed_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress, null);
            using (listener)
            {
                listener.Start();
                listener.Stop();
            }
        }

        // HTTPS requires pre-configuring the server cert to work
        [TestMethod]
        public void OwinHttpListener_HttpsCreatedStartedStoppedDisposed_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpsServerAddress, null);
            using (listener)
            {
                listener.Start();
                listener.Stop();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullDelegate_Throws()
        {
            OwinHttpListener listener = new OwinHttpListener(null, HttpServerAddress, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_BadServerAddress_Throws()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, new string[] { "http://host:9090/BadPathDoesntEndInSlash" }, null);
        }

        [TestMethod]
        public async Task EndToEnd_GetRequest_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(env => TaskHelpers.Completed(), HttpServerAddress, null);
            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task EndToEnd_SingleThreadedTwoGetRequests_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(env => TaskHelpers.Completed(), HttpServerAddress, null);
            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                string result = await client.GetStringAsync(HttpClientAddress);
                Assert.AreEqual(string.Empty, result);
                result = await client.GetStringAsync(HttpClientAddress);
                Assert.AreEqual(string.Empty, result);
            }
        }

        [TestMethod]
        public async Task EndToEnd_GetRequestWithDispose_Success()
        {
            bool callCancelled = false;

            OwinHttpListener listener = new OwinHttpListener(
                env => 
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
            await Task.Delay(1);
            Assert.IsFalse(callCancelled);
        }

        [TestMethod]
        public async Task EndToEnd_HttpsGetRequest_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env => 
                {
                    object obj;
                    Assert.IsTrue(env.TryGetValue("ssl.ClientCertificate", out obj));
                    Assert.IsNotNull(obj);
                    Assert.IsInstanceOfType(obj, typeof(X509Certificate2));
                    return TaskHelpers.Completed();
                },
                HttpsServerAddress, null);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Automatic);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task EndToEnd_HttpsGetRequestNoClientCert_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    object obj;
                    Assert.IsFalse(env.TryGetValue("owin.ClientCertificate", out obj));
                    return TaskHelpers.Completed();
                },
                HttpsServerAddress, null);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Manual);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task AppDelegate_ThrowsSync_500Error()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress, null);
            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task AppDelegate_ReturnsExceptionAsync_500Error()
        {
            bool callCancelled = false;

            OwinHttpListener listener = new OwinHttpListener(
                async env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    await Task.Delay(1);
                    throw new NotImplementedException();
                },
                HttpServerAddress, null);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
            Assert.IsTrue(callCancelled);
        }

        [TestMethod]
        public async Task Body_PostEchoRequest_Success()
        {
            bool callCancelled = false;

            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", requestHeaders["Content-Length"]);

                    Stream requestStream = env.Get<Stream>("owin.RequestBody");
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");

                    return requestStream.CopyToAsync(responseStream, 1024);
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                string dataString = "Hello World";
                HttpResponseMessage result = await client.PostAsync(HttpClientAddress, new StringContent(dataString));
                result.EnsureSuccessStatusCode();
                Assert.AreEqual(dataString.Length, result.Content.Headers.ContentLength.Value);
                Assert.AreEqual(dataString, await result.Content.ReadAsStringAsync());
                Assert.IsFalse(callCancelled);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task BodyDelegate_ThrowsSync_ConnectionClosed()
        {
            bool callCancelled = false;
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "10" });

                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.WriteByte(0xFF);

                    throw new NotImplementedException();
                },
                HttpServerAddress, null);

            try
            {
                await this.SendGetRequest(listener, HttpClientAddress);
            }
            finally
            {
                Assert.IsTrue(callCancelled);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task BodyDelegate_ThrowsAsync_ConnectionClosed()
        {
            bool callCancelled = false;

            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "10" });

                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.WriteByte(0xFF);

                    return TaskHelpers.FromError(new NotImplementedException());
                },
                HttpServerAddress, null);
            
            try
            {
                await this.SendGetRequest(listener, HttpClientAddress);
            }
            finally
            {
                Assert.IsTrue(callCancelled);
            }
        }

        [TestMethod]
        public void TimeoutArgs_Default_Infinite()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress, null);
            Assert.AreEqual(Timeout.InfiniteTimeSpan, listener.MaxRequestLifetime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TimeoutArgs_Negative_Throws()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress, null);
            listener.MaxRequestLifetime = TimeSpan.FromSeconds(-1);
        }

        [TestMethod]
        public void TimeoutArgs_Infiniate_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress, null);
            listener.MaxRequestLifetime = Timeout.InfiniteTimeSpan;
            Assert.AreEqual(Timeout.InfiniteTimeSpan, listener.MaxRequestLifetime);
        }

        [TestMethod]
        public void TimeoutArgs_Huge_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress, null);
            listener.MaxRequestLifetime = TimeSpan.FromSeconds(int.MaxValue);
            Assert.AreEqual(int.MaxValue, listener.MaxRequestLifetime.TotalSeconds);
        }

        [TestMethod]
        public async Task Timeout_GetRequestWithinTimeout_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(env => TaskHelpers.Completed(), HttpServerAddress, null);
            listener.MaxRequestLifetime = TimeSpan.FromSeconds(1);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            response.EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task Timeout_GetRequestTimeoutDurringRequest_500Error()
        {
            OwinHttpListener listener = new OwinHttpListener(
                async env =>
                {
                    await Task.Delay(100);
                },
                HttpServerAddress, null);
            listener.MaxRequestLifetime = TimeSpan.FromMilliseconds(1);

            HttpResponseMessage result = await this.SendGetRequest(listener, HttpClientAddress);            
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreEqual(0, result.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task Timeout_GetRequestTimeoutDurringResponse_ConnectionClose()
        {
            OwinHttpListener listener = new OwinHttpListener(
                async env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "10" });

                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");

                    await Task.Delay(1000);
                    await responseStream.WriteAsync(new byte[10], 0, 10);
                },
                HttpServerAddress, null);

            listener.MaxRequestLifetime = TimeSpan.FromMilliseconds(1);
            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private static CancellationToken GetCallCancelled(IDictionary<string, object> env)
        {
            return env.Get<CancellationToken>("owin.CallCancelled");
        }

        private Task<HttpResponseMessage> SendGetRequest(OwinHttpListener listener, string address)
        {
            return this.SendGetRequest(listener, address, ClientCertificateOption.Automatic);
        }

        private async Task<HttpResponseMessage> SendGetRequest(OwinHttpListener listener, string address, ClientCertificateOption certOptions)
        {
            using (listener)
            {
                listener.Start(1);

                WebRequestHandler handler = new WebRequestHandler();

                // Ignore server cert errors.
                handler.ServerCertificateValidationCallback = (a, b, c, d) => true;
                handler.ClientCertificateOptions = certOptions;

                HttpClient client = new HttpClient(handler);
                return await client.GetAsync(address);
            }
        }
    }
}
