using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Barabasi_Albert_Model_Generator
{
    class Program
    {
        private static int InitverticesNumber = 3;
        private static int MaxverticesNumber = 5000 + InitverticesNumber;

        static void Main(string[] args)
        {
            IDictionary<int, String> vertices = new Dictionary<int, string>();
            IDictionary<int, IList<int>> edges = new Dictionary<int, IList<int>>();

            while (!GatherInput())
                ;

            GenerateInitGraphics(vertices, edges);

            for (int iCount = InitverticesNumber; iCount < MaxverticesNumber; iCount++)
            {
                vertices.Add(iCount, GenerateRandomString());

                int targetVertice = FindTheNextToConnect(edges);

                edges.Add(iCount, new List<int>() { targetVertice });
                if (edges[targetVertice] == null)
                {
                    edges[targetVertice] = new List<int>();
                }
                edges[targetVertice].Add(iCount);
            }

            WriteToNetFile(vertices, edges);

            PrintDegreesDist(vertices, edges);
        }

        private static Boolean GatherInput()
        {
            Console.WriteLine("Please input the max vertices number (default 5000): ");
            String inputNumberString = Console.ReadLine();

            if (String.IsNullOrWhiteSpace(inputNumberString))
                return true;

            int result = -1;
            if (int.TryParse(inputNumberString, out result))
                if(result >= 0)
                {
                    MaxverticesNumber = result + InitverticesNumber;
                    return true;
                }

            Console.WriteLine("Not acceptable value.");
            return false;
        }

        private static void PrintDegreesDist(IDictionary<int, String> vertices, IDictionary<int, IList<int>> edges)
        {
            StringBuilder keyList = new StringBuilder(), countList = new StringBuilder();

            keyList.Append("Degrees : ");
            countList.Append("Counts : ");

            foreach (var groupInfo in edges.GroupBy(x => {
                return x.Value.Count;
                //int nCount = 0;
                //for (; nCount < segmentList.Count; nCount++) { if (x.Value.Count < segmentList[nCount]) break; }
                //return segmentList[nCount];
            }).OrderBy(x=>x.Key))
            {
                //Console.WriteLine(@"{0}:{1}", groupInfo.Key, groupInfo.Count());
                keyList.Append(groupInfo.Key);
                keyList.Append(',');
                countList.Append(groupInfo.Count());
                countList.Append(',');
            }

            Console.WriteLine(keyList.ToString());
            Console.WriteLine(countList.ToString());
        }

        //private static List<int> segmentList = new List<int>() { 1, 2, 5, 10, 20, 60, 80, 150, 300, 1000, 5000, 20000 };

        #region pajek input file format

        private static void WriteToNetFile(IDictionary<int, String> vertices, IDictionary<int, IList<int>> edges, String path = @"ba-sample.net")
        {
            using (StreamWriter textwriter = new StreamWriter(path, false, Encoding.ASCII))//System.Text.Encoding.Unicode))
            {
                textwriter.WriteLine(pajek_net_vertices_prefix, vertices.Count);
                foreach (var vertice in vertices)
                    textwriter.WriteLine(@"{0} ""{1}""", vertice.Key + 1, vertice.Value);
                textwriter.WriteLine(pajek_net_edgelist_prefix);
                //textwriter.WriteLine(pajek_net_edge_prefix);
                foreach (var edge in edges)
                {
                    if (edge.Value == null || edge.Value.Count == 0 || edge.Value.Count( x=> x > edge.Key) == 0)
                        continue;

                    //foreach (var target in edge.Value)
                    //{
                    //    if (target > edge.Key)
                    //        textwriter.WriteLine(@"{0} {1}", edge.Key+1, target+1);
                    //}
                    StringBuilder sb = new StringBuilder();
                    sb.Append(edge.Key + 1);
                    sb.Append(' ');
                    foreach (var target in edge.Value)
                    {
                        if (target > edge.Key)
                        {
                            sb.Append(target + 1);
                            sb.Append(' ');
                        }
                    }
                    textwriter.WriteLine(sb.ToString());
                }
                textwriter.WriteLine();
            }

        }

        public const String pajek_net_vertices_prefix = @"*Vertices {0}";
        public const String pajek_net_edgelist_prefix = @"*Edgeslist";
        public const String pajek_net_edge_prefix = @"*Edges";

        #endregion

        private static int FindTheNextToConnect(IDictionary<int, IList<int>> edges)
        {
            int next = randomSeed.Next(0, GetSumDegrees(edges) - 1);
            int lCount = 0;

            for (; lCount < edges.Count && next >= 0;)
            {
                next -= GetDegreesOfVertex(edges ,lCount++);
            }

            return lCount>=1?lCount-1:0;
        }

        private static int GetSumDegrees(IDictionary<int, IList<int>> edges)
        {
            int sum = 0;
            foreach (var edge in edges)
            {
                if (edge.Value != null)
                    sum += edge.Value.Count;
            }
            return sum;
        }

        private static int GetDegreesOfVertex(IDictionary<int, IList<int>> edges, int vertex)
        {
            return edges[vertex] == null ? 0 : edges[vertex].Count;
        }

        #region Init Graphics

        private static void GenerateInitGraphics(IDictionary<int, String> vertices, IDictionary<int, IList<int>> edges)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (vertices.Count > 0)
                throw new ArgumentOutOfRangeException(nameof(vertices));

            for (int iCount = 0; iCount < InitverticesNumber; iCount++)
            {
                vertices.Add(iCount, GenerateRandomString());
            }
            for (int lCount = 0; lCount < InitverticesNumber; lCount++)
            {
                IList<int> fullEdges = new List<int>();
                for (int iCount = 0; iCount < InitverticesNumber; iCount++)
                {
                    if(iCount != lCount)
                        fullEdges.Add(iCount);
                }
                edges.Add(lCount, fullEdges);
            }

        }

        #endregion

        #region Generate Random String

        private static String GenerateRandomString()
        {
            StringBuilder randomSb = new StringBuilder();
            int length = randomSeed.Next(MinNameLength, MaxNameLength);
            for(int lCount =0; lCount < length; lCount++)
            {
                int position = randomSeed.Next(0, charSets.Count - 1);
                randomSb.Append(charSets[position]);
            }
            return randomSb.ToString();
        }

        private static Random randomSeed = new Random(DateTime.Now.Millisecond);
        private static readonly List<Char> charSets = new List<char>() { ' ', 'a', 'b', 'c', 'q', 'r', 's', 't', 'u', 'v', 'w' };
        private static readonly int MaxNameLength = 8;
        private static readonly int MinNameLength = 6;

        #endregion

    }
}
