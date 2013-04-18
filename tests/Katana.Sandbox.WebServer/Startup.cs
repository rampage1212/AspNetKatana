// <copyright file="Startup.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Forms;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using Owin.Types;

namespace Katana.Sandbox.WebServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHandlerAsync(async (req, res, next) =>
            {
                req.TraceOutput.WriteLine("{0} {1}{2}", req.Method, req.PathBase, req.Path);
                await next();
                req.TraceOutput.WriteLine("{0} {1}{2}", res.StatusCode, req.PathBase, req.Path);
            });

            app.UseFormsAuthentication(new FormsAuthenticationOptions
            {
                LoginPath = "/Login",
                LogoutPath = "/Logout",
                AuthenticationMode = AuthenticationMode.Passive
            });

            app.UseExternalSignInCookie("External");

            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                AppId = "615948391767418",
                AppSecret = "c9b1fa6b68db835890ce469e0d98157f",
                SignInAsAuthenticationType = "External",
                Caption = "Sign in with Facebook",
            });

            app.UseGoogleAuthentication(new GoogleAuthenticationOptions
            {
                SignInAsAuthenticationType = "External",
                Caption = "Sign in with Google",
            });

            var tokenProtection = DataProtectionProviders.Default.Create("Katana.Sandbox.WebServer", "OAuth Bearer Token");

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                DataProtection = tokenProtection,
            });

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/Authorize",
                TokenEndpointPath = "/Token",
                DataProtection = tokenProtection,
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnValidateClientCredentials = async context =>
                    {
                        if (context.ClientId == "123456")
                        {
                            context.ClientFound("abcdef", "http://localhost:18429/ClientApp.aspx");
                        }
                    },
                    OnValidateResourceOwnerCredentials = async context =>
                    {
                        var identity = new ClaimsIdentity(
                            new GenericIdentity(context.Username, "Bearer"), 
                            context.Scope.Split(' ').Select(x => new Claim("urn:oauth:scope", x)));

                        context.Validated(identity, null);
                    }
                }
            });

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "api/{controller}");
            app.UseWebApi(config);
        }
    }
}