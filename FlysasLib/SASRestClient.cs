using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlysasLib
{
    public class SASRestClient
    {
        private AuthResponse auth = null;
        private readonly HttpClient _client;

        string apiDomain = "https://api.flysas.com";
        public SASRestClient(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            _client.DefaultRequestHeaders.Connection.Add("keep-alive");
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            _client.DefaultRequestHeaders.Host = "api.flysas.com";
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible)");
        }

        public bool Login(string userName, string pwd)
        {
            var request = createRequest(apiDomain + "/authorize/oauth/token", HttpMethod.Post);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "U0FTLVVJOg==");
            var dict = new Dictionary<string, string> { { "username", userName }, { "password", pwd }, { "grant_type", "password" } };
            request.Content = new FormUrlEncodedContent(dict);

            var res = downLoad(request);
            var response = Deserialize<AuthResponse>(res);
            auth = response;
            return response.error.IsNullOrEmpty();
        }

        public bool LoggedIn => auth != null && auth.customerSessionId.IsNullOrEmpty() == false;

        public void Logout()
        {
            if (LoggedIn)
            {
                var request = createRequest(apiDomain + "/customer/signout", HttpMethod.Post, auth);
                string cont = JsonConvert.SerializeObject(new { customerSessionId = auth.customerSessionId });
                request.Content = new StringContent(cont, System.Text.Encoding.UTF8, "application/json");
                var res = downLoad(request);
                auth = null;
            }
        }

        public ReservationsResult.Reservations MyReservations()
        {
            var url = apiDomain + $"/reservation/reservations?customerID={auth.customerSessionId}";
            var request = createRequest(url, HttpMethod.Get, auth);
            ReservationsResult.Reservations reservations = new ReservationsResult.Reservations();
            //var res = GetResult<ReservationsResult.Reservations>(request);
            var res = downLoad(request);

            if (res.Success)
            {
                reservations = ReservationsResult.Reservations.FromJson(res.Content);
            }
            else
            {//load blank list so you don't get nullref
                reservations = new ReservationsResult.Reservations();
                reservations.ReservationsReservations = new List<ReservationsResult.Reservation>();

                System.Diagnostics.Debug.WriteLine(res.ToString());
            }


            return reservations;
        }

        public SearchResult Search(SASQuery query)
        {
            var req = createRequest(query.GetUrl(), HttpMethod.Get);
            return GetResult<SearchResult>(req);
        }

        public Task<SearchResult> SearchAsync(SASQuery query)
        {
            var req = createRequest(query.GetUrl(), HttpMethod.Get);
            return GetResultAsync<SearchResult>(req);
        }

        public TransactionRoot History(int page)
        {
            var url = apiDomain + $"/customer/euroBonus/getAccountInfo?pageNumber={page}&customerSessionId={auth.customerSessionId}";
            var request = createRequest(url, HttpMethod.Get, auth);
            var res = GetResult<TransactionRoot>(request);
            return res;
        }

        HttpRequestMessage createRequest(string url, HttpMethod method, AuthResponse authentication = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = method
            };
            if (authentication != null)
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authentication.access_token);
            return request;
        }

        DownloadResult downLoad(HttpRequestMessage request)
        {
            var res = new DownloadResult();
            var task = _client.SendAsync(request);
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
            var task = await _client.SendAsync(request);
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
