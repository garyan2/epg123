﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfWith
    {
        [XmlAttribute("provider")]
        public string Provider { get; set; }

        [XmlArrayItem("Keyword")]
        public List<MxfKeyword> Keywords { get; set; }
        public bool ShouldSerializeKeywords()
        {
            Keywords = Keywords?.OrderBy(k => k.GrpIndex).ThenBy(k => k.Id).ToList();
            return true;
        }

        [XmlArrayItem("KeywordGroup")]
        public List<MxfKeywordGroup> KeywordGroups { get; set; }
        public bool ShouldSerializeKeywordGroups()
        {
            KeywordGroups = KeywordGroups?.OrderBy(k => k.Index).ThenBy(k => k.Uid).ToList();
            return true;
        }

        [XmlArrayItem("GuideImage")]
        public List<MxfGuideImage> GuideImages { get; set; }

        [XmlArrayItem("Person")]
        public List<MxfPerson> People { get; set; }

        [XmlArrayItem("SeriesInfo")]
        public List<MxfSeriesInfo> SeriesInfos { get; set; }

        [XmlArrayItem("Season")]
        public List<MxfSeason> Seasons { get; set; }

        [XmlArrayItem("Program")]
        public List<MxfProgram> Programs { get; set; }

        [XmlArrayItem("Affiliate")]
        public List<MxfAffiliate> Affiliates { get; set; }

        [XmlArrayItem("Service")]
        public List<MxfService> Services { get; set; }

        [XmlElement("ScheduleEntries")]
        public List<MxfScheduleEntries> ScheduleEntries { get; set; }

        [XmlArrayItem("Lineup")]
        public List<MxfLineup> Lineups { get; set; }
    }
}