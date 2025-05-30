import pandas as pd
import numpy as np
from scipy.sparse import csr_matrix
from sklearn.decomposition import TruncatedSVD
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import root_mean_squared_error

# Загрузка и подготовка данных
df = pd.read_csv("data/processed/filtered_ratings.csv")  # или другой DataFrame

user_encoder = LabelEncoder()
item_encoder = LabelEncoder()

df["user_id_enc"] = user_encoder.fit_transform(df["user_id"].astype(str))
df["book_id_enc"] = item_encoder.fit_transform(df["book_id"].astype(str))

# Делим на train/test по строкам
train_df, test_df = train_test_split(df, test_size=0.2, random_state=42)

# Строим обучающую матрицу
train_matrix = csr_matrix(
    (train_df["rating"], (train_df["user_id_enc"], train_df["book_id_enc"]))
)

# Обучаем модель
svd = TruncatedSVD(n_components=50, random_state=42)
svd.fit(train_matrix)

# Получаем матрицы признаков
user_factors = svd.transform(train_matrix)  # shape: [n_users, n_components]
item_factors = svd.components_.T            # shape: [n_items, n_components]

# Предсказываем оценки из теста
predicted_ratings = []
actual_ratings = []

for _, row in test_df.iterrows():
    user_idx = row["user_id_enc"]
    item_idx = row["book_id_enc"]
    
    if user_idx < user_factors.shape[0] and item_idx < item_factors.shape[0]:
        pred = np.dot(user_factors[user_idx], item_factors[item_idx])
        predicted_ratings.append(pred)
        actual_ratings.append(row["rating"])

# Вычисляем RMSE
rmse = root_mean_squared_error(actual_ratings, predicted_ratings)
print(f"RMSE: {rmse:.4f}")