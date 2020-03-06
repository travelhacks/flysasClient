using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlysasLib
{
    public class SASRestClient
    {
        private AuthResponse _auth = null;
        private readonly HttpClient _client;

        const string apiDomain = "https://api.flysas.com";

        public SASRestClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<bool> Login(string userName, string pwd)
        {
            var request = CreateRequest(apiDomain + "/authorize/oauth/token", HttpMethod.Post);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "U0FTLVVJOg==");
            var formValues = new Dictionary<string, string> { { "username", userName }, { "password", pwd }, { "grant_type", "password" } };
            request.Content = new FormUrlEncodedContent(formValues);

            var res = await DownloadAsync(request);
            var response = Deserialize<AuthResponse>(res);
            _auth = response.error.IsNullOrEmpty() ? response : null;
            return _auth != null;
        }

        public bool LoggedIn => _auth != null;

        public async Task Logout()
        {
            if (LoggedIn)
            {
                var request = CreateRequest(apiDomain + "/customer/signout", HttpMethod.Post, _auth);
                string cont = JsonConvert.SerializeObject(new { customerSessionId = _auth.customerSessionId });
                request.Content = new StringContent(cont, System.Text.Encoding.UTF8, "application/json");
                await DownloadAsync(request);
                _auth = null;
            }
        }

        public async Task<ReservationsResult.Reservations> MyReservations()
        {
            var url = apiDomain + $"/reservation/reservations?customerID={_auth.customerSessionId}";
            var request = CreateRequest(url, HttpMethod.Get, _auth);
            ReservationsResult.Reservations reservations;
            //var res = GetResult<ReservationsResult.Reservations>(request);
            var res = await DownloadAsync(request);

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

        public Task<SearchResult> SearchAsync(SASQuery query)
        {
            var req = CreateRequest(query.GetUrl(), HttpMethod.Get);
            return GetResultAsync<SearchResult>(req);
        }

        public async Task<TransactionRoot> History(int page)
        {
            var url = apiDomain + $"/customer/euroBonus/getAccountInfo?pageNumber={page}&customerSessionId={_auth.customerSessionId}";
            var request = CreateRequest(url, HttpMethod.Get, _auth);
            var res = await GetResultAsync<TransactionRoot>(request);
            return res;
        }

        private HttpRequestMessage CreateRequest(string url, HttpMethod method, AuthResponse authentication = null)
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

        private async Task<DownloadResult> DownloadAsync(HttpRequestMessage request)
        {
            var res = new DownloadResult();
            var task = await _client.SendAsync(request);
            res.Content = await task.Content.ReadAsStringAsync();
            res.Success = task.IsSuccessStatusCode;
            task.Content.Dispose();
            return res;
        }

        private T Deserialize<T>(DownloadResult res) where T : RootBaseClass
        {
            var o = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(res.Content);
            o.json = res.Content;
            o.httpSuccess = res.Success;
            return o;
        }

        private async Task<T> GetResultAsync<T>(HttpRequestMessage req) where T : RootBaseClass
        {
            var res = await DownloadAsync(req);
            return Deserialize<T>(res);
        }

        public class DownloadResult
        {
            public string Content;
            public bool Success;
        }
    }
}
