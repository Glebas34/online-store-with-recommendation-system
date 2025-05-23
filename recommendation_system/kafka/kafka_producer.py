from kafka import KafkaProducer
import json
from datetime import datetime
import random
import time
import os

KAFKA_TOPIC = os.getenv("KAFKA_TOPIC", "user_events")
KAFKA_BOOTSTRAP_SERVERS = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "localhost:9092")

producer = KafkaProducer(
    bootstrap_servers=KAFKA_BOOTSTRAP_SERVERS,
    value_serializer=lambda v: json.dumps(v).encode("utf-8")
)

def send_event(user_id, item_id, event_type):
    event = {
        "user_id": user_id,
        "item_id": item_id,
        "event_type": event_type,
        "timestamp": datetime.utcnow().isoformat()
    }
    producer.send(KAFKA_TOPIC, value=event)
    print(f"📤 Отправлено событие: {event}")

# 📦 Пример эмуляции
if __name__ == "__main__":
    users = [f"user_{i}" for i in range(1, 11)]
    items = [f"item_{i}" for i in range(100, 200)]
    event_types = ["view", "click", "purchase"]

    while True:
        user = random.choice(users)
        item = random.choice(items)
        event = random.choices(event_types, weights=[0.7, 0.2, 0.1])[0]
        send_event(user, item, event)
        time.sleep(1)
