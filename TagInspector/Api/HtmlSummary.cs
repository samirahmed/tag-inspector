using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TagInspector.Api
{
    public class HtmlSummary
    {
        public static readonly string[] HtmlMediaTypes = {"application/html", "text/html"};

        public HttpStatusCode StatusCode { get; set; }

        public string FailureReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Url { get; set; }

        public double PageLoadTime { get; set; }

        public Dictionary<string, int> Frequency { get; set; }

        public string Body { get; set; }

        public HtmlSummary()
        {
            this.Body = "";
            this.CreatedAt = DateTime.UtcNow;
            this.PageLoadTime = -1;
            this.Frequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public HtmlSummary(HttpStatusCode statusCode, string url, string body, string mediaType)
            : this()
        {
            this.StatusCode = statusCode;
            this.Url = url;

            if (((int)statusCode)/100 != 2)
            {
                this.FailureReason = string.Format(@"Remote Server returned status code {0} {1}",
                    (int) statusCode, statusCode);
                return;
            }

            if (mediaType.IsNullOrWhiteSpace() ||
                !HtmlMediaTypes.Any(_ => _.Equals(mediaType, StringComparison.OrdinalIgnoreCase)))
            {
                this.FailureReason = string.Format
                    (@"The server at URL {0} returned non-HTML media type '{1}'", this.Url, mediaType);
                return;
            }

            var doc = new HtmlDocument {OptionFixNestedTags = true};
            doc.LoadHtml(body);

            CountFrequency(doc.DocumentNode, this.Frequency);

            using (var writer = new StringWriter())
            {
                doc.Save(writer);
                this.Body = writer.GetStringBuilder().ToString();
            }
        }

        public static void CountFrequency(HtmlNode node, Dictionary<string, int> frequency)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                if (!frequency.ContainsKey(node.Name)) frequency[node.Name] = 0;
                frequency[node.Name]++;
            }

            foreach (var child in node.ChildNodes) CountFrequency(child, frequency);
        }

        public static async Task<HtmlSummary> GenerateSummary(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var timer = Stopwatch.StartNew();
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
                var duration = timer.Elapsed.TotalMilliseconds;

                var body = await response.Content.ReadAsStringAsync();
                var mediaType = response.Content.Headers.ContentType.IfNotNull(_ => _.MediaType);
                
                var summary = new HtmlSummary(response.StatusCode, uri.ToString(), body, mediaType)
                {
                    PageLoadTime = duration
                };
                
                return summary;
            }
        }
    }
}