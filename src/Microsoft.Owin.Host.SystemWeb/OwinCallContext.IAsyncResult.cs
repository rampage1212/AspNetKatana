// <copyright file="OwinCallContext.IAsyncResult.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext : IAsyncResult
    {
        private static readonly AsyncCallback NoopAsyncCallback =
            ar => { };

        private static readonly AsyncCallback ExtraAsyncCallback =
            ar => Trace.WriteLine("OwinHttpHandler: more than one call to complete the same AsyncResult");

        private readonly TaskCompletionSource<Nada> _taskCompletionSource = new TaskCompletionSource<Nada>();

        private AsyncCallback _cb;
        private Exception _exception;

        public bool IsCompleted { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get { throw new InvalidOperationException(Resources.Exception_BlockingNotAllowed); }
        }

        public object AsyncState { get; private set; }

        public bool CompletedSynchronously { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Users callback must not throw")]
        public void Complete(bool completedSynchronously, Exception exception)
        {
            _exception = exception;

            if (exception == null)
            {
                _taskCompletionSource.TrySetResult(default(Nada));
            }
            else
            {
                Trace.WriteLine(Resources.Exception_RequestComplete);
                Trace.WriteLine(exception);
                _taskCompletionSource.TrySetException(exception);
            }

            CompletedSynchronously = completedSynchronously;

            IsCompleted = true;
            try
            {
                Interlocked.Exchange(ref _cb, ExtraAsyncCallback).Invoke(this);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(Resources.Exception_OwinCallContextCallbackThrew);
                Trace.WriteLine(ex.ToString());
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "False positive")]
        public static void End(IAsyncResult result)
        {
            if (!(result is OwinCallContext))
            {
                // "EndProcessRequest must be called with return value of BeginProcessRequest"
                throw new InvalidOperationException();
            }
            var self = ((OwinCallContext)result);
            self.Dispose();
            if (self._exception != null)
            {
                throw new TargetInvocationException(self._exception);
            }
            if (!self.IsCompleted)
            {
                // Calling EndProcessRequest before IsComplete is true is not allowed
                throw new InvalidOperationException();
            }
        }

        private struct Nada
        {
        }
    }
}
