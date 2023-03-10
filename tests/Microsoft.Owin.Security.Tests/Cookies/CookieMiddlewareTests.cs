// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Security.Tests
{
    public class CookieMiddlewareTests
    {
        [Fact]
        public async Task NormalRequestPassesThrough()
        {
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
            });
            HttpResponseMessage response = await server.HttpClient.GetAsync("http://example.com/normal");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ProtectedRequestShouldRedirectToLogin()
        {
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/login")
            });

            Transaction transaction = await SendAsync(server, "http://example.com/protected");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);

            Uri location = transaction.Response.Headers.Location;
            location.LocalPath.ShouldBe("/login");
            location.Query.ShouldBe("?ReturnUrl=%2Fprotected");
        }

        [Fact]
        public async Task ProtectedCustomRequestShouldRedirectToCustomLogin()
        {
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/login")
            });

            Transaction transaction = await SendAsync(server, "http://example.com/protected/CustomRedirect");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);

            Uri location = transaction.Response.Headers.Location;
            location.ToString().ShouldBe("/CustomRedirect");
        }

        private Task SignInAsAlice(IOwinContext context)
        {
            context.Authentication.SignIn(
                new AuthenticationProperties(),
                new ClaimsIdentity(new GenericIdentity("Alice", "Cookies")));
            return Task.FromResult<object>(null);
        }

        private Task SignInAsAlicePersistent(IOwinContext context)
        {
            context.Authentication.SignIn(
                new AuthenticationProperties() { IsPersistent = true },
                new ClaimsIdentity(new GenericIdentity("Alice", "Cookies")));
            return Task.FromResult<object>(null);
        }

        [Fact]
        public async Task SignInCausesDefaultCookieToBeCreated()
        {
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/login"),
                CookieName = "TestCookie",
            }, SignInAsAlice);

            Transaction transaction = await SendAsync(server, "http://example.com/testpath");

            string setCookie = transaction.SetCookie;
            setCookie.ShouldStartWith("TestCookie=");
            setCookie.ShouldContain("; path=/");
            setCookie.ShouldContain("; HttpOnly");
            setCookie.ShouldNotContain("; expires=");
            setCookie.ShouldNotContain("; domain=");
            setCookie.ShouldNotContain("; secure");
        }

        [Theory]
        [InlineData(CookieSecureOption.Always, "http://example.com/testpath", true)]
        [InlineData(CookieSecureOption.Always, "https://example.com/testpath", true)]
        [InlineData(CookieSecureOption.Never, "http://example.com/testpath", false)]
        [InlineData(CookieSecureOption.Never, "https://example.com/testpath", false)]
        [InlineData(CookieSecureOption.SameAsRequest, "http://example.com/testpath", false)]
        [InlineData(CookieSecureOption.SameAsRequest, "https://example.com/testpath", true)]
        public async Task SecureSignInCausesSecureOnlyCookieByDefault(
            CookieSecureOption cookieSecureOption,
            string requestUri,
            bool shouldBeSecureOnly)
        {
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/login"),
                CookieName = "TestCookie",
                CookieSecure = cookieSecureOption
            }, SignInAsAlice);

            Transaction transaction = await SendAsync(server, requestUri);
            string setCookie = transaction.SetCookie;

            if (shouldBeSecureOnly)
            {
                setCookie.ShouldContain("; secure");
            }
            else
            {
                setCookie.ShouldNotContain("; secure");
            }
        }

        [Fact]
        public async Task CookieOptionsAlterSetCookieHeader()
        {
            TestServer server1 = CreateServer(new CookieAuthenticationOptions
            {
                CookieName = "TestCookie",
                CookiePath = "/foo",
                CookieDomain = "another.com",
                CookieSecure = CookieSecureOption.Always,
                CookieHttpOnly = true,
            }, SignInAsAlice);

            Transaction transaction1 = await SendAsync(server1, "http://example.com/testpath");

            TestServer server2 = CreateServer(new CookieAuthenticationOptions
            {
                CookieName = "SecondCookie",
                CookieSecure = CookieSecureOption.Never,
                CookieHttpOnly = false,
            }, SignInAsAlice);

            Transaction transaction2 = await SendAsync(server2, "http://example.com/testpath");

            string setCookie1 = transaction1.SetCookie;
            string setCookie2 = transaction2.SetCookie;

            setCookie1.ShouldContain("TestCookie=");
            setCookie1.ShouldContain(" path=/foo");
            setCookie1.ShouldContain(" domain=another.com");
            setCookie1.ShouldContain(" secure");
            setCookie1.ShouldContain(" HttpOnly");

            setCookie2.ShouldContain("SecondCookie=");
            setCookie2.ShouldNotContain(" domain=");
            setCookie2.ShouldNotContain(" secure");
            setCookie2.ShouldNotContain(" HttpOnly");
        }

        [Fact]
        public async Task CookieContainsIdentity()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock
            }, SignInAsAlice);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");
        }

        [Fact]
        public async Task CookieStopsWorkingAfterExpiration()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = false,
            }, SignInAsAlice);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            clock.Add(TimeSpan.FromMinutes(7));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            clock.Add(TimeSpan.FromMinutes(7));

            Transaction transaction4 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");
            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe("Alice");
            transaction4.SetCookie.ShouldBe(null);
            FindClaimValue(transaction4, ClaimTypes.Name).ShouldBe(null);
        }

        [Fact]
        public async Task CookieExpirationCanBeOverridenInSignin()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = false,
            },
            context =>
            {
                context.Authentication.SignIn(
                    new AuthenticationProperties() { ExpiresUtc = clock.UtcNow.Add(TimeSpan.FromMinutes(5)) },
                    new ClaimsIdentity(new GenericIdentity("Alice", "Cookies")));
                return Task.FromResult<object>(null);
            });

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            clock.Add(TimeSpan.FromMinutes(3));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            clock.Add(TimeSpan.FromMinutes(3));

            Transaction transaction4 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");
            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe("Alice");
            transaction4.SetCookie.ShouldBe(null);
            FindClaimValue(transaction4, ClaimTypes.Name).ShouldBe(null);
        }

        [Fact]
        public async Task CookieExpirationCanBeOverridenInEvent()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = false,
                Provider = new CookieAuthenticationProvider()
                {
                    OnResponseSignIn = context =>
                    {
                        context.Properties.ExpiresUtc = clock.UtcNow.Add(TimeSpan.FromMinutes(5));
                    }
                }
            }, SignInAsAlice);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            clock.Add(TimeSpan.FromMinutes(3));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            clock.Add(TimeSpan.FromMinutes(3));

            Transaction transaction4 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");
            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe("Alice");
            transaction4.SetCookie.ShouldBe(null);
            FindClaimValue(transaction4, ClaimTypes.Name).ShouldBe(null);
        }

        [Fact]
        public async Task CookieIsRenewedWithSlidingExpiration()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = true,
            }, SignInAsAlice);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            transaction1.SetCookie.ShouldNotBe(null);
            transaction1.SetCookie.ShouldNotContain("Expires");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            // transaction4 should arrive with a new SetCookie value
            Transaction transaction4 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction4.SetCookie.ShouldNotBe(null);
            transaction4.SetCookie.ShouldNotContain("Expires");
            FindClaimValue(transaction4, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction5 = await SendAsync(server, "http://example.com/me/Cookies", transaction4.CookieNameValue);

            transaction5.SetCookie.ShouldBe(null);
            FindClaimValue(transaction5, ClaimTypes.Name).ShouldBe("Alice");
        }

        [Fact]
        public async Task CookieIsRejectedAtTheSameTimeAsSlidingExpirationRenewal()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = true,
                Provider = new CookieAuthenticationProvider()
                {
                    OnValidateIdentity = context => { context.RejectIdentity(); return Task.FromResult(0); }
                }
            }, SignInAsAlice);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            transaction1.SetCookie.ShouldNotBe(null);
            transaction1.SetCookie.ShouldNotContain("Expires");

            clock.Add(TimeSpan.FromMinutes(6));

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe(null);

            clock.Add(TimeSpan.FromMinutes(5));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe(null);
        }

        [Fact]
        public async Task PersistentCookieIsRenewedWithSlidingExpiration()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = true,
            }, SignInAsAlicePersistent);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            transaction1.SetCookie.ShouldNotBe(null);
            transaction1.SetCookie.ShouldContain("Expires");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            // transaction4 should arrive with a new SetCookie value
            Transaction transaction4 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction4.SetCookie.ShouldNotBe(null);
            transaction4.SetCookie.ShouldContain("Expires");
            FindClaimValue(transaction4, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction5 = await SendAsync(server, "http://example.com/me/Cookies", transaction4.CookieNameValue);

            transaction5.SetCookie.ShouldBe(null);
            FindClaimValue(transaction5, ClaimTypes.Name).ShouldBe("Alice");
        }

        [Fact]
        public async Task SessionStoreCookieIsRenewedWithSlidingExpiration()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = true,
                SessionStore = new TestSessionStore()
            }, SignInAsAlice);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            transaction1.SetCookie.ShouldNotBe(null);
            transaction1.SetCookie.ShouldNotContain("Expires");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            // transaction4 should arrive with a new SetCookie value
            Transaction transaction4 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction4.SetCookie.ShouldNotBe(null);
            transaction4.SetCookie.ShouldNotContain("Expires");
            FindClaimValue(transaction4, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction5 = await SendAsync(server, "http://example.com/me/Cookies", transaction4.CookieNameValue);

            transaction5.SetCookie.ShouldBe(null);
            FindClaimValue(transaction5, ClaimTypes.Name).ShouldBe("Alice");
        }

        [Fact]
        public async Task PersistentSessionStoreCookieIsRenewedWithSlidingExpiration()
        {
            var clock = new TestClock();
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                SystemClock = clock,
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = true,
                SessionStore = new TestSessionStore()
            }, SignInAsAlicePersistent);

            Transaction transaction1 = await SendAsync(server, "http://example.com/testpath");

            transaction1.SetCookie.ShouldNotBe(null);
            transaction1.SetCookie.ShouldContain("Expires");

            Transaction transaction2 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction2.SetCookie.ShouldBe(null);
            FindClaimValue(transaction2, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction3 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction3.SetCookie.ShouldBe(null);
            FindClaimValue(transaction3, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            // transaction4 should arrive with a new SetCookie value
            Transaction transaction4 = await SendAsync(server, "http://example.com/me/Cookies", transaction1.CookieNameValue);

            transaction4.SetCookie.ShouldNotBe(null);
            transaction4.SetCookie.ShouldContain("Expires");
            FindClaimValue(transaction4, ClaimTypes.Name).ShouldBe("Alice");

            clock.Add(TimeSpan.FromMinutes(4));

            Transaction transaction5 = await SendAsync(server, "http://example.com/me/Cookies", transaction4.CookieNameValue);

            transaction5.SetCookie.ShouldBe(null);
            FindClaimValue(transaction5, ClaimTypes.Name).ShouldBe("Alice");
        }

        [Fact]
        public async Task AjaxRedirectsAsExtraHeaderOnTwoHundred()
        {
            TestServer server = CreateServer(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/login")
            });

            Transaction transaction = await SendAsync(server, "http://example.com/protected", ajaxRequest: true);

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var responded = transaction.Response.Headers.GetValues("X-Responded-JSON");

            responded.Count().ShouldBe(1);
            responded.Single().ShouldContain("\"location\"");
        }

        private static string FindClaimValue(Transaction transaction, string claimType)
        {
            XElement claim = transaction.ResponseElement.Elements("claim").SingleOrDefault(elt => elt.Attribute("type").Value == claimType);
            if (claim == null)
            {
                return null;
            }
            return claim.Attribute("value").Value;
        }

        private static async Task<XElement> GetAuthData(TestServer server, string url, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", cookie);

            HttpResponseMessage response2 = await server.HttpClient.SendAsync(request);
            string text = await response2.Content.ReadAsStringAsync();
            XElement me = XElement.Parse(text);
            return me;
        }

        private static TestServer CreateServer(CookieAuthenticationOptions options, Func<IOwinContext, Task> testpath = null)
        {
            return TestServer.Create(app =>
            {
                app.Properties["host.AppName"] = "Microsoft.Owin.Security.Tests";
                app.UseCookieAuthentication(options);
                app.Use(async (context, next) =>
                {
                    IOwinRequest req = context.Request;
                    IOwinResponse res = context.Response;
                    PathString remainder;
                    if (req.Path == new PathString("/normal"))
                    {
                        res.StatusCode = 200;
                    }
                    else if (req.Path == new PathString("/protected"))
                    {
                        res.StatusCode = 401;
                    }
                    else if (req.Path == new PathString("/protected/CustomRedirect"))
                    {
                        context.Authentication.Challenge(new AuthenticationProperties() { RedirectUri = "/CustomRedirect" });
                    }
                    else if (req.Path == new PathString("/me"))
                    {
                        Describe(res, new AuthenticateResult(req.User.Identity, new AuthenticationProperties(), new AuthenticationDescription()));
                    }
                    else if (req.Path.StartsWithSegments(new PathString("/me"), out remainder))
                    {
                        AuthenticateResult result = await context.Authentication.AuthenticateAsync(remainder.Value.Substring(1));
                        Describe(res, result);
                    }
                    else if (req.Path == new PathString("/testpath") && testpath != null)
                    {
                        await testpath(context);
                    }
                    else
                    {
                        await next();
                    }
                });
            });
        }

        private static void Describe(IOwinResponse res, AuthenticateResult result)
        {
            res.StatusCode = 200;
            res.ContentType = "text/xml";
            var xml = new XElement("xml");
            if (result != null && result.Identity != null)
            {
                xml.Add(result.Identity.Claims.Select(claim => new XElement("claim", new XAttribute("type", claim.Type), new XAttribute("value", claim.Value))));
            }
            if (result != null && result.Properties != null)
            {
                xml.Add(result.Properties.Dictionary.Select(extra => new XElement("extra", new XAttribute("type", extra.Key), new XAttribute("value", extra.Value))));
            }
            using (var memory = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(memory, Encoding.UTF8))
                {
                    xml.WriteTo(writer);
                }
                res.Body.Write(memory.ToArray(), 0, memory.ToArray().Length);
            }
        }

        private static async Task<Transaction> SendAsync(TestServer server, string uri, string cookieHeader = null, bool ajaxRequest = false)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
            if (ajaxRequest)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            }
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.HttpClient.SendAsync(request),
            };
            if (transaction.Response.Headers.Contains("Set-Cookie"))
            {
                transaction.SetCookie = transaction.Response.Headers.GetValues("Set-Cookie").SingleOrDefault();
            }
            if (!string.IsNullOrEmpty(transaction.SetCookie))
            {
                transaction.CookieNameValue = transaction.SetCookie.Split(new[] { ';' }, 2).First();
            }
            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }
            return transaction;
        }

        private class Transaction
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }

            public string SetCookie { get; set; }
            public string CookieNameValue { get; set; }

            public string ResponseText { get; set; }
            public XElement ResponseElement { get; set; }
        }

        private class TestSessionStore : IAuthenticationSessionStore
        {
            IDictionary<string, AuthenticationTicket> _store = new Dictionary<string, AuthenticationTicket>(StringComparer.Ordinal);

            public Task RemoveAsync(string key)
            {
                _store.Remove(key);
                return Task.FromResult(0);
            }

            public Task RenewAsync(string key, AuthenticationTicket ticket)
            {
                _store[key] = ticket;
                return Task.FromResult(0);
            }

            public Task<AuthenticationTicket> RetrieveAsync(string key)
            {
                AuthenticationTicket ticket;
                if (_store.TryGetValue(key, out ticket))
                {
                    return Task.FromResult(ticket);
                }
                return Task.FromResult<AuthenticationTicket>(null);
            }

            public Task<string> StoreAsync(AuthenticationTicket ticket)
            {
                var key = Guid.NewGuid().ToString();
                _store[key] = ticket;
                return Task.FromResult(key);
            }
        }
    }
}
