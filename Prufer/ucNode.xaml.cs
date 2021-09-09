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
    public partial class ucNode : UserControl
    {
        public int AssignedNumber { get; set; }

        public event NodeDeleteEventHandler NodeDelete;
        public event NodeMoveEventHandler NodeMove;

        private Canvas _RelativeTo;

        public ucNode(Canvas relative)
        {
            InitializeComponent();
            _RelativeTo = relative;
        }

        public void AsignNumber(int number)
        {
            if (number < 0 || number >= 100) return;
            AssignedNumber = number;
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Canvas.SetZIndex(this, 2);
                if (e.GetPosition(_RelativeTo).X - 24 >= 0 && e.GetPosition(_RelativeTo).X - 24 <= 500 - 48)
                    Canvas.SetLeft(this, e.GetPosition(_RelativeTo).X - 24);
                if (e.GetPosition(_RelativeTo).Y - 24 >= 0 && e.GetPosition(_RelativeTo).Y - 24 <= 500 - 48)
                    Canvas.SetTop(this, e.GetPosition(_RelativeTo).Y - 24);
                NodeMove?.Invoke(this, new NodeMoveEventArgs() { PosX = e.GetPosition(_RelativeTo).X, PosY = e.GetPosition(_RelativeTo).Y });
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            NodeDelete?.Invoke(this, new NodeDeleteEventArgs()
            {
                AssignedNumber = this.AssignedNumber
            });
        }

        private void control_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                Canvas.SetZIndex(this, 1);
        }

        public override string ToString() => AssignedNumber.ToString();
    }

    // NodeDeleteEvent
    public delegate void NodeDeleteEventHandler(object sender, NodeDeleteEventArgs e);
    public class NodeDeleteEventArgs : EventArgs
    {
        public int AssignedNumber { get; set; }
    }

    // NodeMoveEvent
    public delegate void NodeMoveEventHandler(object sender, NodeMoveEventArgs e);
    public class NodeMoveEventArgs
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
    }
}
