using System;
using System.Collections.Generic;
using System.Text;

namespace Streamlet
{
    using System.Collections;
    using System.Collections.Generic;
    // Problems:
    // Can I just mock signing?
    // Can I assume that node know total number of nodes?
    // Can I assume that honest nodes know each other ids?
    // What about message echo?
    // Should mark in code what is missing. 
    // Can block contents be dummy? 
    // Should I provide all the necessary things to run the code on your side?
    // Print event log from the program and add to the documentation
    public class NodeScript
    {
        // TODO: change int to unsigned
        public int id;
        public Dictionary<int, Block> blockDictionary = new Dictionary<int, Block>();
        private HashSet<int> notarized = new HashSet<int>();
        private HashSet<int> final = new HashSet<int>();
        int LNCH; // longestNotarizedChainHeadHash
                  // TODO: add typealias for int -> BLockHash
        private Dictionary<int, int> notarizedDistanceFromGenesis = new Dictionary<int, int>();
        // Voting for block hashes.
        private Dictionary<int, int> votes = new Dictionary<int, int>();
        private int N;
        public const int p = 107;
        private long lastTime;
        // TODO: verify
        public int GetEpoch(long time)
        {
            return (int)time/1000;
        }
        public int GetLeader(long time)
        {
            int epoch = GetEpoch(time);
            return (epoch * p) % N;
        }
        // TODO: move to constructor
        public NodeScript(int id, int N)
        {
            // TODO: block zero should have parent hash null
            this.N = N;
            this.id = id;
            var b0 = new Block(0, -1, "");
            var b0Hash = b0.GetHash();
            blockDictionary.Add(b0Hash, b0);
            notarized.Add(b0Hash);
            final.Add(b0Hash);
            LNCH = b0Hash;
            notarizedDistanceFromGenesis.Add(b0Hash, 0);
        }
        public int GetId()
        {
            return id;
        }

        private void FinalizePrefix(int hash)
        {
            while (hash != -1)
            {
                if (final.Contains(hash))
                {
                    return;
                }
                final.Add(hash);
                Console.WriteLine("Node " + id + " finalized block of epoch " + blockDictionary[hash].epoch + "!");
                hash = blockDictionary[hash].parentHash;
            }
        }
        public List<Message> OnMessageReceived(Message msg)
        {
            List<Message> result = new List<Message>();
            if (msg.GetType() == Message.Type.EpochAnnouncment)
            {
                // This should be verified by signing protocol
                if (msg.GetSender() == GetLeader(lastTime))
                {
                    var block = msg.GetBlock();
                    // TODO: remove this check (should be unnecessary)
                    if (block.epoch == GetEpoch(lastTime))
                    {
                        var hash = block.GetHash();
                        if (!blockDictionary.ContainsKey(hash))
                        {
                            blockDictionary.Add(hash, block);
                            //Debug.Log("LNCH" + LNCH);
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
                    //Debug.Log("Node " + id + " received vote nr " + votes[hash] + " for block " + block.epoch);
                    // TODO: verify exact number
                    if (votes[hash] >= 2 / 3 * N + 1)
                    {
                        notarized.Add(hash);
                        int parentDistance = notarizedDistanceFromGenesis[block.parentHash];
                        notarizedDistanceFromGenesis.Add(hash, parentDistance + 1);

                        if (notarizedDistanceFromGenesis[LNCH] < parentDistance + 1)
                        {
                            LNCH = hash;
                        }
                        int parent2Hash = blockDictionary[block.parentHash].parentHash;
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
        // TODO: correct function order
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
