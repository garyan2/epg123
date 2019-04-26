using System.Collections;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;

/// <summary>
/// This class is an implementation of the 'IComparer' interface.
/// </summary>
public class LineupListViewSorter : System.Collections.Generic.IComparer<Channel>
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
    /// Class constructor.  Initializes various elements
    /// </summary>
    public LineupListViewSorter()
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
    /// <param name="channelX">First object to be compared</param>
    /// <param name="channelY">Second object to be compared</param>
    /// <returns>The result of the comparison. "0" if equal, negative if 'channelX' is less than 'channelY' and positive if 'channelX' is greater than 'channelY'</returns>
    public int Compare(Channel channelX, Channel channelY)
    {
        int compareResult;

        // compare items based on selected column
        switch (ColumnToSort)
        {
            case 0:
            case 3:
            case 4:
                compareResult = ObjectCompare.Compare(channelX.CallSign, channelY.CallSign);
                break;
            case 1:
                compareResult = ObjectCompare.Compare(extendChannelSubchannel(channelX.ChannelNumber.ToString()), extendChannelSubchannel(channelY.ChannelNumber.ToString()));
                break;
            case 2:
                compareResult = ObjectCompare.Compare(channelX.Service.Name, channelY.Service.Name);
                break;
            default:
                compareResult = 0;
                break;
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

    private string extendChannelSubchannel(string text)
    {
        string[] split = text.Split('.');
        switch (split.Length)
        {
            case 1:
                return (split[0].PadLeft(6, '0') + ".000000");
            case 2:
            default:
                return (split[0].PadLeft(6, '0') + "." + split[1].PadLeft(6, '0'));
        }
    }

    /// <summary>
    /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
    /// </summary>
    public int SortColumn
    {
        get
        {
            return ColumnToSort;
        }
        set
        {
            ColumnToSort = value;
        }
    }

    /// <summary>
    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
    /// </summary>
    public SortOrder Order
    {
        get
        {
            return OrderOfSort;
        }
        set
        {
            OrderOfSort = value;
        }
    }
}