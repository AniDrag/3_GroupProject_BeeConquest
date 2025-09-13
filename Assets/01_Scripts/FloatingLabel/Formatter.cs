using System.Text;

public static class Formatter
{
    // Formats signed numbers with space every 3 digits and leading + or -
    public static string FormatSignedWithSpaces(long value)
    {
        // handle min value safely
        ulong abs = value >= 0 ? (ulong)value : (ulong)(-(value + 1)) + 1;
        string digits = abs.ToString();
        var sb = new StringBuilder();

        int len = digits.Length;
        int first = len % 3;
        if (first == 0) first = 3;

        sb.Append(digits.Substring(0, first));
        for (int i = first; i < len; i += 3)
        {
            sb.Append(' ');
            sb.Append(digits.Substring(i, 3));
        }

        return (value >= 0 ? "+" : "-") + sb.ToString();
    }

    // Formats unsigned (no sign) with spaces
    public static string FormatUnsignedWithSpaces(ulong value)
    {
        string digits = value.ToString();
        var sb = new StringBuilder();
        int len = digits.Length;
        int first = len % 3;
        if (first == 0) first = 3;
        sb.Append(digits.Substring(0, first));
        for (int i = first; i < len; i += 3)
        {
            sb.Append(' ');
            sb.Append(digits.Substring(i, 3));
        }
        return sb.ToString();
    }
}
