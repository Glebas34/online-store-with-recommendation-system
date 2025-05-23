# scripts/load_train_to_db.py
import os
import asyncio
import pandas as pd
from sqlalchemy import text
from db.db import AsyncSessionLocal
from dotenv import load_dotenv

load_dotenv()

CSV_PATH = "data/processed/train_ratings.csv"

async def load_data_to_db():
    if not os.path.exists(CSV_PATH):
        raise FileNotFoundError(f"❌ Файл не найден: {CSV_PATH}")

    print(f"📥 Загружаем данные из {CSV_PATH}...")
    df = pd.read_csv(CSV_PATH)

    print(f"🔄 Подключение к базе данных...")

    async with AsyncSessionLocal() as session:
        async with session.begin():
            for _, row in df.iterrows():
                try:
                    await session.execute(text("""
                        INSERT INTO user_ratings (user_id, book_id, rating)
                        VALUES (:user_id, :book_id, :rating)
                    """), {
                        "user_id": str(row["user_id"]),
                        "book_id": str(row["book_id"]),
                        "rating": row["rating"],
                    })
                    print(f"✅ Добавлена запись: user_id={row['user_id']}, book_id={row['book_id']}")
                except Exception as e:
                    print(f"❌ Ошибка при вставке записи: {e}")
        await session.commit()

    print("✅ Все записи успешно загружены в базу данных.")

if __name__ == "__main__":
    try:
        asyncio.run(load_data_to_db())
    except Exception as e:
        print(f"❌ Ошибка при загрузке данных: {e}")
