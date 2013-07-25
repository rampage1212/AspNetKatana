// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth.Messages;
using Newtonsoft.Json;

namespace Microsoft.Owin.Security.OAuth
{
    internal class OAuthAuthorizationServerHandler : AuthenticationHandler<OAuthAuthorizationServerOptions>
    {
        private readonly ILogger _logger;

        private AuthorizeEndpointRequest _authorizeEndpointRequest;
        private OAuthLookupClientContext _clientContext;

        public OAuthAuthorizationServerHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            return Task.FromResult<AuthenticationTicket>(null);
        }

        public override async Task<bool> InvokeAsync()
        {
            var matchRequestContext = new OAuthMatchEndpointContext(Context, Options);
            if (!string.IsNullOrEmpty(Options.AuthorizeEndpointPath) && string.Equals(Options.AuthorizeEndpointPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                matchRequestContext.MatchesAuthorizeEndpoint();
            }
            else if (!string.IsNullOrEmpty(Options.TokenEndpointPath) && string.Equals(Options.TokenEndpointPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                matchRequestContext.MatchesTokenEndpoint();
            }
            await Options.Provider.MatchEndpoint(matchRequestContext);
            if (matchRequestContext.IsAuthorizeEndpoint)
            {
                return await InvokeAuthorizeEndpointAsync();
            }
            if (matchRequestContext.IsTokenEndpoint)
            {
                await InvokeTokenEndpointAsync();
                return true;
            }
            return false;
        }

        private async Task<bool> InvokeAuthorizeEndpointAsync()
        {
            var authorizeRequest = new AuthorizeEndpointRequest(Request.Query);

            OAuthLookupClientContext clientContext = await ValidateAuthorizeEndpointClientAsync(authorizeRequest);

            if (!clientContext.IsValidated)
            {
                _logger.WriteVerbose("Unable to validate client information");
                return await SendErrorRedirectAsync(clientContext, clientContext);
            }

            var validatingContext = new OAuthValidateAuthorizeRequestContext(
                Context,
                Options,
                authorizeRequest,
                clientContext);

            // the request is initially assumed valid
            validatingContext.Validated();

            if (string.IsNullOrEmpty(authorizeRequest.ResponseType))
            {
                _logger.WriteVerbose("Authorize endpoint request missing required response_type parameter");
                validatingContext.SetError(Constants.Errors.InvalidRequest);
            }
            else if (!authorizeRequest.IsAuthorizationCodeGrantType &&
                !authorizeRequest.IsImplicitGrantType)
            {
                _logger.WriteVerbose("Authorize endpoint request contains unsupported response_type parameter");
                validatingContext.SetError(Constants.Errors.UnsupportedResponseType);
            }
            else
            {
                await Options.Provider.ValidateAuthorizeRequest(validatingContext);
            }

            if (!validatingContext.IsValidated)
            {
                // an invalid request is not processed further
                return await SendErrorRedirectAsync(clientContext, validatingContext);
            }

            _clientContext = clientContext;
            _authorizeEndpointRequest = authorizeRequest;

            var authorizeEndpointContext = new OAuthAuthorizeEndpointContext(Context, Options);

            await Options.Provider.AuthorizeEndpoint(authorizeEndpointContext);

            return authorizeEndpointContext.IsRequestCompleted;
        }

        protected override async Task ApplyResponseGrantAsync()
        {
            // only successful results of an authorize request are altered
            if (_clientContext == null ||
                _authorizeEndpointRequest == null ||
                Response.StatusCode != 200)
            {
                return;
            }

            // only apply with signin of matching authentication type
            AuthenticationResponseGrant signin = Helper.LookupSignIn(Options.AuthenticationType);
            if (signin == null)
            {
                return;
            }

            string location = _clientContext.EffectiveRedirectUri;

            if (_authorizeEndpointRequest.IsAuthorizationCodeGrantType)
            {
                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
                signin.Properties.IssuedUtc = currentUtc;
                signin.Properties.ExpiresUtc = currentUtc.Add(Options.AuthenticationCodeExpireTimeSpan);

                // associate client_id with all subsequent tickets
                signin.Properties.Dictionary[Constants.Extra.ClientId] = _authorizeEndpointRequest.ClientId;
                if (!string.IsNullOrEmpty(_authorizeEndpointRequest.RedirectUri))
                {
                    // keep original request parameter for later comparison
                    signin.Properties.Dictionary[Constants.Extra.RedirectUri] = _authorizeEndpointRequest.RedirectUri;
                }

                var context = new AuthenticationTokenCreateContext(
                    Context,
                    Options.AuthenticationCodeFormat,
                    new AuthenticationTicket(signin.Identity, signin.Properties));

                await Options.AuthenticationCodeProvider.CreateAsync(context);

                string code = context.Token;
                if (string.IsNullOrEmpty(code))
                {
                    code = context.SerializeTicket();
                }

                location = WebUtilities.AddQueryString(location, Constants.Parameters.Code, code);
                if (!String.IsNullOrEmpty(_authorizeEndpointRequest.State))
                {
                    location = WebUtilities.AddQueryString(location, Constants.Parameters.State, _authorizeEndpointRequest.State);
                }
                Response.Redirect(location);
            }
            else if (_authorizeEndpointRequest.IsImplicitGrantType)
            {
                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
                signin.Properties.IssuedUtc = currentUtc;
                signin.Properties.ExpiresUtc = currentUtc.Add(Options.AccessTokenExpireTimeSpan);

                // associate client_id with access token
                signin.Properties.Dictionary[Constants.Extra.ClientId] = _authorizeEndpointRequest.ClientId;

                var accessTokenContext = new AuthenticationTokenCreateContext(
                    Context,
                    Options.AccessTokenFormat,
                    new AuthenticationTicket(signin.Identity, signin.Properties));

                await Options.AccessTokenProvider.CreateAsync(accessTokenContext);

                string accessToken = accessTokenContext.Token;
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = accessTokenContext.SerializeTicket();
                }

                DateTimeOffset? accessTokenExpiresUtc = accessTokenContext.Ticket.Properties.ExpiresUtc;

                var appender = new Appender(location, '#');
                appender
                    .Append(Constants.Parameters.AccessToken, accessToken)
                    .Append(Constants.Parameters.TokenType, Constants.TokenTypes.Bearer);
                if (accessTokenExpiresUtc.HasValue)
                {
                    TimeSpan? expiresTimeSpan = accessTokenExpiresUtc - currentUtc;
                    var expiresIn = (long)(expiresTimeSpan.Value.TotalSeconds + .5);
                    appender.Append(Constants.Parameters.ExpiresIn, expiresIn.ToString(CultureInfo.InvariantCulture));
                }
                if (!String.IsNullOrEmpty(_authorizeEndpointRequest.State))
                {
                    appender.Append(Constants.Parameters.State, _authorizeEndpointRequest.State);
                }
                Response.Redirect(appender.ToString());
            }
        }

        private async Task InvokeTokenEndpointAsync()
        {
            DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
            // remove milliseconds in case they don't round-trip
            currentUtc = currentUtc.Subtract(TimeSpan.FromMilliseconds(currentUtc.Millisecond));

            IFormCollection form = await Request.ReadFormAsync();
            var tokenEndpointRequest = new TokenEndpointRequest(form);

            OAuthLookupClientContext clientContext = await ValidateTokenEndpointClientAsync(tokenEndpointRequest);
            if (!clientContext.IsValidated)
            {
                _logger.WriteError("clientID is not valid.");
                if (!clientContext.HasError)
                {
                    clientContext.SetError(Constants.Errors.InvalidClient);
                }
                await SendErrorAsJsonAsync(clientContext);
                return;
            }

            var validatingContext = new OAuthValidateTokenRequestContext(Context, Options, tokenEndpointRequest, clientContext);
            // initially valid at this point
            validatingContext.Validated();

            AuthenticationTicket ticket = null;
            if (tokenEndpointRequest.IsAuthorizationCodeGrantType)
            {
                // Authorization Code Grant http://tools.ietf.org/html/rfc6749#section-4.1
                // Access Token Request http://tools.ietf.org/html/rfc6749#section-4.1.3
                ticket = await InvokeTokenEndpointAuthorizationCodeGrantAsync(validatingContext, currentUtc);
            }
            else if (tokenEndpointRequest.IsResourceOwnerPasswordCredentialsGrantType)
            {
                // Resource Owner Password Credentials Grant http://tools.ietf.org/html/rfc6749#section-4.3
                // Access Token Request http://tools.ietf.org/html/rfc6749#section-4.3.2
                ticket = await InvokeTokenEndpointResourceOwnerPasswordCredentialsGrantAsync(validatingContext, currentUtc);
            }
            else if (tokenEndpointRequest.IsClientCredentialsGrantType)
            {
                // Client Credentials Grant http://tools.ietf.org/html/rfc6749#section-4.4
                // Access Token Request http://tools.ietf.org/html/rfc6749#section-4.4.2
                ticket = await InvokeTokenEndpointClientCredentialsGrantAsync(validatingContext, currentUtc);
            }
            else if (tokenEndpointRequest.IsRefreshTokenGrantType)
            {
                // Refreshing an Access Token
                // http://tools.ietf.org/html/rfc6749#section-6
                ticket = await InvokeTokenEndpointRefreshTokenGrantAsync(validatingContext, currentUtc);
            }
            else if (tokenEndpointRequest.IsCustomExtensionGrantType)
            {
                // Defining New Authorization Grant Types
                // http://tools.ietf.org/html/rfc6749#section-8.3
                ticket = await InvokeTokenEndpointCustomGrantAsync(validatingContext, currentUtc);
            }
            else
            {
                // Error Response http://tools.ietf.org/html/rfc6749#section-5.2
                // The authorization grant type is not supported by the
                // authorization server.
                _logger.WriteError("grant type is not recognized");
                validatingContext.SetError(Constants.Errors.UnsupportedGrantType);
            }

            if (ticket == null)
            {
                await SendErrorAsJsonAsync(validatingContext);
                return;
            }

            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(Options.AccessTokenExpireTimeSpan);

            var tokenEndpointContext = new OAuthTokenEndpointContext(
                Context,
                Options,
                ticket,
                tokenEndpointRequest);

            await Options.Provider.TokenEndpoint(tokenEndpointContext);

            if (tokenEndpointContext.TokenIssued)
            {
                ticket = new AuthenticationTicket(
                    tokenEndpointContext.Identity,
                    tokenEndpointContext.Properties);
            }
            else
            {
                _logger.WriteError("Token was not issued to tokenEndpointContext");
                validatingContext.SetError(Constants.Errors.InvalidGrant);
                await SendErrorAsJsonAsync(validatingContext);
                return;
            }

            var accessTokenContext = new AuthenticationTokenCreateContext(
                Context,
                Options.AccessTokenFormat,
                ticket);

            await Options.AccessTokenProvider.CreateAsync(accessTokenContext);

            string accessToken = accessTokenContext.Token;
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = accessTokenContext.SerializeTicket();
            }
            DateTimeOffset? accessTokenExpiresUtc = ticket.Properties.ExpiresUtc;

            var refreshTokenCreateContext = new AuthenticationTokenCreateContext(
                Context,
                Options.RefreshTokenFormat,
                accessTokenContext.Ticket);
            await Options.RefreshTokenProvider.CreateAsync(refreshTokenCreateContext);
            string refreshToken = refreshTokenCreateContext.Token;

            var memory = new MemoryStream();
            byte[] body;
            using (var writer = new JsonTextWriter(new StreamWriter(memory)))
            {
                writer.WriteStartObject();
                writer.WritePropertyName(Constants.Parameters.AccessToken);
                writer.WriteValue(accessToken);
                writer.WritePropertyName(Constants.Parameters.TokenType);
                writer.WriteValue(Constants.TokenTypes.Bearer);
                if (accessTokenExpiresUtc.HasValue)
                {
                    TimeSpan? expiresTimeSpan = accessTokenExpiresUtc - currentUtc;
                    var expiresIn = (long)(expiresTimeSpan.Value.TotalSeconds + .5);
                    if (expiresIn > 0)
                    {
                        writer.WritePropertyName(Constants.Parameters.ExpiresIn);
                        writer.WriteValue(expiresIn);
                    }
                }
                if (!String.IsNullOrEmpty(refreshToken))
                {
                    writer.WritePropertyName(Constants.Parameters.RefreshToken);
                    writer.WriteValue(refreshToken);
                }
                foreach (var additionalResponseParameter in tokenEndpointContext.AdditionalResponseParameters)
                {
                    writer.WritePropertyName(additionalResponseParameter.Key);
                    writer.WriteValue(additionalResponseParameter.Value);
                }
                writer.WriteEndObject();
                writer.Flush();
                body = memory.ToArray();
            }
            Response.ContentType = "application/json;charset=UTF-8";
            Response.Headers.Set("Cache-Control", "no-cache");
            Response.Headers.Set("Pragma", "no-cache");
            Response.Headers.Set("Expires", "-1");
            Response.ContentLength = memory.ToArray().Length;
            await Response.WriteAsync(body, Request.CallCancelled);
        }

