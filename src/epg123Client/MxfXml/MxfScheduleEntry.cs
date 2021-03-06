﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.MxfXml
{
    public class MxfScheduleEntries
    {
        [XmlAttribute("service")] 
        public string Service { get; set; }

        [XmlElement("ScheduleEntry")] 
        public List<MxfScheduleEntry> ScheduleEntry { get; set; }
    }

    public class MxfScheduleEntry
    {
        /// <summary>
        /// An ID of a Program element.
        /// </summary>
        [XmlAttribute("program")]
        public string Program { get; set; }

        /// <summary>
        /// Specifies the start time of the broadcast.
        /// The dateTime type is in UTC. This attribute is only specified for the first ScheduleEntry element in a group.
        /// </summary>
        [XmlAttribute("startTime")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The duration of the broadcast, in seconds.
        /// </summary>
        [XmlAttribute("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// Indicates whether this broadcast is closed captioned.
        /// </summary>
        [XmlAttribute("isCC")]
        public bool IsCc { get; set; }

        /// <summary>
        /// Indicates the audio format of the broadcast.
        /// Possible values are:
        /// 0 = Not specified
        /// 1 = Mono
        /// 2 = Stereo
        /// 3 = Dolby
        /// 4 = Dolby Digital
        /// 5 = THX
        /// </summary>
        [XmlAttribute("audioFormat")]
        public int AudioFormat { get; set; }

        /// <summary>
        /// Indicates whether this is a live broadcast.
        /// </summary>
        [XmlAttribute("isLive")]
        public bool IsLive { get; set; }

        /// <summary>
        /// Indicates whether this is live sports event.
        /// </summary>
        [XmlAttribute("isLiveSports")]
        public bool IsLiveSports { get; set; }

        /// <summary>
        /// Indicates whether this program has been taped and is being replayed (for example, a sports event).
        /// </summary>
        [XmlAttribute("isTape")]
        public bool IsTape { get; set; }

        /// <summary>
        /// Indicates whether this program is being broadcast delayed (for example, an award show such as the Academy Awards).
        /// </summary>
        [XmlAttribute("isDelay")]
        public bool IsDelay { get; set; }

        /// <summary>
        /// Indicates whether this program is subtitled.
        /// </summary>
        [XmlAttribute("isSubtitled")]
        public bool IsSubtitled { get; set; }

        /// <summary>
        /// Indicates whether this program is a premiere.
        /// </summary>
        [XmlAttribute("isPremiere")]
        public bool IsPremiere { get; set; }

        /// <summary>
        /// Indicates whether this program is a finale.
        /// </summary>
        [XmlAttribute("isFinale")]
        public bool IsFinale { get; set; }

        /// <summary>
        /// Indicates whether this program was joined in progress.
        /// </summary>
        [XmlAttribute("isInProgress")]
        public bool IsInProgress { get; set; }

        /// <summary>
        /// Indicates whether this program has a secondary audio program broadcast at the same time.
        /// </summary>
        [XmlAttribute("isSap")]
        public bool IsSap { get; set; }

        /// <summary>
        /// Indicates whether this program has been blacked out.
        /// </summary>
        [XmlAttribute("isBlackout")]
        public bool IsBlackout { get; set; }

        /// <summary>
        /// Indicates whether this program has been broadcast with an enhanced picture.
        /// </summary>
        [XmlAttribute("isEnhanced")]
        public bool IsEnhanced { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast in 3D.
        /// </summary>
        [XmlAttribute("is3D")]
        public bool Is3D { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast in letterbox format.
        /// </summary>
        [XmlAttribute("isLetterbox")]
        public bool IsLetterbox { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast in high definition (HD).
        /// Determines whether the HD icon is displayed.
        /// </summary>
        [XmlAttribute("isHdtv")]
        public bool IsHdtv { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast simultaneously in HD.
        /// </summary>
        [XmlAttribute("isHdtvSimulCast")]
        public bool IsHdtvSimulCast { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast with Descriptive Video Service (DVS).
        /// </summary>
        [XmlAttribute("isDvs")]
        public bool IsDvs { get; set; }

        /// <summary>
        /// Specifies the part number (for instance, if this is part 1 of 3, use "1").
        /// </summary>
        [XmlAttribute("part")]
        public int Part { get; set; }

        /// <summary>
        /// Specifies the total number of parts (for instance, if this is part 1 of 3, use "3").
        /// </summary>
        [XmlAttribute("parts")]
        public int Parts { get; set; }

        /// <summary>
        /// Specifies the TV parental rating (not documented on website)
        /// </summary>
        [XmlAttribute("tvRating")]
        public int TvRating { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isClassroom")]
        public bool IsClassroom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isRepeat")]
        public bool IsRepeat { get; set; }
    }
}