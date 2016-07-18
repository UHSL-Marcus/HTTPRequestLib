using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPRequestLibUWP
{
    class Get
    {
        private Shared shared;

        public Get()
        {
            shared = new Shared();
        }

        public bool Request(string url, out byte[] reply, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            bool success = false;
            bool complete = false;
            byte[] responseBytes = new byte[0];
            HTTPAsyncCallback callback = delegate (HTTPResponse resp)
            {
                responseBytes = resp.bytes;
                success = resp.success;
                complete = true;
            };

            if (Request(url, callback, headers, parameters))
                while (!complete) { };

            reply = responseBytes;
            return success;
        }

        public bool Request(string url, HTTPAsyncCallback callback, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            shared.headers = headers;
            shared.parameters = parameters;
            shared.url = url;

            Task<HTTPResponse> getTask = RequestTask(url, shared.cts.Token);
            getTask.Start();
            getTask.ContinueWith((i) => callback(i.Result));

            return (getTask.Status == TaskStatus.Running || getTask.Status == TaskStatus.RanToCompletion);
        }

        public void Cancel()
        {
            shared.cts.Cancel();
        }

        private async Task<HTTPResponse> RequestTask(string url, CancellationToken ct)
        {
            HTTPResponse responseStruct = new HTTPResponse();
            
            int count = 0;
            List<Exception> exceptions = new List<Exception>();

            while (!responseStruct.success && count++ < 3)
            {
                try
                {
                    HttpResponseMessage response = await shared.client.GetAsync(shared.uri, ct);

                    responseStruct.success = response.IsSuccessStatusCode;
                    
                    Tuple<bool, byte[]> decode = await shared.DecodeResponseStream(response);
                    if (!decode.Item1) responseStruct.success = false;
                    else responseStruct.bytes = decode.Item2;
                    
                }
                catch (OperationCanceledException)
                {
                    responseStruct.wasCancelled = true;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (count < 3)
                        await Task.Delay(500, ct);
                    else
                        ExceptionHistory.lastException = new HTTPGetRequestException(exceptions.ToArray());
                }
            }

            return responseStruct;
           
        } 
    }
}
