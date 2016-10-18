﻿using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var actor = new RpcActor();

            try
            {
                actor.Bootup();

                var helloService = new HelloService(actor);
                var orderService = new OrderService(actor);

                actor.RegisterRpcService(helloService);
                actor.RegisterRpcService(orderService);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant();
                    if (text == "quit" || text == "exit")
                    {
                        break;
                    }
                    else
                    {
                        int times = 0;
                        if (int.TryParse(text, out times))
                        {
                            for (int i = 0; i < times; i++)
                            {
                                NotifyOrderChanged(log, actor);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }

            actor.Shutdown();
        }

        private static void NotifyOrderChanged(ILog log, RpcActor actor)
        {
            var notification = new ActorMessageEnvelope<OrderStatusChangedNotification>()
            {
                Message = new OrderStatusChangedNotification()
                {
                    OrderID = Guid.NewGuid().ToString(),
                    OrderStatus = 1,
                },
            };

            log.DebugFormat("NotifyOrderChanged, notify order changed with MessageID[{0}].",
                notification.MessageID);
            actor.BeginSend("client", notification);
        }
    }
}
