// <copyright file="DpapiDataProtecter.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    internal class DpapiDataProtector : IDataProtector
    {
        private readonly System.Security.Cryptography.DpapiDataProtector _protector;

        public DpapiDataProtector(string appName, string[] purposes)
        {
            _protector = new System.Security.Cryptography.DpapiDataProtector(appName, "Microsoft.Owin.Security.IDataProtector", purposes)
            {
                Scope = DataProtectionScope.CurrentUser
            };
        }

        public byte[] Protect(byte[] userData)
        {
            return _protector.Protect(userData);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return _protector.Unprotect(protectedData);
        }
    }
}
