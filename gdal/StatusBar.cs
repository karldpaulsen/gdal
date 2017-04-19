using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PanAndZoom
{
    class StatusBar : Grid
    {
        int rows;
        int columns;

        public StatusBar() : base()
        {
            this.Loaded += new RoutedEventHandler(myLoaded);
        }


        private void myLoaded(object sender, EventArgs e)
        {
            for (var r = 0; r < this.RowDefinitions.Count; r++)
                for (var c = 0; c < this.ColumnDefinitions.Count; c++)
                {
                    addItem("title", r.ToString() + "," + c.ToString(), r, c);
                }
        }

        public void addItem(string title, string value, int row, int column)
        {
            StatusItem si = new StatusItem(title, value);
            this.Children.Add(si);
            Grid.SetColumn(si, column);
            Grid.SetRow(si, row);
        }

    }
}
