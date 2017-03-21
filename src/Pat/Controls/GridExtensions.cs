using System.Windows;
using System.Windows.Controls;

namespace Pat.Controls
{
    public static class GridExtensions
    {
        public static void Add(this Grid grid, UIElement element, int row, int column)
        {
            grid.Children.Add(element);
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
        }
    }
}