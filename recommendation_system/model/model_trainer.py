import os
import pickle
import numpy as np
import pandas as pd
from sklearn.decomposition import TruncatedSVD
from sklearn.preprocessing import LabelEncoder
from scipy.sparse import csr_matrix
from dotenv import load_dotenv

from db.db import AsyncSessionLocal
from sqlalchemy import text
import asyncio

load_dotenv()

MODEL_PATH = "model/svd_model.pkl"

async def load_data():
    """Асинхронно загружаем данные из базы данных. Если пусто — из CSV."""
    async with AsyncSessionLocal() as session:
        try:
            result = await session.execute(text("""
                SELECT user_id, book_id, rating FROM user_ratings
            """))
            rows = result.fetchall()

            if rows:
                print(f"Загружено {len(rows)} записей из базы данных.")
                user_ids = [row[0] for row in rows]
                book_ids = [row[1] for row in rows]
                ratings = [row[2] for row in rows]

                return pd.DataFrame({
                    "user_id": user_ids,
                    "book_id": book_ids,
                    "rating": ratings
                })
            else:
                print("База пуста. Загружаем из CSV...")
                csv_path = "data/processed/train_ratings.csv"
                if not os.path.exists(csv_path):
                    raise FileNotFoundError(f"CSV-файл не найден: {csv_path}")

                df = pd.read_csv(csv_path)
                print(f"Загружено {len(df)} записей из CSV.")
                return df

        except Exception as e:
            print(f"Ошибка при загрузке данных: {e}")
            return None

async def train_model():
    """Основная функция обучения модели."""
    df = await load_data()

    if df is None or df.empty:
        print("Обучение не выполнено: нет данных.")
        return

    print("Начинаем обучение модели...")

    user_encoder = LabelEncoder()
    item_encoder = LabelEncoder()

    user_ids = user_encoder.fit_transform(df["user_id"].astype(str))
    book_ids = item_encoder.fit_transform(df["book_id"].astype(str))
    ratings = df["rating"].values

    user_item_matrix = csr_matrix((ratings, (user_ids, book_ids)))

    svd = TruncatedSVD(n_components=50, random_state=42)
    svd.fit(user_item_matrix)

    model_data = {
        "svd": svd,
        "user_encoder": user_encoder,
        "item_encoder": item_encoder,
        "user_item_matrix": user_item_matrix
    }

    os.makedirs(os.path.dirname(MODEL_PATH), exist_ok=True)

    with open(MODEL_PATH, "wb") as f:
        pickle.dump(model_data, f)

    print("Модель успешно обучена и сохранена.")

if __name__ == "__main__":
    asyncio.run(train_model())
