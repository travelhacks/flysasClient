using System;
using System.Net.Http;
using System.Collections.Generic;



namespace FlysasLib
{
    public class SASRestClient
    {
        AuthResponse auth = null;
        HttpClient client = new HttpClient();        

        public SASRestClient()
        {

            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
            client.DefaultRequestHeaders.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Host = "api.flysas.com";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible)");
            //client.DefaultRequestHeaders.Add("Origin", "https://www.sas.se");
            //client.DefaultRequestHeaders.Referrer  = new Uri("https://www.sas.se");
        }              
        
        public bool Login(string userName, string pwd)
        {
            var request = createRequest("https://api.flysas.com/authorize/oauth/token", HttpMethod.Post);            
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "U0FTLVVJOg==");
            var dict = new Dictionary<string, string> { { "username", userName }, { "password", pwd }, { "grant_type", "password" } };
            request.Content = new FormUrlEncodedContent(dict);
                        
            var res = downLoad(request);
            var response = Deserialize<AuthResponse>(res);
            auth = response;
            return response.errors == null;
        }

        public bool LoggedIn
        {
            get {
                return auth != null && !string.IsNullOrEmpty(auth.customerSessionId);
            }
        }

        public void Logout()
        {
            if (LoggedIn)
            {
                var request = createRequest("https://api.flysas.com/customer/signout", HttpMethod.Post,auth);                
                string cont = "{ \"customerSessionId\" : \"" + auth.customerSessionId + "\"}";
                request.Content = new StringContent(cont, System.Text.Encoding.UTF8, "application/json");
                var res = downLoad(request);
                auth = null;
            }
        }

        public SearchResult Search(SASQuery query)
        {
            try
            {
                return search(query);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
      
        private SearchResult search(SASQuery query)
        {
            var req = createRequest(query.GetUrl(), HttpMethod.Get);
            return GetRusult<SearchResult>(req);            
        }       

        public TransactionRoot History(int page)        
        {
            var url = $"https://api.flysas.com/customer/euroBonus/getAccountInfo?pageNumber={page}&customerSessionId={auth.customerSessionId}";
            var request = createRequest( url, HttpMethod.Get,auth);                                    
            var res = GetRusult<TransactionRoot>(request);
            return res;
        }

        HttpRequestMessage createRequest(string url, HttpMethod method,AuthResponse authentication = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = method
            };
            if(authentication != null)
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authentication.access_token);
            return request;
        }

        DownloadResult downLoad(HttpRequestMessage request)
        {
            var res = new DownloadResult();
            var task = client.SendAsync(request);
            task.Wait();
            task.Result.Content.ReadAsStringAsync().ContinueWith(t =>
            {
                res.Content = t.Result;
            }
            ).Wait();
            task.Result.Content.Dispose();
            res.Success = task.Result.IsSuccessStatusCode;
            return res;
        }

        T Deserialize<T>(DownloadResult res) where T : FlysasLib.RootBaseClass
        {
            var o = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(res.Content);
            o.json = res.Content;
            o.httpSuccess = res.Success;
            return o;
        }
        T GetRusult<T>(HttpRequestMessage req) where T : FlysasLib.RootBaseClass
        {
            return Deserialize<T>(downLoad(req));
        }

        

        class DownloadResult
        {
            public string Content;
            public bool Success;
        }
    }
}
