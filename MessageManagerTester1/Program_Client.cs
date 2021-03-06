﻿using MsgMgr.Connections;
using MsgMgr.Core;
using MsgMgr.Messages;
using MsgMgr.Receivers;
using MsgMgr.Serialization;
using MsgMgrCommon.Extensions;
using MsgMgrCommon.Logging;
using System;
using System.Threading;

namespace MessageManagerClient
{
    public class Program_Client
    {
        public static volatile bool managing = true;

        public static void Main(string[] args)
        {
            Logger.Init(true, LogPriority.LOW, LogMode.PRIORITY_OR_CATEGORY, LogCategory.ALL, LogCategory.VERBOSE);

            bool managing = true;

            MessageReceiver receiver = new QueuedMessageReceiver(false);

            MessageManager manager = new MessageManager(receiver, SerializationType.XML);
            manager.ManagingStopped += Manager_ManagingStopped;
            manager.StartManaging(new TcpClient("127.0.0.1", 8888));
            int i = 0;

            while(managing && i < 10)
            {
                MessageBase m = new StringMessage("Hello {0}".FormatStr(i++));
                manager.EnqueueMessage(m);

                Thread.Sleep(10);
            }
            Logger.Instance.LogMessage("Ready to cancel on key hit, generated: " + i + " messages", LogPriority.CRITICAL, LogCategory.INFO);

            Console.ReadLine();

            manager.StopManaging("Called in client");
            Console.ReadLine();
        }

        private static void Manager_ManagingStopped(MessageManagerManagingStoppedEventArgs e)
        {
            managing = false;
        }
    }
}
