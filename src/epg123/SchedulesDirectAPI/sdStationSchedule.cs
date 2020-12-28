using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirectAPI
{
    public class sdScheduleRequest
    {
        [JsonProperty("stationID")]
        public string StationId { get; set; }

        [JsonProperty("date")]
        public string[] Date { get; set; }
    }

    public class sdScheduleResponse
    {
        [JsonProperty("stationID")]
        public string StationId { get; set; }

        //[JsonProperty("serverID")]
        //public string ServerID { get; set; }

        //[JsonProperty("code")]
        //public int Code { get; set; }

        //[JsonProperty("response")]
        //public string Response { get; set; }

        //[JsonProperty("message")]
        //public string Message { get; set; }

        //[JsonProperty("datetime")]
        //public string DateTime { get; set; }

        //[JsonProperty("retryTime")]
        //public string RetryTime { get; set; }

        //[JsonProperty("minDate")]
        //public string MinDate { get; set; }

        //[JsonProperty("maxDate")]
        //public string MaxDate { get; set; }

        //[JsonProperty("requestedDate")]
        //public string RequestedDate { get; set; }

        [JsonProperty("programs")]
        public IList<sdSchedProgram> Programs { get; set; }

        [JsonProperty("metadata")]
        public sdSchedMetadata Metadata { get; set; }
    }

    public class sdScheduleMd5DateResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        //[JsonProperty("lastModified")]
        //public string LastModified { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }

    public class sdSchedProgram
    {
        [JsonProperty("programID")]
        public string ProgramId { get; set; }

        [JsonProperty("airDateTime")]
        public DateTime AirDateTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("new")]
        public bool New { get; set; }

        [JsonProperty("cableInTheClassroom")]
        public bool CableInTheClassroom { get; set; }

        //[JsonProperty("catchup")]
        //public bool Catchup { get; set; }

        //[JsonProperty("continued")]
        //public bool Continued { get; set; }

        //[JsonProperty("educational")]
        //public bool Educational { get; set; }

        [JsonProperty("joinedInProgress")]
        public bool JoinedInProgress { get; set; }

        //[JsonProperty("leftInProgress")]
        //public bool LeftInProgress { get; set; }

        [JsonProperty("premiere")]
        public bool Premiere { get; set; }

        //[JsonProperty("programBreak")]
        //public bool ProgramBreak { get; set; }

        //[JsonProperty("repeat")]
        //public bool Repeat { get; set; }

        [JsonProperty("signed")]
        public bool Signed { get; set; }

        [JsonProperty("subjectToBlackout")]
        public bool SubjectToBlackout { get; set; }

        //[JsonProperty("timeApproximate")]
        //public bool TimeApproximate { get; set; }

        [JsonProperty("liveTapeDelay")]
        public string LiveTapeDelay { get; set; }

        [JsonProperty("isPremiereOrFinale")]
        public string IsPremiereOrFinale { get; set; }

        [JsonProperty("audioProperties")]
        public string[] AudioProperties { get; set; }

        [JsonProperty("videoProperties")]
        public string[] VideoProperties { get; set; }

        [JsonProperty("ratings")]
        public IList<sdSchedTvRating> Ratings { get; set; }

        [JsonProperty("multipart")]
        public sdSchedMultipart Multipart { get; set; }
    }

    public class sdSchedMetadata
    {
        //[JsonProperty("code")]
        //public int Code { get; set; }

        //[JsonProperty("modified")]
        //public string Modified { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }
    }

    public class sdSchedMultipart
    {
        [JsonProperty("partNumber")]
        public int PartNumber { get; set; }

        [JsonProperty("totalParts")]
        public int TotalParts { get; set; }
    }

    public class sdSchedTvRating
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
