// 1- dotnet new console -n Console.ServiceBusConsumer.Demo
// 2 - dotnet add package Azure.Messaging.ServiceBus
using Azure.Messaging.ServiceBus;

const string serviceBusConnectionString = "<Your Service Bus Connection String>";
const string queueName = "<Your Queue Name>";

ServiceBusClient client;
ServiceBusProcessor processor = default!; //using 'default!' to avoid the warning of uninitialized variable

//Message Handler to process the messages received from the Service Bus Queue
async Task MessageHander(ProcessMessageEventArgs ProcessMessageEventArgs){
  string body = ProcessMessageEventArgs.Message.Body.ToString();
  Console.WriteLine($"Received: {body}");
  await ProcessMessageEventArgs.CompleteMessageAsync(ProcessMessageEventArgs.Message);
}

//Error Handler to process any error that occurs while receiving messages from the Service Bus Queue
Task ErrorHandler(ProcessErrorEventArgs processErrorEventArgs){
  Console.WriteLine($"Exception: {processErrorEventArgs.Exception.Message.ToString()}");
  return Task.CompletedTask;
}

//Creating the Service Bus Client and Processor to receive messages from the Service Bus Queue
client = new ServiceBusClient(serviceBusConnectionString);
processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

try{
  processor.ProcessMessageAsync += MessageHander; //Registering the Message Handler
  processor.ProcessErrorAsync += ErrorHandler; //Registering the Error Handler

  await processor.StartProcessingAsync(); //Starting the Processor to receive messages from the Service Bus Queue
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