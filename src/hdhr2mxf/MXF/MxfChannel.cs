using System.Xml.Serialization;

namespace hdhr2mxf.MXF
{
    public class MxfChannel
    {
        private int _number;

        [XmlIgnore]
        public string LineupUid;

        [XmlIgnore]
        public string StationId;

        [XmlIgnore]
        public string Match;

        [XmlIgnore]
        public bool IsHd;

        /// <summary>
        /// A unique ID that is consistent between loads. 
        /// This value should take the form "!Channel!uniqueLineupName!number_subNumber", where uniqueLineupName is the Lineup element's uid, and number and subNumber are the values from the number and subNumber attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => ("!Channel!" + LineupUid + "!" + StationId + "_" + Number + "_" + SubNumber);
            set { }
        }

        /// <summary>
        /// A reference to the Lineup element.
        /// This value should always be "l1".
        /// </summary>
        [XmlAttribute("lineup")]
        public string Lineup { get; set; }

        /// <summary>
        /// The service to reference.
        /// This value should be the Service id attribute.
        /// </summary>
        [XmlAttribute("service")]
        public string Service { get; set; }

        /// <summary>
        /// Used to automatically map this channel of listings onto the scanned channel.
        /// If not specified, the value of the Service element's callSign attribute is used as the default value.
        /// If matchName is specified, it should take the following format according to the signal type:
        /// PAL/NTSC: The call sign
        /// DVB-T: The call sign, or a string of format "DVBT:onid:tsid:sid"
        /// DVB-S: A string of format "DVBS:sat:freq:onid:tsid:sid"
        /// Where: 
        /// onid is the originating network ID.
        /// tsid is the transport stream ID.
        /// sid is the service ID.
        /// sat is the satellite position.
        /// freq is the frequency, in MHz.
        /// Note All of these values are expressed as decimal integer numbers.
        /// </summary>
        [XmlAttribute("matchName")]
        public string MatchName { get; set; }

        /// <summary>
        /// The number used to access the service.
        /// Do not specify this value if you want Windows Media Center to assign the channel number.
        /// </summary>
        [XmlAttribute("number")]
        public int Number
        {
            get
            {
                if (_number == 0)
                {
                    return -1;
                }

                return _number;
            }
            set => _number = value;
        }

        /// <summary>
        /// The subnumber used to access the service.
        /// </summary>
        [XmlAttribute("subNumber")]
        public int SubNumber { get; set; }
    }
}
