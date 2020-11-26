using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    [XmlRoot("MXF")]
    public class MXF
    {
        /// <summary>
        /// Definitions for MXF xml format can be located at
        /// https://msdn.microsoft.com/en-us/library/dd776338.aspx
        /// </summary>
        public MXF()
        {
        }

        [XmlArrayItem("Provider")]
        public List<MxfProvider> Providers { get; set; }

        [XmlElement("With")]
        public List<MxfWith> With { get; set; }
    }

    public class MxfWith
    {
        [XmlArrayItem("Program")]
        public List<MxfProgram> Programs { get; set; }

        [XmlArrayItem("Service")]
        public List<MxfService> Services { get; set; }

        [XmlElement("ScheduleEntries")]
        public List<MxfScheduleEntries> ScheduleEntries { get; set; }
    }
}