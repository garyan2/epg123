using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MxfXml
{
    public class MxfScheduleEntries
    {
        public DateTime endTime
        {
            get
            {
                if (ScheduleEntry.Count > 0)
                {
                    int s = ScheduleEntry.Count;
                    int totalSeconds = 0;
                    do
                    {
                        totalSeconds += ScheduleEntry[--s].Duration;
                    } while (ScheduleEntry[s].StartTime == null);
                    return DateTime.Parse(ScheduleEntry[s].StartTime) + TimeSpan.FromSeconds(totalSeconds);
                }
                return DateTime.MinValue;
            }
        }

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
        public string StartTime { get; set; }

        /// <summary>
        /// The duration of the broadcast, in seconds.
        /// </summary>
        [XmlAttribute("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// Indicates whether this broadcast is closed captioned.
        /// </summary>
        [XmlAttribute("isCC")]
        public string IsCC { get; set; }

        /// <summary>
        /// Indicates whether this broadcast is deaf-signed
        /// </summary>
        [XmlAttribute("isSigned")]
        public string IsSigned { get; set; }

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
        public string AudioFormat { get; set; }

        /// <summary>
        /// Indicates whether this is a live broadcast.
        /// </summary>
        [XmlAttribute("isLive")]
        public string IsLive { get; set; }

        /// <summary>
        /// Indicates whether this is live sports event.
        /// </summary>
        [XmlAttribute("isLiveSports")]
        public string IsLiveSports { get; set; }

        /// <summary>
        /// Indicates whether this program has been taped and is being replayed (for example, a sports event).
        /// </summary>
        [XmlAttribute("isTape")]
        public string IsTape { get; set; }

        /// <summary>
        /// Indicates whether this program is being broadcast delayed (for example, an award show such as the Academy Awards).
        /// </summary>
        [XmlAttribute("isDelay")]
        public string IsDelay { get; set; }

        /// <summary>
        /// Indicates whether this program is subtitled.
        /// </summary>
        [XmlAttribute("isSubtitled")]
        public string IsSubtitled { get; set; }

        /// <summary>
        /// Indicates whether this program is a premiere.
        /// </summary>
        [XmlAttribute("isPremiere")]
        public string IsPremiere { get; set; }

        /// <summary>
        /// Indicates whether this program is a finale.
        /// </summary>
        [XmlAttribute("isFinale")]
        public string IsFinale { get; set; }

        /// <summary>
        /// Indicates whether this program was joined in progress.
        /// </summary>
        [XmlAttribute("isInProgress")]
        public string IsInProgress { get; set; }

        /// <summary>
        /// Indicates whether this program has a secondary audio program broadcast at the same time.
        /// </summary>
        [XmlAttribute("isSap")]
        public string IsSap { get; set; }

        /// <summary>
        /// Indicates whether this program has been blacked out.
        /// </summary>
        [XmlAttribute("isBlackout")]
        public string IsBlackout { get; set; }

        /// <summary>
        /// Indicates whether this program has been broadcast with an enhanced picture.
        /// </summary>
        [XmlAttribute("isEnhanced")]
        public string IsEnhanced { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast in 3D.
        /// </summary>
        [XmlAttribute("is3D")]
        public string Is3D { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast in letterbox format.
        /// </summary>
        [XmlAttribute("isLetterbox")]
        public string IsLetterbox { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast in high definition (HD).
        /// Determines whether the HD icon is displayed.
        /// </summary>
        [XmlAttribute("isHdtv")]
        public string IsHdtv { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast simultaneously in HD.
        /// </summary>
        [XmlAttribute("isHdtvSimulCast")]
        public string IsHdtvSimulCast { get; set; }

        /// <summary>
        /// Indicates whether this program is broadcast with Descriptive Video Service (DVS).
        /// </summary>
        [XmlAttribute("isDvs")]
        public string IsDvs { get; set; }

        /// <summary>
        /// Specifies the part number (for instance, if this is part 1 of 3, use "1").
        /// </summary>
        [XmlAttribute("part")]
        public string Part { get; set; }

        /// <summary>
        /// Specifies the total number of parts (for instance, if this is part 1 of 3, use "3").
        /// </summary>
        [XmlAttribute("parts")]
        public string Parts { get; set; }

        /// <summary>
        /// Specifies the TV parental rating (not documented on website)
        /// </summary>
        [XmlAttribute("tvRating")]
        public string TvRating { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isClassroom")]
        public string IsClassroom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isRepeat")]
        public string IsRepeat { get; set; }
    }
}