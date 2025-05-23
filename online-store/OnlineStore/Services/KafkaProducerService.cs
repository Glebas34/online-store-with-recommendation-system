using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OnlineStore.Models;
using System.Text.Json;

namespace OnlineStore.Services;

public class KafkaProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IOptions<KafkaSettings> kafkaOptions, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        var config = kafkaOptions.Value;

        _topic = config.Topic ?? "user_events";

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendEventAsync((string, string, float) eventData)
    {
        try
        {
            var json = JsonSerializer.Serialize(eventData);
            var message = new Message<Null, string> { Value = json };

            var result = await _producer.ProduceAsync(_topic, message);
            _logger.LogInformation("📤 Kafka: событие отправлено в {Topic}, offset: {Offset}", _topic, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при отправке события в Kafka.");
        }
    }
}
