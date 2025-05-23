import pickle
import numpy as np
from sklearn.decomposition import TruncatedSVD
from sklearn.preprocessing import LabelEncoder
from scipy.sparse import vstack, csr_matrix
import os

MODEL_PATH = "model/svd_model.pkl"

def update_model_batch(events):
    """
    Обновляет модель на основе новых событий:
    events = [(user_id, item_id, rating), ...]
    """
    if not os.path.exists(MODEL_PATH):
        print("⚠️ Модель не найдена. Невозможно дообучить.")
        return

    with open(MODEL_PATH, "rb") as f:
        model_data = pickle.load(f)

    svd = model_data["svd"]
    user_encoder = model_data["user_encoder"]
    book_encoder = model_data["book_encoder"]
    matrix = model_data["user_item_matrix"]

    # Расширяем классы при необходимости
    new_users = set()
    new_books = set()
    for user_id, book_id, _ in events:
        if user_id not in user_encoder.classes_:
            new_users.add(user_id)
        if book_id not in book_encoder.classes_:
            new_books.add(book_id)

    if new_users:
        user_encoder.classes_ = np.append(user_encoder.classes_, list(new_users))
    if new_books:
        book_encoder.classes_ = np.append(book_encoder.classes_, list(new_books))

    # Строим новую матрицу
    rows, cols, data = [], [], []
    for user_id, book_id, rating in events:
        try:
            user_idx = user_encoder.transform([user_id])[0]
            book_idx = book_encoder.transform([book_id])[0]
        except Exception as e:
            print(f"❌ Ошибка трансформации: {e}")
            continue

        rows.append(user_idx)
        cols.append(book_idx)
        data.append(float(rating))

    shape = (len(user_encoder.classes_), len(book_encoder.classes_))
    new_matrix = csr_matrix((data, (rows, cols)), shape=shape)

    # Объединяем с текущей матрицей
    full_matrix = vstack([matrix, new_matrix])
    svd.fit(full_matrix)

    # Сохраняем обновлённую модель
    with open(MODEL_PATH, "wb") as f:
        pickle.dump({
            "svd": svd,
            "user_encoder": user_encoder,
            "book_encoder": book_encoder,
            "user_book_matrix": full_matrix
        }, f)

    print(f"✅ Модель обновлена на основе {len(events)} новых рейтингов.")
