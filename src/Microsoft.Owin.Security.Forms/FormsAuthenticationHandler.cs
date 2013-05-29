// <copyright file="FormsAuthenticationHandler.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.Forms
{
    internal class FormsAuthenticationHandler : AuthenticationHandler<FormsAuthenticationOptions>
    {
        private const string HeaderNameCacheControl = "Cache-Control";
        private const string HeaderNamePragma = "Pragma";
        private const string HeaderNameExpires = "Expires";
        private const string HeaderValueNoCache = "no-cache";
        private const string HeaderValueMinusOne = "-1";

        private readonly ILogger _logger;

        private bool _shouldRenew;
        private DateTimeOffset _renewIssuedUtc;
        private DateTimeOffset _renewExpiresUtc;

        public FormsAuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            _logger.WriteVerbose("AuthenticateCore");

            IDictionary<string, string> cookies = Request.GetCookies();
            string cookie;
            if (!cookies.TryGetValue(Options.CookieName, out cookie))
            {
                _logger.WriteWarning("No cookie found");
                return null;
            }

            AuthenticationTicket model = Options.TicketDataHandler.Unprotect(cookie);

            if (model == null)
            {
                _logger.WriteWarning("null model");
                return null;
            }

            DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
            DateTimeOffset? issuedUtc = model.Extra.IssuedUtc;
            DateTimeOffset? expiresUtc = model.Extra.ExpiresUtc;

            if (expiresUtc != null && expiresUtc.Value < currentUtc)
            {
                return null;
            }

            if (issuedUtc != null && expiresUtc != null && Options.SlidingExpiration)
            {
                TimeSpan timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                TimeSpan timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                if (timeRemaining < timeElapsed)
                {
                    _shouldRenew = true;
                    _renewIssuedUtc = currentUtc;
                    TimeSpan timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                    _renewExpiresUtc = currentUtc.Add(timeSpan);
                }
            }

            var context = new FormsValidateIdentityContext(model);

            await Options.Provider.ValidateIdentity(context);

            return new AuthenticationTicket(context.Identity, context.Extra);
        }

        protected override async Task ApplyResponseGrant()
        {
            _logger.WriteVerbose("ApplyResponseGrant");
            AuthenticationResponseGrant signin = Helper.LookupSignIn(Options.AuthenticationType);
            bool shouldSignin = signin != null;
            AuthenticationResponseRevoke signout = Helper.LookupSignOut(Options.AuthenticationType, Options.AuthenticationMode);
            bool shouldSignout = signout != null;

            if (shouldSignin || shouldSignout || _shouldRenew)
            {
                var cookieOptions = new CookieOptions
                {
                    Domain = Options.CookieDomain,
                    HttpOnly = Options.CookieHttpOnly,
                    Path = Options.CookiePath ?? "/",
                };
                if (Options.CookieSecure == CookieSecureOption.SameAsRequest)
                {
                    cookieOptions.Secure = Request.IsSecure;
                }
                else
                {
                    cookieOptions.Secure = Options.CookieSecure == CookieSecureOption.Always;
                }

                if (shouldSignin)
                {
                    var context = new FormsResponseSignInContext(
                        Response.Environment,
                        Options.AuthenticationType,
                        signin.Identity,
                        signin.Extra);

                    DateTimeOffset issuedUtc = Options.SystemClock.UtcNow;
                    DateTimeOffset expiresUtc = issuedUtc.Add(Options.ExpireTimeSpan);

                    context.Extra.IssuedUtc = issuedUtc;
                    context.Extra.ExpiresUtc = expiresUtc;

                    Options.Provider.ResponseSignIn(context);

                    if (context.Extra.IsPersistent)
                    {
                        cookieOptions.Expires = expiresUtc.ToUniversalTime().DateTime;
                    }

                    var model = new AuthenticationTicket(context.Identity, context.Extra.Properties);
                    string cookieValue = Options.TicketDataHandler.Protect(model);

                    Response.AddCookie(
                        Options.CookieName,
                        cookieValue,
                        cookieOptions);
                }
                else if (shouldSignout)
                {
                    Response.DeleteCookie(
                        Options.CookieName,
                        cookieOptions);
                }
                else if (_shouldRenew)
                {
                    AuthenticationTicket model = await Authenticate();

                    model.Extra.IssuedUtc = _renewIssuedUtc;
                    model.Extra.ExpiresUtc = _renewExpiresUtc;

                    string cookieValue = Options.TicketDataHandler.Protect(model);

                    if (model.Extra.IsPersistent)
                    {
                        cookieOptions.Expires = _renewExpiresUtc.ToUniversalTime().DateTime;
                    }

                    Response.AddCookie(
                        Options.CookieName,
                        cookieValue,
                        cookieOptions);
                }

                Response.SetHeader(
                    HeaderNameCacheControl,
                    HeaderValueNoCache);

                Response.SetHeader(
                    HeaderNamePragma,
                    HeaderValueNoCache);

                Response.SetHeader(
                    HeaderNameExpires,
                    HeaderValueMinusOne);

                bool shouldLoginRedirect = shouldSignin && !string.IsNullOrEmpty(Options.LoginPath) && string.Equals(Request.Path, Options.LoginPath, StringComparison.OrdinalIgnoreCase);
                bool shouldLogoutRedirect = shouldSignout && !string.IsNullOrEmpty(Options.LogoutPath) && string.Equals(Request.Path, Options.LogoutPath, StringComparison.OrdinalIgnoreCase);

                if ((shouldLoginRedirect || shouldLogoutRedirect) && Response.StatusCode == 200)
                {
                    IDictionary<string, string[]> query = Request.GetQuery();
                    string[] redirectUri;
                    if (query.TryGetValue(Options.ReturnUrlParameter ?? FormsAuthenticationDefaults.ReturnUrlParameter, out redirectUri) &&
                        redirectUri != null &&
                        redirectUri.Length == 1 &&
                        IsHostRelative(redirectUri[0]))
                    {
                        Response.Redirect(redirectUri[0]);
                    }
                }
            }
        }

        private bool IsHostRelative(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (path.Length == 1)
            {
                return path[0] == '/';
            }
            return path[0] == '/' && path[1] != '/' && path[1] != '\\';
        }

        protected override Task ApplyResponseChallenge()
        {
            _logger.WriteVerbose("ApplyResponseChallenge");
            if (Response.StatusCode != 401 || string.IsNullOrEmpty(Options.LoginPath))
            {
                return Task.FromResult<object>(null);
            }

            AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string baseUri = Request.Scheme + "://" + Request.Host + Request.PathBase;

                string currentUri = WebUtilities.AddQueryString(
                    Request.PathBase + Request.Path,
                    Request.QueryString);

                string loginUri = WebUtilities.AddQueryString(
                    baseUri + Options.LoginPath,
                    Options.ReturnUrlParameter ?? FormsAuthenticationDefaults.ReturnUrlParameter,
                    currentUri);

                Response.Redirect(loginUri);
            }

            return Task.FromResult<object>(null);
        }
    }
}
