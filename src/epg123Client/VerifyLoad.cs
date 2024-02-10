using GaRyan2.MxfXml;
using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Pvr;
using System;
using System.Collections.Generic;
using System.Linq;

namespace epg123Client
{
    public class VerifyLoad
    {
        public VerifyLoad(string mxfFile, bool verbose = false)
        {
            if (mxfFile.StartsWith("http")) mxfFile = Helper.Epg123MxfPath;

            MXF mxf;
            Logger.WriteMessage("Entering VerifyLoad()");
            Helper.SendPipeMessage("Importing|Verifying MXF Load...");

            mxf = Helper.ReadXmlFile(mxfFile, typeof(MXF));
            if (!(mxf.Providers[0]?.Name ?? string.Empty).Equals("EPG123") && !(mxf.Providers[0]?.Name ?? string.Empty).Equals("HDHR2MXF"))
            {
                Logger.WriteInformation("The imported MXF file is not a guide listings file created by EPG123. Skipping schedule entry verifications.");
                Logger.WriteInformation("Exiting VerifyLoad()");
                return;
            }

            var entriesChecked = 0;
            var correctedCount = 0;
            foreach (var mxfService in mxf.With.Services)
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
                var mxfScheduleEntries = mxf.With.ScheduleEntries.FirstOrDefault(scheduleEntries =>
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
                foreach (var entry in mxfScheduleEntries.ScheduleEntry.Where(entry => entry.StartTime != DateTime.MinValue))
                {
                    if (mxfStartTime == DateTime.MinValue) mxfStartTime = entry.StartTime;
                    ++discontinuities;
                    if (discontinuities != 1) continue;
                    Logger.WriteInformation($"Service {mxfService.Uid}: {mxfService.CallSign} has a time discontinuity at {entry.StartTime.ToLocalTime()}. Skipping verification of this station's schedule entries.");
                    break;
                }
                if (discontinuities > 0)
                {
                    continue;
                }
                if (mxfStartTime - TimeSpan.FromHours(4.0) > DateTime.UtcNow)
                {
                    Logger.WriteInformation($"Service {mxfService.Uid}: {mxfService.CallSign}: first mxf schedule entry to verify is in the future at {mxfStartTime.ToLocalTime()}.");
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
                            RemoveScheduleEntry(wmcScheduleEntry);
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
                        var mxfProgram = mxf.With.Programs[mxfScheduleEntry.Program - 1];
                        var mxfEndTime = mxfStartTime + TimeSpan.FromSeconds(mxfScheduleEntry.Duration);

                        // verify a schedule entry exists matching the MXF file and determine whether there needs to be intervention
                        var addEntry = false;
                        var replaceEntry = false;
                        if (!wmcScheduleEntryTimes.TryGetValue(mxfStartTime, out var wmcScheduleEntry)) addEntry = true;
                        else if (wmcScheduleEntry.Program.GetUIdValue() != mxfProgram.Uid)
                        {
                            if (!IsSameSeries(wmcScheduleEntry.Program, mxfProgram))
                            {
                                if (wmcScheduleEntry.EndTime == mxfEndTime && wmcScheduleEntry.ProgramContent == null) replaceEntry = true;
                                else addEntry = true;
                            }
                            else if (wmcScheduleEntry.ProgramContent == null || wmcScheduleEntry.Program.IsGeneric) replaceEntry = true;
                            else if (wmcScheduleEntry.Program.IsMovie && wmcScheduleEntry.Program.Description.Equals(mxfProgram.Description)) replaceEntry = true;
                            else addEntry = true;
                        }

                        if (addEntry)
                        {
                            try
                            {
                                if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign}: Adding schedule entry from {mxfStartTime.ToLocalTime()} to {mxfEndTime.ToLocalTime()} for program [{mxfProgram.Uid.Substring(9)} - [{mxfProgram.Title}] - [{mxfProgram.EpisodeTitle}]].");
                                var addProgram = WmcStore.WmcObjectStore.UIds[mxfProgram.Uid].Target as Microsoft.MediaCenter.Guide.Program;
                                var addScheduleEntry = new ScheduleEntry(addProgram, wmcService, mxfStartTime, TimeSpan.FromSeconds(mxfScheduleEntry.Duration), mxfScheduleEntry.Part, mxfScheduleEntry.Parts);
                                UpdateScheduleEntryTags(addScheduleEntry, mxfScheduleEntry);
                                WmcStore.WmcObjectStore.Add(addScheduleEntry);
                                ++correctedCount;
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteWarning($"Service {mxfService.CallSign}: Failed to add schedule entry from {mxfStartTime.ToLocalTime()} to {mxfEndTime.ToLocalTime()} for program [{mxfProgram.Uid.Substring(9)} - [{mxfProgram.Title}] - [{mxfProgram.EpisodeTitle}]].\nException:{Helper.ReportExceptionMessages(ex)}");
                                break;
                            }
                        }

                        if (replaceEntry)
                        {
                            if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign}: Replacing schedule entry program on {mxfStartTime.ToLocalTime()} from [{wmcScheduleEntry.Program.GetUIdValue().Substring(9)} - [{wmcScheduleEntry.Program.Title}]-[{wmcScheduleEntry.Program.EpisodeTitle}]] to [{mxfProgram.Uid.Substring(9)} - [{mxfProgram.Title}]-[{mxfProgram.EpisodeTitle}]]");
                            UpdateScheduleEntryTags(wmcScheduleEntry, mxfScheduleEntry);
                            wmcScheduleEntry.Update(delegate
                            {
                                wmcScheduleEntry.Program = WmcStore.WmcObjectStore.UIds[mxfProgram.Uid].Target as Microsoft.MediaCenter.Guide.Program;
                                wmcScheduleEntry.EndTime = mxfEndTime;
                            });
                            ++correctedCount;

                            if (wmcScheduleEntry.ProgramContent != null)
                            {
                                UpdateOneTimeRequest(wmcScheduleEntry);
                            }
                        }

                        if (!addEntry && Math.Abs(wmcScheduleEntry.Duration.TotalSeconds - mxfScheduleEntry.Duration) > 1.0)
                        {
                            // change the start time of the next wmc schedule entry if possible/needed
                            if (!wmcScheduleEntryTimes.ContainsKey(mxfEndTime) &&
                                wmcScheduleEntryTimes.TryGetValue(wmcScheduleEntry.EndTime, out var scheduleEntry) &&
                                scheduleEntry.EndTime > mxfEndTime)
                            {
                                try
                                {
                                    wmcScheduleEntryTimes.Remove(scheduleEntry.StartTime);
                                    scheduleEntry.Update(delegate
                                    {
                                        scheduleEntry.StartTime = mxfEndTime;
                                    });
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
                                if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}: Changing end time of [{wmcScheduleEntry.Program.GetUIdValue().Substring(9)} - [{wmcScheduleEntry.Program.Title}]-[{wmcScheduleEntry.Program.EpisodeTitle}]] from {wmcScheduleEntry.EndTime.ToLocalTime()} to {mxfEndTime.ToLocalTime()}");
                                wmcScheduleEntry.Update(delegate
                                {
                                    wmcScheduleEntry.EndTime = mxfEndTime;
                                });
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteWarning($"Service {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}: Failed to change end time of [{wmcScheduleEntry.Program.GetUIdValue().Substring(9)} - [{wmcScheduleEntry.Program.Title}]-[{wmcScheduleEntry.Program.EpisodeTitle}]] from {wmcScheduleEntry.EndTime.ToLocalTime()} to {mxfEndTime.ToLocalTime()}\nException:{Helper.ReportExceptionMessages(ex)}");
                                break;
                            }
                        }

                        if (!addEntry) wmcScheduleEntryTimes.Remove(mxfStartTime);
                        mxfLastStartTime = mxfStartTime;
                        mxfStartTime = mxfEndTime;
                    }

