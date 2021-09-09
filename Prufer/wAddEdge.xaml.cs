using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prufer
{
    /// <summary>
    /// Interaction logic for wAddEdge.xaml
    /// </summary>
    public partial class wAddEdge : Window
    {
        private IEnumerable<int> _Numbers { get; set; }

        public wAddEdge(IEnumerable<int> numbers)
        {
            InitializeComponent();
            this._Numbers = numbers;
        }

        private ucEdge _CreatedEdge { get; set; } = null;

        public ucEdge ShowDialog(Point coords)
        {
            this.Left = coords.X - 40;
            this.Top = coords.Y - 75;
            base.ShowDialog();
            return _CreatedEdge;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _Numbers.Count(); i++)
            {
                cb_node1.Items.Add(_Numbers.ElementAt(i));
                cb_node2.Items.Add(_Numbers.ElementAt(i));
            }
        }

        private void b_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void b_create_Click(object sender, RoutedEventArgs e)
        {
            if (cb_node1.SelectedIndex == -1 || cb_node2.SelectedIndex == -1)
                return;

            if ((int)cb_node1.SelectedValue == (int)cb_node2.SelectedValue)
                return;

            _CreatedEdge = new ucEdge() { NodeNumber1 = (int)cb_node1.SelectedValue, NodeNumber2 = (int)cb_node2.SelectedValue };
            this.Close();
        }
    }
}
