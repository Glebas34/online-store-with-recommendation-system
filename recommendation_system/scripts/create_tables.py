import asyncio
import os
from db.db import engine
from db.models import Base
from scripts.load_train_to_db import load_data_to_db
from dotenv import load_dotenv
from sqlalchemy import inspect

load_dotenv()

async def create_tables():
    async with engine.begin() as conn:
        def do_inspect(sync_conn):
            inspector = inspect(sync_conn)
            return inspector.get_table_names()

        existing_tables = await conn.run_sync(do_inspect)

        if "user_ratings" in existing_tables:
            print("⚠️ Таблица 'user_ratings' уже существует. Пропускаем создание.")
        else:
            await conn.run_sync(Base.metadata.create_all)
            await load_data_to_db()
            print("✅ Таблица 'user_ratings' успешно создана.")

if __name__ == "__main__":
    asyncio.run(create_tables())