                    // remove orphaned wmcScheduleEntries
                    foreach (var orphans in wmcScheduleEntryTimes)
                    {
                        try
                        {
                            if (orphans.Value.StartTime <= DateTime.UtcNow || orphans.Value.StartTime >= mxfStartTime) continue;
                            if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign}: Removing schedule entry on {orphans.Value.StartTime.ToLocalTime()} for [{orphans.Value.Program.GetUIdValue().Replace("!Program!", "")} - [{orphans.Value.Program.Title}]-[{orphans.Value.Program.EpisodeTitle}]] due to being replaced/overlapped by another schedule entry.");
                            RemoveScheduleEntry(orphans.Value);
                        }
                        catch (Exception ex)
                        {
                            if (verbose) Logger.WriteInformation($"Service {mxfService.CallSign} at {orphans.Value.StartTime.ToLocalTime()}: Failed to remove [{orphans.Value.Program.GetUIdValue().Replace("!Program!", "")} - [{orphans.Value.Program.Title}]-[{orphans.Value.Program.EpisodeTitle}]] due to being overlapped by another schedule entry.\nException:{Helper.ReportExceptionMessages(ex)}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteInformation($"Exception caught for {mxfService.CallSign} at {mxfStartTime.ToLocalTime()}. Message: {e.Message}");
                }
            }
            Logger.WriteInformation($"Checked {entriesChecked} entries and corrected {correctedCount} of them.");
            Logger.WriteMessage("Exiting VerifyLoad()");
        }

