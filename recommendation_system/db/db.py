import os
from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine
from sqlalchemy.orm import sessionmaker, declarative_base
from dotenv import load_dotenv

load_dotenv()

# Загружаем строку подключения
DATABASE_URL = os.getenv("DATABASE_URL")

# Создаём асинхронный движок
engine = create_async_engine(
    DATABASE_URL,
    echo=True,  # Показывать SQL-запросы в логах (можно убрать на проде)
)

# Создаём фабрику сессий
AsyncSessionLocal = sessionmaker(
    bind=engine,
    expire_on_commit=False,
    class_=AsyncSession,
)

# Базовый класс для моделей
Base = declarative_base()

# Утилита для получения новой сессии
async def get_async_session() -> AsyncSession:
    async with AsyncSessionLocal() as session:
        yield session
