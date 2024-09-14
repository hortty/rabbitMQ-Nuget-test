using Microsoft.Extensions.Configuration;
using RabbitMQMessaging.Interfaces;
using RabbitMQMessaging.Services;
using RabbitMQMessagingConsoleApp.Models;

namespace RabbitMQMessagingConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var configuration = LoadConfiguration();
			var rabbitMqConfig = configuration.GetSection("RabbitMQ");

			if(rabbitMqConfig is null)
			{
				throw new Exception("RabbitMQ configuration not found.");
			}

			int port = 0;
			string hostName = rabbitMqConfig["HostName"] ?? throw new Exception("Could not found hostName");
			string userName = rabbitMqConfig["UserName"] ?? "guest";
			string password = rabbitMqConfig["Password"] ?? "guest";
			string exchange = rabbitMqConfig["Exchange"] ?? throw new Exception("Could not found exchange");
			string queue = rabbitMqConfig["Queue"] ?? throw new Exception("Could not found queue");

			if(!int.TryParse(rabbitMqConfig["Port"], out port))
				throw new Exception("Could not found Port");

			IMessagingService rabbitMqService = new RabbitMQMessagingService(
				hostname: hostName,
				port: port,
				username: userName,
				password: password
			);

			rabbitMqService.DeclareExchangeAndQueue(exchange, queue);

			PublishMessage(rabbitMqService, exchange, queue);

			Console.WriteLine("Press enter to exit.");
			Console.ReadLine();
		}

		private static void PublishMessage(IMessagingService rabbitMqService, string exchange, string queue)
		{
			var message = new MsgBodyModel
			{
				Text = "MYTest"
			};

			rabbitMqService.Publish(message, exchange, queue);

			Console.WriteLine("Message Published!");
		}

		private static IConfiguration LoadConfiguration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

			return builder.Build();
		}
	}
}
