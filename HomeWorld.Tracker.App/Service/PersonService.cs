using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HomeWorld.Tracker.App.DAL.Model;
using HomeWorld.Tracker.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpClient = Windows.Web.Http.HttpClient;

namespace HomeWorld.Tracker.App.Service
{
    public class PersonService : IPersonService
    {
        private readonly HttpClient _httpClient;
        private string ApiBaseUrl = "http://192.168.0.50:8089/";
        //var url = "http://trackerdemosite.azurewebsites.net/api/";

        public PersonService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));

        }
        public async Task<IEnumerable<Person>> GetPeople(string excludeIds, int deviceId)
        {
            var result = Enumerable.Empty<Person>();

            var uriBuilder = new UriBuilder(ApiBaseUrl)
            {
                Path = "api/People",
                Query = $"excludeIds={excludeIds}&deviceId={deviceId}"
            };

            try
            {
                //Send the GET request
                var httpResponse = await _httpClient.GetAsync(uriBuilder.Uri);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                //guarded parse
                if (string.IsNullOrEmpty(httpResponseBody))
                {
                    return null;
                }

                result = JObject.Parse(httpResponseBody).SelectToken("People").ToObject<List<Person>>();

                return result;
            }
            catch (JsonSerializationException jex)
            {
                //todo log error
                Debug.WriteLine(jex.Message);
            }
            catch (HttpRequestException httpex)
            {
                // Handle failure
                Debug.WriteLine(httpex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return result;
        }

        public async Task<MovementResponseDto> PostMovement(string cardId)
        {
            MovementResponseDto result = null;

            //TODO move to config
            var deviceId = 4;

            var uriBuilder = new UriBuilder(ApiBaseUrl)
            {
                Path = "api/Movement"
            };

            try
            {
                //Send the POST request
                var movementDto = new MovementDto { Uid = cardId, DeviceId = deviceId, SwipeTime = DateTime.UtcNow};

                var postBody = JsonConvert.SerializeObject(movementDto);

                var httpResponse = await _httpClient.PostAsync(uriBuilder.Uri,
                            new HttpStringContent(postBody, UnicodeEncoding.Utf8, "application/json"));

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return null;
                }

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
            catch (HttpRequestException httpex)
            {
                // Handle failure
                Debug.WriteLine(httpex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
        public string CardUid { get; set; }
    }
}