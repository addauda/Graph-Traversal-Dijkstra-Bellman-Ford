using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using FinalProjectAlgorithms.GeocodeService;

namespace FinalProjectClasses
{
    //Vertex class transforms GraphNode's into {int source,int dest,int cost }
    //Integer numbers based on positon in the Graph Class
    public class Vertex : IComparable<Vertex>
    {
        public Vertex() { u = v = edge = 0; }
        public int u;
        public int v;
        public int edge;

        //overlading IComparable compareTo method
        public int CompareTo(Vertex comparePart) //sorting Vertex's by edge cost
        {
            // A null value means that this object is greater. 
            if (comparePart.edge == -1)
                return 1;
            else
                return this.edge.CompareTo(comparePart.edge);
        }

        public bool CompareTo(Vertex comparePart, int count)//comparing two vertex objects-->for removing duplicates
        {
            ///returns true if the same
            if (this.u == comparePart.u && this.v == comparePart.v)
                return true;
            else
                return false;
        }
    }
    
    //class that holds a list of all neighbours
    public class NeighbourList
    {
        public List<GraphNode> nodes; //list of graphnodes

        public NeighbourList() { nodes = new List<GraphNode>(); } //default constructor

        public GraphNode FindByValue(string value) //searching for a city
        {
            // search the list for the value
            foreach (GraphNode node in nodes)
                if (node.city.Equals(value))
                    return node; //return the node if found
            //if we reached here, we didn't find a matching node
            return null;
        }
    }
   

    //class for a node in the graph
    public class GraphNode
    {
        public List<int> costs; //costs to neighbours
        public NeighbourList neighbours; //list of neighbours
        public string city; //node city

        public Location cityLocation; //location property for mapping contains -->longitude and latitude for city

        public GraphNode() { neighbours = new NeighbourList(); costs = new List<int>(); city = ""; cityLocation = new Location(); } //default constructor
        public GraphNode(string city) { Geocode(city); this.city = city; neighbours = new NeighbourList(); costs = new List<int>(); } //overloaded constructor takes city value create a node

        private void Geocode(string address) //geocodes the city--retrieves geolocation info of the city
        {
            cityLocation = new Location();
            //new request to geocode service
            GeocodeRequest geocodeRequest = new GeocodeRequest();
            //bing maps key
            string key = "AjJTSCM6t3UwfdK1gxK1CyKgWmgtEbC3IQKbM875XgidWiDMnkMuQf6_Fufi7abH";

            // Set credentials using a Bing Maps key
            geocodeRequest.Credentials = new Microsoft.Maps.MapControl.WPF.Credentials();
            geocodeRequest.Credentials.ApplicationId = key;

            // Set the address of city
            geocodeRequest.Query = address;

            // Make the geocode request
            GeocodeServiceClient geocodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            GeocodeResponse geocodeResponse = geocodeService.Geocode(geocodeRequest);

            //get info from reponse and save to cityLocation
            cityLocation.Latitude = geocodeResponse.Results[0].Locations[0].Latitude;
            cityLocation.Longitude = geocodeResponse.Results[0].Locations[0].Longitude;
        }
    }

    //class to hold all nodes in our graph
    public class Graph
    {
        public NeighbourList nodeSet; //all nodes in the graph

        public List<Vertex> Vertices; //list of graphnodes transformed to vertices format    

        public int[] distance; //distance to each node used by BELLMAN FORD

        public int mst_cost;//mst cost used by KRUSKALS

        public int[] pred_index;//predecessor index used by BELLMAN FORD

        public List<Vertex> mst_Tree;//spannign tree list of vertices

        //default constructor create new Graph
        public Graph() { nodeSet = new NeighbourList(); Vertices = new List<Vertex>(); mst_Tree = new List<Vertex>(); mst_cost = 0; }

        //overloaded constructor that takes a neighbour list to build the graph
        public Graph(NeighbourList nodeSet) 
        {
            if (nodeSet == null)
                this.nodeSet = new NeighbourList();
            else
                this.nodeSet = nodeSet;
        }

