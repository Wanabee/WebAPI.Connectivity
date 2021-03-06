using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PageJaunesResto.WebAPI.Connectivity.Framework.Helpers;

namespace PageJaunesResto.WebAPI.Connectivity.Framework.RequestCommands.RequestCommands.Http
{
    public class PutHttpRequestBuilderCommand : IRequestBuilderCommand
    {
        private readonly string _methodName;
        private readonly IRequestSerializer _requestSerializer;

        public PutHttpRequestBuilderCommand(string methodName, IRequestSerializer requestSerializer)
        {
            _methodName = methodName;
            _requestSerializer = requestSerializer;
        }

        public async Task<TReturnType> BuildRequest<TReturnType>(string url, params KeyValuePair<string, object>[] parameters)
        {
            var result = await MakeRequest(url, parameters);

            return _requestSerializer.DeserializeObject<TReturnType>(await result.Content.ReadAsStringAsync());
        }

        public async Task BuildRequest(string url, params KeyValuePair<string, object>[] parameters)
        {
            await MakeRequest(url, parameters);
        }


        private async Task<HttpResponseMessage> MakeRequest(string url, KeyValuePair<string, object>[] parameters)
        {
            var request = new HttpClient();
            Uri uri = new Uri(url);

            uri = new Uri(uri + _methodName.ToLower());


            if (parameters.Any())
                uri = UriBuildingHelpers.AttachParameters(uri,
                    parameters.Where(x => x.Value is string || x.Value is Guid || x.Value is int)
                        .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())).ToArray());

            var postItem = parameters.First(x => !(x.Value is string || x.Value is Guid || x.Value is int));

            var content =
                new StringContent(_requestSerializer.SerializeObject(postItem.Value), Encoding.UTF8, "application/json");

            return await request.PutAsync(uri, content);
        }
    }
}