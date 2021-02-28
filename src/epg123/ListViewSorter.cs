using System;
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
    private int _columnToSort;
    /// <summary>
    /// Specifies the order in which to sort (i.e. 'Ascending').
    /// </summary>
    private SortOrder _orderOfSort;
    /// <summary>
    /// Case insensitive comparer object
    /// </summary>
    private readonly CaseInsensitiveComparer _objectCompare;

    private DateTime _lastSort;

    private int _clickCount;

    private int _lastColumn;

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
    public int Compare(object x, object y)
    {
        int compareResult;

        // Cast the objects to be compared to ListViewItem objects
        var stringX = ((ListViewItem)x)?.SubItems[_columnToSort].Text.Replace("-", "");
        var stringY = ((ListViewItem)y)?.SubItems[_columnToSort].Text.Replace("-", "");

        // Compare the two items either by number or text
        if (stringY != null && stringX != null && stringX.Replace(".", "").All(char.IsDigit) && stringY.Replace(".", "").All(char.IsDigit))
        {
            var doubleX = double.Parse(ExtendChannelSubchannel(stringX));
            var doubleY = double.Parse(ExtendChannelSubchannel(stringY));

            if (_clickCount >= 2)
            {
                _orderOfSort = SortOrder.Ascending;
                if (((ListViewItem)x)?.Checked ?? false) doubleX -= 1000000;
                else doubleX += 1000000;

                if (((ListViewItem)y)?.Checked ?? false) doubleY -= 1000000;
                else doubleY += 1000000;
            }
            compareResult = _objectCompare.Compare(doubleX, doubleY);
        }
        else
        {
            if (_clickCount >= 2)
            {
                _orderOfSort = SortOrder.Ascending;
                if (((ListViewItem) x)?.Checked ?? false) stringX = $"00000{stringX}";
                else stringX = $"zzzzz{stringX}";

                if (((ListViewItem)y)?.Checked ?? false) stringY = $"00000{stringY}";
                else stringY = $"zzzzz{stringY}";
            }
            compareResult = _objectCompare.Compare(stringX, stringY);
        }

        _lastSort = DateTime.Now;

        switch (_orderOfSort)
        {
            // Calculate correct return value based on object comparison
            case SortOrder.Ascending:
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            case SortOrder.Descending:
                // Descending sort is selected, return negative result of compare operation
                return -compareResult;
            default:
                // Return '0' to indicate they are equal
                return 0;
        }
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

    public void ClickHeader()
    {
        if (DateTime.Now - _lastSort > TimeSpan.FromMilliseconds(1000)) _clickCount = 0;
        else if (_lastColumn != _columnToSort) _clickCount = 1;
        else ++_clickCount;
        _lastColumn = _columnToSort;
    }
}
