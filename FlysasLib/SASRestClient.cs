using System;
using System.Net.Http;
using System.Collections.Generic;

namespace FlysasLib
{
    public class SASRestClient
    {
        AuthResponse auth = null;
        HttpClient client = new HttpClient();
        object padLock = new object();

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


            //Headers from chrome
            //Accept: */*
            //Accept-Encoding:gzip, deflate, sdch, br
            //Accept-Language:sv-SE,sv;q=0.8,en-US;q=0.6,en;q=0.4
            //Access-Control-Request-Headers:authorization
            //Access-Control-Request-Method:GET
            //Cache-Control:no-cache
            //Connection:keep-alive
            //Host:api.flysas.com
            //Origin:https://www.sas.se
            //Pragma:no-cache
            //Referer:https://www.sas.se/
            //User-Agent:Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36
        }              
        
        public bool Login(string userName, string pwd)
        {
            var request = createRequest(new Uri("https://api.flysas.com/authorize/oauth/token"), HttpMethod.Post);            
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "U0FTLVVJOg==");
            var dict = new Dictionary<string, string> { { "username", userName }, { "password", pwd }, { "grant_type", "password" } };
            request.Content = new FormUrlEncodedContent(dict);
                        
            var jSon = downLoad(request);
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthResponse>(jSon);
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
                var request = createRequest(new Uri("https://api.flysas.com/customer/signout"), HttpMethod.Post,auth);                
                string cont = "{ \"customerSessionId\" : \"" + auth.customerSessionId + "\"}";
                request.Content = new StringContent(cont, System.Text.Encoding.UTF8, "application/json");
                var jSon = downLoad(request);
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
            var jSon = downLoad(req);            
            var res =  Newtonsoft.Json.JsonConvert.DeserializeObject<SearchResult>(jSon);
            res.json = jSon;
            return res;
        }

        public TransactionRoot History(int page)        
        {
            var url = $"https://api.flysas.com/customer/euroBonus/getAccountInfo?pageNumber={page}&customerSessionId={auth.customerSessionId}";
            var request = createRequest( new Uri(url), HttpMethod.Get,auth);                        
            var jSon = downLoad(request);
            var res = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionRoot>(jSon);
            return res;
        }

        HttpRequestMessage createRequest(Uri uri, HttpMethod method,AuthResponse authentication = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = method
            };
            if(authentication != null)
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authentication.access_token);
            return request;
        }

        private string downLoad(HttpRequestMessage request)
        {
            string res = null;
            var task = client.SendAsync(request);
            task.Wait();
            task.Result.Content.ReadAsStringAsync().ContinueWith(t =>
            {
                res = t.Result;
            }
            ).Wait();
            task.Result.Content.Dispose();
            if (!task.Result.IsSuccessStatusCode)
                throw new MyHttpException(task.Result.StatusCode, res);
            return res;
        }
    }
}
