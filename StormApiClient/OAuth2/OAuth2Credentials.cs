﻿using System;

namespace Enferno.StormApiClient.OAuth2
{
  
    public class OAuth2Credentials
    {
        public OAuth2Credentials(
            string clientId,
            string clientSecretId,
            string scope,
            int applicationId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException($"'{nameof(clientId)}' cannot be null or whitespace.", nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecretId))
            {
                throw new ArgumentException($"'{nameof(clientSecretId)}' cannot be null or whitespace.", nameof(clientSecretId));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException($"'{nameof(scope)}' cannot be null or whitespace.", nameof(scope));
            }

            if (applicationId < 1)
            {
                throw new ArgumentException($"'{nameof(applicationId)}' must be greater then 0.", nameof(applicationId));
            }

            ClientId = clientId;
            ClientSecretId = clientSecretId;
            Scope = scope;
            ApplicationId = applicationId;
        }

        public string ClientId { get; }
        public string ClientSecretId { get; }
        public string Scope { get; }
        public int ApplicationId { get; }
    }
}