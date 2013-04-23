﻿// <copyright file="OwinMiddlewareTransition.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Infrastructure
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public sealed class OwinMiddlewareTransition
    {
        private readonly OwinMiddleware _next;

        public OwinMiddlewareTransition(OwinMiddleware next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            return _next.Invoke(new OwinRequest(environment), new OwinResponse(environment));
        }
    }
}
