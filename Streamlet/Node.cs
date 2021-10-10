using System;
using System.Collections.Generic;
using System.Text;

namespace Streamlet
{
    // Can I just mock signing?
    // Can I assume that node know total number of nodes?
    // Can I assume that honest nodes know each other ids?
    // What about message echo?
    // Can block contents be dummy? 
    // Should I provide all the necessary things to run the code on your side?
    // Print event log from the program and add to the documentation
    using BlockHash = System.Int32;
    public class Node
    {
        public int id;
        public Dictionary<BlockHash, Block> blockDictionary = new Dictionary<BlockHash, Block>();
        private HashSet<BlockHash> notarized = new HashSet<BlockHash>();
        private HashSet<BlockHash> final = new HashSet<BlockHash>();
        int LNCH; // longestNotarizedChainHeadHash
        private Dictionary<BlockHash, int> distance = new Dictionary<BlockHash, int>(); // distance from genesis block
        private Dictionary<BlockHash, int> votes = new Dictionary<BlockHash, int>();
        private int N;
        public const int p = 107;
        private long lastTime;
        public Node(int id, int N)
        {
            this.N = N;
            this.id = id;

            var b0 = new Block(0, -1, "");
            var b0Hash = b0.GetHash();

            blockDictionary.Add(b0Hash, b0);
            notarized.Add(b0Hash);
            final.Add(b0Hash);
            LNCH = b0Hash;
            distance.Add(b0Hash, 0);
        }
        public int GetEpoch(long time)
        {
            return (int)time/1000;
        }
        public int GetLeader(long time)
        {
            int epoch = GetEpoch(time);
            return (epoch * p) % N;
        }
        public int GetId()
        {
            return id;
        }

        private void FinalizePrefix(BlockHash hash)
        {
            while (hash != -1)
            {
                if (final.Contains(hash))
                {
                    return;
                }
                Console.WriteLine("Node " + id + " finaled block of epoch " + blockDictionary[hash].epoch+"!");
                final.Add(hash);
                hash = blockDictionary[hash].parentHash;
            }
        }
        public List<Message> OnMessageReceived(Message msg)
        {
            List<Message> result = new List<Message>();
            if (msg.GetType() == Message.Type.EpochAnnouncment)
            {
                if (msg.GetSender() == GetLeader(lastTime))
                {
                    var block = msg.GetBlock();
                    var hash = block.GetHash();
                    if (!blockDictionary.ContainsKey(hash))
                    {
                        blockDictionary.Add(hash, block);
                        if (block.parentHash == LNCH)
                        {
                            for (int idIter = 0; idIter < N; idIter++)
                            {
                                if (id != idIter)
                                {
                                    var m = new Message(Message.Type.Vote, id, idIter, block);
                                    result.Add(m);
                                }
                            }
                        }
                    }  
                }
            }
            if (msg.GetType() == Message.Type.Vote)
            {
                var block = msg.GetBlock();
                var hash = block.GetHash();
                if (!notarized.Contains(hash))
                {
                    if (votes.ContainsKey(hash))
                    {
                        votes[hash]++;
                    }
                    else
                    {
                        votes.Add(hash, 1);
                    }
                    if (votes[hash] > 2 / 3 * N)
                    {
                        notarized.Add(hash);
                        var parentDistance = distance[block.parentHash];
                        distance.Add(hash, parentDistance + 1);

                        if (distance[LNCH] < parentDistance + 1)
                        {
                            LNCH = hash;
                        }
                        var parent2Hash = blockDictionary[block.parentHash].parentHash;
                        if (blockDictionary.ContainsKey(parent2Hash))
                        {
                            if (blockDictionary[parent2Hash].epoch + 1 == blockDictionary[block.parentHash].epoch)
                            {
                                if (blockDictionary[block.parentHash].epoch + 1 == blockDictionary[hash].epoch)
                                {
                                    FinalizePrefix(block.parentHash);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        public List<Message> AtTime(long time)
        {
            List<Message> result = new List<Message>();
            int epoch = GetEpoch(time);
            if (epoch != GetEpoch(lastTime))
            {
                if (GetLeader(time) == id)
                {
                    var b = new Block(epoch, LNCH, "block contents");
                    blockDictionary.Add(b.GetHash(), b);
                    for (int idIter = 0; idIter < N; idIter++)
                    {
                        if (id != idIter)
                        {
                            var m = new Message(Message.Type.EpochAnnouncment, id, idIter, b);
                            result.Add(m);
                        }
                    }
                }
            }
            lastTime = time;
            return result;
        }
    }

}
