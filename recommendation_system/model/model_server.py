from fastapi import FastAPI, HTTPException
import pickle
from sqlalchemy import text
import numpy as np
import os
import asyncio
import subprocess
from fastapi.middleware.cors import CORSMiddleware
from db.db import AsyncSessionLocal
from scripts.create_tables import create_tables

MODEL_PATH = "model/svd_model.pkl"

app = FastAPI()

# Разрешить CORS для фронта
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

svd = None
user_encoder = None
item_encoder = None
user_item_matrix = None
item_decoder = None

async def load_model():
    """Пробует загрузить модель, если её нет — обучает с нуля."""
    if not os.path.exists(MODEL_PATH):
        print("⚠️ Модель не найдена. Запускаем обучение...")
        result = await asyncio.create_subprocess_exec(
            "python", "-m", "model.model_trainer",
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )
        stdout, stderr = await result.communicate()

        if result.returncode != 0:
            print(stderr.decode())
            raise RuntimeError("Ошибка при обучении модели.")
        else:
            print(stdout.decode())

    try:
        with open(MODEL_PATH, "rb") as f:
            model_data = pickle.load(f)
        print("✅ Модель успешно загружена.")
        return model_data
    except Exception as e:
        raise RuntimeError(f"❌ Не удалось загрузить модель: {e}")

@app.on_event("startup")
async def startup_event():
    global svd, user_encoder, item_encoder, user_item_matrix, item_decoder

    # 1. Создаём таблицы в БД
    await create_tables()

    # 2. Загружаем или обучаем модель
    model_data = await load_model()

    svd = model_data["svd"]
    user_encoder = model_data["user_encoder"]
    item_encoder = model_data["item_encoder"]
    user_item_matrix = model_data["user_item_matrix"]
    item_decoder = {i: item for item, i in zip(item_encoder.classes_, range(len(item_encoder.classes_)))}

@app.get("/recommend/{user_id}")
async def recommend(user_id: str, top_k: int = 5):
    try:
        user_idx = user_encoder.transform([user_id])[0]
    except ValueError:
        raise HTTPException(status_code=404, detail="Пользователь не найден")

    user_vector = svd.transform(user_item_matrix[user_idx])
    item_vectors = svd.components_.T
    scores = np.dot(item_vectors, user_vector.T).flatten()

    # Исключаем просмотренные
    interacted_items = user_item_matrix[user_idx].nonzero()[1]
    scores[interacted_items] = -np.inf

    top_indices = np.argsort(scores)[::-1][:top_k]
    top_items = [item_decoder[i] for i in top_indices]

    return {
        "user_id": str(user_id),
        "recommendations": [str(item) for item in top_items]
    }

@app.get("/popular")
async def popular(top_k: int = 10, min_ratings: int = 5):
    try:
        async with AsyncSessionLocal() as session:
            result = await session.execute(text(f"""
                SELECT book_id
                FROM user_ratings
                GROUP BY book_id
                HAVING COUNT(*) >= :min_ratings
                ORDER BY AVG(rating) DESC
                LIMIT :top_k
            """), {"min_ratings": min_ratings, "top_k": top_k})

            books = [row[0] for row in result.fetchall()]

        if not books:
            raise HTTPException(status_code=404, detail="Нет популярных книг.")

        return books

    except Exception as e:
        print(f"❌ Ошибка при вычислении популярных книг: {e}")
        raise HTTPException(status_code=500, detail="Ошибка при вычислении популярных книг.")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("model.model_server:app", host="0.0.0.0", port=8001, reload=True)
