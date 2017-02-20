using System;
using System.Net;
using System.Net.Http;


namespace FlysasLib
{
    public class SASRestClient
    {
        string accessToken;
        HttpClient client = new HttpClient();        
        object padLock = new object();
       
        string AccessToken
        {
            get
            {
                if (accessToken == null)
                {
                    lock (padLock)
                    {
                        var req = new HttpRequestMessage()
                        {
                            RequestUri = new Uri("https://www.sas.se/bin/sas/d360/getOauthToken"),
                            Method = HttpMethod.Post                            
                        };
                        var task = client.SendAsync(req).ContinueWith(t =>
                        {
                            var stringTask = t.Result.Content.ReadAsStringAsync();
                            stringTask.Wait();
                            var auth = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthResponse>(stringTask.Result);                            
                            accessToken = auth.access_token;
                        });
                        task.Wait();
                    }
                }                
                return accessToken;
            }
        }

        public SearchResult Search(SASQuery query)
        {
            
            try
            {
              return search(query);
            }
            catch(Exception ex)
            {
                accessToken = null;
                return search(query);
            }
        }
        private SearchResult search(SASQuery query)
        {
            SearchResult root = null;
            var req = new HttpRequestMessage(){ RequestUri = query.GetUrl()};                        
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AccessToken);
            req.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            req.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            req.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
            req.Headers.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
            req.Headers.Connection.Add("keep-alive");

            var task = client.SendAsync(req).ContinueWith(t =>
            {
                HttpResponseMessage response = t.Result;
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new Exception("Bad request");
                var stringTask = response.Content.ReadAsStringAsync();                
                stringTask.Wait();
                root = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchResult>(stringTask.Result);
            });
            task.Wait();            
            return root;
        }
    }
}
