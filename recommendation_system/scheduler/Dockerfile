FROM python:3.11-slim

WORKDIR /app

# Установим только нужные зависимости
RUN pip install --no-cache-dir apscheduler

# Копируем только скрипт бэкапа
COPY recommendation_system/scheduler/backup_scheduler.py .

# Создаём директории для модели и бэкапов
RUN mkdir -p /app/model /app/backups

CMD ["python", "backup_scheduler.py"]