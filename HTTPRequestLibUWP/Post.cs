using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPRequestLibUWP
{
    public class Post : IDisposable
    {
        private Shared shared;

        public Post()
        {
            shared = new Shared();
        }

        public async Task<HTTPResponse> Request(string url, string data, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            return await Request(url, data, (new CancellationTokenSource()).Token, headers, parameters);
        }
        public async Task<HTTPResponse> Request(string url, string data, CancellationToken ct, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            shared.headers = headers;
            shared.parameters = parameters;
            shared.url = url;
            shared.data = data;

            return await RequestTask(ct);
        }

        public bool Request(string url, string data, HTTPAsyncCallback callback, NameValueCollection headers = null, NameValueCollection parameters = null)
        {
            shared.headers = headers;
            shared.parameters = parameters;
            shared.url = url;
            shared.data = data;

            Task<HTTPResponse> getTask = RequestTask(shared.cts.Token);
            getTask.ContinueWith((i) => callback(i.Result));

            return (getTask.Status == TaskStatus.Running || getTask.Status == TaskStatus.RanToCompletion);
        }

        public void Cancel()
        {
            shared.cts.Cancel();
        }

        private async Task<HTTPResponse> RequestTask(CancellationToken ct)
        {
            HTTPResponse responseStruct = new HTTPResponse();

            int count = 0;
            List<Exception> exceptions = new List<Exception>();

            while (!responseStruct.success && count++ < 3)
            {
                try
                {
                    HttpResponseMessage response = await shared.client.PostAsync(shared.uri, new StringContent(shared.data), ct);

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

        public void Dispose()
        {
            shared.Dispose();
        }
    }
}
