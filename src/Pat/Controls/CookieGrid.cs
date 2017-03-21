using System.Windows.Controls;

namespace Pat.Controls
{
    public class CookieGrid : DataGrid
    {
        public CookieGrid()
        {
            CanUserReorderColumns = false;
            CanUserAddRows = false;
            CanUserDeleteRows = false;
            CanUserSortColumns = true;
            CanUserResizeColumns = false;
            CanUserResizeRows = false;
        }

        protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {
            base.OnAutoGeneratingColumn(e);
            switch (e.PropertyName)
            {
                case "Domain":
                    e.Column.DisplayIndex = 0;
                    break;
                case "Name":
                    e.Column.DisplayIndex = 1;
                    break;
                case "Value":
                    e.Column.DisplayIndex = 2;
                    break;
            }
        }
    }
}