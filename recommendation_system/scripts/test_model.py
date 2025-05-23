import pickle
import pandas as pd
import numpy as np
from sklearn.metrics import recall_score
from tqdm import tqdm

MODEL_PATH = "model/svd_model.pkl"
TEST_PATH = "data/processed/user_product_test.csv"
TOP_K = 10

def load_model(path):
    with open(path, "rb") as f:
        model_data = pickle.load(f)
    return model_data

def evaluate(model_data, test_df, top_k=TOP_K):
    svd = model_data["svd"]
    user_encoder = model_data["user_encoder"]
    item_encoder = model_data["item_encoder"]
    matrix = model_data["user_item_matrix"]

    item_vectors = svd.components_.T
    item_decoder = {i: item for item, i in zip(item_encoder.classes_, range(len(item_encoder.classes_)))}

    hit_count = 0
    total_users = 0

    # Только те пользователи, которые есть в обучении
    valid_users = test_df["user_id"].isin(user_encoder.classes_)
    test_df = test_df[valid_users]

    print(f"👥 Пользователей для теста: {test_df['user_id'].nunique()}")

    for user_id, group in tqdm(test_df.groupby("user_id")):
        try:
            user_idx = user_encoder.transform([user_id])[0]
        except ValueError:
            print("ValueError")
            continue

        # уже взаимодействовал
        seen_items = matrix[user_idx].nonzero()[1]

        # вектор пользователя
        user_vector = svd.transform(matrix[user_idx])

        scores = np.dot(item_vectors, user_vector.T).flatten()
        scores[seen_items] = -np.inf

        top_k_indices = np.argsort(scores)[::-1][:top_k]
        top_k_items = [item_decoder[i] for i in top_k_indices]

        # Есть ли в top_k товар из ground-truth?
        actual = set(group["product_id"])
        if any(item in actual for item in top_k_items):
            hit_count += 1
        total_users += 1

    recall_at_k = hit_count / total_users if total_users > 0 else 0.0
    return recall_at_k

def main():
    print("🔁 Загрузка модели и тестовых данных...")
    model_data = load_model(MODEL_PATH)
    test_df = pd.read_csv(TEST_PATH)

    print(f"📊 Тестируем модель на {len(test_df)} событиях...")
    recall = evaluate(model_data, test_df, top_k=TOP_K)

    print(f"\n✅ Recall@{TOP_K}: {recall:.4f}")

if __name__ == "__main__":
    main()
