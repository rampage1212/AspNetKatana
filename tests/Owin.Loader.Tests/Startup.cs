﻿// <copyright file="Startup.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
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

[assembly: Microsoft.Owin.OwinStartup(typeof(Owin.Loader.Tests.Startup))]
[assembly: Microsoft.Owin.OwinStartup("AFriendlyName", typeof(Owin.Loader.Tests.Startup))]
[assembly: Microsoft.Owin.OwinStartup("AlternateConfiguration", typeof(Owin.Loader.Tests.Startup), "AlternateConfiguration")]

namespace Owin.Loader.Tests
{
    public class Startup
    {
        public static int ConfigurationCalls { get; set; }
        public static int AlternateConfigurationCalls { get; set; }

        public void Configuration(IAppBuilder builder)
        {
            ConfigurationCalls++;
        }

        public void AlternateConfiguration(IAppBuilder builder)
        {
            AlternateConfigurationCalls++;
        }
    }
}
