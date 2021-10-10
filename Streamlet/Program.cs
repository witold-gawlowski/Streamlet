using System;
using System.Collections.Generic;

namespace Streamlet
{
    class Program
    {
        static int N = 5;
        static void Main(string[] args)
        {
            var nodes = new List<NodeScript>();
            for(int i=0; i<N; i++)
            {
                var newNode = new NodeScript(i, N);
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
