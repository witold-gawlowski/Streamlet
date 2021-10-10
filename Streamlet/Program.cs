using System;
using System.Collections.Generic;

namespace Streamlet
{
    class Program
    {
        static int N = 5;
        static void Main(string[] args)
        {
            var nodes = new List<Node>();
            for(int i=0; i<N; i++)
            {
                var newNode = new Node(i, N);
                nodes.Add(newNode);
            }
            SchedulerScript scheduler = new SchedulerScript(nodes);
            while (true)
            {
                scheduler.Update();
            }
        }
    }
}
