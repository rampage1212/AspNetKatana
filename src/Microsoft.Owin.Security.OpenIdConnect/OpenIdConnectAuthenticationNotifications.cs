﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// Specifies events which the <see cref="OpenIdConnectAuthenticationMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public class OpenIdConnectAuthenticationNotifications
    {
        /// <summary>
        /// Creates a new set of notifications. Each notification has a default no-op behavior unless otherwise documented.
        /// </summary>
        public OpenIdConnectAuthenticationNotifications()
        {
            AuthenticationFailed = notification => Task.FromResult(0);
            AuthorizationCodeReceived = notification => Task.FromResult(0);
            MessageReceived = notification => Task.FromResult(0);
            SecurityTokenReceived = notification => Task.FromResult(0);
            SecurityTokenValidated = notification => Task.FromResult(0);
            ApplyRedirectToIdentityProvider = notification =>
            {
                string redirectUri;
                if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                {
                    redirectUri = notification.ProtocolMessage.CreateAuthenticationRequestUrl();
                }
                else
                {
                    // LogoutRequest
                    redirectUri = notification.ProtocolMessage.CreateLogoutRequestUrl();
                }
                if (Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    // TODO: else log error?
                    notification.Response.Redirect(redirectUri);
                }
                return Task.FromResult(0);
            };
        }

        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> AuthenticationFailed { get; set; }

        /// <summary>
        /// Invoked after security token validation if an authorization code is present in the protocol message.
        /// </summary>
        public Func<AuthorizationCodeReceivedNotification, Task> AuthorizationCodeReceived { get; set; }

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        public Func<MessageReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> MessageReceived { get; set; }

        /// <summary>
        /// Invoked to generate redirects to the identity provider for SignIn, SignOut, or Challenge. This event has a default implementation.
        /// </summary>
        public Func<RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> ApplyRedirectToIdentityProvider { get; set; }

        /// <summary>
        /// Invoked with the security token that has been extracted from the protocol message.
        /// </summary>
        public Func<SecurityTokenReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }

        public Func<Task> SignedIn { get; set; }
        public Func<Task> SignedOut { get; set; }
    }
}