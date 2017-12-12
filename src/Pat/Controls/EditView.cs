using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Pat.Reflection;

namespace Pat.Controls
{
    public class EditView : TreeView
    {
        protected override void OnInitialized(EventArgs e)
        {
            if (DataContext != null)
            {
                Populate();
            }
            base.OnInitialized(e);
        }

        private void Populate()
        {
            var properties = DataContext.GetType().GetProperties();

            var scalars = properties.Where(p => p.PropertyType.GetCategory() == TypeCategory.Scalar).ToArray();
            if (scalars.Any())
            {
                var item = new TreeViewItem
                {
                    IsExpanded = true
                };
                item.Items.Add(CreateScalarGrid(scalars));
                Items.Add(item);
            }
            var collections = properties.Where(p => p.PropertyType.GetCategory() == TypeCategory.Collection).ToArray();
            foreach (var collection in collections)
            {
                var treeViewItem = new TreeViewItem { Header = collection.Name, IsExpanded = true};
                treeViewItem.Items.Add(CreateCollectionGrid(collection));

                Items.Add(treeViewItem);
            }

            var complexes = properties.Where(p => p.PropertyType.GetCategory() == TypeCategory.Complex).ToArray();
            foreach (var complex in complexes)
            {
                var treeViewItem = new TreeViewItem { Header = complex.Name, IsExpanded = true};
                treeViewItem.Items.Add(new EditView { DataContext = complex.GetValue(DataContext) });
                Items.Add(treeViewItem);
            }
        }

        private DataGrid CreateCollectionGrid(PropertyInfo property)
        {
            var grid = new DataGrid
            {
                AutoGenerateColumns = true,
                HeadersVisibility = DataGridHeadersVisibility.Column
            };
            var binding = new Binding(property.Name) {Mode = BindingMode.TwoWay};
            grid.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            return grid;
        }

        private static UIElement CreateScalarGrid(PropertyInfo[] scalars)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1.0, GridUnitType.Star)});

            var row = 0;
            foreach (var scalar in scalars)
            {
                grid.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
                grid.Add(new Label {Content = scalar.Name}, row, 0);
                var editor = CreateEditorFor(scalar);
                grid.Add(editor, row, 1);
                row++;
            }

            return grid;
        }

        private static Control CreateEditorFor(PropertyInfo scalar)
        {
            var type = scalar.PropertyType;
            var binding = new Binding(scalar.Name) {Mode = BindingMode.TwoWay};
            if (type == typeof(bool))
            {
                var checkbox = new CheckBox();
                checkbox.SetBinding(ToggleButton.IsCheckedProperty, binding);
                return checkbox;
            }
            if (type.IsEnum)
            {
                var combobox = new ComboBox()
                {
                    IsEditable = false
                };
                foreach (var value in Enum.GetValues(type))
                {
                    combobox.Items.Add(value);
                }
                combobox.SelectedIndex = 0;
                combobox.SetBinding(Selector.SelectedValueProperty, binding);
                return combobox;
            }
            
            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, binding);
            if (scalar.Name == "Body")
            {
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.AcceptsReturn = true;
                textBox.AcceptsTab = true;
            }
            return textBox;
        }
    }
}