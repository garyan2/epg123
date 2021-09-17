using System;

namespace tokenServer
{
    public static class WebStats
    {
        public static DateTime StartTime { get; set; }
        private static readonly object StatLock = new object();

        private static int imageNotFound;
        private static int maxDownloads;
        private static int unknownUser;
        private static int tokenRefresh;

        private static int sdDownloadCnt;
        private static long sdDownloadSz;
        private static int cacheDownloadCnt;
        private static long cacheDownloadSz;
        private static int logoCnt;
        private static long logoSz;

        private static int reqRcvd;
        private static int provReqRcvd;
        private static int reqSent;
        private static int provReqSent;
        private static int resp304;

        public static bool LimitLocked;

        public static string Html
        {
            get
            {
                var uptime = DateTime.Now - StartTime;
                return $"<html><body><h1>Status</h1><p>" +
                       $"Uptime: {uptime.Days:D2} days, {uptime.Hours:D2} hours, {uptime.Minutes:D2} minutes, {uptime.Seconds:D2} seconds<br>" +
                       $"Image requests received by service: {reqRcvd}<br>" +
                       $"Image requests sent to Schedules Direct: {reqSent}<br>" +
                       $"Provisional requests received by service: {provReqRcvd}<br>" +
                       $"Provisional requests sent to Schedules Direct: {provReqSent}<br>" +
                       $"Responded to client with 304 Not Modified: {resp304}<br>" +
                       $"Images downloaded from Schedules Direct: {sdDownloadCnt} ({sdDownloadSz:N0} bytes)<br>" +
                       $"Images provided by service cache: {cacheDownloadCnt} ({cacheDownloadSz:N0} bytes)<br>" +
                       $"Station logos provided by service: {logoCnt} ({logoSz:N0} bytes)<br>" +
                       $"Number of cached images: {JsonImageCache.ImageCache.Count}<br>" +
                       $"Number of token refreshes: {tokenRefresh}<br>" +
                       $"Valid token: {TokenService.GoodToken}<br>" +
                       $"Download limit exceeded: {LimitLocked}</p>" +
                       $"<h1>Errors</h1><p>" +
                       $"IMAGE_NOT_FOUND: {imageNotFound}<br>" +
                       $"MAX_IMAGE_DOWNLOADS: {maxDownloads}<br>" +
                       $"UNKNOWN_USER: {unknownUser}</p>" +
                       $"<h1>Configuration</h1><p>" +
                       $"Image cache enabled: {JsonImageCache.cacheImages}<br>" +
                       $"Image retention: {JsonImageCache.cacheRetention} days<br>" +
                       $"Automatic token refresh: {TokenService.RefreshToken}</p>" +
                       $"<h1>Links</h1><p>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/trace.log\" target=\"_blank\">View EPG123 Log</a><br>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/server.log\" target=\"_blank\">View Service Log</a><br>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/output/epg123.mxf\">Download MXF file</a><br>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/output/epg123.xmltv\">Download XMLTV file</a></p>" +
                       $"</body></html>";
            }
        }

        public static void IncrementImageNotFound()
        {
            lock (StatLock) ++imageNotFound;
        }

        public static void IncrementMaxDownloads()
        {
            lock (StatLock) ++maxDownloads;
        }

        public static void IncrementUnknownUser()
        {
            lock (StatLock) ++unknownUser;
        }

        public static void AddSdDownload(long size)
        {
            lock (StatLock)
            {
                ++sdDownloadCnt;
                sdDownloadSz += size;
            }
        }

        public static void AddCacheDownload(long size)
        {
            lock (StatLock)
            {
                ++cacheDownloadCnt;
                cacheDownloadSz += size;
            }
        }

        public static void IncrementRequestReceived()
        {
            lock (StatLock) ++reqRcvd;
        }

        public static void IncrementProvisionRequestReceived()
        {
            lock (StatLock) ++provReqRcvd;
        }

        public static void IncrementRequestSent()
        {
            lock (StatLock) ++reqSent;
        }

        public static void IncrementProvisionRequestSent()
        {
            lock (StatLock) ++provReqSent;
        }

        public static void IncrementTokenRefresh()
        {
            lock (StatLock) ++tokenRefresh;
        }

        public static void Increment304Response()
        {
            lock (StatLock) ++resp304;
        }

        public static void AddLogoDownload(long size)
        {
            lock (StatLock)
            {
                ++logoCnt;
                logoSz += size;
            }
        }
    }
}
