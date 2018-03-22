// Copyright 2018 Louis S.Berman.
//
// This file is part of HookLineAndSinker.
//
// HookLineAndSinker is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or (at your option) 
// any later version.
//
// HookLineAndSinker is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for 
// more details.
//
// You should have received a copy of the GNU General Public License along with 
// HookLineAndSinker.  If not, see <http://www.gnu.org/licenses/>.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HookLineAndSinker
{
    public static class PayloadEmailer
    {
        [FunctionName("PayloadEmailer")]
        public static async Task<HttpResponseMessage> Run(TraceWriter log,
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req)
        {
            if (req.Content.Headers.ContentType.MediaType != "application/json")
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    "The function only accepts JSON web-hooks!");
            }

            var keyValues = req.GetQueryNameValuePairs()
                .ToDictionary(d => d.Key.ToLower(), d => d.Value);

            if (!keyValues.TryGetValue("email", out string email))
                return MissingQueryValue(req, "email");

            if (!keyValues.TryGetValue("subject", out string subject))
                subject = "[HookLineAndSinker] WebHook Received";

            if(string.IsNullOrWhiteSpace(subject))
                return MissingQueryValue(req, "subject");

            var data = await req.Content.ReadAsAsync<JObject>();

            await SendEmail(email, subject, data);

            var text = $"An email payload (body) was forwarded to {email}";

            log.Info($"{text} (Subject: {subject}, JSON: {data.ToString(Formatting.None)})");

            return req.CreateResponse(HttpStatusCode.OK, text);
        }

        private static HttpResponseMessage MissingQueryValue(
            HttpRequestMessage req, string key)
        {
            var prefix = "a";

            if ("aeiou".IndexOf(key[0]) != -1)
                prefix += "n";

            return req.CreateResponse(HttpStatusCode.BadRequest,
                $"The query string did not include {prefix} \"{key}\" parameter!");
        }

        private static async Task SendEmail(string email, string subject, JObject data)
        {
            var client = new SendGridClient(
                ConfigurationManager.AppSettings["SendGridApiKey"]);

            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(
                ConfigurationManager.AppSettings["EmailFrom"]));

            var recipients = new List<EmailAddress> { new EmailAddress(email) };

            msg.AddTos(recipients);

            msg.SetSubject(subject);

            msg.AddAttachment("Event.json", 
                ToBase64(data.ToString(Formatting.Indented)), "application/json");

            msg.AddContent(MimeType.Text, data.ToString(Formatting.None));

            var response = await client.SendEmailAsync(msg);
        }

        private static string ToBase64(string value) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }
}
