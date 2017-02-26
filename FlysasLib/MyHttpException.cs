using System;
using System.Net;


namespace FlysasLib
{
    public class MyHttpException : Exception
    {
       public string Json { get; private set; }
       public HttpStatusCode Code { get; private set; }
        public MyHttpException(HttpStatusCode code, string json) : base()
        {
            this.Json = json;
            this.Code = code;
        }
    }
}
