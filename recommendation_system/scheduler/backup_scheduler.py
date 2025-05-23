import os
import shutil
from datetime import datetime
from apscheduler.schedulers.blocking import BlockingScheduler

MODEL_PATH = "model/svd_model.pkl"
BACKUP_DIR = "backups/"
INTERVAL_MIN = int(os.getenv("BACKUP_INTERVAL_MIN", 360))

scheduler = BlockingScheduler()

@scheduler.scheduled_job('interval', minutes=INTERVAL_MIN)
def backup_model():
    if os.path.exists(MODEL_PATH):
        timestamp = datetime.now().strftime("%Y%m%d_%H%M")
        backup_path = os.path.join(BACKUP_DIR, f"svd_model_{timestamp}.pkl")
        os.makedirs(BACKUP_DIR, exist_ok=True)
        shutil.copy(MODEL_PATH, backup_path)
        print(f"🗃️ Бэкап модели сохранён: {backup_path}")
    else:
        print("⚠️ Модель не найдена для бэкапа.")

if __name__ == "__main__":
    print("⏰ Запуск планировщика бэкапов...")
    scheduler.start()
