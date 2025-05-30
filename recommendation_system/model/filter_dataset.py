import pandas as pd

# Загрузка исходного файла
input_path = "data/raw/ratings.csv"  # путь к исходному файлу
output_path = "data/processed/filtered_ratings.csv"  # путь к сохранению отфильтрованного файла

# Загрузка данных
df = pd.read_csv(input_path)

# Фильтрация активных пользователей (оставили >= 20 оценок)
active_users = df["user_id"].value_counts()
active_users = active_users[active_users >= 20].index

# Фильтрация популярных товаров (получили >= 20 оценок)
popular_items = df["book_id"].value_counts()
popular_items = popular_items[popular_items >= 20].index

# Применение фильтра
filtered_df = df[
    df["user_id"].isin(active_users) &
    df["book_id"].isin(popular_items)
]

# Вывод статистики
print("Число пользователей после фильтрации:", filtered_df["user_id"].nunique())
print("Число книг после фильтрации:", filtered_df["book_id"].nunique())
print("Число оценок после фильтрации:", len(filtered_df))

# Сохранение результата
filtered_df.to_csv(output_path, index=False)
print(f"Отфильтрованный файл сохранён как {output_path}")
