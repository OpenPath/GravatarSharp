﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GravatarSharp.Core.Json;
using GravatarSharp.Core.Model;
using Newtonsoft.Json;

namespace GravatarSharp.Core
{
    /// <summary>
    /// Get a Gravatar user profile and a Gravatar image url by using this class.
    /// </summary>
    public class GravatarController
    {
        /// <summary>
        /// The user agent used in the HTTP request headers
        /// </summary>
        public string UserAgent => string.Format("GravatarSharp/1.0 ({0}; {1})",
            Environment.OSVersion.Platform, Environment.OSVersion.VersionString);

        /// <summary>
        /// Gets the Gravatar profile for the given user/email
        /// </summary>
        /// <param name="email">The email that will be used to request the user profile</param>
        /// <returns>The user profile corresponding to the provided email address</returns>
        public async Task<GetProfileResult> GetProfile(string email)
        {
            var json = await GetStringResponse(string.Format("https://en.gravatar.com/{0}.json", Hashing.CalculateMd5Hash(email)));
            if (string.IsNullOrEmpty(json.ErrorMessage))
                return new GetProfileResult
                {
                    Profile = new GravatarProfile(JsonConvert.DeserializeObject<Profile>(json.Result), json.Result)
                };
            return new GetProfileResult
            {
                ErrorMessage = json.ErrorMessage
            };
        }

        /// <summary>
        /// Gets the Gravatar image url for the given user/email
        /// </summary>
        /// <param name="email">The email that will be used to request the user image url</param>
        /// <param name="width">The width in pixels. Default is 128</param>
        /// <returns>The image url corresponding to the provided email address</returns>
        public static string GetImageUrl(string email, int width = 128)
        {
            return string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=identicon&r=PG", Hashing.CalculateMd5Hash(email), width);
        }

        private class HttpStringResponse
        {
            public string Result { get; set; }
            public string ErrorMessage { get; set; }
        }

        private async Task<HttpStringResponse> GetStringResponse(string uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    var result = await httpClient.GetStringAsync(uri);
                    return new HttpStringResponse
                    {
                        Result = result
                    };
                }
                catch (HttpRequestException httpRequestException)
                {
                    return new HttpStringResponse
                    {
                        ErrorMessage = httpRequestException.Message
                    };
                }
            }
        }
    }
}
