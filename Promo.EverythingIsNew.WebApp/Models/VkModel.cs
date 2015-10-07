using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Models
{
    public class VkModel
    {
        [JsonProperty("response")]
        public List<Response> Response { get; set; }
    }
    public class Response
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("bdate")]
        public DateTime Birthday { get; set; }
        [JsonProperty("city")]
        public Data City { get; set; }
        [JsonProperty("country")]
        public Data Country { get; set; }
        public string Email { get; set; }
    }

    public class Data
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }
    public class AccessData
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}