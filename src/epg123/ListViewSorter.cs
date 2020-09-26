using epg123;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

/// <summary>
/// This class is an implementation of the 'IComparer' interface.
/// </summary>
public class ListViewColumnSorter : IComparer
{
    /// <summary>
    /// Specifies the column to be sorted
    /// </summary>
    private int ColumnToSort;
    /// <summary>
    /// Specifies the order in which to sort (i.e. 'Ascending').
    /// </summary>
    private SortOrder OrderOfSort;
    /// <summary>
    /// Case insensitive comparer object
    /// </summary>
    private CaseInsensitiveComparer ObjectCompare;
    /// <summary>
    /// Specifies the suspend state
    /// </summary>
    private bool SuspendSort;

    /// <summary>
    /// Class constructor.  Initializes various elements
    /// </summary>
    public ListViewColumnSorter()
    {
        // Initialize the column to '0'
        ColumnToSort = 0;

        // Initialize the sort order to 'none'
        OrderOfSort = SortOrder.Ascending;

        // Initialize the CaseInsensitiveComparer object
        ObjectCompare = new CaseInsensitiveComparer();
    }

    /// <summary>
    /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
    /// </summary>
    /// <param name="x">First object to be compared</param>
    /// <param name="y">Second object to be compared</param>
    /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
    public int Compare(object x, object y)
    {
        int compareResult;

        // if sorting is suspended, return 0 on the compares
        if (SuspendSort) return 0;

        // Cast the objects to be compared to ListViewItem objects
        string stringX = ((ListViewItem)x).SubItems[ColumnToSort].Text.Replace("-", "");
        string stringY = ((ListViewItem)y).SubItems[ColumnToSort].Text.Replace("-", "");

        // Compare the two items either by number or text
        if (stringX.Replace(".", "").All(char.IsDigit) && stringY.Replace(".", "").All(char.IsDigit))
        {
            compareResult = ObjectCompare.Compare(extendChannelSubchannel(stringX), extendChannelSubchannel(stringY));
        }
        else
        {
            //if (ColumnToSort == 0)
            //{
            //    stringX = ((SdChannelDownload)((ListViewItem)x).Tag).CallSign;
            //    stringY = ((SdChannelDownload)((ListViewItem)y).Tag).CallSign;
            //}
            compareResult = ObjectCompare.Compare(stringX, stringY);
        }

        // Calculate correct return value based on object comparison
        if (OrderOfSort == SortOrder.Ascending)
        {
            // Ascending sort is selected, return normal result of compare operation
            return compareResult;
        }
        else if (OrderOfSort == SortOrder.Descending)
        {
            // Descending sort is selected, return negative result of compare operation
            return (-compareResult);
        }
        else
        {
            // Return '0' to indicate they are equal
            return 0;
        }
    }

    /// <summary>
    /// Expands the channel subchannel number for sorting (pads left with zeros)
    /// </summary>
    /// <param name="text">channel</param>
    /// <returns></returns>
    private string extendChannelSubchannel(string text)
    {
        string[] split = text.Split('.');
        switch (split.Length)
        {
            case 1:
                return (split[0].PadLeft(6, '0') + ".000000");
            case 2:
            default:
                if (split[0] == "-1") split[0] = "0";
                return (split[0].PadLeft(6, '0') + "." + split[1].PadLeft(6, '0'));
        }
    }

    /// <summary>
    /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
    /// </summary>
    public int SortColumn
    {
        set
        {
            ColumnToSort = value;
        }
        get
        {
            return ColumnToSort;
        }
    }

    /// <summary>
    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
    /// </summary>
    public SortOrder Order
    {
        set
        {
            OrderOfSort = value;
        }
        get
        {
            return OrderOfSort;
        }
    }

    /// <summary>
    /// Gets or sets the flag to suspend sorting (all compares return '0' for equals)
    /// </summary>
    public bool Suspend
    {
        set
        {
            SuspendSort = value;
        }
        get
        {
            return SuspendSort;
        }
    }
}
