using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PanAndZoom
{

    public class StatusItem : Border
    {
        private TextBlock titleTextBlock;
        private TextBlock valueTextBlock;
 
        public StatusItem( string t, string v)
        {
            var sp = new StackPanel { Orientation = Orientation.Vertical };

            titleTextBlock = new TextBlock();
            titleTextBlock.Text = t;
            titleTextBlock.TextAlignment = TextAlignment.Center;
            sp.Children.Add(titleTextBlock);

            valueTextBlock = new TextBlock();
            valueTextBlock.Text = v;
            valueTextBlock.TextAlignment = TextAlignment.Center;
            sp.Children.Add(valueTextBlock);

            this.Child = sp;
        }
    }
}
