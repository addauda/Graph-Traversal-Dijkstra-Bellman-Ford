using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FinalProjectClasses;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Design;


namespace FinalProjectAlgorithms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
       public Graph graph = new Graph(); //new object of the graph class
      
        public MainWindow()
        {
            InitializeComponent(); //initialize all window controls
            OpenFile.BorderBrush = Brushes.Red;
            OpenFile.Focus();
            disable();
        }
        private void enable() //enables all other controls after a file is loaded
        {
            tbCost.IsEnabled = true;
            tbDest.IsEnabled = true;
            tbSource.IsEnabled = true;
            btnChangeWeight.IsEnabled = true;
            btnKruskal.IsEnabled = true;
            btnRemoveEdge.IsEnabled = true;
            btnRemoveNode.IsEnabled = true;
            buttonBellmanFord.IsEnabled = true;
            manually_add.IsEnabled = true;
            SaveFile.IsEnabled = true;
            print.IsEnabled = true;
            help.IsEnabled = true;
        }
        private void disable() //disables all other controls until a file is loaded
        {
            tbCost.IsEnabled = false;
            tbDest.IsEnabled = false;
            tbSource.IsEnabled = false;
            btnChangeWeight.IsEnabled = false;
            btnKruskal.IsEnabled = false;
            btnRemoveEdge.IsEnabled = false;
            btnRemoveNode.IsEnabled = false;
            buttonBellmanFord.IsEnabled = false;
            manually_add.IsEnabled = false;
            SaveFile.IsEnabled = false;
            print.IsEnabled = false;
            help.IsEnabled = false;
        }
        //load xml file
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {           
            graph.nodeSet.nodes.Clear();//clears the graph nodes

             OpenFileDialog openDialog = new OpenFileDialog();

            //default extension for files 
            openDialog.DefaultExt = "xml";

            //filter for files in a directory
            openDialog.Filter = "XML files (*.xml)|*.xml"; //only open xml files

            // establish an initial directory displayed by the file dialog box-->environment specific, my documents folder
            openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // assign the title for file dialog box
            openDialog.Title = "Load Cities";

            // accept only valid Win32 filenames
            openDialog.ValidateNames = true;

            if (openDialog.ShowDialog().Value)//if dialog is succesful
            {
                 graph.LoadXMLData(openDialog.FileName);//load data to graph object   
                 NodesToTree();//add nodes to screen      
                 enable();
                 OpenFile.BorderBrush = Brushes.White;
            }
             
        }

        public void printBellManFordResults(int index)
        {        
            List<string> path = new List<string>(graph.nodeSet.nodes.Count); //new list to holds cities in path
            if (graph.pred_index[index] == -1) //if the pred_index = -1, add that city to the path
                path.Add(graph.nodeSet.nodes.ElementAt(index).city);
            else
            {
                path.Add(graph.nodeSet.nodes.ElementAt(index).city);//else add that city to path
                printBellManFordResults(graph.pred_index[index]);//recursive call to the index specified by the pred_index of previous node
            }
            path.Reverse(); //reverse the path for displaying

            foreach (string node in path)//loop through and display
                algoResults.Text += node + "->";           
        }
       
        private void buttonBellmanFord_Click(object sender, RoutedEventArgs e)
        {
            if (tbSource.Text != "") //if input is not empty
            {
                if (graph.Contains(tbSource.Text.ToLower())) //chekc if city is in the graph
                {

                    algoResults.Clear();//clear display box

                    algoResults.Text += "BELLMAN FORD ALGORITHMS\n--------------------------------------------------------\n";
                    graph.BellmanFord(tbSource.Text.ToLower()); //change the city to lower case and run bellman ford

                    for (int i = 0; i < graph.nodeSet.nodes.Count; i++) //print results
                    {
                        printBellManFordResults(i);
                        algoResults.Text += "Cost :" + graph.distance[i].ToString() + "\n";
                    }
                    graph.Vertices.Clear();//reset values in vertices

                    //reset pred_index and distance arrays
                    for (int i = 0; i < graph.nodeSet.nodes.Count; i++)
                    {
                        graph.distance[i] = 0; graph.pred_index[i] = 0;
                    }
                    clearTB();//clear input textboxes
                }

                else //source city not found              
                {
                    MessageBox.Show("Source City Not Found", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            else //input is blank
            {

                tbSource.Background = Brushes.Red;
                MessageBox.Show("Source City Field Is Blank", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbSource.Background = Brushes.White;
            }
           
        }

        private void btnRemoveEdge_Click(object sender, RoutedEventArgs e)
        {
            if (tbSource.Text == "" || tbDest.Text == "") //if input is blank
            {
              
                if (tbSource.Text == "" )
                tbSource.Background = Brushes.Red;

                if (tbDest.Text == "")                
                tbDest.Background = Brushes.Red;

                MessageBox.Show("Source Or Destination City Missing\n Check Input Values", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbSource.Background = Brushes.White;
                tbDest.Background = Brushes.White;
                
            }

            else
            {
               //check if graph contains specified cities
                if (graph.Contains(tbSource.Text.ToLower()) && graph.Contains(tbDest.Text.ToLower()))
                {
                    graph.RemoveAnEdge(tbSource.Text.ToLower(), tbDest.Text.ToLower());
                    NodesToTree();
                }
                else
                {
                    //nodes not found display error
                    MessageBox.Show("A Node Does Not Exist In Graph\nCheck Input Values or Check Input File", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                   
                }
                clearTB();
            }
        }

        private void btnRemoveNode_Click(object sender, RoutedEventArgs e)
        {
            if (tbSource.Text != "")
            {
                if (graph.Contains(tbSource.Text.ToLower())) //check if city is in the graph
                {                  
                    graph.RemoveANode(tbSource.Text.ToLower());
                      
                    NodesToTree(); //refesh nodes display
                 
                }
                else
                    MessageBox.Show("A Node Does Not Exist In Graph\nCheck Input Values or Check Input File", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);

                clearTB();
            }
            else //input field is blank
            {
                tbSource.Background = Brushes.Red;
                MessageBox.Show("Source City Field Is Blank", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbSource.Background = Brushes.White;
            }
        }

        public void clearTB()
        {
           //reset input boxes
            tbSource.Clear(); tbDest.Clear(); tbCost.Clear();
        }

        private void btnChangeWeight_Click(object sender, RoutedEventArgs e)
        {
            if (tbSource.Text == "" || tbDest.Text == "" || tbCost.Text == "") //blank input field
            {
                if (tbSource.Text == "")
                    tbSource.Background = Brushes.Red;

                if (tbDest.Text == "")
                    tbDest.Background = Brushes.Red;

                if (tbCost.Text == "")
                    tbCost.Background = Brushes.Red;


                MessageBox.Show("Required Input(s) Is Missing", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbSource.Background = Brushes.White;
                tbDest.Background = Brushes.White;
                tbCost.Background = Brushes.White;
            }
            else
            {
                try //try used to take care of parsing int values
                {
                    //check if cities are in the graph
                    if (graph.Contains(tbSource.Text.ToLower()) && graph.Contains(tbDest.Text.ToLower()))
                    {
                        //change weight
                        graph.ChangeWeight(tbSource.Text.ToLower(), tbDest.Text.ToLower(), int.Parse(tbCost.Text));
                        NodesToTree();     
                    }
                    else
                        //nodes not found display error
                        MessageBox.Show("A Node Does Not Exist In Graph\nCheck Input Values or Check Input File", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    clearTB();
                }

                catch (FormatException ferr) { MessageBox.Show(ferr.Message, "Error"); }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //printing
            System.Windows.Forms.PrintDialog print = new  System.Windows.Forms.PrintDialog();
            print.ShowDialog();                      
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (graph.nodeSet.nodes.Count == 0) //if no nodes exists display error
            {
                MessageBox.Show("Graph is Empty \nPlease Load A Node File", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else //run save dialog
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFile.DefaultExt = "xml";
                saveFile.AddExtension = true;
                saveFile.Title = "Save File";
                saveFile.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

                // assign the file name and path
                saveFile.FileName = "Network";
                saveFile.OverwritePrompt = true;
                saveFile.ValidateNames = true;

                // if the common dialog box has been set up
                if (saveFile.ShowDialog().Value)
                {
                    graph.createXMLData(saveFile.FileName); //pass graph nodes, back to xml data
                }               
            }
        }

        public void NodesToTree()
        {
            nodeList.Clear(); //clear previous display

            addNewPolyline(); //map refresh method

            foreach (GraphNode item in graph.nodeSet.nodes)//for every node in gaph
            {
                nodeList.Text += item.city.ToUpper() + "\n"; //display city name

                //check neighbbours
                if (item.neighbours.nodes.Count == 0) //if no neighbour, display no neighbours
                    nodeList.Text +=  "No Neighbour Relationships \n";
                else
                {
                    //display neighbour details for each neighbour
                    foreach (GraphNode neighbour in item.neighbours.nodes)
                    {
                        int index = item.neighbours.nodes.IndexOf(neighbour);

                        nodeList.Text += "Neighbouring City:" + neighbour.city + "    Cost: " + item.costs.ElementAt(index).ToString() + "\n";
                    }
                }
                nodeList.Text += "---------------------------------------------------------------\n";
            }        
        }

        private void btnKruskal_Click(object sender, RoutedEventArgs e)
        {
            if (graph.nodeSet.nodes.Count != 0) //if there is a node is graph
            {
                graph.Kruskals();
                algoResults.Clear();//clear algorithms result textbox
                //display results
                algoResults.Text += "MINIMUM SPANNING TREE VERTICES\n--------------------------------------------------------\n";
                foreach (Vertex vert in graph.mst_Tree)
                {
                    algoResults.Text += graph.nodeSet.nodes.ElementAt(vert.u).city + " --> " +
                        graph.nodeSet.nodes.ElementAt(vert.v).city + "\n";
                }

                algoResults.Text += "------------------------------------------\n";
                algoResults.Text += "MST COST : "+ graph.mst_cost.ToString();
                //reset values
                graph.mst_cost = 0;
                graph.mst_Tree.Clear();
                graph.Vertices.Clear();

            }

            else //no nodes in the graph so display error
                MessageBox.Show("Graph Is Empty\nPlease Add Nodes", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void manually_add_Click(object sender, RoutedEventArgs e)
        {
            if (tbSource.Text == "" || tbDest.Text == "" || tbCost.Text == "") //inputs fields are blank
            {
                if (tbSource.Text == "")
                    tbSource.Background = Brushes.Red;

                if (tbDest.Text == "")
                    tbDest.Background = Brushes.Red;

                if (tbCost.Text == "")
                    tbCost.Background = Brushes.Red;

                MessageBox.Show("Cost,Source Or Destination City Missing/Check Input Values", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbSource.Background = Brushes.White;
                tbDest.Background = Brushes.White;
                tbCost.Background = Brushes.White;
            }

            else
            {
                try //for parsing to be safe
                {
                    //create add new edge to graph
                    graph.AddUndirectedEdge(tbSource.Text.ToLower(), tbDest.Text.ToLower(), int.Parse(tbCost.Text));
                    NodesToTree();
                }
                catch (FormatException ferr) { MessageBox.Show(ferr.Message, "Error"); }
            }
            clearTB(); //clear input
        }

        public void addNewPolyline()//insert cities pushpins and lines between cities in the map
        {
            myMap.Children.Clear(); //clear all children xaml properties of map before proceeding-->refreshing the map
                       
            foreach (GraphNode node in graph.nodeSet.nodes) //loop to every node in the graph and add node as line endpoint
            {
                    ///loop through every neighbour of the node and add neighbour as a polyline endpoint
                foreach (GraphNode neighbour in node.neighbours.nodes) 
                {
                    MapPolyline polyline = new MapPolyline(); //new polyline object 

                    //sets the properties of the line
                    polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);//set the colour to red
                    polyline.StrokeThickness = 5;//thickness of line
                    polyline.Opacity = 0.7;

                    polyline.Locations = new LocationCollection(); //new collection of location info
                    //get the location property of node
                    Location yop = new Location();
                    yop.Latitude = node.cityLocation.Latitude;
                    yop.Longitude = node.cityLocation.Longitude;

                    polyline.Locations.Add(yop);//add location collection
                    Location neigh = new Location();
                    neigh.Latitude = neighbour.cityLocation.Latitude;
                    neigh.Longitude = neighbour.cityLocation.Longitude;

                  polyline.Locations.Add(neigh);
                  myMap.Children.Add(polyline);
                }
             
            }
           

            foreach (GraphNode node in graph.nodeSet.nodes) //loop to every node in the graph
            {
                //get location information of object
                Location yop = new Location();
                yop.Latitude = node.cityLocation.Latitude;
                yop.Longitude = node.cityLocation.Longitude;

                //create a new pushpin object and set the location of pushpin to object location
               Pushpin pin = new Pushpin();
               pin.Location = yop;

               //add pushpin as child xaml property to map
               myMap.Children.Add(pin);
           }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
           
        }  
    }
}
