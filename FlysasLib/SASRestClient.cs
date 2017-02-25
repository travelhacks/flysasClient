﻿using System;
using System.Net.Http;
using System.Collections.Generic;

namespace FlysasLib
{
    public class SASRestClient
    {
        AuthResponse auth = null;
        HttpClient client = new HttpClient();
        object padLock = new object();

        AuthResponse Auth
        {
            get
            {
                if (auth == null)
                {
                    lock (padLock)
                    {
                        var request = new HttpRequestMessage()
                        {
                            RequestUri = new Uri("https://www.sas.se/bin/sas/d360/getOauthToken"),
                            Method = HttpMethod.Post
                        };
                        var jSon = downLoad(request);
                        var response = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthResponse>(jSon);
                        auth = response;
                    }
                }
                return auth;
            }
        }

        public bool AnonymousLogin()
        {
            auth = null;
            return Auth != null;
        }

        public void Logout()
        {

        }

        public bool Login(string userName, string pwd)
        {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://api.flysas.com/authorize/oauth/token"),
                Method = HttpMethod.Post                
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic","U0FTLVVJOg==");
            var dict = new Dictionary<string, string> { { "username", userName }, { "password", pwd }, { "grant_type", "password" } };
            request.Content = new FormUrlEncodedContent(dict);
            var jSon = downLoad(request);
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthResponse>(jSon);
            auth =response;
            return true;
        }

        public SearchResult Search(SASQuery query)
        {            
            try
            {
              return search(query);
            }
            catch(Exception ex)
            {
                auth = null;
                return search(query);
            }
        }
      
        private SearchResult search(SASQuery query)
        {            
            var req = new HttpRequestMessage(){ RequestUri = query.GetUrl()};                        
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(Auth.access_token);
            req.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            req.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            req.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
            req.Headers.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
            req.Headers.Connection.Add("keep-alive");
            var jSon = downLoad(req);            
            var res =  Newtonsoft.Json.JsonConvert.DeserializeObject<SearchResult>(jSon);
            res.json = jSon;
            return res;
        }

        public TransactionRoot History(int page)        
        {
            var url = $"https://api.flysas.com/customer/euroBonus/getAccountInfo?pageNumber={page}&customerSessionId={Auth.customerSessionId}";
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),                
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(Auth.access_token);
            var jSon = downLoad(request);
            var res = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionRoot>(jSon);
            return res;
        }

        private string downLoad(HttpRequestMessage request)
        {
            string res = null;
            var task = client.SendAsync(request).ContinueWith(t =>
            {
                HttpResponseMessage response = t.Result;
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new Exception("Bad request");
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                res = readTask.Result;
            });
            task.Wait();
            return res;
        }
    }
}
