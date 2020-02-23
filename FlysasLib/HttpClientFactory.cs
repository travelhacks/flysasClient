using System.Net.Http;
namespace FlysasLib
{
    public static class HttpClientFactory
    {
        public static HttpClientHandler CreateHandler()
        {
            return new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
        }

        public static void SetDefaultHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Host = "api.flysas.com";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible)");
        }
        public static HttpClient CreateClient()
        {
            var client =  new HttpClient(CreateHandler());
            SetDefaultHeaders(client);
            return client;
        }
    }
}
