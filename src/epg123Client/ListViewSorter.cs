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
    private int _columnToSort;

    /// <summary>
    /// Specifies the order in which to sort (i.e. 'Ascending').
    /// </summary>
    private SortOrder _orderOfSort;

    /// <summary>
    /// Case insensitive comparer object
    /// </summary>
    private readonly CaseInsensitiveComparer _objectCompare;

    /// <summary>
    /// Value used to show neither compare values are null
    /// </summary>
    private const int NoNulls = 12345;

    /// <summary>
    /// Class constructor.  Initializes various elements
    /// </summary>
    public ListViewColumnSorter()
    {
        // Initialize the column to '0'
        _columnToSort = 0;

        // Initialize the sort order to 'none'
        _orderOfSort = SortOrder.Ascending;

        // Initialize the CaseInsensitiveComparer object
        _objectCompare = new CaseInsensitiveComparer();
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
        switch (_columnToSort)
        {
            case 0:
                if ((compareResult = InitialNullResult(x, y)) == NoNulls) compareResult = _objectCompare.Compare(x?.Text, y?.Text);
                if (compareResult == 0) compareResult = _objectCompare.Compare(ExtendChannelSubchannel(x?.SubItems[1].Text), ExtendChannelSubchannel(y?.SubItems[1].Text));
                break;
            case 1:
                if ((compareResult = InitialNullResult(x, y)) == NoNulls) compareResult = _objectCompare.Compare(ExtendChannelSubchannel(x?.SubItems[_columnToSort].Text), ExtendChannelSubchannel(y?.SubItems[_columnToSort].Text));
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                if ((compareResult = InitialNullResult(x, y)) == NoNulls) compareResult = _objectCompare.Compare(x?.SubItems[_columnToSort].Text, y?.SubItems[_columnToSort].Text);
                if (compareResult == 0) compareResult = _objectCompare.Compare(ExtendChannelSubchannel(x?.SubItems[1].Text), ExtendChannelSubchannel(y?.SubItems[1].Text));
                break;
            case 6:
                var xDateTime = string.IsNullOrEmpty(x?.SubItems[_columnToSort].Text) ? DateTime.MinValue : DateTime.Parse(x.SubItems[_columnToSort].Text);
                var yDateTime = string.IsNullOrEmpty(y?.SubItems[_columnToSort].Text) ? DateTime.MinValue : DateTime.Parse(y.SubItems[_columnToSort].Text);
                if ((compareResult = InitialNullResult(x, y)) == NoNulls) compareResult = _objectCompare.Compare(xDateTime, yDateTime);
                if (compareResult == 0) compareResult = _objectCompare.Compare(ExtendChannelSubchannel(x?.SubItems[1].Text), ExtendChannelSubchannel(y?.SubItems[1].Text));
                break;
            default:
                compareResult = 0;
                break;
        }

        switch (_orderOfSort)
        {
            // Calculate correct return value based on object comparison
            case SortOrder.Ascending:
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            case SortOrder.Descending:
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            default:
                // Return '0' to indicate they are equal
                return 0;
        }
    }

    private int InitialNullResult(ListViewItem x, ListViewItem y)
    {
        if (string.IsNullOrEmpty(x.SubItems[_columnToSort].Text) && !string.IsNullOrEmpty(y.SubItems[_columnToSort].Text)) return 1;
        if (string.IsNullOrEmpty(x.SubItems[_columnToSort].Text) && string.IsNullOrEmpty(y.SubItems[_columnToSort].Text)) return 0;
        if (!string.IsNullOrEmpty(x.SubItems[_columnToSort].Text) && string.IsNullOrEmpty(y.SubItems[_columnToSort].Text)) return -1;
        return NoNulls;
    }

    /// <summary>
    /// Expands the channel subchannel number for sorting (pads left with zeros)
    /// </summary>
    /// <param name="text">channel</param>
    /// <returns></returns>
    private static string ExtendChannelSubchannel(string text)
    {
        var split = text.Split('.');
        switch (split.Length)
        {
            case 1:
                return (split[0].PadLeft(6, '0') + ".000000");
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
        set => _columnToSort = value;
        get => _columnToSort;
    }

    /// <summary>
    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
    /// </summary>
    public SortOrder Order
    {
        set => _orderOfSort = value;
        get => _orderOfSort;
    }
}