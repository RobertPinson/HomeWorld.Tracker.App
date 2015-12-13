using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Newtonsoft.Json;

namespace HomeWorld.Tracker.App.Service
{
    public class PersonService : IPersonService
    {
        public async Task<PersonDto> GetPerson(string cardId)
        {
            PersonDto result = null;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));
            var url = $"http://homeworldtracker.azurewebsites.net/api/Track/{cardId}";
            var requestUri = new Uri(url);

            try
            {
                //Send the GET request
                var httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                var httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                //guarded parse
                if (string.IsNullOrEmpty(httpResponseBody))
                {
                    return null;
                }

                result = JsonConvert.DeserializeObject<PersonDto>(httpResponseBody);

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
}