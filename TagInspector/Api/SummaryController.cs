using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace TagInspector.Api
{
    public class SummaryController : ApiController
    {
        public InstanceCache Cache = new InstanceCache(TimeSpan.FromHours(1));

        [Route("api/summary")]
        public async Task<HtmlSummary> Get(string url, bool useCache = true)
        {
            Uri uri = null;

            if (url.IsNullOrWhiteSpace()) 
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var requestUrl = uri.ToString();

            if (useCache)
            {
                var cachedItem = Cache.Get<HtmlSummary>(requestUrl);
                if (cachedItem != null) return cachedItem;
            }

            HtmlSummary summary;
            try
            {
                summary = await HtmlSummary.GenerateSummary(uri);
                Cache.Set(requestUrl, summary);
            }
            catch (HttpRequestException ex)
            {
                summary = new HtmlSummary
                {
                    FailureReason = ex.InnerException.Message
                };
            }

            return summary;
        }
    }
}
