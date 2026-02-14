# Azure Service Bus and Pub/Sub Pattern Tutorial

## Overview

Azure Service Bus is a fully managed enterprise message broker that allows applications and services to communicate with each other using messages. It supports both **message queues** and **publish/subscribe (pub/sub)** messaging patterns, enabling reliable and asynchronous communication between distributed systems.

### What is the Pub/Sub Pattern?

The **Publish/Subscribe (Pub/Sub)** pattern is a messaging pattern where senders (publishers) send messages without knowing the recipients (subscribers). Subscribers express interest in specific types of messages, and the messaging system ensures that only the relevant subscribers receive those messages. This decouples the producers and consumers, making the system more scalable and flexible.

![Pub/Sub Pattern](./assets/pub-sub-pattern.png)

---

## When to Use Azure Service Bus Queues

Azure Service Bus queues are ideal for scenarios where:

- **Point-to-point communication** is required (one sender, one receiver).
- Messages need to be processed in a **first-in, first-out (FIFO)** order.
- You need to decouple producers and consumers to improve scalability and reliability.
- You want to ensure **at-least-once delivery** of messages.

### Example Use Cases for Queues:

- Order processing systems.
- Task scheduling and background job processing.
- Decoupling microservices in a distributed architecture.

![Azure Service Bus Queue](./assets/message-queue.png)

---

## When to Use Azure Service Bus Topics and Subscriptions

Azure Service Bus topics and subscriptions are ideal for scenarios where:

- **Publish/Subscribe communication** is required (one sender, multiple receivers).
- Multiple subscribers need to receive the same message.
- You need to filter messages for specific subscribers using **rules and filters**.

### Example Use Cases for Topics and Subscriptions:

- Event-driven architectures.
- Real-time notifications to multiple systems or users.
- Broadcasting updates to multiple services or applications.

![Azure Service Bus Topics and Subscriptions](./assets/topics-subscriptions.png)

---

## How to Create a Service Bus Namespace in Azure