        //converts nodes in the graph to vertex format to be used by BELLMAN FORD AND KRUSKALS
        public void convertToVertex()
        {
            foreach (GraphNode node in nodeSet.nodes)
            {
                foreach (GraphNode neighbour in node.neighbours.nodes)
                {
                    Vertex newVertex = new Vertex();
                    Vertex oldVertex = new Vertex();
                    int index = 0;
                    newVertex.u = nodeSet.nodes.IndexOf(node);
                    newVertex.v = nodeSet.nodes.IndexOf(neighbour);
                    index = node.neighbours.nodes.IndexOf(neighbour);
                    newVertex.edge = node.costs.ElementAt(index);
                    Vertices.Add(newVertex);
                }
            }
        }

        //add undirected edge-->add the 'to' node to the 'from' neighbour list and vice versa
        public void AddUndirectedEdge(string from, string to, int cost) 
        {
            bool fromNodeExists, toNodeExists;
            fromNodeExists = Contains(from); //if the node exists
            toNodeExists = Contains(to);//if the node exists

            if (fromNodeExists && !toNodeExists)//case --> the first node is already on the list but the second node is new
            {
                GraphNode toNode = new GraphNode(to);//create new node
                toNode.neighbours.nodes.Add(nodeSet.FindByValue(from)); //find that node and add it to the new node's neighbours
                toNode.costs.Add(cost);//add the cost to get to that neigbour

                //for node that already exists, just update neighbour list for that node
                nodeSet.FindByValue(from).neighbours.nodes.Add(toNode);
                nodeSet.FindByValue(from).costs.Add(cost);

                nodeSet.nodes.Add(toNode);//add the new node to Graph and increment the node counter
            }

            if (!fromNodeExists && !toNodeExists) //case--> none of the nodes exist
            {
                //create new nodes
                GraphNode fromNode = new GraphNode(from);
                GraphNode toNode = new GraphNode(to);

                //add other neighbour and cost to relevant lists
                fromNode.neighbours.nodes.Add(toNode);
                fromNode.costs.Add(cost);

                toNode.neighbours.nodes.Add(fromNode);
                toNode.costs.Add(cost);

                //add nodes 
                nodeSet.nodes.Add(fromNode); nodeSet.nodes.Add(toNode);
            }


            if (fromNodeExists && toNodeExists) //case--> both of the nodes exist
            {
                //find already existing first node and update its neighbour and cost lists
                nodeSet.FindByValue(from).neighbours.nodes.Add(nodeSet.FindByValue(to));
                nodeSet.FindByValue(from).costs.Add(cost);

                //find second already existing node and update its neighbour and cost lists
                nodeSet.FindByValue(to).neighbours.nodes.Add(nodeSet.FindByValue(from));
                nodeSet.FindByValue(to).costs.Add(cost);
            }

            if (!fromNodeExists && toNodeExists) //case--> both of the nodes exist
            {
                GraphNode fromNode = new GraphNode(from);//create new node
                fromNode.neighbours.nodes.Add(nodeSet.FindByValue(to)); //find that node and add it to the new node's neighbours
                fromNode.costs.Add(cost);//add the cost to get to that neigbour

                //for node that already exists, just update neighbour list for that node
                nodeSet.FindByValue(to).neighbours.nodes.Add(fromNode);
                nodeSet.FindByValue(to).costs.Add(cost);

                nodeSet.nodes.Add(fromNode); //add the new node to Graph            
            }

        }

        //check if the graph contains a city
        public bool Contains(string value)
        {
            return nodeSet.FindByValue(value) != null; //uses the 'find by value' method to search the graph
        }

        //remove a city from the graph
        public bool RemoveANode(string value) 
        {
             //first remove the node from the nodeset
            GraphNode nodeToRemove = (GraphNode)nodeSet.FindByValue(value);     
             
            // otherwise, the node was found
            nodeSet.nodes.Remove(nodeToRemove);

            // iterate through each node in the nodeSet, removing edges to this node
            foreach (GraphNode gnode in nodeSet.nodes)
            {
                int index = gnode.neighbours.nodes.IndexOf(nodeToRemove);
                if (index != -1)                {
                    // remove the reference to the node and associated cost
                    gnode.neighbours.nodes.RemoveAt(index);
                    gnode.costs.RemoveAt(index);
                }
            }
            return true;
        }

