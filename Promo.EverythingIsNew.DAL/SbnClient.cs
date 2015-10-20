using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http.Headers;
using Promo.EverythingIsNew.DAL;
using System.Net;
namespace Promo.EverythingIsNew.DAL
{
    public class CbnClient
    {
        private string UsssUrl;
        private string UsssUser;
        private string UsssPassword;
        private HttpClient client;

        public CbnClient(string usssUrl, string usssUser, string usssPassword)
        {
            this.UsssUrl = usssUrl;
            this.UsssUser = usssUser;
            this.UsssPassword = usssPassword;
            this.client = new HttpClient();

            client.BaseAddress = new Uri(UsssUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var byteArray = Encoding.ASCII.GetBytes(UsssUser + ":" + UsssPassword);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
        
        public async Task<StatusResult> GetStatus(Status status)
        {
            var request = String.Format("status?ctn={0}&uid={1}", status.ctn, status.uid);
            HttpResponseMessage response = await client.GetAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<StatusResult>();
                return result;
            }
            new CbnException(response.ToString());
            return null;
        }

        public async Task<MessageResult> PostMessage(Message message)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("message", message);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<MessageResult>();
                return result;
            }
            new CbnException(response.ToString());
            return null;
        }

        public async Task<UpdateResult> Update(Update update)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("update", update);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<UpdateResult>();
                return result;
            }
            new CbnException(response.ToString());
            return null;
        }


    }
}