        private async Task<AuthenticationTicket> InvokeTokenEndpointAuthorizationCodeGrantAsync(
            OAuthValidateTokenRequestContext validatingContext,
            DateTimeOffset currentUtc)
        {
            TokenEndpointRequest tokenEndpointRequest = validatingContext.TokenRequest;

            var authenticationCodeContext = new AuthenticationTokenReceiveContext(
                Context,
                Options.AuthenticationCodeFormat,
                tokenEndpointRequest.AuthorizationCodeGrant.Code);

            await Options.AuthenticationCodeProvider.ReceiveAsync(authenticationCodeContext);

            AuthenticationTicket ticket = authenticationCodeContext.Ticket;

            if (ticket == null)
            {
                _logger.WriteError("invalid authorization code");
                validatingContext.SetError(Constants.Errors.InvalidGrant);
                return null;
            }

            if (!ticket.Properties.ExpiresUtc.HasValue ||
                ticket.Properties.ExpiresUtc < currentUtc)
            {
                _logger.WriteError("expired authorization code");
                validatingContext.SetError(Constants.Errors.InvalidGrant);
                return null;
            }

            string clientId;
            if (!ticket.Properties.Dictionary.TryGetValue(Constants.Extra.ClientId, out clientId) ||
                !String.Equals(clientId, validatingContext.ClientContext.ClientId, StringComparison.Ordinal))
            {
                _logger.WriteError("authorization code does not contain matching client_id");
                validatingContext.SetError(Constants.Errors.InvalidGrant);
                return null;
            }

            string redirectUri;
            if (ticket.Properties.Dictionary.TryGetValue(Constants.Extra.RedirectUri, out redirectUri))
            {
                ticket.Properties.Dictionary.Remove(Constants.Extra.RedirectUri);
                if (!String.Equals(redirectUri, tokenEndpointRequest.AuthorizationCodeGrant.RedirectUri, StringComparison.Ordinal))
                {
                    _logger.WriteError("authorization code does not contain matching redirect_uri");
                    validatingContext.SetError(Constants.Errors.InvalidGrant);
                    return null;
                }
            }

            if (validatingContext.IsValidated)
            {
                await Options.Provider.ValidateTokenRequest(validatingContext);
            }

            var grantContext = new OAuthGrantAuthorizationCodeContext(
                Context, Options, ticket);

            if (validatingContext.IsValidated)
            {
                await Options.Provider.GrantAuthorizationCode(grantContext);
            }

            return ReturnOutcome(
                validatingContext,
                grantContext,
                grantContext.Ticket,
                Constants.Errors.InvalidGrant);
        }

