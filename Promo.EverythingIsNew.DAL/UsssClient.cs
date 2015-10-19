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
    public class UsssClient
    {
        public static string UsssUrl;
        public static string UsssUser;
        public static string UsssPassword;

        static void Main(string[] args)
        {
            RunAsync().Wait();
            Console.WriteLine("Press any key to end...");
            Console.ReadKey();
        }

        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                Console.WriteLine("Start request");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                client.BaseAddress = new Uri(UsssUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var byteArray = Encoding.ASCII.GetBytes(UsssUser + ":" + UsssPassword);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                GetStatus(client, "3", "3").Wait();
                GetStatus(client, "3", "2").Wait();
                GetStatus(client, "2", "3").Wait();
                GetStatus(client, "2", "2").Wait();
                PostMessage(client, "2", "2", "aaa@aa.ru").Wait();
                GetStatus(client, "2", "2").Wait();

                Console.WriteLine("End requests");
            }
        }

        static async Task GetStatus(HttpClient client, string ctn, string uid)
        {
            var request = String.Format("status?ctn={0}&uid={1}", ctn, uid);
            HttpResponseMessage response = await client.GetAsync(request);
            if (response.IsSuccessStatusCode)
            {
                StatusResult result = await response.Content.ReadAsAsync<StatusResult>();
                Console.WriteLine("is_used_ctn: {0}\t is_used_uid: {1}\t is_beeline_subscriber: {2}", result.is_used_ctn, result.is_used_uid, result.is_beeline_subscriber);
                Console.WriteLine();
            }
            else
            {
                Print(response);
            }
        }

        static async Task PostMessage(HttpClient client, string ctn, string uid, string email)
        {
            // HTTP POST
            var message = new Message() { ctn = ctn, uid = uid, email = email };
            HttpResponseMessage response = await client.PostAsJsonAsync("message", message);
            if (response.IsSuccessStatusCode)
            {
                MessageResult result = await response.Content.ReadAsAsync<MessageResult>();
                Console.WriteLine("is_message_sent: {0}\t description: {1}\t code: {2}", result.is_message_sent, result.description, result.code);
                Console.WriteLine();
            }
            else
            {
                Print(response);
            }
        }


        private static void Print(HttpResponseMessage response)
        {
            Console.WriteLine("Request message: " + response.RequestMessage);
            Console.WriteLine();
            Console.WriteLine("Request result: " + response);
            Console.WriteLine();
        }
    }
}
