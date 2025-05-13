using System.Net.Http.Json;

namespace CodServerBrowser.Core.Utilities
{
    public static class HttpContentExtensions
    {
        public static async Task<T?> TryReadFromJsonAsync<T>(this HttpContent content, CancellationToken cancellationToken = default)
        {
            try
            {
                return await content.ReadFromJsonAsync<T>(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                return default;
            }
        }
    }
}
