using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GaRyan2
{
    public class BaseAPI
    {
        public JsonSerializerSettings JsonOptions = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        /// <summary>
        /// The base URL to the API. Do not forget to include the trailing '/'.
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// Sets the decompression method for the return content. Default is Deflate and GZip.
        /// </summary>
        public DecompressionMethods DecompressMethods { get; set; } = DecompressionMethods.Deflate | DecompressionMethods.GZip;

        /// <summary>
        /// The user agent to include in http message header.
        /// </summary>
        public string UserAgent { get; set; }

        public enum Method
        {
            GET,
            POST,
            PUT,
            DELETE,
            PUTX,
            PUTB
        }

        public HttpClient _httpClient;

        /// <summary>
        /// Reguired before first http transaction to instantiate the http client.
        /// </summary>
        public virtual void Initialize()
        {
            ServicePointManager.DefaultConnectionLimit = 10;
            //ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressMethods })
            {
                BaseAddress = new Uri(BaseAddress),
                Timeout = TimeSpan.FromMinutes(5)
            };
            if (UserAgent != null) _ = _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
            _ = _httpClient.DefaultRequestHeaders.ExpectContinue = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="method">The http method to use.</param>
        /// <param name="uri">The relative uri from base address to form a complete url.</param>
        /// <param name="classObject">Payload of message to be serialized into json.</param>
        /// <returns>Object requested.</returns>
        public virtual T GetApiResponse<T>(Method method, string uri, object classObject = null)
        {
            try
            {
                switch (method)
                {
                    case Method.GET:
                        return GetHttpResponse<T>(HttpMethod.Get, uri).Result;
                    case Method.POST:
                        return GetHttpResponse<T>(HttpMethod.Post, uri, classObject).Result;
                    case Method.PUT:
                        return GetHttpResponse<T>(HttpMethod.Put, uri).Result;
                    case Method.DELETE:
                        return GetHttpResponse<T>(HttpMethod.Delete, uri).Result;
                    case Method.PUTX:
                        return PutXmlClass<T>(HttpMethod.Put, uri, classObject).Result;
                    case Method.PUTB:
                        return PutBinary<T>(HttpMethod.Put, uri, (byte[])classObject).Result;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteVerbose($"HTTP request exception thrown. Message:{Helper.ReportExceptionMessages(ex)}");
            }
            return default;
        }

        public virtual async Task<T> GetHttpResponse<T>(HttpMethod method, string uri, object content = null)
        {
            using (var request = new HttpRequestMessage(method, uri)
            {
                Content = (content != null)
                    ? new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
                    : null
            })
            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                return !response.IsSuccessStatusCode
                    ? HandleHttpResponseError<T>(response, await response.Content?.ReadAsStringAsync())
                    : JsonConvert.DeserializeObject<T>(await response.Content?.ReadAsStringAsync(), JsonOptions);
            }
        }

        public virtual async Task<T> PutXmlClass<T>(HttpMethod method, string uri, object content)
        {
            var serializer = new XmlSerializer(content.GetType());
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var sw = new StringWriter();
            serializer.Serialize(sw, content);

            using (var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent(sw.ToString(), Encoding.UTF8, "text/xml")
            })
            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                return !response.IsSuccessStatusCode
                    ? HandleHttpResponseError<T>(response, await response.Content?.ReadAsStringAsync())
                    : JsonConvert.DeserializeObject<T>(await response.Content?.ReadAsStringAsync(), JsonOptions);
            }
        }

        public virtual async Task<T> PutBinary<T>(HttpMethod method, string uri, byte[] content)
        {
            using (var request = new HttpRequestMessage(method, uri)
            {
                Content = new ByteArrayContent(content)
            })
            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                return !response.IsSuccessStatusCode
                    ? HandleHttpResponseError<T>(response, await response.Content?.ReadAsStringAsync())
                    : JsonConvert.DeserializeObject<T>(await response.Content?.ReadAsStringAsync(), JsonOptions);
            }
        }

        public virtual T HandleHttpResponseError<T>(HttpResponseMessage response, string content)
        {
            Logger.WriteVerbose($"HTTP request failed with status code \"{(int)response.StatusCode} {response.ReasonPhrase}\"");
            return default;
        }
    }
}