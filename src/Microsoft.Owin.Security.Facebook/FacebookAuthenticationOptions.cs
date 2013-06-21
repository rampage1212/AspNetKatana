// <copyright file="FacebookAuthenticationOptions.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.Facebook
{
    public class FacebookAuthenticationOptions : AuthenticationOptions
    {
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", 
            MessageId = "Microsoft.Owin.Security.Facebook.FacebookAuthenticationOptions.set_Caption(System.String)", Justification = "Not localizable.")]
        public FacebookAuthenticationOptions()
            : base(Constants.DefaultAuthenticationType)
        {
            Caption = Constants.DefaultAuthenticationType;
            ReturnEndpointPath = "/signin-facebook";
            AuthenticationMode = AuthenticationMode.Passive;
        }

        public string AppId { get; set; }
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// in back channel communications belong to twitter.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public IPinnedCertificateValidator PinnedCertificateValidator { get; set; }

        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }
        public string ReturnEndpointPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }

        public IFacebookAuthenticationProvider Provider { get; set; }
        public ISecureDataHandler<AuthenticationExtra> StateDataHandler { get; set; }

        public string Scope { get; set; }
    }
}
