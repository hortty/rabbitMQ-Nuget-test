using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQMessaging.Interfaces;
using System;
using System.Net;

namespace RabbitMQMessaging.Services
{
	public class RabbitMQMessagingService : IMessagingService
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private int _maxTry = 0;
		private int _retryTime = 0;

		public RabbitMQMessagingService(
			string hostname, 
			int port, 
			string username, 
			string password,
			int maxTry = 3, 
			int retryTime = 1500)
		{
			_maxTry = maxTry;
			_retryTime = retryTime;

			var factory = new ConnectionFactory()
			{
				HostName = hostname,
				Port = port,
				UserName = username,
				Password = password
			};

			_connection = factory.CreateConnection();
			_channel = _connection.CreateModel();
		}

		public void DeclareExchangeAndQueue(string exchange, string queue)
		{
			_channel.ExchangeDeclare (
				exchange: exchange, 
				type: ExchangeType.Direct
			);

			_channel.QueueDeclare (
				queue: queue,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_channel.QueueBind (
				queue: queue,
				exchange: exchange,
				routingKey: queue
			);
		}

		public void Publish<T>(T msg, string exchangeName, string queueName)
		{
			if (msg is null || msg.ToString() is null)
				throw new ArgumentNullException(nameof(msg), "Cannot publish null messages");

			var jsonMessage = JsonConvert.SerializeObject(msg);
    		var body = System.Text.Encoding.UTF8.GetBytes(jsonMessage);

			RetryPolicy(() =>
				{
					_channel.BasicPublish(
						exchange: exchangeName,
						routingKey: queueName,
						basicProperties: null,
						body: body
					);
			});
		}

		public void Subscribe<T>(string exchangeName, string queueName, Action<T> onMessageReceived)
		{
			DeclareExchangeAndQueue(exchangeName, queueName);

			var consumer = new EventingBasicConsumer(_channel);

			consumer.Received += (model, args) =>
			{
				T? message = default(T);
				var body = args.Body.ToArray();

				var jsonString = System.Text.Encoding.UTF8.GetString(body); 
        
				try
				{
					message = JsonConvert.DeserializeObject<T>(jsonString);
				}
				catch (JsonSerializationException ex)
				{
					Console.WriteLine($"Deserialization failed: {ex.Message}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}

				if(message is null)
				{
					Console.WriteLine($"Failed to deserialize message: {jsonString}");
					return;
				}

				RetryPolicy( () => 
					onMessageReceived(message)
				);
				
			};

			_channel.BasicConsume (
				queue: queueName,
				autoAck: true,
				consumer: consumer
			);
		}

		private void RetryPolicy(Action action)
		{
			int attempt = 0;

			while (attempt < _maxTry)
			{
				try
				{
					action();
					break;
				}
				catch (Exception ex)
				{
					attempt++;

					if (attempt >= _maxTry)
					{
						throw new Exception($"Max retry attempts reached: {ex.Message}", ex);
					}

					Console.WriteLine($"Error {attempt}: {ex.Message}. Retrying in {_retryTime}ms");

					Thread.Sleep(_retryTime);
				}
			}
		}

	}
}