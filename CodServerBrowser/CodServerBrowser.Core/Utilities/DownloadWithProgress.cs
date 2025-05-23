﻿namespace CodServerBrowser.Core.Utilities
{
    public delegate void DownloadProgressHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

    public static class DownloadWithProgress
    {
        public static async Task ExecuteAsync(HttpClient httpClient, string downloadPath, string destinationPath, 
            DownloadProgressHandler progress, CancellationToken cancellationToken, Func<HttpRequestMessage>? requestMessageBuilder = null)
        {
            requestMessageBuilder ??= GetDefaultRequestBuilder(downloadPath);
            var download = new HttpClientDownloadWithProgress(httpClient, destinationPath, requestMessageBuilder);
            download.ProgressChanged += progress;
            await download.StartDownloadAsync(cancellationToken);
            download.ProgressChanged -= progress;
        }

        private static Func<HttpRequestMessage> GetDefaultRequestBuilder(string downloadPath)
        {
            return () => new HttpRequestMessage(HttpMethod.Get, downloadPath);
        }
    }
}