        private async Task<AuthenticationTicket> InvokeTokenEndpointResourceOwnerPasswordCredentialsGrantAsync(
            OAuthValidateTokenRequestContext validatingContext,
            DateTimeOffset currentUtc)
        {
            TokenEndpointRequest tokenEndpointRequest = validatingContext.TokenRequest;

            if (validatingContext.IsValidated)
            {
                await Options.Provider.ValidateTokenRequest(validatingContext);
            }

            var grantContext = new OAuthGrantResourceOwnerCredentialsContext(
                Context,
                Options,
                validatingContext.ClientContext.ClientId,
                tokenEndpointRequest.ResourceOwnerPasswordCredentialsGrant.UserName,
                tokenEndpointRequest.ResourceOwnerPasswordCredentialsGrant.Password,
                tokenEndpointRequest.ResourceOwnerPasswordCredentialsGrant.Scope);

            if (validatingContext.IsValidated)
            {
                await Options.Provider.GrantResourceOwnerCredentials(grantContext);
            }

            return ReturnOutcome(
                validatingContext,
                grantContext,
                grantContext.Ticket,
                Constants.Errors.InvalidGrant);
        }

        private async Task<AuthenticationTicket> InvokeTokenEndpointClientCredentialsGrantAsync(
            OAuthValidateTokenRequestContext validatingContext,
            DateTimeOffset currentUtc)
        {
            TokenEndpointRequest tokenEndpointRequest = validatingContext.TokenRequest;

            await Options.Provider.ValidateTokenRequest(validatingContext);
            if (!validatingContext.IsValidated)
            {
                return null;
            }

            var grantContext = new OAuthGrantClientCredentialsContext(
                Context,
                Options,
                validatingContext.ClientContext.ClientId,
                tokenEndpointRequest.ClientCredentialsGrant.Scope);

            await Options.Provider.GrantClientCredentials(grantContext);

            return ReturnOutcome(
                validatingContext,
                grantContext,
                grantContext.Ticket,
                Constants.Errors.UnauthorizedClient);
        }

