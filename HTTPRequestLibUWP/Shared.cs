

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPRequestLibUWP
{
    public class HTTPResponse
    {
        public bool wasCancelled;
        public bool success;
        public byte[] bytes;
        public NameValueCollection headers;

        public HTTPResponse()
        {
            wasCancelled = false;
            success = false;
            bytes = new byte[0];
            headers = new NameValueCollection();
        }
    }
    public delegate void HTTPAsyncCallback(HTTPResponse reponse);
    internal class Shared : IDisposable
    {
        private HttpClient _client;
        internal HttpClient client
        {
            get
            {
                if (_client == null)
                    _client = new HttpClient();

                AddHeaders();
                return _client;
            }
        }

        private CancellationTokenSource _cts;
        internal CancellationTokenSource cts
        {
            get
            {
                if (_cts == null)
                    _cts = new CancellationTokenSource();

                return _cts;
            }
        }

        internal NameValueCollection headers
        {
            get; set;
        }

        internal NameValueCollection parameters
        {
            get; set;
        }

        internal string url { get; set; }
        internal string data { get; set; }

        internal Uri uri
        {
            get
            {
                return GetUri();
            }
        }

        private Uri GetUri()
        {
            string path = url;
            if (parameters != null)
            {
                if (parameters.Count > 0)
                {
                    bool first = false;
                    foreach (string param in parameters.AllKeys)
                        path += (first ? "&" : "?") + param + "=" + parameters[param];
                }
            }

            return new Uri(url);
        }

        private void AddHeaders()
        {
            _client.DefaultRequestHeaders.Clear();
            if (headers != null)
            {
                foreach (string header in headers.AllKeys)
                {
                    
                    string val = headers[header];
                    switch (header)
                    {
                        case "Connection": _client.DefaultRequestHeaders.Connection.Add(val); break;
                        case "Host": _client.DefaultRequestHeaders.Host = val; break;
                        case "Accept": _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(val)); break;
                        case "User-Agent": _client.DefaultRequestHeaders.UserAgent.ParseAdd(val); break;
                        case "Referer": _client.DefaultRequestHeaders.Referrer = new Uri(val); break;
                        default: _client.DefaultRequestHeaders.Add(header, val); break;
                    }
                }
            }
        }

        internal async Task<Tuple<bool, byte[]>> DecodeResponseStream(HttpResponseMessage info)
        {
            bool success = false;
            byte[] bytes = new byte[0];
            try
            {
                using (Stream responseStream = await info.Content.ReadAsStreamAsync())
                {
                    IEnumerable<string> encoding;
                    if ((info.Headers.TryGetValues("Content-Encoding", out encoding) || info.Content.Headers.TryGetValues("Content-Encoding", out encoding)) && encoding.Contains("gzip"))
                    {
                        using (GZipStream decompressStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            bytes = StreamToArray(decompressStream);
                            success = true;
                        }

                    }
                    else { bytes = StreamToArray(responseStream); success = true; }
                }
            }
            catch (Exception e)
            {
                ExceptionHistory.lastException = new HTTPDecodeException(e);
            }

            return Tuple.Create(success, bytes);
        }

        private static byte[] StreamToArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
