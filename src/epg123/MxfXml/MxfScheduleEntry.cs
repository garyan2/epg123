using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
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
                    } while (ScheduleEntry[s].StartTime == DateTime.MinValue);
                    return ScheduleEntry[s].StartTime + TimeSpan.FromSeconds(totalSeconds);
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
        // duplicate of Microsoft.MediaCenter.Guide.TVRating enum
        private enum McepgTVRating
        {
            Unknown,
            UsaY,
            UsaY7,
            UsaG,
            UsaPG,
            UsaTV14,
            UsaMA,
            DeAll,
            De6,
            De12,
            De16,
            DeAdults,
            FrAll,
            Fr10,
            Fr12,
            Fr16,
            Fr18,
            KrAll,
            Kr7,
            Kr12,
            Kr15,
            Kr19,
            GB_UC,
            GbU,
            GbPG,
            Gb12,
            Gb15,
            Gb18,
            GbR18
        }

        [XmlIgnore]
        private Dictionary<string, string> programContentRatings
        {
            get
            {
                return sdJson2mxf.sdMxf.With[0].Programs[int.Parse(Program) - 1].contentRatings;
            }
        }

        [XmlIgnore]
        public Dictionary<string, string> schedTvRatings { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> Ratings
        {
            get
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                if (schedTvRatings != null)
                {
                    string dummy;
                    foreach (KeyValuePair<string, string> keyValuePair in schedTvRatings)
                    {
                        if (!ret.TryGetValue(keyValuePair.Key, out dummy))
                        {
                            ret.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                    }
                }
                if (programContentRatings != null)
                {
                    string dummy;
                    foreach (KeyValuePair<string, string> keyValuePair in programContentRatings)
                    {
                        if (!ret.TryGetValue(keyValuePair.Key, out dummy))
                        {
                            ret.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                    }
                }
                return ret;
            }
        }

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
        public bool ShouldSerializeStartTime() { return StartTime != DateTime.MinValue; }

        /// <summary>
        /// The duration of the broadcast, in seconds.
        /// </summary>
        [XmlAttribute("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// Indicates whether this broadcast is closed captioned.
        /// </summary>
        [XmlAttribute("isCC")]
        public bool IsCC { get; set; } = false;
        public bool ShouldSerializeIsCC() { return IsCC; }

        /// <summary>
        /// Indicates whether this broadcast is deaf-signed
        /// </summary>
        [XmlAttribute("isSigned")]
        public bool IsSigned { get; set; } = false;
        public bool ShouldSerializeIsSigned() { return IsSigned; }

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
        public int AudioFormat { get; set; } = 0;
        public bool ShouldSerializeAudioFormat() { return AudioFormat > 0; }

        /// <summary>
        /// Indicates whether this is a live broadcast.
        /// </summary>
        [XmlAttribute("isLive")]
        public bool IsLive { get; set; } = false;
        public bool ShouldSerializeIsLive() { return IsLive; }

        /// <summary>
        /// Indicates whether this is live sports event.
        /// </summary>
        [XmlAttribute("isLiveSports")]
        public bool IsLiveSports { get; set; } = false;
        public bool ShouldSerializeIsLiveSports() { return IsLiveSports; }

        /// <summary>
        /// Indicates whether this program has been taped and is being replayed (for example, a sports event).
        /// </summary>
        [XmlAttribute("isTape")]
        public bool IsTape { get; set; } = false;
        public bool ShouldSerializeIsTape() { return IsTape; }

        /// <summary>
        /// Indicates whether this program is being broadcast delayed (for example, an award show such as the Academy Awards).
        /// </summary>
        [XmlAttribute("isDelay")]
        public bool IsDelay { get; set; } = false;
        public bool ShouldSerializeIsDelay() { return IsDelay; }

        /// <summary>
        /// Indicates whether this program is subtitled.
        /// </summary>
        [XmlAttribute("isSubtitled")]
        public bool IsSubtitled { get; set; } = false;
        public bool ShouldSerializeIsSubtitled() { return IsSubtitled; }

        /// <summary>
        /// Indicates whether this program is a premiere.
        /// </summary>
        [XmlAttribute("isPremiere")]
        public bool IsPremiere { get; set; } = false;
        public bool ShouldSerializeIsPremiere() { return IsPremiere; }

        /// <summary>
        /// Indicates whether this program is a finale.
        /// </summary>
        [XmlAttribute("isFinale")]
        public bool IsFinale { get; set; } = false;
        public bool ShouldSerializeIsFinale() { return IsFinale; }

        /// <summary>
        /// Indicates whether this program was joined in progress.
        /// </summary>
        [XmlAttribute("isInProgress")]
        public bool IsInProgress { get; set; } = false;
        public bool ShouldSerializeIsInProgress() { return IsInProgress; }

        /// <summary>
        /// Indicates whether this program has a secondary audio program broadcast at the same time.
        /// </summary>
        [XmlAttribute("isSap")]
        public bool IsSap { get; set; } = false;
        public bool ShouldSerializeIsSap() { return IsSap; }

        /// <summary>
        /// Indicates whether this program has been blacked out.
        /// </summary>
        [XmlAttribute("isBlackout")]
        public bool IsBlackout { get; set; } = false;
        public bool ShouldSerializeIsBlackout() { return IsBlackout; }

        /// <summary>
        /// Indicates whether this program has been broadcast with an enhanced picture.
        /// </summary>
        [XmlAttribute("isEnhanced")]
        public bool IsEnhanced { get; set; } = false;
        public bool ShouldSerializeIsEnhanced() { return IsEnhanced; }

        /// <summary>
        /// Indicates whether this program is broadcast in 3D.
        /// </summary>
        [XmlAttribute("is3D")]
        public bool Is3D { get; set; } = false;
        public bool ShouldSerializeIs3D() { return Is3D; }

        /// <summary>
        /// Indicates whether this program is broadcast in letterbox format.
        /// </summary>
        [XmlAttribute("isLetterbox")]
        public bool IsLetterbox { get; set; } = false;
        public bool ShouldSerializeIsLetterbox() { return IsLetterbox; }

        /// <summary>
        /// Indicates whether this program is broadcast in high definition (HD).
        /// Determines whether the HD icon is displayed.
        /// </summary>
        [XmlAttribute("isHdtv")]
        public bool IsHdtv { get; set; } = false;
        public bool ShouldSerializeIsHdtv() { return IsHdtv; }

        /// <summary>
        /// Indicates whether this program is broadcast simultaneously in HD.
        /// </summary>
        [XmlAttribute("isHdtvSimulCast")]
        public bool IsHdtvSimulCast { get; set; } = false;
        public bool ShouldSerializeIsHdtvSimulCast() { return IsHdtvSimulCast; }

        /// <summary>
        /// Indicates whether this program is broadcast with Descriptive Video Service (DVS).
        /// </summary>
        [XmlAttribute("isDvs")]
        public bool IsDvs { get; set; } = false;
        public bool ShouldSerializeIsDvs() { return IsDvs; }

        /// <summary>
        /// Specifies the part number (for instance, if this is part 1 of 3, use "1").
        /// </summary>
        [XmlAttribute("part")]
        public int Part { get; set; } = 0;
        public bool ShouldSerializePart() { return Part != 0; }

        /// <summary>
        /// Specifies the total number of parts (for instance, if this is part 1 of 3, use "3").
        /// </summary>
        [XmlAttribute("parts")]
        public int Parts { get; set; } = 0;
        public bool ShouldSerializeParts() { return Parts != 0; }

        /// <summary>
        /// Specifies the TV parental rating (not documented on website)
        /// </summary>
        [XmlAttribute("tvRating")]
        public int TvRating
        {
            get
            {
                int maxValue = 0;
                foreach (KeyValuePair<string, string> keyValue in Ratings)
                {
                    switch (keyValue.Key)
                    {
                        case "USA Parental Rating":
                            switch (keyValue.Value.ToLower())
                            {
                                // USA Parental Rating
                                case "tvy":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.UsaY);
                                    break;
                                case "tvy7":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.UsaY7);
                                    break;
                                case "tvg":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.UsaG);
                                    break;
                                case "tvpg":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.UsaPG);
                                    break;
                                case "tv14":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.UsaTV14);
                                    break;
                                case "tvma":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.UsaMA);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Freiwillige Selbstkontrolle der Filmwirtschaft":
                            switch (keyValue.Value.ToLower())
                            {
                                // DEU Freiwillige Selbstkontrolle der Filmwirtschaft
                                case "0":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.DeAll);
                                    break;
                                case "6":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.De6);
                                    break;
                                case "12":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.De12);
                                    break;
                                case "16":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.De16);
                                    break;
                                case "18":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.DeAdults);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Conseil Supérieur de l'Audiovisuel":
                            switch (keyValue.Value.ToLower())
                            {
                                // FRA Conseil Supérieur de l'Audiovisuel
                                //case "":
                                //    maxValue = Math.Max(maxValue, (int)McepgTVRating.FrAll);
                                //    break;
                                case "-10":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.Fr10);
                                    break;
                                case "-12":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.Fr12);
                                    break;
                                case "-16":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.Fr16);
                                    break;
                                case "-18":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.Fr18);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "UK Content Provider":
                        case "British Board of Film Classification":
                            switch (keyValue.Value.ToLower())
                            {
                                // GBR UK Content Provider
                                // GBR British Board of Film Classification
                                case "uc":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.GB_UC);
                                    break;
                                case "u":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.GbU);
                                    break;
                                case "pg":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.GbPG);
                                    break;
                                case "12":
                                case "12a":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.Gb12);
                                    break;
                                case "15":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.Gb15);
                                    break;
                                case "18":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.Gb18);
                                    break;
                                case "r18":
                                    maxValue = Math.Max(maxValue, (int)McepgTVRating.GbR18);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
                return maxValue;
            }
            set { }
        }
        public bool ShouldSerializeTvRating() { return TvRating != 0; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isClassroom")]
        public bool IsClassroom { get; set; } = false;
        public bool ShouldSerializeIsClassroom() { return IsClassroom; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isRepeat")]
        public bool IsRepeat { get; set; } = false;
        public bool ShouldSerializeIsRepeat() { return IsRepeat; }
    }
}