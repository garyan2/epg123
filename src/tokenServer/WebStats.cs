using System;
using System.IO;
using System.Linq;

namespace tokenServer
{
    public static class WebStats
    {
        public static DateTime StartTime { get; set; }
        private static readonly object StatLock = new object();

        private static int tokenRefresh;
        public static bool LimitLocked;
        public static bool RegWatcherRunning;

        private static int reqRcvd;
        private static int condReqRcvd;
        private static int reqLogo;
        private static int reqFile;
        private static int reqSent;
        private static int condReqSent;

        private static int resp304;
        private static int resp401;
        private static int resp404;
        private static int resp429;
        private static int resp500;
        private static int resp502;
        private static int resp503;
        private static int respOther;

        private static int sdDownloadCnt;
        private static long sdDownloadSz;
        private static int cacheDownloadCnt;
        private static long cacheDownloadSz;
        private static int logoCnt;
        private static long logoSz;
        private static int fileCnt;
        private static long fileSz;

        public static string Html
        {
            get
            {
                var fiMxf = new FileInfo(Helper.Epg123MxfPath);
                var fiXmltv = new FileInfo(Helper.Epg123XmltvPath);
                var uptime = DateTime.Now - StartTime;
                return $"<html><title>{Environment.MachineName} Server Status</title><body>" +
                       $"<h1>Status</h1><p>" +
                       $"Uptime: <font color=\"blue\">{uptime.Days:D2}</font> days, <font color=\"blue\">{uptime.Hours:D2}</font> hours, <font color=\"blue\">{uptime.Minutes:D2}</font> minutes, <font color=\"blue\">{uptime.Seconds:D2}</font> seconds<br>" +
                       $"Registry watcher running: <font color=\"{(RegWatcherRunning ? "blue" : "red")}\">{RegWatcherRunning}</font><br>" +
                       $"Number of cached images: <font color=\"blue\">{JsonImageCache.ImageCache.Count} ({JsonImageCache.ImageCache.Select(x => x.Value.ByteSize).Sum():N0} bytes)</font><br>" +
                       $"Download limit exceeded: <font color=\"{(LimitLocked ? "red" : "blue")}\">{LimitLocked}</font><br>" +
                       $"Number of token refreshes: <font color=\"blue\">{tokenRefresh}</font><br>" +
                       $"Valid token: <font color=\"{(TokenService.GoodToken ? "blue" : "red")}\">{TokenService.GoodToken}</font></p><p>" +
                       $"MXF file date/size: {(fiMxf.Exists ? $"<font color=\"{(DateTime.Now - fiMxf.LastWriteTime > TimeSpan.FromDays(1) ? "red" : "blue")}\">{fiMxf.LastWriteTime} ({fiMxf.Length:N0} bytes)</font>" : "")}<br>" +
                       $"XMLTV file date/size: {(fiXmltv.Exists ? $"<font color=\"{(DateTime.Now - fiXmltv.LastWriteTime > TimeSpan.FromDays(1) ? "red" : "blue")}\">{fiXmltv.LastWriteTime} ({fiXmltv.Length:N0} bytes)</font>" : "")}</p>" +

                       $"<h1>Configuration</h1><p>" +
                       $"Automatic token refresh: <font color=\"blue\">{TokenService.RefreshToken}</font><br>" +
                       $"Image cache enabled: <font color=\"blue\">{JsonImageCache.cacheImages}</font><br>" +
                       $"Image retention: <font color=\"blue\">{JsonImageCache.cacheRetention} days</font> after last request</p>" +

                       $"<h1>Stats</h1><p>" +
                       $"<u><strong>Requests received by service (<font color=\"blue\">{reqRcvd + condReqRcvd + reqLogo + reqFile}</font>):</strong></u><br>" +
                       $"Logo requests: <font color=\"blue\">{reqLogo}</font><br>" +
                       $"Image requests: <font color=\"blue\">{reqRcvd}</font><br>" +
                       $"Conditional requests: <font color=\"blue\">{condReqRcvd}</font><br>" + 
                       $"File requests: <font color=\"blue\">{reqFile}</font><br><br>" +
                       $"<u><strong>Requests sent to Schedules Direct (<font color=\"blue\">{reqSent + condReqSent}</font>):</strong></u><br>" +
                       $"Image requests: <font color=\"blue\">{reqSent}</font><br>" +
                       $"Conditional requests: <font color=\"blue\">{condReqSent}</font><br><br>" +
                       $"<u><strong>Responses to clients (<font color=\"blue\">{resp304 + sdDownloadCnt + cacheDownloadCnt + logoCnt + fileCnt + resp401 + resp404 + resp429 + resp500 + resp502 + resp503}</font>):</strong></u><br>" +
                       $"Images downloaded from Schedules Direct: <font color=\"blue\">{sdDownloadCnt} ({sdDownloadSz:N0} bytes)</font><br>" +
                       $"Images provided by service cache: <font color=\"blue\">{cacheDownloadCnt} ({cacheDownloadSz:N0} bytes)</font><br>" +
                       $"Station logos provided by service: <font color=\"blue\">{logoCnt} ({logoSz:N0} bytes)</font><br>" +
                       $"Files provided by service: <font color=\"blue\">{fileCnt} ({fileSz:N0} bytes)</font><br>" +
                       $"304 Not Modified: <font color=\"blue\">{resp304}</font><br>" +
                       $"401 Unauthorized: <font color=\"{(resp401 > 0 ? "red" : "blue")}\">{resp401}</font><br>" +
                       $"404 Not Found: <font color=\"{(resp404 > 0 ? "red" : "blue")}\">{resp404}</font><br>" +
                       $"429 Too Many Requests: <font color=\"{(resp429 > 0 ? "red" : "blue")}\">{resp429}</font><br>" +
                       $"500 Internal Server Error: <font color=\"{(resp500 > 0 ? "red" : "blue")}\">{resp500}</font><br>" +
                       $"502 Bad Gateway: <font color=\"{(resp502 > 0 ? "red" : "blue")}\">{resp502}</font><br>" +
                       $"503 Service Unavailable: <font color=\"{(resp503 > 0 ? "red" : "blue")}\">{resp503}</font><br>" +
                       $"Other (see log): <font color=\"{(respOther > 0 ? "red" : "blue")}\">{respOther}</font></p>" +

                       $"<h1>Logs</h1><p>" +
                       $"<a href=\"trace.log\" target=\"_blank\">View EPG123 Log</a><br>" +
                       $"<a href=\"server.log\" target=\"_blank\">View Service Log</a><br>" +
                       //$"<a href=\"output/epg123.mxf\">Download MXF file</a><br>" +
                       //$"<a href=\"output/epg123.xmltv\">Download XMLTV file</a></p>" +

                       $"<p><small><b><i>EPG123 Server v{Helper.Epg123Version}</i></b></small></p>" +
                       $"</body></html>";
            }
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

        public static void AddFileDownload(long size)
        {
            lock (StatLock)
            {
                ++fileCnt;
                fileSz += size;
            }
        }

        public static void IncrementRequestReceived()
        {
            lock (StatLock) ++reqRcvd;
        }

        public static void IncrementConditionalRequestReceived()
        {
            lock (StatLock) ++condReqRcvd;
        }

        public static void IncrementLogoRequestReceived()
        {
            lock (StatLock) ++reqLogo;
        }

        public static void IncrementFileRequestReceived()
        {
            lock (StatLock) ++reqFile;
        }

        public static void IncrementRequestSent()
        {
            lock (StatLock) ++reqSent;
        }

        public static void IncrementConditionalRequestSent()
        {
            lock (StatLock) ++condReqSent;
        }

        public static void IncrementTokenRefresh()
        {
            lock (StatLock) ++tokenRefresh;
        }

        public static void Increment304Response()
        {
            lock (StatLock) ++resp304;
        }

        public static void Increment401Response()
        {
            lock (StatLock) ++resp401;
        }

        public static void Increment404Response()
        {
            lock (StatLock) ++resp404;
        }

        public static void Increment429Response()
        {
            lock (StatLock) ++resp429;
        }

        public static void Increment500Response()
        {
            lock (StatLock) ++resp500;
        }

        public static void Increment502Response()
        {
            lock (StatLock) ++resp502;
        }

        public static void Increment503Response()
        {
            lock (StatLock) ++resp503;
        }

        public static void IncrementOtherResponse()
        {
            lock (StatLock) ++respOther;
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
