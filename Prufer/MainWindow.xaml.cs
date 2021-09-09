using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using System.Collections.Specialized;
using System.Data.Sql;
using System.Windows.Baml2006;

namespace Prufer
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Connection> AllConnections { get; set; }
        public ObservableCollection<ucNode> AllNodes { get; set; }
        public ObservableCollection<ucEdge> AllEdges { get; set; }

        private Point _NodeCreatePosition { get; set; }

        public ObservableCollection<int> CurrentNumbers { get; set; }
        public int NextNumber = 0;

        public MainWindow()
        {
            InitializeComponent();
            AllConnections = new ObservableCollection<Connection>();
            AllNodes = new ObservableCollection<ucNode>();
            AllEdges = new ObservableCollection<ucEdge>();
            CurrentNumbers = new ObservableCollection<int>();

            CurrentNumbers.CollectionChanged += this.CurrentNumbers_CollectionChanged;
        }

        private void CurrentNumbers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                bool flag = false;
                for (int i = 0; i < CurrentNumbers.Count; i++)
                    if (!CurrentNumbers.Contains(i))
                    {
                        NextNumber = i;
                        flag = true;
                        break;
                    }
                if (!flag)
                    NextNumber = CurrentNumbers.Count;
            }
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                bool flag = false;
                for (int i = 0; i < CurrentNumbers.Count; i++)
                    if (!CurrentNumbers.Contains(i))
                    {
                        NextNumber = i;
                        flag = true;
                        break;
                    }
                if (!flag)
                    NextNumber = CurrentNumbers.Count;
            }
        }

        private int AssignNumberToNode()
        {
            int returnNumber = NextNumber;
            CurrentNumbers.Add(NextNumber);
            return returnNumber;
        }

        private void mi_addNode_Click(object sender, RoutedEventArgs e)
        {
            ucNode node = new ucNode(c_field);
            node.NodeDelete += (o, args) =>
            {
                try
                {
                    DeleteNode(node.AssignedNumber);
                }
                catch (Exception ex) { MessageBox.Show($"Не удалось удалить вершину с номером {node.AssignedNumber}. Ошибка: {ex.Message}"); }
            };
            node.NodeMove += (o, args) =>
            {
                UpdateAllEdges();
            };
            node.AssignedNumber = AssignNumberToNode();
            c_field.Children.Add(node);
            AllNodes.Add(node);
            Canvas.SetLeft(node, _NodeCreatePosition.X - 24);
            Canvas.SetTop(node, _NodeCreatePosition.Y - 24);
            Canvas.SetZIndex(node, 1);
        }

        private void DeleteNode(int number)
        {
            ucNode nodeToDelete = AllNodes.Where(x => x.AssignedNumber == number).FirstOrDefault();
            c_field.Children.Remove(nodeToDelete);
            AllNodes.Remove(nodeToDelete);
            CurrentNumbers.Remove(CurrentNumbers.Where(x => x == number).FirstOrDefault());

            var edges = AllEdges.Where(x => x.NodeNumber1 == number || x.NodeNumber2 == number).ToList();
            for (int i = 0; i < edges.Count; i++)
            {
                AllEdges.Remove(edges[i]);
                c_field.Children.Remove(edges[i]);
            }

            var connections = AllConnections.Where(x => x.Edge.NodeNumber1 == number || x.Edge.NodeNumber2 == number).ToList();
            for (int i = 0; i < connections.Count; i++)
                AllConnections.Remove(connections[i]);

        }

        private void c_field_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _NodeCreatePosition = e.GetPosition(c_field);
            }
        }

        private void mi_addEdge_Click(object sender, RoutedEventArgs e)
        {
            wAddEdge wnd = new wAddEdge(CurrentNumbers);

            Point total = new Point(
                this.Left + ((this.Width - c_field.Width) / 2) + _NodeCreatePosition.X,
                this.Top + 70 + ((this.Height - c_field.Height) / 2) + _NodeCreatePosition.Y
                );

            ucEdge edge = wnd.ShowDialog(total);
            if (edge == null) return;

            if (AllConnections.Any(x =>
                ((x.Node1.AssignedNumber == edge.NodeNumber1 || x.Node2.AssignedNumber == edge.NodeNumber1) &&
                (x.Node1.AssignedNumber == edge.NodeNumber2 || x.Node2.AssignedNumber == edge.NodeNumber2))
                ))
                return;

            AddEdge(edge);
        }

        private void AddEdge(ucEdge edge)
        {
            ucNode node1 = AllNodes.Where(x => x.AssignedNumber == edge.NodeNumber1).FirstOrDefault();
            ucNode node2 = AllNodes.Where(x => x.AssignedNumber == edge.NodeNumber2).FirstOrDefault();

            edge.EdgeDelete += (o, args) =>
            {
                try
                {
                    DeleteEdge(edge.NodeNumber1, edge.NodeNumber2);
                }
                catch (Exception ex) { MessageBox.Show($"Не удалось удалить ребро {edge.NodeNumber1}-{edge.NodeNumber2}. Ошибка: {ex.Message}"); }
            };

            c_field.Children.Add(edge);
            AllEdges.Add(edge);

            Connection ct = new Connection()
            {
                Node1 = node1,
                Node2 = node2,
                Edge = edge
            };
            AllConnections.Add(ct);

            Canvas.SetLeft(edge, Canvas.GetLeft(node1) + 24 - 2);
            Canvas.SetTop(edge, Canvas.GetTop(node1) + 24 - 2);

            Canvas.SetZIndex(edge, -1);

            double angle = Math.Atan2(
                Canvas.GetTop(node2) - Canvas.GetTop(node1),
                Canvas.GetLeft(node2) - Canvas.GetLeft(node1)
                );

            edge.Width = Math.Sqrt(Math.Pow(Canvas.GetLeft(node1) - Canvas.GetLeft(node2), 2) + Math.Pow(Canvas.GetTop(node1) - Canvas.GetTop(node2), 2));

            RotateTransform rot = new RotateTransform(angle / Math.PI * 180, 2, 2);
            edge.RenderTransform = rot;
        }

        public void UpdateAllEdges()
        {
            for (int i = 0; i < c_field.Children.Count; i++)
            {
                var c = c_field.Children[i];
                if (c.GetType() == typeof(ucEdge))
                {
                    ucEdge edge = (ucEdge)c;

                    ucNode n1 = AllNodes.Where(x => x.AssignedNumber == edge.NodeNumber1).FirstOrDefault();
                    ucNode n2 = AllNodes.Where(x => x.AssignedNumber == edge.NodeNumber2).FirstOrDefault();

                    Canvas.SetLeft(edge, Canvas.GetLeft(n1) + 24 - 2);
                    Canvas.SetTop(edge, Canvas.GetTop(n1) + 24 - 2);

                    Canvas.SetZIndex(edge, -1);

                    double angle = Math.Atan2(
                        Canvas.GetTop(n2) - Canvas.GetTop(n1),
                        Canvas.GetLeft(n2) - Canvas.GetLeft(n1)
                        ) / Math.PI * 180;

                    edge.Width = Math.Sqrt(Math.Pow(Canvas.GetLeft(n1) - Canvas.GetLeft(n2), 2) + Math.Pow(Canvas.GetTop(n1) - Canvas.GetTop(n2), 2));

                    RotateTransform rot = new RotateTransform(angle, 2, 2);
                    edge.RenderTransform = rot;
                }
            }
        }

        private void DeleteEdge(int node1, int node2)
        {
            ucEdge edgeToDelete = AllEdges.Where(x => (x.NodeNumber1 == node1 || x.NodeNumber1 == node2) && (x.NodeNumber2 == node1 || x.NodeNumber2 == node2)).FirstOrDefault();

            c_field.Children.Remove(edgeToDelete);
            AllEdges.Remove(edgeToDelete);

            Connection ct = AllConnections.Where(x => (x.Node1.AssignedNumber == node1 || x.Node1.AssignedNumber == node2) && (x.Node2.AssignedNumber == node1 || x.Node2.AssignedNumber == node2)).FirstOrDefault();

            AllConnections.Remove(ct);
        }

        private int GetNodeConnections(ucNode node, out List<ucNode> connectedNodes)
        {
            var f1 = AllConnections.Where(x => x.Node1.AssignedNumber == node.AssignedNumber).ToList();
            var f2 = AllConnections.Where(x => x.Node2.AssignedNumber == node.AssignedNumber).ToList();

            connectedNodes = new List<ucNode>();
            connectedNodes.AddRange(f1.Select(x => x.Node2));
            connectedNodes.AddRange(f2.Select(x => x.Node1));

            return connectedNodes.Count;
        }

        private List<(int a, int b)> ConvertConnectionsToList()
        {
            List<(int a, int b)> temp = new List<(int, int)>();
            for (int i = 0; i < AllConnections.Count; i++)
            {
                temp.Add((AllConnections[i].Node1.AssignedNumber, AllConnections[i].Node2.AssignedNumber));
            }
            return temp;
        }

        private void toTree_Click(object sender, RoutedEventArgs e)
        {
            string code = "";

            if (CurrentNumbers.Count != CurrentNumbers.Last() + 1)
                return;

            List<(int, int)> edgeLinks = ConvertConnectionsToList();
            List<(int node, int concount)> nodeList = new List<(int, int)>();


            for (int i = edgeLinks.Count; i > 1; i--)
            {
                nodeList = new List<(int node, int concount)>();
                for (int j = 0; j < CurrentNumbers.Count; j++)
                {
                    (int, int) temp = (j, edgeLinks.Count(x => x.Item1 == j || x.Item2 == j));
                    nodeList.Add(temp);
                }

                List<int> leafs = new List<int>();
                leafs.AddRange(nodeList.Where(x => x.concount == 1).Select(x => x.node));

                var removeLeaf = leafs.OrderBy(x => x).FirstOrDefault();

                var edge = edgeLinks.Where(x => x.Item1 == removeLeaf || x.Item2 == removeLeaf).FirstOrDefault();
                var connectedNode = removeLeaf == edge.Item1 ? edge.Item2 : edge.Item1;

                code += connectedNode + " ";
                edgeLinks.Remove(edge);
            }

            tb_PruferCode.Text = code.Trim();
        }

        private void toCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tb_PruferCode.Text))
                    return;

                AllEdges.Clear();
                AllNodes.Clear();
                AllConnections.Clear();
                c_field.Children.Clear();
                CurrentNumbers.Clear();
                NextNumber = 0;

                List<string> prufer = tb_PruferCode.Text.Split(' ').ToList();

                int topElement = prufer
                    .GroupBy(x => x)
                    .Select(x => new { node = x.Key, sum = x.Count() })
                    .OrderByDescending(x => x.sum)
                    .Select(x => int.Parse(x.node))
                    .FirstOrDefault();

                List<int> restoreList = new List<int>();
                for (int i = 0; i < prufer.Count + 2; i++)
                {
                    restoreList.Add(i);
                    ucNode node = new ucNode(c_field);
                    node.AssignedNumber = AssignNumberToNode();
                    node.NodeDelete += (o, args) =>
                    {
                        try
                        {
                            DeleteNode(node.AssignedNumber);
                        }
                        catch (Exception ex) { MessageBox.Show($"Не удалось удалить вершину с номером {node.AssignedNumber}. Ошибка: {ex.Message}"); }
                    };
                    node.NodeMove += (o, args) =>
                    {
                        UpdateAllEdges();
                    };
                    AllNodes.Add(node);
                }

                int cnt = prufer.Count;
                for (int i = 0; i < cnt; i++)
                {
                    for (int j = 0; j < cnt + 2; j++)
                    {
                        if (!prufer.Contains(j.ToString()) && restoreList.Contains(j))
                        {
                            ucEdge edge = new ucEdge();
                            edge.NodeNumber1 = j;
                            edge.NodeNumber2 = int.Parse(prufer[0]);
                            AllEdges.Add(edge);
                            c_field.Children.Add(edge);
                            prufer.RemoveAt(0);
                            restoreList.Remove(j);
                            break;
                        }
                    }
                }

                int node1 = restoreList[0];
                int node2 = restoreList[1];

                ucEdge last = new ucEdge();
                last.NodeNumber1 = node1;
                last.NodeNumber2 = node2;
                c_field.Children.Add(last);
                AllEdges.Add(last);

                for (int i = 0; i < AllEdges.Count; i++)
                {
                    ucEdge edge = AllEdges[i];
                    Connection ct = new Connection();
                    ct.Node1 = AllNodes.Where(x => x.AssignedNumber == edge.NodeNumber1).FirstOrDefault();
                    ct.Node2 = AllNodes.Where(x => x.AssignedNumber == edge.NodeNumber2).FirstOrDefault();
                    ct.Edge = edge;
                    AllConnections.Add(ct);
                }

                // отрисовка графики

                List<int> nodesToDraw = new List<int>();
                for (int i = 0; i < AllNodes.Count; i++)
                    nodesToDraw.Add(AllNodes[i].AssignedNumber);
                List<(int node, int level, int parentposition)> list = new List<(int node, int level, int parentposition)>();
                list.Add((topElement, 0, -1));
                nodesToDraw.Remove(topElement);
                for (int i = 0; nodesToDraw.Count > 0; i++)
                {
                    List<int> highLevelNodes = list.Where(x => x.level == i).Select(x => x.node).ToList();

                    for (int j = 0, k = 0; j < AllEdges.Count; j++)
                    {
                        if (highLevelNodes.Contains(AllEdges[j].NodeNumber1) && (nodesToDraw.Contains(AllEdges[j].NodeNumber1) || nodesToDraw.Contains(AllEdges[j].NodeNumber2)))
                        {
                            list.Add((AllEdges[j].NodeNumber2, i + 1, k));
                            nodesToDraw.Remove(AllEdges[j].NodeNumber2);
                            k++;
                        }
                        if (highLevelNodes.Contains(AllEdges[j].NodeNumber2) && (nodesToDraw.Contains(AllEdges[j].NodeNumber1) || nodesToDraw.Contains(AllEdges[j].NodeNumber2)))
                        {
                            list.Add((AllEdges[j].NodeNumber1, i + 1, k));
                            nodesToDraw.Remove(AllEdges[j].NodeNumber1);
                            k++;
                        }
                    }
                }

                int totalLevels = list.GroupBy(x => x.level).Count();

                for (int i = 0; i < totalLevels; i++)
                {
                    int widthToDivide = 500 - 48 - 48;

                    List<int> nodesOfCurrentLevel = list.Where(x => x.level == i).OrderBy(x => x.parentposition).Select(x => x.node).ToList();

                    int hSpan = nodesOfCurrentLevel.Count == 1 ? 250 - 48 : widthToDivide / (nodesOfCurrentLevel.Count - 1);
                    int vSpan = 48 + 24;

                    for (int j = 0; j < nodesOfCurrentLevel.Count; j++)
                    {
                        try
                        {
                            ucNode selected = AllNodes.Where(x => x.AssignedNumber == nodesOfCurrentLevel[j]).FirstOrDefault();
                            c_field.Children.Add(selected);
                            if (nodesOfCurrentLevel.Count == 1)
                                Canvas.SetLeft(selected, 250 - 24);
                            else
                                Canvas.SetLeft(selected, 24 + (hSpan * j));
                            Canvas.SetTop(selected, 24 + (vSpan * i));
                        }
                        catch
                        {

                        }
                    }
                }
                UpdateAllEdges();
            }
            catch
            {

            }
        }
    }
}
