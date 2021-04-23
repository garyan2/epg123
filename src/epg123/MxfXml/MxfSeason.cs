using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public partial class Mxf
    {
        private readonly Dictionary<string, MxfSeason> _seasons = new Dictionary<string, MxfSeason>();
        public MxfSeason GetSeason(string seriesId, int seasonNumber, string protoTypicalProgram)
        {
            if (_seasons.TryGetValue($"{seriesId}_{seasonNumber}", out var season)) return season;
            With.Seasons.Add(season = new MxfSeason
            {
                Index = With.Seasons.Count + 1,
                mxfSeriesInfo = GetSeriesInfo(seriesId),
                SeasonNumber = seasonNumber,
                ProtoTypicalProgram = protoTypicalProgram
            });
            _seasons.Add(seriesId + "_" + seasonNumber, season);
            return season;
        }
    }

    public class MxfSeason
    {
        public override string ToString() { return Id; }

        [XmlIgnore] public int Index;
        [XmlIgnore] public string ProtoTypicalProgram;
        [XmlIgnore] public string UidOverride;
        [XmlIgnore] public MxfGuideImage mxfGuideImage;
        [XmlIgnore] public MxfSeriesInfo mxfSeriesInfo;

        [XmlIgnore] public Dictionary<string, dynamic> extras = new Dictionary<string, dynamic>();

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as sn1, sn2, and sn3. Seasons are referenced by Program elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"sn{Index}";
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!Season!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => string.IsNullOrEmpty(UidOverride) ? $"!Season!{mxfSeriesInfo.SeriesId}_{SeasonNumber}" : $"!Season!{UidOverride}";
            set { }
        }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("endAirdate")]
        public string EndAirdate { get; set; }

        /// <summary>
        /// An image to display for this season.
        /// This value contains the GuideImage id attribute.
        /// </summary>
        [XmlAttribute("guideImage")]
        public string GuideImage
        {
            get => mxfGuideImage?.ToString() ?? "";
            set { }
        }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("seasonNumber")]
        public int SeasonNumber { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("startAirdate")]
        public string StartAirdate { get; set; }

        /// <summary>
        /// The series ID to which this season belongs.
        /// This value should be specified because it doesn't make sense for a season to not be part of a series.
        /// </summary>
        [XmlAttribute("series")]
        public string Series
        {
            get => mxfSeriesInfo?.ToString();
            set { }
        }

        /// <summary>
        /// The name of this season.
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("title"), XmlIgnore]
        public string Title
        {
            get => $"{mxfSeriesInfo?.Title}, Season {SeasonNumber}";
            set { }
        }

        /// <summary>
        /// The name of the studio that created this season.
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("studio")]
        public string Studio { get; set; }

        /// <summary>
        /// The year this season was aired.
        /// </summary>
        [XmlAttribute("year")]
        public string Year { get; set; }
    }
}