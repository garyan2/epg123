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
        private static int badGateway;

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
                return $"<html><title>{Environment.MachineName} Server Status</title><body><h1>Status</h1><p>" +
                       $"Uptime: <font color=\"blue\">{uptime.Days:D2}</font> days, <font color=\"blue\">{uptime.Hours:D2}</font> hours, <font color=\"blue\">{uptime.Minutes:D2}</font> minutes, <font color=\"blue\">{uptime.Seconds:D2}</font> seconds<br>" +
                       $"Image requests received by service: <font color=\"blue\">{reqRcvd}</font><br>" +
                       $"Image requests sent to Schedules Direct: <font color=\"blue\">{reqSent}</font><br>" +
                       $"Conditional requests received by service: <font color=\"blue\">{provReqRcvd}</font><br>" +
                       $"Conditional requests sent to Schedules Direct: <font color=\"blue\">{provReqSent}</font><br>" +
                       $"Responded to client with 304 Not Modified: <font color=\"blue\">{resp304}</font><br>" +
                       $"Images downloaded from Schedules Direct: <font color=\"blue\">{sdDownloadCnt} ({sdDownloadSz:N0} bytes)</font><br>" +
                       $"Images provided by service cache: <font color=\"blue\">{cacheDownloadCnt} ({cacheDownloadSz:N0} bytes)</font><br>" +
                       $"Station logos provided by service: <font color=\"blue\">{logoCnt} ({logoSz:N0} bytes)</font><br>" +
                       $"Number of cached images: <font color=\"blue\">{JsonImageCache.ImageCache.Count}</font><br>" +
                       $"Number of token refreshes: <font color=\"blue\">{tokenRefresh}</font><br>" +
                       $"Valid token: <font color=\"{(TokenService.GoodToken ? "blue" : "red")}\">{TokenService.GoodToken}</font><br>" +
                       $"Download limit exceeded: <font color=\"{(LimitLocked ? "red" : "blue")}\">{LimitLocked}</font></p>" +
                       $"<h1>Errors</h1><p>" +
                       $"Empty response (502 Bad Gateway): <font color=\"red\">{badGateway}</font><br>" +
                       $"IMAGE_NOT_FOUND: <font color=\"red\">{imageNotFound}</font><br>" +
                       $"MAX_IMAGE_DOWNLOADS: <font color=\"red\">{maxDownloads}</font><br>" +
                       $"UNKNOWN_USER: <font color=\"red\">{unknownUser}</font></p>" +
                       $"<h1>Configuration</h1><p>" +
                       $"Image cache enabled: <font color=\"blue\">{JsonImageCache.cacheImages}</font><br>" +
                       $"Image retention: <font color=\"blue\">{JsonImageCache.cacheRetention} days</font><br>" +
                       $"Automatic token refresh: <font color=\"blue\">{TokenService.RefreshToken}</font></p>" +
                       $"<h1>Links</h1><p>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/trace.log\" target=\"_blank\">View EPG123 Log</a><br>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/server.log\" target=\"_blank\">View Service Log</a><br>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/output/epg123.mxf\">Download MXF file</a><br>" +
                       $"<a href=\"http://{Environment.MachineName}:{Helper.TcpPort}/output/epg123.xmltv\">Download XMLTV file</a></p>" +
                       $"<p><small><b><i>EPG123 Server v{Helper.Epg123Version}</i></b></small></p>" +
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

        public static void IncrementBadGateway()
        {
            lock (StatLock) ++badGateway;
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
