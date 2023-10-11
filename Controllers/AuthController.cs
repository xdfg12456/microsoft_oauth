using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using microsoft_oauth.Models;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace microsoft_oauth.Controllers
{
    public class AuthController : Controller
    {
        public void SignIn()
        {
            var authCode = Request.Query["code"].First();
            var accessToken = RetrieveAccessToken(authCode).Result;
            var profile = RetrieveProfile(accessToken).Result;
            Response.HttpContext.Session.SetString("access_token", accessToken);
            Response.HttpContext.Session.SetString("profile", profile.ToString());
            Response.Redirect("http://localhost:3000/home/index");
        }

        async private Task<string> RetrieveAccessToken(string code)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/eebb4dd9-a155-4a43-9f1c-047474641f9c/oauth2/v2.0/token");
            var collection = new List<KeyValuePair<string, string>>
            {
                new("client_id", "bc30b589-c2ca-42ff-9b3d-e2b9d4091993"),
                new("code", code),
                new("scope", "https://graph.microsoft.com/mail.read https://graph.microsoft.com/user.readBasic.all"),
                new("redirect_uri", "http://localhost:3000/auth/signIn"),
                new("grant_type", "authorization_code"),
                new("client_secret","ey28Q~DkX3XJgqB4QCh2o5BzlMV3PZKIjXLw0drd")
            };
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            JObject jsonResult = JObject.Parse(result);
            return jsonResult["access_token"].ToString();
        }

        async private Task<JObject> RetrieveProfile(string accessToken)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            JObject jsonResult = JObject.Parse(result);
            return jsonResult;
        }
    }
}