using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.MxfXml
{
    [XmlRoot("MXF")]
    public class mxf
    {
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