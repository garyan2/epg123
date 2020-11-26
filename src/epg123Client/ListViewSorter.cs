using System;
using System.Collections;
using System.Windows.Forms;

/// <summary>
/// This class is an implementation of the 'IComparer' interface.
/// </summary>
public class ListViewColumnSorter : System.Collections.Generic.IComparer<ListViewItem>
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
    /// Value used to show neither compare values are null
    /// </summary>
    private int NoNulls = 12345;

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
    public int Compare(ListViewItem x, ListViewItem y)
    {
        int compareResult;

        // compare items based on selected column
        switch (ColumnToSort)
        {
            case 0:
                if ((compareResult = initialNullResult(x, y)) == NoNulls) compareResult = ObjectCompare.Compare(x.Text, y.Text);
                if (compareResult == 0) compareResult = ObjectCompare.Compare(extendChannelSubchannel(x.SubItems[1].Text), extendChannelSubchannel(y.SubItems[1].Text));
                break;
            case 1:
                if ((compareResult = initialNullResult(x, y)) == NoNulls) compareResult = ObjectCompare.Compare(extendChannelSubchannel(x.SubItems[ColumnToSort].Text), extendChannelSubchannel(y.SubItems[ColumnToSort].Text));
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                if ((compareResult = initialNullResult(x, y)) == NoNulls) compareResult = ObjectCompare.Compare(x.SubItems[ColumnToSort].Text, y.SubItems[ColumnToSort].Text);
                if (compareResult == 0) compareResult = ObjectCompare.Compare(extendChannelSubchannel(x.SubItems[1].Text), extendChannelSubchannel(y.SubItems[1].Text));
                break;
            case 6:
                DateTime xDateTime = string.IsNullOrEmpty(x.SubItems[ColumnToSort].Text) ? DateTime.MinValue : DateTime.Parse(x.SubItems[ColumnToSort].Text);
                DateTime yDateTime = string.IsNullOrEmpty(y.SubItems[ColumnToSort].Text) ? DateTime.MinValue : DateTime.Parse(y.SubItems[ColumnToSort].Text);
                if ((compareResult = initialNullResult(x, y)) == NoNulls) compareResult = ObjectCompare.Compare(xDateTime, yDateTime);
                if (compareResult == 0) compareResult = ObjectCompare.Compare(extendChannelSubchannel(x.SubItems[1].Text), extendChannelSubchannel(y.SubItems[1].Text));
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

    private int initialNullResult(ListViewItem x, ListViewItem y)
    {
        if (string.IsNullOrEmpty(x.SubItems[ColumnToSort].Text) && !string.IsNullOrEmpty(y.SubItems[ColumnToSort].Text)) return 1;
        else if (string.IsNullOrEmpty(x.SubItems[ColumnToSort].Text) && string.IsNullOrEmpty(y.SubItems[ColumnToSort].Text)) return 0;
        else if (!string.IsNullOrEmpty(x.SubItems[ColumnToSort].Text) && string.IsNullOrEmpty(y.SubItems[ColumnToSort].Text)) return -1;
        return NoNulls;
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
