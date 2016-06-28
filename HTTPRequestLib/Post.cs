using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPRequestLib
{
    public class Post
    {
        public static bool HTTPPostRequestAsync(string url, string data, HTTPAsyncCallback callback, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            bool success = false;

            RequestStruct requestStruct;
            if (HTTPBuildPostRequest(url, data, out requestStruct, headers, parameters))
            {
                requestStruct.asyncCallback = callback;
                IAsyncResult r = requestStruct.request.BeginGetResponse(new AsyncCallback(Shared.RequestCallback), requestStruct);
                success = true;
            }

            return success;
        }

        public static bool HTTPPostRequest(string url, string data, out byte[] reply, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            bool success = false;
            reply = new byte[0];

            RequestStruct requestStruct;
            if (HTTPBuildPostRequest(url, data, out requestStruct, headers, parameters))
            {
                success = Shared.DoRequest(ref requestStruct, ref reply);
            }

            return success;
        }

        private static bool HTTPBuildPostRequest(string url, string data, out RequestStruct reqSt, NameValueCollection headers, NameValueCollection parameters)
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
                    req.Method = "POST";
                    req.ContentLength = Encoding.UTF8.GetByteCount(data);

                    Shared.AddHeaders(ref req, headers);

                    StreamWriter stmw = new StreamWriter(req.GetRequestStream());
                    stmw.Write(data);
                    stmw.Flush();

                    reqSt.request = req;

                    success = true;

                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (count < 3)
                        Thread.Sleep(1000);
                    else
                        ExceptionHistory.lastException = new HTTPPostCreateRequestException(exceptions.ToArray());
                }
            }

            return success;
        }
    }
}
