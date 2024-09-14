using RabbitMQMessaging.Interfaces;
using RabbitMQMessaging.Services;
using RabbitConsumerWorker.Models;

namespace RabbitConsumerWorker
{
    public class ConsumerWorker : BackgroundService
	{
		private readonly ILogger<ConsumerWorker> _logger;
		private IMessagingService rabbitMqService;
		private string _exchange = string.Empty;
		private string _queue = string.Empty;

		public ConsumerWorker(ILogger<ConsumerWorker> logger)
		{
			_logger = logger;
			InitializeRabbitMQ();
		}

		private void InitializeRabbitMQ()
		{
			var configuration = LoadConfiguration();
			var rabbitMqConfig = configuration.GetSection("RabbitMQ");

			if (rabbitMqConfig is null)
			{
				throw new Exception("RabbitMQ configuration not found.");
			}

			int port = 0;
			string hostName = rabbitMqConfig["HostName"] ?? throw new Exception("Could not found hostName");
			string userName = rabbitMqConfig["UserName"] ?? "guest";
			string password = rabbitMqConfig["Password"] ?? "guest";
			_exchange = rabbitMqConfig["Exchange"] ?? throw new Exception("Could not found exchange"); ;
			_queue = rabbitMqConfig["Queue"] ?? throw new Exception("Could not found queue");

			if (!int.TryParse(rabbitMqConfig["Port"], out port))
				throw new Exception("Could not found Port");

			rabbitMqService = new RabbitMQMessagingService(
				hostname: hostName,
				port: port,
				username: userName,
				password: password
			);

			rabbitMqService.DeclareExchangeAndQueue(_exchange, _queue);
		}

		private static IConfiguration LoadConfiguration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

			return builder.Build();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			stoppingToken.Register(() =>
				_logger.LogInformation("RabbitConsumerWorker is stopping."));

			StartSubscription(rabbitMqService: rabbitMqService, exchange: _exchange, queue: _queue);

			await Task.Delay(Timeout.Infinite, stoppingToken);
		}

		private static void StartSubscription(IMessagingService rabbitMqService, string exchange, string queue)
		{
			rabbitMqService.Subscribe(exchange, queue, (MsgBodyModel? body) =>
			{
				if (body != null)
				{
					Console.WriteLine($"Received Message: {body.Text}");
				}
				else
				{
					Console.WriteLine("Received Message: Invalid or unrecognized message format.");
				}
			});

			Console.WriteLine("RabbitConsumerWorker waiting for messages...");
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			return base.StopAsync(cancellationToken);
		}
	}
}
