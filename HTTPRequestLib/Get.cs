using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPRequestLib
{
    public class Get
    {
        public static bool HTTPGetRequestAsync(string url, HTTPAsyncCallback callback, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            bool success = false;

            RequestStruct requestStruct;
            if (HTTPBuildGetRequest(url, out requestStruct, headers, parameters))
            {
                requestStruct.asyncCallback = callback;
                IAsyncResult r = requestStruct.request.BeginGetResponse(new AsyncCallback(Shared.RequestCallback), requestStruct);
                success = true;
            }

            return success;
        }
        public static bool HTTPGetRequest(string url, out byte[] reply, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            bool success = false;
            reply = new byte[0];

            RequestStruct requestStruct;
            if (HTTPBuildGetRequest(url, out requestStruct, headers, parameters))
            {
                success = Shared.DoRequest(ref requestStruct, ref reply);
            }

            return success;
        }

        private static bool HTTPBuildGetRequest(string url, out RequestStruct reqSt, NameValueCollection headers, NameValueCollection parameters)
        {

            bool success = false;
            reqSt = null;
            int count = 0;
            List<Exception> exceptions = new List<Exception>();

            while (!success && count++ < 3)
            {
                try
                {
                    reqSt = new RequestStruct();

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Shared.GetUrl(url, parameters));
                    req.Method = "GET";

                    Shared.AddHeaders(ref req, headers);

                    reqSt.request = req;

                    success = true;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (count < 3)
                        Thread.Sleep(1000);
                    else
                        ExceptionHistory.lastException = new HTTPGetCreateRequestException(exceptions.ToArray());
                    
                }
            }

            return success;
        }
    }
}
