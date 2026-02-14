// 1- dotnet new console -n Console.ServiceBusConsumerSubscription.Demo
// 2 - dotnet add package Azure.Messaging.ServiceBus
using Azure.Messaging.ServiceBus;
using ProcessMessageEventArgs = Azure.Messaging.ServiceBus.ProcessMessageEventArgs;

const string serviceBusConnectionString = "<Your Service Bus Connection String>";
const string topicName = "<Your Topic Name>";
const string sub1Name = "<Your Subscription Name>";

ServiceBusClient client;
ServiceBusProcessor processor = default!; //using 'default!' to avoid the warning of uninitialized variable


async Task MessageHander(ProcessMessageEventArgs ProcessMessageEventArgs){
  string body = ProcessMessageEventArgs.Message.Body.ToString();
  Console.WriteLine($"Received - Subscription: {sub1Name} : {body}");
  await ProcessMessageEventArgs.CompleteMessageAsync(ProcessMessageEventArgs.Message);
}

Task ErrorHandler(ProcessErrorEventArgs processErrorEventArgs){
  Console.WriteLine($"Exception: {processErrorEventArgs.Exception.Message.ToString()}");
  return Task.CompletedTask;
}

client = new ServiceBusClient(serviceBusConnectionString);
processor = client.CreateProcessor(topicName,sub1Name, new ServiceBusProcessorOptions());

try{
  processor.ProcessMessageAsync += MessageHander; 
  processor.ProcessErrorAsync += ErrorHandler; 

  await processor.StartProcessingAsync(); 
  Console.WriteLine("Press any key to end the processing");
  Console.ReadKey();

  Console.WriteLine("Stopping the receiver...");
  await processor.StopProcessingAsync(); 
  Console.WriteLine("Receiver stopped");
}catch(Exception ex){
  Console.WriteLine($"Cosumer Exception: {ex.Message}");
}
finally{
  await processor.DisposeAsync();
  await client.DisposeAsync();
}