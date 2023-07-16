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

        /// <summary>
        /// Applied to any show/episode (including all episodes of a miniseries) indicated as “New” by the provider the first time it airs per country.
        /// </summary>
        [JsonProperty("new")]
        public bool New { get; set; }

        /// <summary>
        /// Educational content provided for classroom use.
        /// </summary>
        [JsonProperty("cableInTheClassroom")]
        public bool CableInTheClassroom { get; set; }

        /// <summary>
        /// Program is available after air via an on demand catchup service (VOD or OTT).
        /// </summary>
        [JsonProperty("catchup")]
        public bool Catchup { get; set; }

        /// <summary>
        /// Identifies a continuation of a program listed in the previous time slot on schedule.
        /// </summary>
        [JsonProperty("continued")]
        public bool Continued { get; set; }

        /// <summary>
        /// "Educational and informational" (or "educational and informative"), refers to a type of children's television programming shown in the United States.
        /// </summary>
        [JsonProperty("educational")]
        public bool Educational { get; set; }

        /// <summary>
        /// Broadcast of this program/movie on a channel will be joined after the originating station has already begun the telecast.
        /// </summary>
        [JsonProperty("joinedInProgress")]
        public bool JoinedInProgress { get; set; }

        /// <summary>
        /// Broadcast of this program/movie on a channel will terminate before the originating station finishes the telecast.
        /// </summary>
        [JsonProperty("leftInProgress")]
        public bool LeftInProgress { get; set; }

        /// <summary>
        /// Applied to movies and the first episode of a miniseries where indicated as such on schedules by the provider. Could indicate a world premiere of a movie or a channel premiere.
        /// </summary>
        [JsonProperty("premiere")]
        public bool Premiere { get; set; }

        /// <summary>
        /// Same as Continued.
        /// </summary>
        [JsonProperty("programBreak")]
        public bool ProgramBreak { get; set; }

        /// <summary>
        /// Identifies the second or later airing of a sporting event when a station is broadcasting a game more than once.
        /// </summary>
        [JsonProperty("repeat")]
        public bool Repeat { get; set; }

        /// <summary>
        /// Program is translated into sign language by an on-screen interpreter.
        /// </summary>
        [JsonProperty("signed")]
        public bool Signed { get; set; }

        /// <summary>
        /// Sport event may be blacked out in certain markets based on specific agreements between networks and sports leagues.
        /// </summary>
        [JsonProperty("subjectToBlackout")]
        public bool SubjectToBlackout { get; set; }

        /// <summary>
        /// Indicates that the start time of a program may change, usually due to a live event taking place immediately before it on a schedule.
        /// </summary>
        [JsonProperty("timeApproximate")]
        public bool TimeApproximate { get; set; }

        /// <summary>
        /// Live := Program is being broadcast live, as indicated by the provider.
        /// Tape := Identifies the first airing of a sporting event that took place on a prior calendar day.
        /// Delay := Identifies the first airing of a sporting event that took place early on the same schedule day.
        /// </summary>
        [JsonProperty("liveTapeDelay")]
        public string LiveTapeDelay { get; set; }

        /// <summary>
        /// Season Finale := Indicates the initial airing of final episode of a season of a TV series.
        /// Season Premiere := Indicates the initial airing of the first episode of a season of a TV series.
        /// Series Finale := Indicates the initial airing of the final episode of the final season of a TV series.
        /// Series Premiere := Indicates the initial airing of the first episode of the first season of a season of a TV series.
        /// </summary>
        [JsonProperty("isPremiereOrFinale")]
        public string IsPremiereOrFinale { get; set; }

        /// <summary>
        /// cc := Program is encoded with captioning of dialogue for the hearing impaired.
        /// DD := Programs is available in Dolby Digital sound.
        /// DD 5.1 := Program is available in Dolby Digital 5.1 digital sound
        /// Dolby := Program is available in some form of Dolby digital sound
        /// DVS := "Descriptive Video Service" - Program is available with an audio descriptive feed (audio narration of what is taking place on the screen).
        /// stereo := Program is available in some form of stereo sound.
        /// </summary>
        [JsonProperty("audioProperties")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] AudioProperties { get; set; }

        /// <summary>
        /// HDTV := Program has been identified by the provider on a schedule as being available in a high-definition format. Could be either native HD or upconverted from SDTV.
        /// HD 1080i, HD 1080p, HD 720p, HD 480p
        /// HD Unknown := The last level (HD Unknown) indicates specific program/event manually marked as unknown level.
        /// Letterbox := Program is telecast in a Letterbox/Widescreen format.
        /// UHDTV := This qualifier indicates that the program has been identified by the provider on a schedule as being available in Ultra high-definition format.
        /// HDR := HDR is the generic label that Gracenote is using to identify any brand of HDR. All HDR content is UHD (4K),  but NOT all 4K is HDR.
        /// Widescreen := Same as Letterbox.
        /// </summary>
        [JsonProperty("videoProperties")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] VideoProperties { get; set; }

        /// <summary>
        /// Programs for which provider suggests parental advisory based on program content.
        /// </summary>
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