1. Log in to the [Azure Portal](https://portal.azure.com).
2. Search for **Service Bus** in the search bar and select **Service Bus**.
3. Click on **+ Create**.
4. Fill in the required details:
   - **Subscription**: Select your Azure subscription.
   - **Resource Group**: Choose an existing resource group or create a new one.
   - **Namespace Name**: Enter a unique name for your Service Bus namespace.
   - **Pricing Tier**: Choose the appropriate pricing tier (**Basic**, **Standard**, or **Premium**). **Note:** You need at least **Standard** to use topics.
   - **Region**: Select the Azure region where the namespace will be created.
5. Click **Review + Create** and then **Create**.

---

## How to Create a Queue

1. Navigate to your Service Bus namespace in the Azure Portal.
2. Under **Entities**, select **Queues**.
3. Click on **+ Queue**.
4. Enter a name for your queue.
5. Configure additional settings such as **Max Size**, **Message Time to Live (TTL)**, and **Duplicate Detection** as needed.
6. Click **Create**.

---

## How to Create a Topic

1. Navigate to your Service Bus namespace in the Azure Portal.
2. Under **Entities**, select **Topics**.
3. Click on **+ Topic**.
4. Enter a name for your topic.
5. Configure additional settings such as **Max Size**, **Message Time to Live (TTL)**, and **Duplicate Detection** as needed.
6. **Note:** You need at least **Standard** pricing tier to use topics.
7. Click **Create**.

---

## How to Send and Receive Messages from Queues

### Sending Messages to a Queue

1. Install the Azure.Messaging.ServiceBus NuGet package in your project:
   ```bash
   dotnet add package Azure.Messaging.ServiceBus
   ```
2. Use the following code to send a message to a queue:

   ```csharp
   // File: Console.ServiceBusSender.Demo/Program.cs
   using Azure.Messaging.ServiceBus;

   const string serviceBusConnectionString = "<Your Service Bus Connection String>";
   const string queueName = "<Your Queue Name>";
   const int maxNumberOfMessages = 3;

   ServiceBusClient client;
   ServiceBusSender sender;

   client = new ServiceBusClient(serviceBusConnectionString);
   sender = client.CreateSender(queueName);

   using ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync();

   for (int i = 0; i <= maxNumberOfMessages; i++)
   {
       if (!batch.TryAddMessage(new ServiceBusMessage($"This is a message - {i}")))
       {
           Console.WriteLine($"Message {i} was not added to the batch");
       }
   }

   try
   {
       await sender.SendMessagesAsync(batch);
       Console.WriteLine("Messages sent");
   }
   catch (Exception ex)
   {
       Console.WriteLine($"Exception: {ex.Message}");
   }
   finally
   {
       await sender.DisposeAsync();
       await client.DisposeAsync();
   }
   ```

### Receiving Messages from a Queue

1. Use the following code to receive messages from a queue:

   ```csharp
   // File: Console.ServiceBusConsumer.Demo/Program.cs
   using Azure.Messaging.ServiceBus;

   const string serviceBusConnectionString = "<Your Service Bus Connection String>";
   const string queueName = "<Your Queue Name>";

   ServiceBusClient client;
   ServiceBusProcessor processor = default!; // Using 'default!' to avoid the warning of uninitialized variable

   async Task MessageHandler(ProcessMessageEventArgs args)
   {
       string body = args.Message.Body.ToString();
       Console.WriteLine($"Received: {body}");
       await args.CompleteMessageAsync(args.Message);
   }

   Task ErrorHandler(ProcessErrorEventArgs args)
   {
       Console.WriteLine($"Exception: {args.Exception.Message}");
       return Task.CompletedTask;
   }

   client = new ServiceBusClient(serviceBusConnectionString);
   processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

   try
   {
       processor.ProcessMessageAsync += MessageHandler;
       processor.ProcessErrorAsync += ErrorHandler;

       await processor.StartProcessingAsync();
       Console.WriteLine("Press any key to end the processing");
       Console.ReadKey();

       Console.WriteLine("Stopping the receiver...");
       await processor.StopProcessingAsync();
       Console.WriteLine("Receiver stopped");
   }
   catch (Exception ex)
   {
       Console.WriteLine($"Consumer Exception: {ex.Message}");
   }
   finally
   {
       await processor.DisposeAsync();
       await client.DisposeAsync();
   }
   ```

---

## How to Use Topics and Subscriptions (Pub/Sub Pattern)

### Sending Messages to a Topic

1. Install the Azure.Messaging.ServiceBus NuGet package in your project:
   ```bash
   dotnet add package Azure.Messaging.ServiceBus
   ```
2. Use the following code to send a message to a topic:

   ```csharp
   // File: Console.ServiceBusSenderWithTopics.Demo/Program.cs
   using Azure.Messaging.ServiceBus;

   const string serviceBusConnectionString = "<Your Service Bus Connection String>";
   const string topicName = "<Your Topic Name>";
   const int maxNumberOfMessages = 3;

   ServiceBusClient client;
   ServiceBusSender sender;

   client = new ServiceBusClient(serviceBusConnectionString);
   sender = client.CreateSender(topicName);

   using ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync();

   for (int i = 0; i <= maxNumberOfMessages; i++)
   {
       if (!batch.TryAddMessage(new ServiceBusMessage($"This is a message - {i}")))
       {
           Console.WriteLine($"Message {i} was not added to the batch");
       }
   }

   try
   {
       await sender.SendMessagesAsync(batch);
       Console.WriteLine("Messages sent");
   }
   catch (Exception ex)
   {
       Console.WriteLine($"Exception: {ex.Message}");
   }
   finally
   {
       await sender.DisposeAsync();
       await client.DisposeAsync();
   }
   ```

### Receiving Messages from a Subscription

1. Use the following code to receive messages from a subscription:

   ```csharp
   // File: Console.ServiceBusConsumerSubscription.Demo/Program.cs
   using Azure.Messaging.ServiceBus;

   const string serviceBusConnectionString = "<Your Service Bus Connection String>";
   const string topicName = "<Your Topic Name>";
   const string subscriptionName = "<Your Subscription Name>";

   ServiceBusClient client;
   ServiceBusProcessor processor = default!; // Using 'default!' to avoid the warning of uninitialized variable

   async Task MessageHandler(ProcessMessageEventArgs args)
   {
       string body = args.Message.Body.ToString();
       Console.WriteLine($"Received - Subscription: {subscriptionName} : {body}");
       await args.CompleteMessageAsync(args.Message);
   }

   Task ErrorHandler(ProcessErrorEventArgs args)
   {
       Console.WriteLine($"Exception: {args.Exception.Message}");
       return Task.CompletedTask;
   }

   client = new ServiceBusClient(serviceBusConnectionString);
   processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());

   try
   {
       processor.ProcessMessageAsync += MessageHandler;
       processor.ProcessErrorAsync += ErrorHandler;

       await processor.StartProcessingAsync();
       Console.WriteLine("Press any key to end the processing");
       Console.ReadKey();

       Console.WriteLine("Stopping the receiver...");
       await processor.StopProcessingAsync();
       Console.WriteLine("Receiver stopped");
   }
   catch (Exception ex)
   {
       Console.WriteLine($"Consumer Exception: {ex.Message}");
   }
   finally
   {
       await processor.DisposeAsync();
       await client.DisposeAsync();
   }
   ```

---

## Additional Resources

- [Azure Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Azure Service Bus Pricing](https://azure.microsoft.com/en-us/pricing/details/service-bus/)
- [Azure SDK for .NET](https://learn.microsoft.com/en-us/dotnet/azure/)

This tutorial provides a comprehensive guide to understanding and using Azure Service Bus for both queue-based and pub/sub messaging patterns. Happy coding!
