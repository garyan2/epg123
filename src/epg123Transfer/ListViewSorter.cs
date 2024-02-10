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
    public int SortColumn { get; set; }

    /// <summary>
    /// Specifies the order in which to sort (i.e. 'Ascending').
    /// </summary>
    public SortOrder Order { get; set; }

    /// <summary>
    /// Specifies whether selected items should be grouped together
    /// </summary>
    public bool GroupOrder { get; set; }

    /// <summary>
    /// Case insensitive comparer object
    /// </summary>
    private readonly CaseInsensitiveComparer _objectCompare;

    /// <summary>
    /// Class constructor.  Initializes various elements
    /// </summary>
    public ListViewColumnSorter()
    {
        // Initialize the column to '0'
        SortColumn = 0;

        // Initialize the sort order to 'none'
        Order = SortOrder.Ascending;

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
        var stringX = ((ListViewItem)x)?.SubItems[SortColumn].Text.Replace("-", "");
        var stringY = ((ListViewItem)y)?.SubItems[SortColumn].Text.Replace("-", "");

        // Compare the two items either by number or text
        if (stringY != null && stringX != null && stringX.Replace(".", "").All(char.IsDigit) && stringY.Replace(".", "").All(char.IsDigit))
        {
            var doubleX = double.Parse(ExtendChannelSubchannel(stringX));
            var doubleY = double.Parse(ExtendChannelSubchannel(stringY));

            if (GroupOrder)
            {
                if (((ListViewItem)x)?.Checked ?? false) doubleX -= 1000000;
                else doubleX += 1000000;

                if (((ListViewItem)y)?.Checked ?? false) doubleY -= 1000000;
                else doubleY += 1000000;
            }
            compareResult = _objectCompare.Compare(doubleX, doubleY);
        }
        else
        {
            if (GroupOrder)
            {
                if (((ListViewItem)x)?.Checked ?? false) stringX = $"00000{stringX}";
                else stringX = $"zzzzz{stringX}";

                if (((ListViewItem)y)?.Checked ?? false) stringY = $"00000{stringY}";
                else stringY = $"zzzzz{stringY}";
            }
            compareResult = _objectCompare.Compare(stringX, stringY);
        }

        switch (Order)
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

    public void ClickHeader(int column)
    {
        if (column != SortColumn)
        {
            Order = SortOrder.None;
        }

        switch (Order)
        {
            case SortOrder.None:
                SortColumn = column;
                GroupOrder = false;
                Order = SortOrder.Ascending;
                break;
            case SortOrder.Ascending:
                if (GroupOrder)
                {
                    GroupOrder = false;
                    Order = SortOrder.Ascending;
                }
                else
                {
                    Order = SortOrder.Descending;
                }
                break;
            case SortOrder.Descending:
                Order = SortOrder.Ascending;
                GroupOrder = true;
                break;
        }
    }
}
