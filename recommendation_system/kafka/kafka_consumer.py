import asyncio
from aiokafka import AIOKafkaConsumer
from sqlalchemy import text
from db import AsyncSessionLocal
import json
import os
from dotenv import load_dotenv
from model_updater import update_model_batch

load_dotenv()

KAFKA_TOPIC = os.getenv("KAFKA_TOPIC")
KAFKA_BOOTSTRAP_SERVERS = os.getenv("KAFKA_BOOTSTRAP_SERVERS")

event_buffer = []
EVENT_THRESHOLD = 100

async def process_event(event):
    async with AsyncSessionLocal() as session:
        try:
            await session.execute(text("""
                INSERT INTO user_ratings (user_id, item_id, rating)
                VALUES (:user_id, :item_id, :rating)
            """), {
                "user_id": event["user_id"],
                "item_id": event["book_id"],
                "rating": event["rating"]
            })
            await session.commit()
            print("Сохранено в БД.")
        except Exception as e:
            print("Ошибка при записи в БД:", e)

async def consume():
    consumer = AIOKafkaConsumer(
        KAFKA_TOPIC,
        bootstrap_servers=KAFKA_BOOTSTRAP_SERVERS,
        value_deserializer=lambda x: json.loads(x.decode("utf-8")),
        auto_offset_reset="earliest",
        enable_auto_commit=True,
        group_id="recommendation-group"
    )

    await consumer.start()
    print("Kafka Consumer (aiokafka) запущен...")
    try:
        async for message in consumer:
            event = message.value
            print(f"Получено событие: {event}")

            if event.get("user_id") and event.get("book_id") and event.get("rating") is not None:
                await process_event(event)

                event_buffer.append((event["user_id"], event["book_id"], event["rating"]))

                if len(event_buffer) >= EVENT_THRESHOLD:
                    try:
                        print(f"Обновляем модель на основе {len(event_buffer)} событий...")
                        update_model_batch(event_buffer)
                        event_buffer.clear()
                    except Exception as e:
                        print("Ошибка при обновлении модели:", e)
    finally:
        await consumer.stop()

if __name__ == "__main__":
    asyncio.run(consume())
