using System;
using System.Collections.Generic;
using System.Text;

namespace Streamlet
{
    public struct Block
    {
        public int epoch;
        public int parentHash;
        public string content;
        public Block(int epoch, int parentHash, string content)
        {
            this.epoch = epoch;
            this.parentHash = parentHash;
            this.content = content;
        }
        public int GetHash()
        {
            int result = 13;
            result = result * 31 + epoch;
            result = result * 31 + parentHash;
            result = result * 31 + content.GetHashCode();
            return result;
        }
    }
}
