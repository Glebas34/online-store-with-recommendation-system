#!/usr/bin/env bash
# wait-for-db.sh

set -e

host="$1"
shift
cmd="$@"

until nc -z ${host%:*} ${host#*:}; do
  echo "⏳ Ожидание доступности $host..."
  sleep 2
done

echo "✅ $host доступен! Запускаем приложение..."
exec $cmd
