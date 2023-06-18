using GaRyan2;
using GaRyan2.Utilities;
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

        private static int reqRcvd;
        private static int condReqRcvd;
        private static int reqLogo;
        private static int reqFile;
        private static int reqSent;
        private static int condReqSent;

        private static int resp304;
        private static int resp401;
        private static int resp403;
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
                return PageHeader() +
                       $"<html><title>{Environment.MachineName} Server Status</title><body>" +
                       $"<h1>Status</h1><p>" +
                       $"Uptime: <font color=\"blue\">{uptime.Days:D2}</font> days, <font color=\"blue\">{uptime.Hours:D2}</font> hours, <font color=\"blue\">{uptime.Minutes:D2}</font> minutes, <font color=\"blue\">{uptime.Seconds:D2}</font> seconds<br>" +
                       $"Image cache enabled: <font color=\"blue\">{JsonImageCache.cacheImages}</font><br>" +
                       $"Image retention: <font color=\"blue\">{JsonImageCache.cacheRetention} days</font> after last request<br>" +
                       $"Number of cached images: <font color=\"blue\">{JsonImageCache.ImageCache.Count} ({JsonImageCache.ImageCache.Select(x => x.Value.ByteSize).Sum():N0} bytes)</font><br>" +
                       $"Download limit exceeded: <font color=\"{(LimitLocked ? "red" : "blue")}\">{LimitLocked}</font><br>" +
                       $"Number of token refreshes: <font color=\"blue\">{tokenRefresh - 1}</font><br>" +
                       $"Valid token: <font color=\"{(SchedulesDirect.GoodToken ? "blue" : "red")}\">{SchedulesDirect.GoodToken}</font></p><p>" +
                       BuildFileTable() +

                       $"<h1>Stats</h1><p>" +
                       $"<u><strong>Requests received by service (<font color=\"blue\">{reqRcvd + condReqRcvd + reqLogo + reqFile}</font>):</strong></u><br>" +
                       $"Logo requests: <font color=\"blue\">{reqLogo}</font><br>" +
                       $"Image requests: <font color=\"blue\">{reqRcvd}</font><br>" +
                       $"Conditional requests: <font color=\"blue\">{condReqRcvd}</font><br>" + 
                       $"File requests: <font color=\"blue\">{reqFile}</font><br><br>" +
                       $"<u><strong>Requests sent to Schedules Direct (<font color=\"blue\">{reqSent + condReqSent}</font>):</strong></u><br>" +
                       $"Image requests: <font color=\"blue\">{reqSent}</font><br>" +
                       $"Conditional requests: <font color=\"blue\">{condReqSent}</font><br><br>" +
                       $"<u><strong>Responses to clients (<font color=\"blue\">{resp304 + sdDownloadCnt + cacheDownloadCnt + logoCnt + fileCnt + resp401 + resp404 + resp429 + resp500 + resp502 + resp503 + respOther}</font>):</strong></u><br>" +
                       $"Images downloaded from Schedules Direct: <font color=\"blue\">{sdDownloadCnt} ({sdDownloadSz:N0} bytes)</font><br>" +
                       $"Images provided by service cache: <font color=\"blue\">{cacheDownloadCnt} ({cacheDownloadSz:N0} bytes)</font><br>" +
                       $"Station logos provided by service: <font color=\"blue\">{logoCnt} ({logoSz:N0} bytes)</font><br>" +
                       $"Files provided by service: <font color=\"blue\">{fileCnt} ({fileSz:N0} bytes)</font><br>" +
                       $"304 Not Modified: <font color=\"blue\">{resp304}</font><br>" +
                       $"401 Unauthorized: <font color=\"{(resp401 > 0 ? "red" : "blue")}\">{resp401}</font><br>" +
                       $"403 Forbidden: <font color=\"{(resp403 > 0 ? "red" : "blue")}\">{resp403}</font><br>" +
                       $"404 Not Found: <font color=\"{(resp404 > 0 ? "red" : "blue")}\">{resp404}</font><br>" +
                       $"429 Too Many Requests: <font color=\"{(resp429 > 0 ? "red" : "blue")}\">{resp429}</font><br>" +
                       $"500 Internal Server Error: <font color=\"{(resp500 > 0 ? "red" : "blue")}\">{resp500}</font><br>" +
                       $"502 Bad Gateway: <font color=\"{(resp502 > 0 ? "red" : "blue")}\">{resp502}</font><br>" +
                       $"503 Service Unavailable: <font color=\"{(resp503 > 0 ? "red" : "blue")}\">{resp503}</font><br>" +
                       $"Other (see log): <font color=\"{(respOther > 0 ? "red" : "blue")}\">{respOther}</font></p>" +

                       $"<h1>Logs</h1><p>" +
                       $"<a href=\"trace.log\" target=\"_blank\">View EPG123 Log</a><br>" +
                       $"<a href=\"server.log\" target=\"_blank\">View Service Log</a><br>" +

                       $"<p><small><b><i>EPG123 Server v{Helper.Epg123Version}{(Github.UpdateAvailable() ? " <font color=\"red\">(<a href=\"https://garyan2.github.io/download.html\">Update Available</a>)</font>" : "")}</i></b></small></p>" +
                       $"</body></html>";
            }
        }

        private static string PageHeader()
        {
            var header = "<head><style>" +
                "table, td, th { border: 1px solid #dddddd; text-align: center; }" +
                "td, th { padding: 8px; }" +
                "</style></head>";
            return header;
        }

        private static string BuildFileTable()
        {
            var ret = "<table><tr><th>Source</th><th>M3U</th><th>MXF</th><th>XMLTV</th></tr>";
            if (File.Exists(Helper.Epg123ExePath))
            {
                ret += $"<tr><td>EPG123</td>";
                ret += "<td>N/A</td>";
                ret += FileDetail(Helper.Epg123MxfPath);
                ret += FileDetail(Helper.Epg123XmltvPath);
                ret += "</tr>";
            }
            if (File.Exists(Helper.Hdhr2MxfExePath))
            {
                ret += $"<tr><td>HDHR2MXF</td>";
                ret += FileDetail(Helper.Hdhr2MxfM3uPath);
                ret += FileDetail(Helper.Hdhr2MxfMxfPath);
                ret += FileDetail(Helper.Hdhr2mxfXmltvPath);
                ret += "</tr>";
            }
            if (File.Exists(Helper.PlutoTvExePath))
            {
                ret += $"<tr><td>PlutoTV</td>";
                ret += FileDetail(Helper.PlutoTvM3uPath);
                ret += "<td>N/A</td>";
                ret += FileDetail(Helper.PlutoTvXmltvPath);
                ret += "</tr>";
            }
            if (File.Exists(Helper.StirrTvExePath))
            {
                ret += $"<tr><td>StirrTV</td>";
                ret += FileDetail(Helper.StirrTvM3uPath);
                ret += "<td>N/A</td>";
                ret += FileDetail(Helper.StirrTvXmltvPath);
                ret += "</tr>";
            }
            ret += "</table>";
            ret += $"<p><small><b>Links to files above can be constructed from server address + \"/output/&lt;source&gt;.&lt;extension&gt;\"; ex. http://{Environment.MachineName}:9009/output/epg123.mxf</b></small></p>";
            return ret;
        }

        private static string FileDetail(string path)
        {
            var file = new FileInfo(path);
            return file.Exists ? $"<td>{file.LastWriteTime}<br>{file.Length:N0} bytes</td>" : "<td></td>";
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

        public static void DecrementHttpStat(int stat = 0)
        {
            lock (StatLock)
            {
                switch (stat)
                {
                    case 304: // Not Modified
                        --resp304; break;
                    case 401: // Unauthorized
                        --resp401; break;
                    case 403: // Forbidden
                        --resp403; break;
                    case 404: // Not Found
                        --resp404; break;
                    case 429: // Too Many Requests
                        --resp429; break;
                    case 500: // Internal Server Error
                        --resp500; break;
                    case 502: // Bad Gateway
                        --resp502; break;
                    case 503: // Service Unavailable
                        --resp503; break;
                }
            }
        }

        public static void IncrementHttpStat(int stat = 0)
        {
            lock (StatLock)
            {
                switch (stat)
                {
                    case 304: // Not Modified
                        ++resp304; break;
                    case 401: // Unauthorized
                        ++resp401; break;
                    case 403: // Forbidden
                        ++resp403; break;
                    case 404: // Not Found
                        ++resp404; break;
                    case 429: // Too Many Requests
                        ++resp429; break;
                    case 500: // Internal Server Error
                        ++resp500; break;
                    case 502: // Bad Gateway
                        ++resp502; break;
                    case 503: // Service Unavailable
                        ++resp503; break;
                    default:
                        ++respOther; break;
                }
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