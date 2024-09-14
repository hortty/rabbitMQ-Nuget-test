namespace RabbitMQMessaging.Interfaces;

public interface IMessagingService
{
	void Publish<T>(T message, string exchange, string routingKey);
	void Subscribe<T>(string exchangeName, string queueName, Action<T> onMessageReceived);
	void DeclareExchangeAndQueue(string exchange, string queue);
}
