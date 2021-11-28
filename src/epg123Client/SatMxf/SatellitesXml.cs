using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace epg123Client.SatXml
{
    [XmlRoot("satellites")]
    public class Satellites
    {
        [XmlAnyElement]
        public XmlComment CreationDate { get; set; }

        [XmlElement("sat")]
        public List<Satellite> Satellite { get; set; }
    }

    public class Satellite
    {
        public override string ToString()
        {
            return $"{Name},{Position}";
        }

        private string _name;
        private int _position;

        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                var name = _name.Replace("C-band ", "").Replace("Ku-band ", "").Replace("Ka-band ", "");
                var space = name.IndexOf(' ');
                return $"{name.Substring(space + 1)} ({name.Substring(0, space)})";
            }
            set => _name = value;
        }

        [XmlAttribute("position")]
        public int Position
        {
            get => _position < 0 ? _position + 3600 : _position;
            set => _position = value;
        }

        [XmlElement("transponder")]
        public List<Transponder> Transponder { get; set; }
    }

    public class Transponder
    {
        public override string ToString()
        {
            return $"{Frequency},{Polarization},{SymbolRate}";
        }

        private int _frequency;
        private int _symbolRate;

        [XmlAttribute("frequency")]
        public int Frequency
        {
            get => _frequency / 1000;
            set => _frequency = value;
        }

        [XmlAttribute("symbol_rate")]
        public int SymbolRate
        {
            get => _symbolRate / 1000;
            set => _symbolRate = value;
        }

        [XmlAttribute("polarization")]
        public int Polarization { get; set; }
    }
}
