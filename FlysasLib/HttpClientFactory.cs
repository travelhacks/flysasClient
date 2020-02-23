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
        public static HttpClient CreateClient()
        {
            return new HttpClient(CreateHandler());            
        }
    }
}
