// <copyright file="FormsAuthenticationOptions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationOptions : AuthenticationOptions
    {
        public FormsAuthenticationOptions()
            : base(FormsAuthenticationDefaults.AuthenticationType)
        {
            CookieName = FormsAuthenticationDefaults.CookiePrefix + FormsAuthenticationDefaults.AuthenticationType;
            CookiePath = "/";
            ExpireTimeSpan = TimeSpan.FromDays(14);
            SlidingExpiration = true;
            CookieHttpOnly = true;
            CookieSecure = CookieSecureOption.SameAsRequest;
        }

        public string CookieName { get; set; }
        public string CookieDomain { get; set; }
        public string CookiePath { get; set; }
        public bool CookieHttpOnly { get; set; }
        public CookieSecureOption CookieSecure { get; set; }

        public TimeSpan ExpireTimeSpan { get; set; }
        public bool SlidingExpiration { get; set; }

        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }

        public string ReturnUrlParameter { get; set; }

        public IFormsAuthenticationProvider Provider { get; set; }

        public ISecureDataHandler<AuthenticationTicket> TicketDataHandler { get; set; }
    }
}
