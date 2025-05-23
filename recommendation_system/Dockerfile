FROM python:3.13.3-slim

WORKDIR /app
ENV PYTHONPATH=/app

# Устанавливаем system dependencies заранее для кеширования
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    python3-dev \
    libffi-dev \
    libopenblas-dev \
    gfortran \
    liblapack-dev \
    gcc \
    libjpeg-dev \
    zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

# Копируем requirements.txt отдельно для использования кэша
COPY recommendation_system/requirements.txt .

# Обновляем pip и устанавливаем зависимости
RUN pip install --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Копируем остальной проект
COPY recommendation_system/db/ /app/db/
COPY recommendation_system/model/ /app/model/
COPY recommendation_system/kafka/ /app/kafka/
COPY recommendation_system/scripts/ /app/scripts/
COPY recommendation_system/data/processed/ /app/data/processed/
#COPY recommendation_system/main.py .

# Создаем необходимые директории
RUN mkdir -p /logs /model /backups


#RUN chmod +x /app/scripts/wait-for-db.sh
