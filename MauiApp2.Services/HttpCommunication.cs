using System.Net.Http;
using MauiApp2.Interfaces;

namespace MauiApp2.Services
{
    /// <inheritdoc />
    public class HttpCommunication : IHttpCommunication
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <inheritdoc />
        public async Task<string> GetStringAsync(string requestUri)
        {
            return await HttpClient.GetStringAsync(requestUri);
        }
    }
}
