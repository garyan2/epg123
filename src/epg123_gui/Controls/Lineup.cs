using GaRyan2.SchedulesDirectAPI;
using System.Collections.Generic;

namespace epg123_gui
{
    internal class MemberLineup
    {
        public override string ToString()
        {
            return $"{(Lineup.IsDeleted ? "[Deleted] " : null)}{Lineup.Name} ({Lineup.Location})";
        }

        public MemberLineup(SubscribedLineup lineup)
        {
            Lineup = lineup;
        }

        public List<LineupChannel> Channels { get; set; }

        public bool DiscardNumbers { get; set; }

        public bool Include { get; set; }

        public SubscribedLineup Lineup { get; set; }
    }
}