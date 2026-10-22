using System.Threading.Tasks;

namespace MauiApp2.Interfaces
{
    /// <summary>
    /// Contract for performing HTTP communication.
    /// </summary>
    public interface IHttpCommunication
    {
        /// <summary>
        /// Gets the response body as a string for the supplied request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <returns>The response body.</returns>
        Task<string> GetStringAsync(string requestUri);
    }
}
