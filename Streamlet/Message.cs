using System;
using System.Collections.Generic;
using System.Text;

namespace Streamlet
{
    public class Message
    {
        public enum Type { EpochAnnouncment, Vote };
        private int senderId;
        private int recipientId;
        private Block block;
        private Type type;
        public Message(Type type, int senderId, int recipientId, Block block)
        {
            this.type = type;
            this.senderId = senderId;
            this.recipientId = recipientId;
            this.block = block;
        }
        public int GetRecipientId()
        {
            return recipientId;
        }
        public Type GetType()
        {
            return type;
        }
        public int GetSender()
        {
            return senderId;
        }
        public Block GetBlock()
        {
            return block;
        }
    }

}