        private async Task<AuthenticationTicket> InvokeTokenEndpointRefreshTokenGrantAsync(
            OAuthValidateTokenRequestContext validatingContext,
            DateTimeOffset currentUtc)
        {
            TokenEndpointRequest tokenEndpointRequest = validatingContext.TokenRequest;

            var refreshTokenContext = new AuthenticationTokenReceiveContext(
                Context,
                Options.RefreshTokenFormat,
                tokenEndpointRequest.RefreshTokenGrant.RefreshToken);

            await Options.RefreshTokenProvider.ReceiveAsync(refreshTokenContext);

            AuthenticationTicket ticket = refreshTokenContext.Ticket;

            if (ticket == null)
            {
                _logger.WriteError("invalid refresh token");
                validatingContext.SetError(Constants.Errors.InvalidGrant);
                return null;
            }

            if (!ticket.Properties.ExpiresUtc.HasValue ||
                ticket.Properties.ExpiresUtc < currentUtc)
            {
                _logger.WriteError("expired refresh token");
                validatingContext.SetError(Constants.Errors.InvalidGrant);
                return null;
            }

            await Options.Provider.ValidateTokenRequest(validatingContext);

            var grantContext = new OAuthGrantRefreshTokenContext(Context, Options, ticket);

            await Options.Provider.GrantRefreshToken(grantContext);

            return ReturnOutcome(
                validatingContext,
                grantContext,
                grantContext.Ticket,
                Constants.Errors.InvalidGrant);
        }

