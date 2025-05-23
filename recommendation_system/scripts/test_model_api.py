import pandas as pd
import requests
from tqdm import tqdm

# Настройки
CSV_PATH = "data/processed/user_product_train.csv"
API_URL = "http://localhost:8001/recommend"  # URL model_server
TOP_K = 5

def test_model_api():
    # Загружаем данные
    df = pd.read_csv(CSV_PATH)

    if "user_id" not in df.columns:
        raise ValueError("В CSV не найден столбец user_id")

    user_ids = df["user_id"].unique()

    print(f"🧪 Тестируем модель на {len(user_ids)} пользователях...")

    success = 0
    fail = 0
    failed_users = []

    for user_id in tqdm(user_ids):
        url = f"{API_URL}/{user_id}?top_k={TOP_K}"
        try:
            response = requests.get(url, timeout=5)
            if response.status_code == 200:
                data = response.json()
                if "recommendations" in data:
                    success += 1
                else:
                    fail += 1
                    failed_users.append(user_id)
            else:
                fail += 1
                failed_users.append(user_id)
        except Exception as e:
            print(f"❌ Ошибка для пользователя {user_id}: {e}")
            fail += 1
            failed_users.append(user_id)

    print("\n✅ Успешно обработано:", success)
    print("❌ Ошибок:", fail)

    if failed_users:
        print("\n📋 Пользователи с ошибкой:")
        for uid in failed_users[:10]:  # Покажем только первых 10
            print("-", uid)

if __name__ == "__main__":
    test_model_api()
