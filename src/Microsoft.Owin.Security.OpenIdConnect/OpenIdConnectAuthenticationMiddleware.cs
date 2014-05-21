﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

using Microsoft.IdentityModel.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// OWIN middleware for obtaining identities using OpenIdConnect protocol.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Middleware are not disposable.")]
    public class OpenIdConnectAuthenticationMiddleware : AuthenticationMiddleware<OpenIdConnectAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a <see cref="OpenIdConnectAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "redirect_uri", Justification = "False positive")]
        public OpenIdConnectAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, OpenIdConnectAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<OpenIdConnectAuthenticationMiddleware>();

            if (string.IsNullOrEmpty(Options.SignInAsAuthenticationType))
            {
                Options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            if (Options.StateDataFormat == null)
            {
                var dataProtector = app.CreateDataProtector(
                    typeof(OpenIdConnectAuthenticationMiddleware).FullName,
                    Options.AuthenticationType, "v1");
                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }

            if (Options.SecurityTokenHandlers == null)
            {
                Options.SecurityTokenHandlers = SecurityTokenHandlerCollectionExtensions.GetDefaultHandlers(Options.SignInAsAuthenticationType);
            }
            
            // if the user has not set the AuthorizeCallback, set it from the redirect_uri
            if (!Options.AuthorizeCallback.HasValue)
            {
                Uri redirect_uri;
                if (!string.IsNullOrEmpty(Options.RedirectUri) && Uri.TryCreate(Options.RedirectUri, UriKind.Absolute, out redirect_uri))
                {
                    // Redirect_Uri must be a very specific, case sensitive value, so we can't generate it. Instead we generate AuthorizeCallback from it.
                    Options.AuthorizeCallback = PathString.FromUriComponent(redirect_uri);
                }
            }

            _httpClient = new HttpClient(ResolveHttpMessageHandler(Options));
            _httpClient.Timeout = Options.BackchannelTimeout;
            _httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB

            OpenIdConnectMetadata metadata = null;
            if (!string.IsNullOrWhiteSpace(Options.MetadataAddress))
            {
                metadata = GetMetadata(Options.MetadataAddress, _httpClient);
            }
            else if (metadata == null && !string.IsNullOrWhiteSpace(Options.Authority))
            {
                metadata = GetMetadataBuildingAddress(Options.Authority, _httpClient);
            }

            if (metadata != null)
            {
                Options.AuthorizationEndpoint = metadata.AuthorizationEndpoint;
                Options.EndSessionEndpoint = metadata.EndSessionEndpoint;
                Options.TokenEndpoint = metadata.TokenEndpoint;
                Options.TokenValidationParameters.IssuerSigningKeys = metadata.SigningKeys;
                Options.TokenValidationParameters.ValidIssuer = metadata.Issuer;
            }

            if (string.IsNullOrWhiteSpace(Options.TokenValidationParameters.ValidAudience) && !string.IsNullOrWhiteSpace(Options.ClientId))
            {
                Options.TokenValidationParameters.ValidAudience = Options.ClientId;
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="WsFederationAuthenticationOptions"/> supplied to the constructor.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design, backchannel errors should not be shown to the request.")]
        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new OpenIdConnectAuthenticationHandler(_logger);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(OpenIdConnectAuthenticationOptions options)
        {
            HttpMessageHandler handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

            if (options.BackchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = handler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
                }

                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }

            return handler;
        }

        private static OpenIdConnectMetadata GetMetadataBuildingAddress(string authority, HttpClient httpClient)
        {
            string metadataAddress = authority;
            if (!authority.EndsWith("/", StringComparison.Ordinal))
            {
                metadataAddress += "/";
            }

            metadataAddress += ".well-known/openid-configuration";
            return GetMetadata(metadataAddress, httpClient);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It is not important why the http call failed")]
        private static OpenIdConnectMetadata GetMetadata(string metadataAddress, HttpClient httpClient)
        {
            OpenIdConnectMetadata openIdConnectMetadata = null;
            try
            {
                openIdConnectMetadata = OpenIdConnectMetadataRetriever.GetMetadata(metadataAddress, httpClient);
            }
            catch (Exception)
            {
                // TODO - need to log
            }

            return openIdConnectMetadata;
        }
    }
}