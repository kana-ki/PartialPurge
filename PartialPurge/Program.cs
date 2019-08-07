using System;
using System.Messaging;

namespace PartialPurge
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Incorrect number of arguments.\nArgument 1: expected string - the name of the queue to partially purge .e.g. \".\\private$\\TestQueue\"\nArgument 2: expected number - the number of most recent messages to keep.\n\nExample:\n.\\partialpurge .\\private$\\TestQueue 100");
                return 1;
            }

            var queueName = args[0];
            if (!MessageQueue.Exists(queueName))
            {
                Console.Error.WriteLine("Error: argument 1: a queue with that name does not exist.");
                return 2;
            }

            long toKeep;
            if (!long.TryParse(args[1], out toKeep))
            {
                Console.Error.WriteLine("Error: argument 2: expected number, got {0}.", args[1]);
                return 3;
            }

            var queue = new MessageQueue(args[0]);

            var totalInQueue = queue.GetAllMessages().LongLength;
            var toDelete = totalInQueue - toKeep;

            var cursor = queue.CreateCursor();

            for (var i = 0; i < toKeep; i++)
            {
                _ = queue.Peek(TimeSpan.FromMilliseconds(1), cursor, PeekAction.Current);
            }

            var messageAvailable = true;
            while (messageAvailable && toDelete > 0)
            try
            {
                _ = queue.Receive(TimeSpan.FromMilliseconds(1), cursor);
                toDelete--;
            } catch (MessageQueueException)
            {
                messageAvailable = false;
            }

            return 0;
        }
    }
}