        private static void UpdateOneTimeRequest(ScheduleEntry wmc)
        {
            using (var allRecordings = new Recordings(WmcStore.WmcObjectStore))
            {
                var programContentKey = new ProgramContentKey(wmc.ProgramContent);
                var recordings = (Recordings)allRecordings.WhereKeyIsInRange(programContentKey, programContentKey);
                if (!recordings.Any()) return;

                var enumerator = recordings.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var recordingToUpdate = (Recording)enumerator.Current;
                    if (!(recordingToUpdate?.Request is OneTimeRequest)) continue;
                    recordingToUpdate.Update(delegate
                    {
                        recordingToUpdate.Program = wmc.Program;
                    });
                    Logger.WriteWarning($"OneTimeRequest recording on {wmc.Service.CallSign} at {wmc.StartTime.ToLocalTime()} was updated to record [{wmc.Program.Title}]-[{wmc.Program.EpisodeTitle}].");
                }
            }
        }

        private static void RemoveScheduleEntry(ScheduleEntry wmc)
        {
            if (wmc.ProgramContent == null) goto RemoveEntry;
            using (var allRecordings = new Recordings(WmcStore.WmcObjectStore))
            {
                var programContentKey = new ProgramContentKey(wmc.ProgramContent);
                var recordings = (Recordings)allRecordings.WhereKeyIsInRange(programContentKey, programContentKey);
                if (recordings.Any())
                {
                    var enumerator = recordings.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var recordingAffected = (Recording)enumerator.Current;
                        if (recordingAffected?.Request is OneTimeRequest)
                        {
                            Logger.WriteWarning($"OneTimeRequest recording on {wmc.Service.CallSign} at {wmc.StartTime.ToLocalTime()} for [{wmc.Program.Title}]-[{wmc.Program.EpisodeTitle}] may have been rescheduled or is no longer valid. Check your guide.");
                        }
                    }
                }
            }

        RemoveEntry:
            wmc.Unlock();
            wmc.Update(delegate
            {
                wmc.Service = null;
                wmc.Program = null;
            });
        }

        private static bool IsSameSeries(Microsoft.MediaCenter.Guide.Program wmc, MxfProgram mxf) // or close enough to same series
        {
            return wmc.GetUIdValue().Substring(11, 8).Equals(mxf.Uid.Substring(11, 8)) || wmc.Title.Equals(mxf.Title);
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
            wmc.IsOnlyWmis = true;
        }
    }
}