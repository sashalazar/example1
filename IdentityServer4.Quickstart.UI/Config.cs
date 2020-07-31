// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4.Quickstart.UI
{
    public static class Config
    {
        private static readonly string _apiName = $"api1";

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                // new IdentityResource("claims", new[] { "given_name" })
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(name: _apiName),
            };

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(_apiName, _apiName)
                {
                    UserClaims = { JwtClaimTypes.Role, JwtClaimTypes.Name, JwtClaimTypes.Id },
                    Scopes = { _apiName }
                },
                //new ApiResource
                //{
                //    Name = _apiName,
                //    Scopes = { _apiName }
                //}
            };
        }

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // machine to machine client (from quickstart 1)
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = {new Secret("secret".Sha256())},

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { _apiName }
                },
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = {new Secret("secret".Sha256())},

                    AllowedGrantTypes = GrantTypes.Code,

                    RequirePkce = true,
                    RefreshTokenUsage = TokenUsage.ReUse,

                    // where to redirect to after login
                    RedirectUris = {"https://localhost:5002/signin-oidc"},

                    // where to redirect to after logout
                    PostLogoutRedirectUris = {"https://localhost:5002/signout-callback-oidc"},

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "claims"
                    }
                },

                new Client
                {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    //AllowAccessTokensViaBrowser = true,

                    RedirectUris = {"https://localhost:5003/callback.html"},
                    PostLogoutRedirectUris = {"https://localhost:5003/index.html"},
                    AllowedCorsOrigins = {"https://localhost:5003"},

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        _apiName
                    }
                },

                new Client
                {
                    ClientId = "spa",
                    ClientName = "Spa Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    //AllowAccessTokensViaBrowser = true,

                    RedirectUris = {"https://localhost:5005/callback", "https://localhost:5005/silent_renew.html"},
                    PostLogoutRedirectUris = {"https://localhost:5005/"},
                    AllowedCorsOrigins = {"https://localhost:5005"},

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        _apiName
                    }
                }
            };

        public static IEnumerable<IdentityRole<int>> GetRoles()
        {
            return new List<IdentityRole<int>>()
            {
                new IdentityRole<int> { Name = "Admin" },
                new IdentityRole<int> { Id = 12, Name = "Vendor" },
                new IdentityRole<int> { Id = 13, Name = "Buyer" },
                new IdentityRole<int> { Id = 14, Name = "Processing" }
            };
        }
    }
}