using epg123Client.SatXml;
using GaRyan2.MxfXml;
using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace epg123Client
{
    internal class SatMxf
    {
        public static bool UpdateDvbsTransponders(bool ignoreDefault)
        {
            var ret = false;
            var mxfPath = Helper.TransponderMxfPath;
            try
            {
                // if defaultsatellites.mxf exists, import it and return
                if (!ignoreDefault && File.Exists(Helper.DefaultSatellitesPath))
                {
                    mxfPath = Helper.DefaultSatellitesPath;
                    goto ImportAndLock;
                }

                // read the satellites.xml file from either the file system or the resource file
                var satXml = new Satellites();
                if (File.Exists(Helper.SatellitesXmlPath))
                {
                    satXml = Helper.ReadXmlFile(Helper.SatellitesXmlPath, typeof(Satellites));
                }
                else
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("epg123Client.SatMxf.satellites.xml"))
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var serializer = new XmlSerializer(typeof(Satellites));
                        TextReader tr = new StringReader(reader.ReadToEnd());
                        satXml = (Satellites)serializer.Deserialize(tr);
                        tr.Close();
                    }
                }

                // populate the mxf class
                var mxf = new MXF(null, null, null, null, MXF.TYPEMXF.SATELLITES);
                var unique = satXml.Satellite.GroupBy(arg => arg.Name).Select(arg => arg.FirstOrDefault());
                foreach (var sat in unique)
                {
                    var freqs = new HashSet<string>();
                    var mxfSat = new MxfDvbsSatellite { Name = sat.Name, PositionEast = sat.Position, _transponders = new List<MxfDvbsTransponder>() };
                    var matches = satXml.Satellite.Where(arg => arg.Name == sat.Name);
                    foreach (var match in matches)
                        foreach (var txp in match.Transponder.Where(txp => freqs.Add($"{txp.Frequency}_{txp.Polarization}")))
                        {
                            mxfSat._transponders.Add(new MxfDvbsTransponder
                            {
                                _satellite = mxfSat,
                                CarrierFrequency = txp.Frequency,
                                Polarization = txp.Polarization,
                                SymbolRate = txp.SymbolRate
                            });
                        }
                    mxf.DvbsDataSet._allSatellites.Add(mxfSat);
                }

                // create the temporary mxf file
                Helper.WriteXmlFile(mxf, Helper.TransponderMxfPath);

            // import the mxf file with new satellite transponders
            ImportAndLock:
                ret = WmcStore.ImportMxfFile(mxfPath);
                var uid = WmcStore.WmcObjectStore.UIds["!DvbsDataSet"];
                uid.Lock();
                uid.Update();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during UpdateDvbsTransponders(). Message:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }
    }
}