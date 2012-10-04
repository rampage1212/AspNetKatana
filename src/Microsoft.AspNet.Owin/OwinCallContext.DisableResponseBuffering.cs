//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace Microsoft.AspNet.Owin
{
    public partial class OwinCallContext 
    {
        private const string IIS7WorkerRequestTypeName = "System.Web.Hosting.IIS7WorkerRequest";
        private static readonly Lazy<RemoveHeaderDel> IIS7RemoveHeader = new Lazy<RemoveHeaderDel>(GetRemoveHeaderDelegate);

        private bool _bufferingDisabled;

        private delegate void RemoveHeaderDel(HttpWorkerRequest workerRequest);

        private void DisableResponseBuffering()
        {
            if (_bufferingDisabled)
            {
                return;
            }

            // This forces the IIS compression module to leave this response alone.
            // If we don't do this, it will buffer the response to suit its own compression
            // logic, resulting in partial messages being sent to the client.
            RemoveAcceptEncoding();

            _httpResponse.CacheControl = "no-cache";
            _httpResponse.AddHeader("Connection", "keep-alive");

            _bufferingDisabled = true;
        }

        private void RemoveAcceptEncoding()
        {
            try
            {
                var workerRequest = (HttpWorkerRequest)_httpContext.GetService(typeof(HttpWorkerRequest));
                if (IsIIS7WorkerRequest(workerRequest))
                {
                    // Optimized code path for IIS7, accessing Headers causes all headers to be read
                    IIS7RemoveHeader.Value.Invoke(workerRequest);
                }
                else
                {
                    try
                    {
                        _httpRequest.Headers.Remove("Accept-Encoding");
                    }
                    catch (PlatformNotSupportedException)
                    {
                        // Happens on cassini
                    }
                }
            }
            catch (NotImplementedException)
            {
            }
        }

        private static bool IsIIS7WorkerRequest(HttpWorkerRequest workerRequest)
        {
            return workerRequest != null && workerRequest.GetType().FullName == IIS7WorkerRequestTypeName;
        }

        private static RemoveHeaderDel GetRemoveHeaderDelegate()
        {
            var iis7workerType = typeof(HttpContext).Assembly.GetType(IIS7WorkerRequestTypeName);
            var methodInfo = iis7workerType.GetMethod("SetKnownRequestHeader", BindingFlags.NonPublic | BindingFlags.Instance);

            var workerParamExpr = Expression.Parameter(typeof(HttpWorkerRequest));
            var iis7workerParamExpr = Expression.Convert(workerParamExpr, iis7workerType);
            var callExpr = Expression.Call(iis7workerParamExpr, methodInfo, 
                Expression.Constant(HttpWorkerRequest.HeaderAcceptEncoding), 
                Expression.Constant(null, typeof(string)), Expression.Constant(false));
            return Expression.Lambda<RemoveHeaderDel>(callExpr, workerParamExpr).Compile();
        }
    }
}