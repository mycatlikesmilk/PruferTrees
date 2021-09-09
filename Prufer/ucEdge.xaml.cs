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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prufer
{
    /// <summary>
    /// Interaction logic for ucEdge.xaml
    /// </summary>
    public partial class ucEdge : UserControl
    {
        public int NodeNumber1 { get; set; }
        public int NodeNumber2 { get; set; }

        public event EdgeDeleteEventHandler EdgeDelete;

        public ucEdge()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            EdgeDelete?.Invoke(this, new EdgeDeleteEventArgs() { Node1 = NodeNumber1, Node2 = NodeNumber2 });
        }

        public override string ToString() => NodeNumber1 + " <-> " + NodeNumber2;
    }


    // EdgeDeleteEvent
    public delegate void EdgeDeleteEventHandler(object sender, EdgeDeleteEventArgs e);
    public class EdgeDeleteEventArgs
    {
        public int Node1 { get; set; }
        public int Node2 { get; set; }
    }
}
