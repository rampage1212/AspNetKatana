// <copyright file="DefaultAppActivator.cs" company="Katana contributors">
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
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Builder
{
    public class DefaultAppActivator : IAppActivator
    {
        private readonly IServiceProvider _services;

        public DefaultAppActivator(IServiceProvider services)
        {
            _services = services;
        }

        public object Activate(Type type)
        {
            try
            {
                object starter = _services.GetService(type);
                if (starter != null)
                {
                    return starter;
                }
            }
            catch
            {
            }
            return ActivatorUtilities.CreateInstance(_services, type);
        }
    }
}
