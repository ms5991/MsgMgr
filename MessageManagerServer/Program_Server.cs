using MsgMgr.Connections;
using MsgMgr.Core;
using MsgMgr.Messages;
using MsgMgr.Receivers;
using MsgMgrCommon.Logging;
using System;

namespace MessageManagerServer
{
    public class Program_Server
    {
        public static volatile int re = 0;

        public static volatile bool stopped = false;

        public static void Main(string[] args)
        {            
            Logger.Init(true, LogPriority.LOW, LogMode.PRIORITY_OR_CATEGORY, LogCategory.ALL, LogCategory.VERBOSE);

            QueuedMessageReceiver receiver = new QueuedMessageReceiver(false);
            
            MessageManager manager = new MessageManager(receiver);
            manager.StartManaging(new TcpServer("127.0.0.1", 8888));

            manager.ManagingStopped += Manager_ManagingStopped;

            while(!stopped)
            {
                MessageBase receivedMessage;
                if(receiver.TryGetMessage(out receivedMessage))
                {
                    StringMessage r = (StringMessage)receivedMessage;

                    Logger.Instance.LogMessage(r.Message + " at " + r.TimeReceived, LogPriority.HIGH, LogCategory.INFO);
                }
            }

            
            Console.ReadLine();

            manager.StopManaging("Called in server");
            
            Console.ReadLine();

        }

        private static void Receiver_MessageReceived(MessageManagerMessageReceivedEventArgs e)
        {
            StringMessage r = (StringMessage)e.Message;

            Logger.Instance.LogMessage(r.Message + " at " + r.TimeReceived, LogPriority.HIGH, LogCategory.INFO);
        }

        private static void Manager_ManagingStopped(MessageManagerManagingStoppedEventArgs e)
        {
            stopped = true;
        }
    }
}
