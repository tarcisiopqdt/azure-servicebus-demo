// 1- dotnet new console -n Console.ServiceBusSender.Demo
// 2 - dotnet add package Azure.Messaging.ServiceBus
using Azure.Messaging.ServiceBus;

const string serviceBusConnectionString = "<Your Service Bus Connection String>";
const string queueName = "<Your Queue Name>";
const int maxNumberOfMessages = 3;

ServiceBusClient client;
ServiceBusSender sender;

client = new ServiceBusClient(serviceBusConnectionString);
sender = client.CreateSender(queueName);

using ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync();

for(int i = 0; i <= maxNumberOfMessages; i++){
  if(!batch.TryAddMessage(new ServiceBusMessage($"This a message - {i}"))){
    Console.WriteLine($"Message {i} was not added to the batch");
  }
}
try{
  await sender.SendMessagesAsync(batch);
  Console.WriteLine("Messages sent");
 
}
catch (Exception ex)
{
  Console.WriteLine($"Expection {ex.Message}");
}
finally
{
 await sender.DisposeAsync();
 await client.DisposeAsync();
}