using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FlysasLib
{
    public class SASRestClient
    {
        AuthResponse auth = null;
        HttpClient client = new HttpClient();        

        public SASRestClient()
        {
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));            
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Host = "api.flysas.com";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible)");            
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
            return string.IsNullOrEmpty(response.error);
        }

        public bool LoggedIn => auth != null && auth.customerSessionId.IsNullOrEmpty() == false;
        
        public void Logout()
        {
            if (LoggedIn)
            {
                var request = createRequest("https://api.flysas.com/customer/signout", HttpMethod.Post,auth);                
                string cont = "{ \"customerSessionId\" : \"" + auth.customerSessionId + "\"}";
                string cont2 = JsonConvert.SerializeObject(new { customerSessionId = auth.customerSessionId });
                request.Content = new StringContent(cont, System.Text.Encoding.UTF8, "application/json");
                var res = downLoad(request);
                auth = null;
            }
        }

        public SearchResult Search(SASQuery query)
        {
            var req = createRequest(query.GetUrl(), HttpMethod.Get);
            return GetResult<SearchResult>(req);            
        }
        public Task<SearchResult>  SearchAsync(SASQuery query)
        {
            var req = createRequest(query.GetUrl(), HttpMethod.Get);
            return GetResultAsync<SearchResult>(req);
        }
        public TransactionRoot History(int page)        
        {
            var url = $"https://api.flysas.com/customer/euroBonus/getAccountInfo?pageNumber={page}&customerSessionId={auth.customerSessionId}";
            var request = createRequest(url, HttpMethod.Get,auth);                                    
            var res = GetResult<TransactionRoot>(request);
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

        async Task<DownloadResult> downLoadAsync(HttpRequestMessage request)
        {
            var res = new DownloadResult();
            var task = await client.SendAsync(request);
            res.Content = await task.Content.ReadAsStringAsync();
            res.Success = task.IsSuccessStatusCode;
            task.Content.Dispose();
            return res;
        }

        T Deserialize<T>(DownloadResult res) where T : FlysasLib.RootBaseClass
        {
            var o = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(res.Content);
            o.json = res.Content;
            o.httpSuccess = res.Success;
            return o;
        }
        T GetResult<T>(HttpRequestMessage req) where T : FlysasLib.RootBaseClass
        {
            return Deserialize<T>(downLoad(req));
        }
        async Task<T> GetResultAsync<T>(HttpRequestMessage req) where T : FlysasLib.RootBaseClass
        {
            var res = await downLoadAsync(req);
            return Deserialize<T>(res);
        }

        class DownloadResult
        {
            public string Content;
            public bool Success;
        }
    }
}
