﻿// <copyright file="DisposableActionTests.cs" company="Microsoft Open Technologies, Inc.">
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

using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public class DisposableActionTests
    {
        public class TheConstructor
        {
            [Fact]
            public void RequiresNonNullAction()
            {
                ContractAssert.NotNull(() => new DisposableAction(null), "act");
            }
        }

        public class TheDisposeMethod
        {
            [Fact]
            public void InvokesTheAction()
            {
                bool invoked = false;
                new DisposableAction(() => invoked = true).Dispose();
                Assert.True(invoked);
            }
        }
    }
}
