using MsgMgr.Connections;
using MsgMgr.Core;
using MsgMgr.Messages;
using MsgMgr.Utilities;
using System;
using System.Threading;

namespace MessageManagerClient
{
    public class Program_Client
    {
        public static volatile bool managing = true;

        public static void Main(string[] args)
        {
            Logger.Init(true, LogPriority.LOW, LogMode.PRIORITY_OR_CATEGORY, LogCategory.ALL);

            bool managing = true;

            MessageManager manager = new MessageManager();
            manager.ManagingStopped += Manager_ManagingStopped;
            manager.StartManaging(new TcpClient("127.0.0.1", 8888));
            int i = 0;

            while(managing)
            {
                MessageBase m = new StringMessage("Hello {0}".FormatStr(i++));
                manager.EnqueueMessage(m);

                Thread.Sleep(1);
            }
            Logger.Instance.LogMessage("Ready to cancel on key hit, generated: " + i + " messages", LogPriority.CRITICAL, LogCategory.INFO);
            

            manager.StopManaging("Called in client");
            Console.ReadLine();
        }

        private static void Manager_ManagingStopped(MessageManagerManagingStoppedEventArgs e)
        {
            managing = false;
        }
    }
}
