using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace epg123
{
    public partial class frmCustomLineup : Form
    {
        public frmCustomLineup(List<myStation> stations)
        {
            InitializeComponent();

            // assign listview sorters
            lvAvailable.ListViewItemSorter = new ListViewColumnSorter
            {
                SortColumn = 0,
                Order = SortOrder.Ascending
            };
            lvCustom.ListViewItemSorter = new ListViewColumnSorter
            {
                SortColumn = 1,
                Order = SortOrder.Ascending
            };

            // populate available station listview
            foreach (var station in stations)
            {
                lvAvailable.Items.Add(new availableStation(station));
            }
            lvAvailable.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            lvAvailable.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            lvAvailable.Sort();

            // populate custom lineup combobox
            if (!File.Exists(Helper.Epg123CustomLineupsXmlPath)) return;
            CustomLineups customLineups;
            using (var stream = new StreamReader(Helper.Epg123CustomLineupsXmlPath, Encoding.Default))
            {
                var serializer = new XmlSerializer(typeof(CustomLineups));
                TextReader reader = new StringReader(stream.ReadToEnd());
                customLineups = (CustomLineups)serializer.Deserialize(reader);
                reader.Close();
            }

            foreach (var lineup in customLineups.CustomLineup)
            {
                cbCustom.Items.Add(lineup);
            }

            if (cbCustom.Items.Count <= 0) return;
            cbCustom.Text = cbCustom.Items[0].ToString();
            cbCustom.SelectedItem = 0;
        }

        private void LvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine which column sorter this click applies to
            var lvcs = (ListViewColumnSorter)((ListView)sender).ListViewItemSorter;

            // Determine if clicked column is already the column that is being sorted
            if (e.Column == lvcs.SortColumn)
            {
                // Reverse the current sort direction for this column
                lvcs.Order = (lvcs.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvcs.SortColumn = e.Column;
                lvcs.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            ((ListView)sender).Sort();
        }

        private void frmCustomLineup_Shown(object sender, EventArgs e)
        {
            //UpdateCustomListView();
        }

        private void cbCustom_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCustomListView();
        }

        private void UpdateCustomListView()
        {
            if (cbCustom.SelectedIndex < 0) return;

            lvCustom.Items.Clear();
            lvCustom.BeginUpdate();
            var lineup = (CustomLineup)cbCustom.SelectedItem;
            foreach (var channel in lineup.Station)
            {
                lvCustom.Items.Add(new customChannel(channel));
            }
            lvCustom.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            lvCustom.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            lvCustom.EndUpdate();
        }

        private void frmCustomLineup_ResizeEnd(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
        {
            cbCustom.Width = splitContainer1.Panel2.Width - btnAddLineup.Width - btnRemoveLineup.Width - 27;
        }
    }

    public class availableStation : ListViewItem
    {
        public myStation Station { get; private set; }

        public availableStation(myStation station) : base(new string[3])
        {
            Station = station;
            SubItems[0].Text = Station.Callsign;
            SubItems[1].Text = Station.StationId;
            SubItems[2].Text = Station.Name;
        }
    }

    public class customChannel : ListViewItem
    {
        public CustomStation Station { get; private set; }

        public customChannel(CustomStation station) : base(new string[5])
        {
            Station = station;
            SubItems[0].Text = Station.Callsign;
            SubItems[1].Text = $"{Station.Number}{(Station.Subnumber == 0 ? "" : $".{Station.Subnumber}")}";
            SubItems[2].Text = Station.StationId;
            SubItems[3].Text = Station.Name;
            SubItems[4].Text = Station.MatchName;
        }
    }
}
