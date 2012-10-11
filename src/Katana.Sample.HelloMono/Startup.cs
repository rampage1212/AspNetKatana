// <copyright file="Startup.cs" company="Katana contributors">
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
using System.Threading.Tasks;
using Gate;
using Gate.Middleware;
using Owin;

namespace Katana.Sample.HelloMono
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            // trace all requests
            builder.UseFunc(LogRequests);

            // serve root path with Index.html file
            builder.UseFunc(ReplacePath("/", "/Index.html"));

            // try to show debug info when unhandled exceptions are thrown
            builder.UseShowExceptions();

            // invoke wilson on any request starting with wilson
            builder.Map("/wilson", new Wilson());

            // invoke this class's Invoke on requests starting with /hello
            builder.Map("/hello", this);

            // all other requests will pass through to aspnet if katana is run
            // with the "--boot aspnet" command-line option... otherwise they
            // get a default empty 404 response
        }

        public Func<AppFunc, AppFunc> ReplacePath(string match, string replacement)
        {
            return next => env =>
            {
                var req = new Request(env);
                if (req.Path == match)
                {
                    req.Path = replacement;
                }
                return next(env);
            };
        }

        public AppFunc LogRequests(AppFunc next)
        {
            return env =>
            {
                var req = new Request(env);
                req.TraceOutput.WriteLine("Request {0} at {1}{2} {3}", req.Method, req.PathBase, req.Path, req.QueryString);
                return next(env);
            };
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var resp = new Response(env) { ContentType = "text/plain" };
            return resp.WriteAsync("Hello, mono");
        }
    }
}
