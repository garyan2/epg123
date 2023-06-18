using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GaRyan2.SchedulesDirectAPI
{
    public class ScheduleRequest
    {
        [JsonProperty("stationID")]
        public string StationId { get; set; }

        [JsonProperty("date")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Date { get; set; }
    }

    public class ScheduleMd5Response : BaseResponse
    {
        [JsonProperty("lastModified")]
        public string LastModified { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }

    public class ScheduleResponse : BaseResponse
    {
        [JsonProperty("stationID")]
        public string StationId { get; set; }

        [JsonProperty("retryTime")]
        public string RetryTime { get; set; }

        [JsonProperty("minDate")]
        public string MinDate { get; set; }

        [JsonProperty("maxDate")]
        public string MaxDate { get; set; }

        [JsonProperty("requestedDate")]
        public string RequestedDate { get; set; }

        [JsonProperty("programs")]
        [JsonConverter(typeof(SingleOrListConverter<ScheduleProgram>))]
        public List<ScheduleProgram> Programs { get; set; }

        [JsonProperty("metadata")]
        public ScheduleMetadata Metadata { get; set; }
    }

    public class ScheduleProgram
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

        [JsonProperty("catchup")]
        public bool Catchup { get; set; }

        [JsonProperty("continued")]
        public bool Continued { get; set; }

        [JsonProperty("educational")]
        public bool Educational { get; set; }

        [JsonProperty("joinedInProgress")]
        public bool JoinedInProgress { get; set; }

        [JsonProperty("leftInProgress")]
        public bool LeftInProgress { get; set; }

        [JsonProperty("premiere")]
        public bool Premiere { get; set; }

        [JsonProperty("programBreak")]
        public bool ProgramBreak { get; set; }

        [JsonProperty("repeat")]
        public bool Repeat { get; set; }

        [JsonProperty("signed")]
        public bool Signed { get; set; }

        [JsonProperty("subjectToBlackout")]
        public bool SubjectToBlackout { get; set; }

        [JsonProperty("timeApproximate")]
        public bool TimeApproximate { get; set; }

        [JsonProperty("liveTapeDelay")]
        public string LiveTapeDelay { get; set; }

        [JsonProperty("isPremiereOrFinale")]
        public string IsPremiereOrFinale { get; set; }

        [JsonProperty("audioProperties")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] AudioProperties { get; set; }

        [JsonProperty("videoProperties")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] VideoProperties { get; set; }

        [JsonProperty("ratings")]
        [JsonConverter(typeof(SingleOrListConverter<ScheduleTvRating>))]
        public List<ScheduleTvRating> Ratings { get; set; }

        [JsonProperty("multipart")]
        public ScheduleMultipart Multipart { get; set; }
    }

    public class ScheduleMetadata
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }
    }

    public class ScheduleMultipart
    {
        [JsonProperty("partNumber")]
        public int PartNumber { get; set; }

        [JsonProperty("totalParts")]
        public int TotalParts { get; set; }
    }

    public class ScheduleTvRating
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}