﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Katana.Server.AspNet.CallEnvironment;
using Katana.Server.AspNet.CallHeaders;
using Owin;
using System.Threading.Tasks;

namespace Katana.Server.AspNet
{
    public partial class OwinCallContext
    {
        private HttpContextBase _httpContext;
        private HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private int _completedSynchronouslyThreadId;

        public void Execute(RequestContext requestContext, string requestPathBase, string requestPath, AppDelegate app)
        {
            _httpContext = requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;

            var requestQueryString = String.Empty;
            if (_httpRequest.Url != null)
            {
                var query = _httpRequest.Url.Query;
                if (query.Length > 1)
                {
                    // pass along the query string without the leading "?" character
                    requestQueryString = query.Substring(1);
                }
            }

            CallParameters call = new CallParameters();
            call.Body = _httpRequest.InputStream;
            call.Headers = AspNetRequestHeaders.Create(_httpRequest);
            call.Environment = new AspNetDictionary
            {
                OwinVersion = "1.0",
                HttpVersion = _httpRequest.ServerVariables["SERVER_PROTOCOL"],
                RequestScheme = _httpRequest.IsSecureConnection ? "https" : "http",
                RequestMethod = _httpRequest.HttpMethod,
                RequestPathBase = requestPathBase,
                RequestPath = requestPath,
                RequestQueryString = requestQueryString,

                ServerVariableLocalAddr = _httpRequest.ServerVariables["LOCAL_ADDR"],
                ServerVariableRemoteAddr = _httpRequest.ServerVariables["REMOTE_ADDR"],
                ServerVariableRemoteHost = _httpRequest.ServerVariables["REMOTE_HOST"],
                ServerVariableRemotePort = _httpRequest.ServerVariables["REMOTE_PORT"],
                ServerVariableServerPort = _httpRequest.ServerVariables["SERVER_PORT"],

                HostCallDisposed = CallDisposed,
                HostDisableResponseBuffering = DisableResponseBuffering,
                HostUser = _httpContext.User,

                RequestContext = requestContext,
                HttpContextBase = _httpContext,
            };

            _completedSynchronouslyThreadId = Int32.MinValue;
            app.Invoke(call)
                .Then(result => OnResult(result))
                .Catch(errorInfo =>
                {
                    OnFault(errorInfo.Exception);
                    return errorInfo.Handled();
                });
            _completedSynchronouslyThreadId = Int32.MinValue;
        }

        private void OnResult(ResultParameters result)
        {
            _httpResponse.StatusCode = result.Status;
            // TODO: Reason Phrase
            foreach (var header in result.Headers)
            {
                foreach (var value in header.Value)
                {
                    _httpResponse.AddHeader(header.Key, value);
                }
            }

            if (result.Body != null)
            {
                try
                {
                    result.Body(_httpResponse.OutputStream, CallDisposed)
                        .Then(() => OnEnd(null))
                        .Catch(errorInfo =>
                        {
                            OnFault(errorInfo.Exception);
                            return errorInfo.Handled();
                        });
                }
                catch (Exception ex)
                {
                    OnFault(ex);
                }
            }
            else
            {
                OnEnd(null);
            }
        }

        private void OnFault(Exception ex)
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, ex);
        }

        private void OnEnd(Exception ex)
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, ex);
        }
    }
}
