using System;
using System.Collections.Generic;
using System.Text;

namespace Streamlet
{
    using BlockHash = System.Int32;
    public struct Block
    {
        public int epoch;
        public BlockHash parentHash;
        public string content;
        public Block(int epoch, BlockHash parentHash, string content)
        {
            this.epoch = epoch;
            this.parentHash = parentHash;
            this.content = content;
        }
        public BlockHash GetHash()
        {
            int result = 13;
            result = result * 31 + epoch;
            result = result * 31 + parentHash;
            result = result * 31 + content.GetHashCode();
            return result;
        }
    }
}
