// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Enter the endpoint to connect to:");
var endpoint = Console.ReadLine();

Console.WriteLine("Enter your auth token");
var authToken = Console.ReadLine();

var connection = new HubConnectionBuilder()
    .WithUrl($"{endpoint}/notifications/orders?access_token={authToken}")
    .Build();

connection.Closed += async (error) =>
{
    await Task.Delay(new Random().Next(0,5) * 1000);
    await connection.StartAsync();
};
        
connection.On<string>("paymentSuccess", (orderIdentifier) =>
{
    Console.WriteLine($"Payment successful for order: {orderIdentifier}");
});
        
connection.On<string>("preparing", (orderIdentifier) =>
{
    Console.WriteLine($"The kitchen is preparing your order: {orderIdentifier}");
});
        
connection.On<string>("prepComplete", (orderIdentifier) =>
{
    Console.WriteLine($"The kitchen has finished preparing your order, it's in the oven: {orderIdentifier}");
});
        
connection.On<string>("bakeComplete", (orderIdentifier) =>
{
    Console.WriteLine($"It's out the oven, we just need to check it meets the quality you expect: {orderIdentifier}");
});
        
connection.On<string>("qualityCheckComplete", (orderIdentifier) =>
{
    Console.WriteLine($"Your order has been quality checked, it's nearly time: {orderIdentifier}");
});
        
connection.On<string>("driverAssigned", (orderIdentifier) =>
{
    Console.WriteLine($"A driver has been assigned to your order: {orderIdentifier}");
});
        
await connection.StartAsync();

Console.WriteLine("Connected");
Console.WriteLine("Press the X key to exit...");

// Keep going until somebody hits 'x'
while (true) {
    ConsoleKeyInfo ki = Console.ReadKey(true);
    if (ki.Key == ConsoleKey.X) {
        break;
    }
}