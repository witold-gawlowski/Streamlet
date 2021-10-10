using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Streamlet
{
    public class SchedulerScript
    {
        public List<NodeScript> nodes;
        private Dictionary<int, NodeScript> nodeDictionary = new Dictionary<int, NodeScript>();
        private Queue<Message> messageQueue = new Queue<Message>();
        private long startTime;
        private long GetTime()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
        public SchedulerScript(List<NodeScript> nodes)
        {
            this.nodes = nodes;
            startTime = GetTime();
            foreach (var n in nodes)
            {
                nodeDictionary.Add(n.id, n);
            }
        }
        private void Enqueue(List<Message> ml)
        {
            foreach (Message m in ml)
            {
                messageQueue.Enqueue(m);
            }
        }
        public void Update()
        {
            // It is critical to AtTime loop before
            // handling message queue: verifying leader by voters should happen 
            // after all nodes have updated their time to the leaders time. (in this particular environment).
            long time = GetTime() - startTime;
            foreach (NodeScript n in nodes)
            {
                var response = n.AtTime(time);
                if (response != null)
                {
                    Enqueue(response);
                }
            }

            while (messageQueue.Count != 0)
            {
                var m = messageQueue.Peek();
                var recipient = m.GetRecipientId();
                var response = nodeDictionary[recipient].OnMessageReceived(m);
                Enqueue(response);
                messageQueue.Dequeue();
            }
        }
    }

}
