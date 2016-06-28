using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPRequestLib
{
    public delegate void HTTPAsyncCallback(byte[] ba);
    class RequestStruct
    {
        public WebRequest request;
        public WebResponse response;
        public Stream responseStream;
        public byte[] responsedata;
        public HTTPAsyncCallback asyncCallback;

        public RequestStruct()
        {
            request = null;
            responseStream = null;
            response = null;
            asyncCallback = null;
            responsedata = new byte[0];
        }

    }
    internal class Shared
    {
        internal static string GetUrl(string url, NameValueCollection parameters)
        {
            if (parameters != null)
            {
                if (parameters.Count > 0)
                {
                    bool first = false;
                    foreach (string param in parameters.AllKeys)
                        url += (first ? "&" : "?") + param + "=" + parameters[param];
                }
            }

            return url;
        }

        internal static void AddHeaders(ref HttpWebRequest req, NameValueCollection headers)
        {
            if (headers != null)
            {
                foreach (string header in headers.AllKeys)
                {
                    string val = headers[header];
                    switch (header)
                    {
                        case "Connection": if (val.Equals("keep-alive")) req.KeepAlive = true; break;
                        case "Host": req.Host = val; break;
                        case "Accept": req.Accept = val; break;
                        case "User-Agent": req.UserAgent = val; break;
                        case "Content-Type": req.ContentType = val; break;
                        case "Referer": req.Referer = val; break;
                        default: req.Headers.Add(header, val); break;
                    }
                }
            }
        }

        internal static bool DoRequest(ref RequestStruct requestStruct, ref byte[] reply)
        {
            int count = 0;
            bool success = false;
            List<Exception> exceptions = new List<Exception>();


            while (!success && count++ < 3)
            {
                try
                {
                    requestStruct.response = requestStruct.request.GetResponse();
                    requestStruct.responseStream = requestStruct.response.GetResponseStream();
                    if (DecodeResponseStream(ref requestStruct))
                    {
                        reply = requestStruct.responsedata;
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (count < 3)
                        Thread.Sleep(1000);
                    else
                        ExceptionHistory.lastException = new HTTPResponseException(exceptions.ToArray());
                }
            }

            return success;
        }

        internal static void RequestCallback(IAsyncResult ar)
        {
            RequestStruct requestStruct = (RequestStruct)ar.AsyncState;
            requestStruct.response = requestStruct.request.EndGetResponse(ar);
            requestStruct.responseStream = requestStruct.response.GetResponseStream();
            if (DecodeResponseStream(ref requestStruct))
            {
                requestStruct.asyncCallback(requestStruct.responsedata);
            }
        }

        internal static bool DecodeResponseStream(ref RequestStruct rs)
        {
            bool success = false;
            try
            {

                using (Stream responseStream = rs.responseStream)
                {
                    string encoding = "";
                    if (rs.response.Headers.TryGetValue("Content-Encoding", out encoding) && encoding.Equals("gzip"))
                    {
                        using (GZipStream decompressStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            rs.responsedata = StreamToArray(decompressStream);
                            success = true;
                        }

                    }
                    else { rs.responsedata = StreamToArray(responseStream); success = true; }
                }
            }
            catch (Exception e)
            {
                ExceptionHistory.lastException = new HTTPDecodeException(e);
            }

            return success;
        }

        internal static byte[] StreamToArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
