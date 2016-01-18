using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HomeWorld.Tracker.Dto;
using Newtonsoft.Json;

namespace HomeWorld.Tracker.App.Service
{
    public class PersonService : IPersonService
    {
        public async Task<MovementResponseDto> PostMovement(string cardId)
        {
            MovementResponseDto result = null;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));

            //TODO move to config
            var deviceId = 4;
            var url = "http://trackerdemosite.azurewebsites.net/api/Movement";
            
            var requestUri = new Uri(url);

            try
            {
                //Send the GET request
                var movementDto = new MovementDto { Uid = cardId, DeviceId = deviceId };

                var postBody = JsonConvert.SerializeObject(movementDto);

                var httpResponse = await httpClient.PostAsync(requestUri, new HttpStringContent(postBody, UnicodeEncoding.Utf8, "application/json"));
                httpResponse.EnsureSuccessStatusCode();
                var httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                //guarded parse
                if (string.IsNullOrEmpty(httpResponseBody))
                {
                    return null;
                }

                result = JsonConvert.DeserializeObject<MovementResponseDto>(httpResponseBody);

                return result;
            }
            catch (JsonSerializationException jex)
            {
                //todo log error
                Debug.WriteLine(jex.Message);
            }

            return result;
        }
    }

    public class MovementResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public bool Ingress { get; set; }
    }
}