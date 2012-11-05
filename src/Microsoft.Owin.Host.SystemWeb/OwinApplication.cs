﻿// <copyright file="OwinApplication.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class OwinApplication
    {
        private static Lazy<Func<IDictionary<string, object>, Task>> _instance = new Lazy<Func<IDictionary<string, object>, Task>>(OwinBuilder.Build);
        private static ShutdownDetector _detector;

        internal static Func<IDictionary<string, object>, Task> Instance
        {
            get { return _instance.Value; }
            set { _instance = new Lazy<Func<IDictionary<string, object>, Task>>(() => value); }
        }

        internal static Func<Func<IDictionary<string, object>, Task>> Accessor
        {
            get { return () => _instance.Value; }
            set { _instance = new Lazy<Func<IDictionary<string, object>, Task>>(value); }
        }

        internal static CancellationToken ShutdownToken
        {
            get { return LazyInitializer.EnsureInitialized(ref _detector, InitShutdownDetector).Token; }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Only cleaned up on shutdown")]
        private static ShutdownDetector InitShutdownDetector()
        {
            var detector = new ShutdownDetector();
            detector.Initialize();
            return detector;
        }
    }
}