        private async Task<AuthenticationTicket> InvokeTokenEndpointCustomGrantAsync(
            OAuthValidateTokenRequestContext validatingContext,
            DateTimeOffset currentUtc)
        {
            TokenEndpointRequest tokenEndpointRequest = validatingContext.TokenRequest;

            if (validatingContext.IsValidated)
            {
                await Options.Provider.ValidateTokenRequest(validatingContext);
            }

            var grantContext = new OAuthGrantCustomExtensionContext(
                Context,
                Options,
                validatingContext.ClientContext.ClientId,
                tokenEndpointRequest.GrantType,
                tokenEndpointRequest.CustomExtensionGrant.Parameters);

            if (validatingContext.IsValidated)
            {
                await Options.Provider.GrantCustomExtension(grantContext);
            }

            return ReturnOutcome(
                validatingContext,
                grantContext,
                grantContext.Ticket,
                Constants.Errors.UnsupportedGrantType);
        }

        private static AuthenticationTicket ReturnOutcome(
            OAuthValidateTokenRequestContext validatingContext,
            BaseValidatingContext<OAuthAuthorizationServerOptions> grantContext,
            AuthenticationTicket ticket,
            string defaultError)
        {
            if (!validatingContext.IsValidated)
            {
                return null;
            }

            if (!grantContext.IsValidated)
            {
                if (grantContext.HasError)
                {
                    validatingContext.SetError(
                        grantContext.Error,
                        grantContext.ErrorDescription,
                        grantContext.ErrorUri);
                }
                else
                {
                    validatingContext.SetError(defaultError);
                }
                return null;
            }

            if (ticket == null)
            {
                validatingContext.SetError(defaultError);
                return null;
            }

            return ticket;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The MemoryStream is Disposed by the StreamWriter")]
        private Task SendErrorAsJsonAsync(
            BaseValidatingContext<OAuthAuthorizationServerOptions> validatingContext)
        {
            string error = validatingContext.HasError ? validatingContext.Error : Constants.Errors.InvalidRequest;
            string errorDescription = validatingContext.HasError ? validatingContext.ErrorDescription : null;
            string errorUri = validatingContext.HasError ? validatingContext.ErrorUri : null;

            var memory = new MemoryStream();
            byte[] body;
            using (var writer = new JsonTextWriter(new StreamWriter(memory)))
            {
                writer.WriteStartObject();
                writer.WritePropertyName(Constants.Parameters.Error);
                writer.WriteValue(error);
                if (!string.IsNullOrEmpty(errorDescription))
                {
                    writer.WritePropertyName(Constants.Parameters.ErrorDescription);
                    writer.WriteValue(errorDescription);
                }
                if (!string.IsNullOrEmpty(errorUri))
                {
                    writer.WritePropertyName(Constants.Parameters.ErrorUri);
                    writer.WriteValue(errorUri);
                }
                writer.WriteEndObject();
                writer.Flush();
                body = memory.ToArray();
            }
            Response.StatusCode = 400;
            Response.ContentType = "application/json;charset=UTF-8";
            Response.Headers.Set("Cache-Control", "no-cache");
            Response.Headers.Set("Pragma", "no-cache");
            Response.Headers.Set("Expires", "-1");
            Response.Headers.Set("Content-Length", body.Length.ToString(CultureInfo.InvariantCulture));
            return Response.WriteAsync(body, Request.CallCancelled);
        }

        private Task<bool> SendErrorRedirectAsync(
            OAuthLookupClientContext clientContext,
            BaseValidatingContext<OAuthAuthorizationServerOptions> validatingContext)
        {
            if (clientContext == null)
            {
                throw new ArgumentNullException("clientContext");
            }

            string error = validatingContext.HasError ? validatingContext.Error : Constants.Errors.InvalidRequest;
            string errorDescription = validatingContext.HasError ? validatingContext.ErrorDescription : null;
            string errorUri = validatingContext.HasError ? validatingContext.ErrorUri : null;

            if (!clientContext.IsValidated)
            {
                // write error in response body if client_id or redirect_uri have not been validated
                return SendErrorPageAsync(error, errorDescription, errorUri);
            }

            // redirect with error if client_id and redirect_uri have been validated
            string location = WebUtilities.AddQueryString(clientContext.EffectiveRedirectUri, Constants.Parameters.Error, error);
            if (!string.IsNullOrEmpty(errorDescription))
            {
                location = WebUtilities.AddQueryString(location, Constants.Parameters.ErrorDescription, errorDescription);
            }
            if (!string.IsNullOrEmpty(errorDescription))
            {
                location = WebUtilities.AddQueryString(location, Constants.Parameters.ErrorUri, errorUri);
            }
            Response.Redirect(location);
            // request is handled, does not pass on to application
            return Task.FromResult(true);
        }

        private async Task<bool> SendErrorPageAsync(string error, string errorDescription, string errorUri)
        {
            Response.StatusCode = 400;
            Response.Headers.Set("Cache-Control", "no-cache");
            Response.Headers.Set("Pragma", "no-cache");
            Response.Headers.Set("Expires", "-1");

            if (Options.AuthorizeEndpointDisplaysError)
            {
                Context.Set("oauth.Error", error);
                Context.Set("oauth.ErrorDescription", errorDescription);
                Context.Set("oauth.ErrorUri", errorUri);
                // request is not handled - pass through to application for rendering
                return false;
            }

            var memory = new MemoryStream();
            byte[] body;
            using (var writer = new StreamWriter(memory))
            {
                writer.WriteLine("error: {0}", error);
                if (!string.IsNullOrEmpty(errorDescription))
                {
                    writer.WriteLine("error_description: {0}", errorDescription);
                }
                if (!string.IsNullOrEmpty(errorUri))
                {
                    writer.WriteLine("error_uri: {0}", errorUri);
                }
                writer.Flush();
                body = memory.ToArray();
            }

            Response.ContentType = "text/plain;charset=UTF-8";
            Response.Headers.Set("Content-Length", body.Length.ToString(CultureInfo.InvariantCulture));
            await Response.WriteAsync(body, Request.CallCancelled);
            // request is handled, does not pass on to application
            return true;
        }

        private async Task<OAuthLookupClientContext> ValidateAuthorizeEndpointClientAsync(AuthorizeEndpointRequest authorizeRequest)
        {
            var clientContext = new OAuthLookupClientContext(
                Context,
                Options,
                new ClientDetails
                {
                    ClientId = authorizeRequest.ClientId,
                    RedirectUri = authorizeRequest.RedirectUri
                },
                isValidatingRedirectUri: true,
                isValidatingClientSecret: false);
            await Options.Provider.LookupClient(clientContext);
            return clientContext;
        }

        private async Task<OAuthLookupClientContext> ValidateTokenEndpointClientAsync(TokenEndpointRequest tokenRequest)
        {
            var clientDetails = new ClientDetails();

            // Client Authentication http://tools.ietf.org/html/rfc6749#section-2.3
            // Client Authentication Password http://tools.ietf.org/html/rfc6749#section-2.3.1
            string authorization = Request.Headers.Get("Authorization");
            if (!string.IsNullOrWhiteSpace(authorization) && authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                byte[] data = Convert.FromBase64String(authorization.Substring("Basic ".Length).Trim());
                string text = Encoding.UTF8.GetString(data);
                int delimiterIndex = text.IndexOf(':');
                if (delimiterIndex >= 0)
                {
                    clientDetails.ClientId = text.Substring(0, delimiterIndex);
                    clientDetails.ClientSecret = text.Substring(delimiterIndex + 1);
                }
            }

            if (!string.IsNullOrEmpty(tokenRequest.ClientId))
            {
                if (string.IsNullOrEmpty(clientDetails.ClientId))
                {
                    clientDetails.ClientId = tokenRequest.ClientId;
                }
                else if (!String.Equals(tokenRequest.ClientId, clientDetails.ClientId, StringComparison.Ordinal))
                {
                    // mismatched client id between authentication header and form parameter,
                    // return non-validated context
                    return new OAuthLookupClientContext(
                        Context,
                        Options,
                        clientDetails, isValidatingRedirectUri: false,
                        isValidatingClientSecret: false);
                }
            }

            var clientContext = new OAuthLookupClientContext(
                Context,
                Options,
                clientDetails,
                isValidatingRedirectUri: false,
                isValidatingClientSecret: true);

            await Options.Provider.LookupClient(clientContext);

            return clientContext;
        }

        private class Appender
        {
            private readonly char _delimiter;
            private readonly StringBuilder _sb;
            private bool _hasDelimiter;

            public Appender(string value, char delimiter)
            {
                _sb = new StringBuilder(value);
                _delimiter = delimiter;
                _hasDelimiter = value.IndexOf(delimiter) != -1;
            }

            public Appender Append(string name, string value)
            {
                _sb.Append(_hasDelimiter ? '&' : _delimiter)
                   .Append(Uri.EscapeDataString(name))
                   .Append('=')
                   .Append(Uri.EscapeDataString(value));
                _hasDelimiter = true;
                return this;
            }

            public override string ToString()
            {
                return _sb.ToString();
            }
        }
    }
}
