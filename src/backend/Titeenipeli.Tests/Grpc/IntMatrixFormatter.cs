using FluentAssertions.Formatting;

namespace Titeenipeli.Tests.Grpc;

public class IntMatrixFormatter : IValueFormatter
{
    public bool CanHandle(object value)
    {
        return value is int[,];
    }

    void IValueFormatter.Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        formattedGraph.AddLine("");

        int[,] matrix = (int[,])value;
        for (int y = 0; y < matrix.GetUpperBound(1); y++)
        {
            string line = "";
            for (int x = 0; x < matrix.GetUpperBound(0); x++)
            {
                line += $"{matrix[x, y]} ";
            }
            formattedGraph.AddLine(line);
        }

        formattedGraph.AddLine("");
    }
}
