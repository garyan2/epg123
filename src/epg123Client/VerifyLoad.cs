using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using epg123;
using epg123Client.MxfXml;
using Microsoft.MediaCenter.Guide;

namespace epg123Client
{
    public class VerifyLoad
    {
        public VerifyLoad(string mxfFile, bool verbose = false)
        {
            mxf mxf;
            Logger.WriteMessage("Entering VerifyLoad()");
            Helper.SendPipeMessage("Importing|Verifying MXF Load...");
            using (var stream = new StreamReader(mxfFile))
            {
                var serializer = new XmlSerializer(typeof(mxf));
                TextReader reader = new StringReader(stream.ReadToEnd());
                mxf = (mxf) serializer.Deserialize(reader);
                reader.Close();
            }
            if (!(mxf.Providers[0]?.Name ?? string.Empty).Equals("EPG123"))
            {
                Logger.WriteInformation("The imported MXF file is not a guide listings file created by EPG123. Skipping schedule entry verifications.");
                Logger.WriteInformation("Exiting VerifyLoad()");
                return;
            }

            var entriesChecked = 0;
            var correctedCount = 0;
            foreach (var mxfService in mxf.With[0].Services)
            {
                // get wmcService that matches mxfService using the UId
                Service wmcService = null;
                try
                {
                    wmcService = WmcStore.WmcObjectStore.UIds[mxfService.Uid].Target as Service;
                }
                catch
                {
                    // ignored
                }

                if (wmcService == null)
                {
                    Logger.WriteError($"Service {mxfService.Uid}: {mxfService.CallSign} is not present in the WMC database.");
                    continue;
                }

                // get schedule entries for service
                var mxfScheduleEntries = mxf.With[0].ScheduleEntries.FirstOrDefault(scheduleEntries =>
                    scheduleEntries.Service != null && scheduleEntries.Service.Equals(mxfService.Id));
                if (mxfScheduleEntries == null || mxfScheduleEntries.ScheduleEntry.Count == 0) continue;

                // check to see if the service has any schedule entries
                if (wmcService.ScheduleEntries.Empty && mxfScheduleEntries.ScheduleEntry.Count > 0)
                {
                    if (verbose) Logger.WriteInformation($"Service {mxfService.Uid}: {mxfService.CallSign} does not have any schedule entries in the WMC database.");
                    continue;
                }

                // check mxf file for discontinuities
                var discontinuities = -1;
                var mxfStartTime = DateTime.MinValue;
                foreach (var entry in mxfScheduleEntries.ScheduleEntry.Where(entry =>
                    entry.StartTime != DateTime.MinValue))
                {
                    if (mxfStartTime == DateTime.MinValue) mxfStartTime = entry.StartTime;
                    ++discontinuities;
                }
                if (discontinuities > 0)
                {
                    Logger.WriteInformation(
                        $"Service {mxfService.CallSign} has a time discontinuity. Skipping verification of this station's schedule entries.");
                    continue;
                }
                if (mxfStartTime > DateTime.UtcNow)
                {
                    Logger.WriteInformation($"Service {mxfService.CallSign}: first mxf schedule entry to verify is in the future at {mxfStartTime.ToLocalTime()}.");
                }

                // build a list of wmc schedule entries based on start times
                var wmcScheduleEntryTimes = new Dictionary<DateTime, ScheduleEntry>(wmcService.ScheduleEntries.Count());
                foreach (ScheduleEntry wmcScheduleEntry in wmcService.ScheduleEntries)
                {
                    try
                    {
                        if (wmcScheduleEntry.LockCount == 0 || wmcScheduleEntry.Program == null) throw new Exception();
                        wmcScheduleEntryTimes.Add(wmcScheduleEntry.StartTime, wmcScheduleEntry);
                    }
                    catch
                    {
                        // remove duplicate start time entry; probably from a discontinuity
                        try
                        {
                            wmcScheduleEntry.Refresh();
                            wmcScheduleEntry.Program = null;
                            wmcScheduleEntry.Service = null;
                            wmcScheduleEntry.Unlock();
                            wmcScheduleEntry.Update();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                // make everything right
                try
                {
                    var mxfLastStartTime = DateTime.MinValue;
                    foreach (var mxfScheduleEntry in mxfScheduleEntries.ScheduleEntry)
                    {
                        ++entriesChecked;

                        // update mxfScheduleEntry start time if needed
                        if (mxfScheduleEntry.StartTime != DateTime.MinValue)
                        {
                            mxfStartTime = mxfScheduleEntry.StartTime;
                        }

                        // only verify programs that are in the future and not currently showing or is the next showing
                        if (mxfStartTime < DateTime.UtcNow || DateTime.UtcNow > mxfLastStartTime && DateTime.UtcNow < mxfStartTime + TimeSpan.FromSeconds(mxfScheduleEntry.Duration))
                        {
                            wmcScheduleEntryTimes.Remove(mxfStartTime);
                            mxfLastStartTime = mxfStartTime;
                            mxfStartTime += TimeSpan.FromSeconds(mxfScheduleEntry.Duration);
                            continue;
                        }

                        // find the program in the MXF file for this schedule entry
                        var mxfProgram = mxf.With[0].Programs[int.Parse(mxfScheduleEntry.Program) - 1];

                        // verify a schedule entry exists matching the MXF file and determine whether there needs to be intervention
                        if (!wmcScheduleEntryTimes.TryGetValue(mxfStartTime, out var wmcScheduleEntry))
                        {
                            try
                            {
                                if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign}: Adding schedule entry from {mxfStartTime.ToLocalTime()} to {mxfStartTime.ToLocalTime() + TimeSpan.FromSeconds(mxfScheduleEntry.Duration)} for program [{mxfProgram.Uid.Replace("!Program!", "")} - [{mxfProgram.Title}] - [{mxfProgram.EpisodeTitle}]].");
                                var addProgram = WmcStore.WmcObjectStore.UIds[mxfProgram.Uid].Target as Microsoft.MediaCenter.Guide.Program;
                                var addScheduleEntry = new ScheduleEntry(addProgram, wmcService, mxfStartTime, TimeSpan.FromSeconds(mxfScheduleEntry.Duration), mxfScheduleEntry.Part, mxfScheduleEntry.Parts);
                                UpdateScheduleEntryTags(addScheduleEntry, mxfScheduleEntry);
                                WmcStore.WmcObjectStore.Add(addScheduleEntry);
                                ++correctedCount;
                            }
                            catch (Exception e)
                            {
                                Logger.WriteWarning($"Service {mxfService.CallSign}: Failed to add schedule entry from {mxfStartTime.ToLocalTime()} to {mxfStartTime.ToLocalTime() + TimeSpan.FromSeconds(mxfScheduleEntry.Duration)} for program [{mxfProgram.Uid.Replace("!Program!", "")} - [{mxfProgram.Title}] - [{mxfProgram.EpisodeTitle}]].\nmessage {e.Message}\n{e.StackTrace}");
                                break;
                            }
                        }
                        else
                        {
                            if (Math.Abs(wmcScheduleEntry.Duration.TotalSeconds - mxfScheduleEntry.Duration) > 1.0)
                            {
                                // change the start time of the next wmc schedule entry if possible/needed
                                if (!wmcScheduleEntryTimes.ContainsKey(mxfScheduleEntry.StartTime + TimeSpan.FromSeconds(mxfScheduleEntry.Duration)) &&
                                    wmcScheduleEntryTimes.TryGetValue(wmcScheduleEntry.EndTime, out var scheduleEntry))
                                {
                                    try
                                    {
                                        scheduleEntry.Refresh();
                                        wmcScheduleEntryTimes.Remove(scheduleEntry.StartTime);
                                        scheduleEntry.StartTime = mxfScheduleEntry.StartTime + TimeSpan.FromSeconds(mxfScheduleEntry.Duration);
                                        scheduleEntry.Update();
                                        wmcScheduleEntryTimes.Add(scheduleEntry.StartTime, scheduleEntry);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }

                                // correct the end time of current wmc schedule entry
                                try
                                {
                                    if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}: Changing end time of [{wmcScheduleEntry.Program.GetUIdValue().Replace("!Program!", "")} - [{wmcScheduleEntry.Program.Title}]-[{wmcScheduleEntry.Program.EpisodeTitle}]] from {wmcScheduleEntry.EndTime.ToLocalTime()} to {mxfStartTime.ToLocalTime() + TimeSpan.FromSeconds(mxfScheduleEntry.Duration)}");
                                    wmcScheduleEntry.Refresh();
                                    wmcScheduleEntry.EndTime = mxfStartTime + TimeSpan.FromSeconds(mxfScheduleEntry.Duration);
                                    wmcScheduleEntry.Update();
                                }
                                catch (Exception e)
                                {
                                    Logger.WriteWarning($"Service {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}: Failed to change end time of [{wmcScheduleEntry.Program.GetUIdValue().Replace("!Program!", "")} - [{wmcScheduleEntry.Program.Title}]-[{wmcScheduleEntry.Program.EpisodeTitle}]] from {wmcScheduleEntry.EndTime.ToLocalTime()} to {mxfStartTime.ToLocalTime() + TimeSpan.FromSeconds(mxfScheduleEntry.Duration)}\nmessage {e.Message}\n{e.StackTrace}");
                                    break;
                                }
                            }

                            try
                            {
                                if (wmcScheduleEntry.Program.GetUIdValue() != mxfProgram.Uid)
                                {
                                    if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}: Replacing [{wmcScheduleEntry.Program.GetUIdValue().Replace("!Program!", "")} - [{wmcScheduleEntry.Program.Title}]-[{wmcScheduleEntry.Program.EpisodeTitle}]] with [{mxfProgram.Uid.Replace("!Program!", "")} - [{mxfProgram.Title}]-[{mxfProgram.EpisodeTitle}]]");
                                    wmcScheduleEntry.Refresh();
                                    wmcScheduleEntry.Program = WmcStore.WmcObjectStore.UIds[mxfProgram.Uid].Target as Microsoft.MediaCenter.Guide.Program;
                                    UpdateScheduleEntryTags(wmcScheduleEntry, mxfScheduleEntry);
                                    wmcScheduleEntry.EndTime = mxfStartTime + TimeSpan.FromSeconds(mxfScheduleEntry.Duration);
                                    wmcScheduleEntry.Update();
                                    ++correctedCount;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.WriteWarning($"Service {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}: Failed to replace [{wmcScheduleEntry.Program.GetUIdValue().Replace("!Program!", "")} - [{wmcScheduleEntry.Program.Title}]-[{wmcScheduleEntry.Program.EpisodeTitle}]] with [{mxfProgram.Uid.Replace("!Program!", "")} - [{mxfProgram.Title}]-[{mxfProgram.EpisodeTitle}]]\nmessage {e.Message}\n{e.StackTrace}");
                                break;
                            }

                            wmcScheduleEntryTimes.Remove(mxfStartTime);
                        }

                        mxfLastStartTime = mxfStartTime;
                        mxfStartTime += TimeSpan.FromSeconds(mxfScheduleEntry.Duration);
                    }

                    // remove orphaned wmcScheduleEntries
                    foreach (var keyValuePair in wmcScheduleEntryTimes)
                    {
                        try
                        {
                            if (keyValuePair.Value.StartTime <= DateTime.UtcNow || keyValuePair.Value.StartTime >= mxfStartTime) continue;
                            if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign} at {keyValuePair.Value.StartTime.ToLocalTime()}: Removing [{keyValuePair.Value.Program.GetUIdValue().Replace("!Program!", "")} - [{keyValuePair.Value.Program.Title}]-[{keyValuePair.Value.Program.EpisodeTitle}]] due to being overlapped by another schedule entry.");
                            keyValuePair.Value.Refresh();
                            keyValuePair.Value.Service = null;
                            keyValuePair.Value.Program = null;
                            keyValuePair.Value.Unlock();
                            keyValuePair.Value.Update();
                        }
                        catch (Exception e)
                        {
                            if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign} at {keyValuePair.Value.StartTime.ToLocalTime()}: Failed to remove [{keyValuePair.Value.Program.GetUIdValue().Replace("!Program!", "")} - [{keyValuePair.Value.Program.Title}]-[{keyValuePair.Value.Program.EpisodeTitle}]] due to being overlapped by another schedule entry.\nmessage {e.Message}\n{e.StackTrace}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteInformation($"Exception caught for {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}, message {e.Message}\n{e.StackTrace}");
                }
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                //GC.Collect();
            }
            Logger.WriteInformation($"Checked {entriesChecked} entries and corrected {correctedCount} of them.");
            Logger.WriteMessage("Exiting VerifyLoad()");
        }

        private static void UpdateScheduleEntryTags(ScheduleEntry wmc, MxfScheduleEntry mxf)
        {
            wmc.AudioFormat = (AudioFormat)mxf.AudioFormat;
            wmc.Is3D = mxf.Is3D;
            wmc.IsBlackout = mxf.IsBlackout;
            wmc.IsCC = mxf.IsCc;
            wmc.IsClassroom = mxf.IsClassroom;
            wmc.IsDelay = mxf.IsDelay;
            wmc.IsDvs = mxf.IsDvs;
            wmc.IsEnhanced = mxf.IsEnhanced;
            wmc.IsFinale = mxf.IsFinale;
            wmc.IsHdtv = mxf.IsHdtv;
            wmc.IsHdtvSimulCast = mxf.IsHdtvSimulCast;
            wmc.IsInProgress = mxf.IsInProgress;
            wmc.IsLetterbox = mxf.IsLetterbox;
            wmc.IsLive = mxf.IsLive;
            wmc.IsLiveSports = mxf.IsLiveSports;
            wmc.IsPremiere = mxf.IsPremiere;
            //wmc.IsRepeatFlag = mxf.IsRepeat;
            wmc.IsSap = mxf.IsSap;
            wmc.IsSubtitled = mxf.IsSubtitled;
            wmc.IsTape = mxf.IsTape;
            wmc.Part = mxf.Part;
            wmc.Parts = mxf.Parts;
            wmc.TVRating = (TVRating)mxf.TvRating;
        }
    }
}