        //change the weight between two edges
        public bool ChangeWeight(string node1,string node2 ,int cost)
        {
            GraphNode nodeOne = (GraphNode)nodeSet.FindByValue(node1); //finds and returns the corresponding nodes
            GraphNode nodeTwo = (GraphNode)nodeSet.FindByValue(node2);            
            
            int index = nodeOne.neighbours.nodes.IndexOf(nodeTwo); //gets the index of node2 in the neighbourlist of node 1

            if (index != -1) //if found change the cost
                nodeOne.costs[index] = cost;

            else //else display error
            {
                MessageBox.Show(node2.ToUpper() + " is not a Neighbouring city to " + node1.ToUpper(), "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            int index2 = nodeTwo.neighbours.nodes.IndexOf(nodeOne);//gets the index of node1 in the neighbourlist of node 2
            if (index2 != -1)
            nodeTwo.costs[index2]=cost;//changes the cost

            else //display error
            {
                MessageBox.Show(node1.ToUpper() + " is not a Neighbouring city to " + node2.ToUpper(), "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
            
        }

        public bool RemoveAnEdge(string node1, string node2)
        {
            GraphNode nodeOne = (GraphNode)nodeSet.FindByValue(node1);//find and return nodes
            GraphNode nodeTwo = (GraphNode)nodeSet.FindByValue(node2);           

            //else //nodes were found //remove from each other's neighbor tables

            int index = nodeOne.neighbours.nodes.IndexOf(nodeTwo);

            if (index != -1) //if found change the cost
            {
                nodeOne.neighbours.nodes.RemoveAt(index);
                nodeOne.costs.RemoveAt(index);
            }
            else //else display error
            {
                MessageBox.Show(node2.ToUpper() + " is not a Neighbouring city to " + node1.ToUpper(), "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            int index2 = nodeTwo.neighbours.nodes.IndexOf(nodeOne);
            if (index2 != -1)
            {
                nodeTwo.neighbours.nodes.RemoveAt(index2);
                nodeTwo.costs.RemoveAt(index2);
            }
            else //display error
            {
                MessageBox.Show(node1.ToUpper() + " is not a Neighbouring city to " + node2.ToUpper(), "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        public void BellmanFord(string city)
        {
                convertToVertex(); //convert graphnodes into vertex form
                int count = 0;         
                //initialize distance and pred_index arrays to the size of nodes
                distance = new int[nodeSet.nodes.Count];
                pred_index = new int[nodeSet.nodes.Count];

                GraphNode source = new GraphNode();
                source = nodeSet.FindByValue(city);//return node with that city

                //initialize the array to all nodes in the graph with infinity except for the source node cost to
                //get to itself
                foreach (GraphNode vertex in nodeSet.nodes)
                {
                    if (vertex == source)
                    {
                        distance[count] = 0;
                    }

                    else
                        distance[count] = 999;

                    pred_index[count] = -1;
                    count++;
                }

                for (int i = 0; i < nodeSet.nodes.Count; i++) //iterates through all nodes in the graph
                {
                    foreach (Vertex v in Vertices) //for each vertex 
                    {
                        if (distance[v.v] > distance[v.u] + v.edge) //if a better distance is found
                        {
                            distance[v.v] = distance[v.u] + v.edge; //relax edge
                            pred_index[v.v] = v.u; //set pred_index
                        }
                    }
                }         
            
        }

        public void createXMLData(string filename)
        {
            
            var Graph = new XDocument(); //new xml document

            foreach (GraphNode item in nodeSet.nodes)//for each graphnode
            {
                foreach (GraphNode neighbour in item.neighbours.nodes)//enter the neighbour list
                {
                    int index = item.neighbours.nodes.IndexOf(neighbour);//get the index of each neighbour
                    //creates XElements based on node values                                                     
                    var newEdge = new XElement("Edge", new XElement("SourceCity", item.city),
                    new XElement("DestinationCity", neighbour.city),
                    new XElement("Cost", item.costs.ElementAt(index).ToString()));

                    if (File.Exists(filename))//if file exists append to it
                    {
                        Graph = XDocument.Load(filename);                        
                        Graph.Element("Edges").Add(newEdge);
                    }
                    else //else create new file
                    {
                        Graph = new XDocument(new XElement("Edges", newEdge));
                    }
                    Graph.Save(filename);//save file
                }
            }
        }

        public void LoadXMLData(string path)
        {            
            try
            {
                XDocument xmldoc = XDocument.Load(path); //load file from specified path
                XElement XEvents = xmldoc.Element("Edges");

                foreach (XElement item in xmldoc.Root.Nodes()) //parses XElement into graphnode object
                {
                    //constructing object of node class with xml data
                    AddUndirectedEdge(item.Element("SourceCity").Value.ToString(), item.Element("DestinationCity").Value.ToString(),
                    int.Parse(item.Element("Cost").Value.ToString()));
                }

            }
                //catch file exceptions
            catch (FileNotFoundException FileEx) { MessageBox.Show(FileEx.Message, "Error"); }
            catch (Exception genEx) { MessageBox.Show(genEx.Message, "Error"); }
        }

        //removes duplicates from a vertex list passed by reference
        public void RemoveDuplicates(ref List<Vertex> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Vertex reverse = new Vertex();//creates a reverse element
                reverse.u = list.ElementAt(i).v;
                reverse.v = list.ElementAt(i).u;
                reverse.edge = list.ElementAt(i).edge;

                for (int j = 0; j < list.Count; j++) //checks if the element exist in the list
                {

                    if (reverse.CompareTo(list.ElementAt(j), j))
                        list.RemoveAt(j); //if yes, element is removed

                }
            }
        }

        public void Kruskals()
        {
            convertToVertex();//convert graphnodes into vertex form
            Vertices.Sort(); //sort vertices in non decreasing order
            RemoveDuplicates(ref Vertices); //remove duplicates

            //iterate through as many vertices there are and as long as graphnodes - 1 are in the graph
            for (int i = 0; i < Vertices.Count && mst_Tree.Count < nodeSet.nodes.Count; i++)
            {
                Vertex k = new Vertex(); //create new element
                k.u = Vertices.ElementAt(i).u; k.v = Vertices.ElementAt(i).v; k.edge = Vertices.ElementAt(i).edge;

                //check if no cycle exists, this is done by looking at what neighbours each index is connected to
                if (!Cycle(k.u, k.v))
                {
                    //if no cycle add to MST and increment cost
                    mst_Tree.Add(k); mst_cost += k.edge;
                }
            }           
        }
        
        //detects cycles in the graph
        public bool Cycle(int u, int v)
        {
            List<int> uneighbour = new List<int>();
            List<int> vneighbour = new List<int>();

            //get all the neighbours of the specifies node as an index in the spanning tree
            foreach (Vertex vert in mst_Tree) 
            {
                if (vert.u == u)
                    uneighbour.Add(vert.v);
                if (vert.v == u)
                    uneighbour.Add(vert.u);
            }
            //get all the neighbours of the specifies node as an index in the spanning tree
            foreach (Vertex vert in mst_Tree)
            {
                if (vert.u == v)
                    vneighbour.Add(vert.v);

                if (vert.v == v)
                    vneighbour.Add(vert.u);
            }

            //if any of the lists are null, a node must have no neighbours in mst, therefore no cycle
            if (uneighbour.Count == 0 || vneighbour.Count == 0)
            {

            }

            else //if lists are not null, iterate through and once node A has a neighbout node B has,return true
            {
                foreach (int num in uneighbour)
                {
                    foreach (int num2 in vneighbour)
                    {
                        if (vneighbour.IndexOf(num) != -1 || uneighbour.IndexOf(num2) != -1)
                            return true; //cycle found
                    }
                }
            }
            return false; //cycle no found
        } 
    }
}