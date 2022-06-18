using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfScheduleEntries
    {
        [XmlAttribute("service")]
        public string Service { get; set; }

        [XmlElement("ScheduleEntry")]
        public List<MxfScheduleEntry> ScheduleEntry { get; set; }
        public bool ShouldSerializeScheduleEntry()
        {
            var endTime = DateTime.MinValue;
            foreach (var entry in ScheduleEntry)
            {
                if (entry.StartTime != endTime) entry.IncludeStartTime = true;
                endTime = entry.StartTime + TimeSpan.FromSeconds(entry.Duration);
            }
            return true;
        }
    }

    public class MxfScheduleEntry
    {
        [XmlIgnore] public MxfProgram mxfProgram;
        [XmlIgnore] public bool IncludeStartTime;

        private int _tvRating;

        [XmlIgnore] public Dictionary<string, dynamic> extras = new Dictionary<string, dynamic>();

        // duplicate of Microsoft.MediaCenter.Guide.TVRating enum
        private enum McepgTvRating
        {
            Unknown = 0,
            UsaY = 1,
            UsaY7 = 2,
            UsaG = 3,
            UsaPg = 4,
            UsaTV14 = 5,
            UsaMA = 6,
            DeAll = 7,
            De6 = 8,
            De12 = 9,
            De16 = 10,
            DeAdults = 11,
            FrAll = 12,
            Fr10 = 13,
            Fr12 = 14,
            Fr16 = 15,
            Fr18 = 16,
            KrAll = 17,
            Kr7 = 18,
            Kr12 = 19,
            Kr15 = 20,
            Kr19 = 21,
            GB_UC = 22,
            GbU = 23,
            GbPG = 24,
            Gb12 = 25,
            Gb15 = 26,
            Gb18 = 27,
            GbR18 = 28
        }

        /// <summary>
        /// An ID of a Program element.
        /// </summary>
        [XmlAttribute("program")]
        public string Program
        {
            get => mxfProgram?.ToString();
            set { }
        }

        /// <summary>
        /// Specifies the start time of the broadcast.
        /// The dateTime type is in UTC. This attribute is only specified for the first ScheduleEntry element in a group.
        /// </summary>
        [XmlAttribute("startTime")]
        public DateTime StartTime { get; set; }
        public bool ShouldSerializeStartTime() { return IncludeStartTime; }

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
        public bool ShouldSerializeIsCc() { return IsCc; }

        /// <summary>
        /// Indicates whether this broadcast is deaf-signed
        /// </summary>
        [XmlAttribute("isSigned")]
        public bool IsSigned { get; set; }
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
        public int AudioFormat { get; set; }
        public bool ShouldSerializeAudioFormat() { return AudioFormat > 0; }

        /// <summary>
        /// Indicates whether this is a live broadcast.
        /// </summary>
        [XmlAttribute("isLive")]
        public bool IsLive { get; set; }
        public bool ShouldSerializeIsLive() { return IsLive; }

        /// <summary>
        /// Indicates whether this is live sports event.
        /// </summary>
        [XmlAttribute("isLiveSports")]
        public bool IsLiveSports { get; set; }
        public bool ShouldSerializeIsLiveSports() { return IsLiveSports; }

        /// <summary>
        /// Indicates whether this program has been taped and is being replayed (for example, a sports event).
        /// </summary>
        [XmlAttribute("isTape")]
        public bool IsTape { get; set; }
        public bool ShouldSerializeIsTape() { return IsTape; }

        /// <summary>
        /// Indicates whether this program is being broadcast delayed (for example, an award show such as the Academy Awards).
        /// </summary>
        [XmlAttribute("isDelay")]
        public bool IsDelay { get; set; }
        public bool ShouldSerializeIsDelay() { return IsDelay; }

        /// <summary>
        /// Indicates whether this program is subtitled.
        /// </summary>
        [XmlAttribute("isSubtitled")]
        public bool IsSubtitled { get; set; }
        public bool ShouldSerializeIsSubtitled() { return IsSubtitled; }

        /// <summary>
        /// Indicates whether this program is a premiere.
        /// </summary>
        [XmlAttribute("isPremiere")]
        public bool IsPremiere { get; set; }
        public bool ShouldSerializeIsPremiere() { return IsPremiere; }

        /// <summary>
        /// Indicates whether this program is a finale.
        /// </summary>
        [XmlAttribute("isFinale")]
        public bool IsFinale { get; set; }
        public bool ShouldSerializeIsFinale() { return IsFinale; }

        /// <summary>
        /// Indicates whether this program was joined in progress.
        /// </summary>
        [XmlAttribute("isInProgress")]
        public bool IsInProgress { get; set; }
        public bool ShouldSerializeIsInProgress() { return IsInProgress; }

        /// <summary>
        /// Indicates whether this program has a secondary audio program broadcast at the same time.
        /// </summary>
        [XmlAttribute("isSap")]
        public bool IsSap { get; set; }
        public bool ShouldSerializeIsSap() { return IsSap; }

        /// <summary>
        /// Indicates whether this program has been blacked out.
        /// </summary>
        [XmlAttribute("isBlackout")]
        public bool IsBlackout { get; set; }
        public bool ShouldSerializeIsBlackout() { return IsBlackout; }

        /// <summary>
        /// Indicates whether this program has been broadcast with an enhanced picture.
        /// </summary>
        [XmlAttribute("isEnhanced")]
        public bool IsEnhanced { get; set; }
        public bool ShouldSerializeIsEnhanced() { return IsEnhanced; }

        /// <summary>
        /// Indicates whether this program is broadcast in 3D.
        /// </summary>
        [XmlAttribute("is3D")]
        public bool Is3D { get; set; }
        public bool ShouldSerializeIs3D() { return Is3D; }

        /// <summary>
        /// Indicates whether this program is broadcast in letterbox format.
        /// </summary>
        [XmlAttribute("isLetterbox")]
        public bool IsLetterbox { get; set; }
        public bool ShouldSerializeIsLetterbox() { return IsLetterbox; }

        /// <summary>
        /// Indicates whether this program is broadcast in high definition (HD).
        /// Determines whether the HD icon is displayed.
        /// </summary>
        [XmlAttribute("isHdtv")]
        public bool IsHdtv { get; set; }
        public bool ShouldSerializeIsHdtv() { return IsHdtv; }

        /// <summary>
        /// Indicates whether this program is broadcast simultaneously in HD.
        /// </summary>
        [XmlAttribute("isHdtvSimulCast")]
        public bool IsHdtvSimulCast { get; set; }
        public bool ShouldSerializeIsHdtvSimulCast() { return IsHdtvSimulCast; }

        /// <summary>
        /// Indicates whether this program is broadcast with Descriptive Video Service (DVS).
        /// </summary>
        [XmlAttribute("isDvs")]
        public bool IsDvs { get; set; }
        public bool ShouldSerializeIsDvs() { return IsDvs; }

        /// <summary>
        /// Specifies the part number (for instance, if this is part 1 of 3, use "1").
        /// </summary>
        [XmlAttribute("part")]
        public int Part { get; set; }
        public bool ShouldSerializePart() { return Part != 0; }

        /// <summary>
        /// Specifies the total number of parts (for instance, if this is part 1 of 3, use "3").
        /// </summary>
        [XmlAttribute("parts")]
        public int Parts { get; set; }
        public bool ShouldSerializeParts() { return Parts != 0; }

        /// <summary>
        /// Specifies the TV parental rating (not documented on website)
        /// </summary>
        [XmlAttribute("tvRating")]
        public int TvRating
        {
            get
            {
                if (_tvRating > 0) return _tvRating;

                var ratings = new Dictionary<string, string>();
                if (extras.ContainsKey("ratings"))
                {
                    foreach (KeyValuePair<string, string> rating in extras["ratings"])
                    {
                        if (!ratings.TryGetValue(rating.Key, out _))
                        {
                            ratings.Add(rating.Key, rating.Value);
                        }
                    }
                }

                if (mxfProgram.extras.ContainsKey("ratings"))
                {
                    foreach (KeyValuePair<string, string> rating in mxfProgram.extras["ratings"])
                    {
                        if (!ratings.TryGetValue(rating.Key, out _))
                        {
                            ratings.Add(rating.Key, rating.Value);
                        }
                    }
                }

                var maxValue = 0;
                foreach (var keyValue in ratings)
                {
                    switch (keyValue.Key)
                    {
                        case "USA Parental Rating":
                            switch (keyValue.Value.ToLower())
                            {
                                // USA Parental Rating
                                case "tvy":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.UsaY);
                                    break;
                                case "tvy7":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.UsaY7);
                                    break;
                                case "tvg":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.UsaG);
                                    break;
                                case "tvpg":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.UsaPg);
                                    break;
                                case "tv14":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.UsaTV14);
                                    break;
                                case "tvma":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.UsaMA);
                                    break;
                            }
                            break;
                        case "Freiwillige Selbstkontrolle der Filmwirtschaft":
                            switch (keyValue.Value.ToLower())
                            {
                                // DEU Freiwillige Selbstkontrolle der Filmwirtschaft
                                case "0":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.DeAll);
                                    break;
                                case "6":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.De6);
                                    break;
                                case "12":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.De12);
                                    break;
                                case "16":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.De16);
                                    break;
                                case "18":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.DeAdults);
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
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.Fr10);
                                    break;
                                case "-12":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.Fr12);
                                    break;
                                case "-16":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.Fr16);
                                    break;
                                case "-18":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.Fr18);
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
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.GB_UC);
                                    break;
                                case "u":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.GbU);
                                    break;
                                case "pg":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.GbPG);
                                    break;
                                case "12":
                                case "12a":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.Gb12);
                                    break;
                                case "15":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.Gb15);
                                    break;
                                case "18":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.Gb18);
                                    break;
                                case "r18":
                                    maxValue = Math.Max(maxValue, (int)McepgTvRating.GbR18);
                                    break;
                            }
                            break;
                    }
                }
                return maxValue;
            }
            set => _tvRating = value;
        }
        public bool ShouldSerializeTvRating() { return TvRating != 0; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isClassroom")]
        public bool IsClassroom { get; set; }
        public bool ShouldSerializeIsClassroom() { return IsClassroom; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("isRepeat")]
        public bool IsRepeat { get; set; }
        public bool ShouldSerializeIsRepeat() { return IsRepeat; }
    }
}