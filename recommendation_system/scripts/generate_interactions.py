import os
import pandas as pd
from sklearn.model_selection import train_test_split

def split_and_save():
    # Загрузка данных
    ratings = pd.read_csv("data/raw/ratings.csv")
    books = pd.read_csv("data/raw/books.csv")

    # Разделение рейтингов на train/test
    train_ratings, test_ratings = train_test_split(ratings, test_size=0.6, random_state=42) #Было 0.2

    # Сохраняем рейтинги
    train_ratings.to_csv("data/processed/train_ratings.csv", index=False)
    test_ratings.to_csv("data/processed/test_ratings.csv", index=False)

    # Книги: делим по book_id из train-рейтингов
    book_ids_train = set(train_ratings['book_id'])
    books_train = books[books['book_id'].isin(book_ids_train)]
    books_rest = books[~books['book_id'].isin(book_ids_train)]

    # Сохраняем книги
    books_train.to_csv("data/processed/books_train.csv", index=False)
    books_rest.to_csv("data/processed/books_rest.csv", index=False)

    # Пользователи: делим по user_id
    user_ids_train = set(train_ratings['user_id'])
    user_ids_test_all = set(ratings['user_id'])
    user_ids_test = user_ids_test_all - user_ids_train

    # Сохраняем пользователей
    pd.DataFrame({'user_id': list(user_ids_train)}).to_csv("data/processed/users_train.csv", index=False)
    pd.DataFrame({'user_id': list(user_ids_test)}).to_csv("data/processed/users_test.csv", index=False)

    print("Файлы успешно созданы.")


def main():
    split_and_save()
    print("🎉 Обработка завершена.")

if __name__ == "__main__":
    main()
