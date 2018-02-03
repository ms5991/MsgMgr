using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgMgr.Core;
using System.Collections.Concurrent;

namespace MsgMgr.Receivers
{
    public class QueuedMessageReceiver : IMessageReceiver
    {
        private ConcurrentQueue<MessageBase> _receivedQueue = new ConcurrentQueue<MessageBase>();        

        /// <summary>
        /// Accepts a received message.  Enqueues the message so that the processing thread can dequeue it.
        /// </summary>
        /// <param name="message">The message.</param>
        public void AcceptReceivedMessage(MessageBase message)
        {
            // this allows other threads to access messages in the order they were received
            // this is better than an event based notification system (though that is supported)
            // because it's not "fire and forget"
            _receivedQueue.Enqueue(message);
        }
        
        /// <summary>
        /// Tries to get a message from the queue.  If queue is empty, return false.
        /// </summary>
        /// <param name="toProcess">Message to process.</param>
        /// <returns>True if there is a message, false otherwise</returns>
        public bool TryGetMessage(out MessageBase toProcess)
        {
            return _receivedQueue.TryDequeue(out toProcess);
        }
    }
}
