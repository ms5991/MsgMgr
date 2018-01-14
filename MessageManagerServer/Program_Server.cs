using MsgMgr.Connections;
using MsgMgr.Core;
using MsgMgr.Messages;
using MsgMgr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageManagerServer
{
    public class Program_Server
    {
        public static volatile int re = 0;

        public static void Main(string[] args)
        {
            
            Logger.Init(false,true, @"M:\Users\Michael\Documents\log.txt", LogPriority.LOW, LogMode.PRIORITY_OR_CATEGORY, LogCategory.ALL);

            MessageManager manager = new MessageManager();
            manager.StartManaging(new TcpServer("127.0.0.1", 8888));

            manager.MessageReceived += Manager_MessageReceived;

            
            Console.ReadLine();

            manager.StopManaging("Called in server");
            
            Console.ReadLine();

        }

        private static void Manager_MessageReceived(MessageManagerMessageReceivedEventArgs e)
        {
            StringMessage r = (StringMessage)e.Message;

            Logger.Instance.LogMessage(r.Message, LogPriority.HIGH, LogCategory.INFO);
        }
    }
}
