#!/usr/bin/env bash
# Gera tráfego na API para disparar alertas específicos no Grafana.
#
#   ./scripts/cenarios.sh pr     [minutos]  -> só erros de código (divide/multiply): esperado Issue + PR
#   ./scripts/cenarios.sh issue  [minutos]  -> só erro de infraestrutura (Postgres): esperado só Issue
#   ./scripts/cenarios.sh externo [minutos] -> dependência externa fora do ar (API de cotação): esperado só Issue
#   ./scripts/cenarios.sh normal [minutos]  -> só tráfego saudável (linha de base)
#
# Em todos os cenários também roda tráfego saudável, para os gráficos terem contexto.
# BASE_URL muda o endereço da API (padrão http://localhost:5017).
set -uo pipefail

SCENARIO="${1:-}"
MINUTES="${2:-5}"
BASE="${BASE_URL:-http://localhost:5017}/api"
INTERVAL=2

case "$SCENARIO" in
  pr|issue|externo|normal) ;;
  *) echo "Uso: $0 pr|issue|externo|normal [minutos]"; exit 1 ;;
esac

declare -A count
hit() {
  local code
  code=$(curl -s -o /dev/null -m 15 -w "%{http_code}" "$@")
  count["$code"]=$(( ${count["$code"]:-0} + 1 ))
}

healthy() {
  hit -X POST "$BASE/products" -H "Content-Type: application/json" \
      -d "{\"name\":\"Produto $1\",\"price\":$((RANDOM % 5000 + 10)).90}"
  hit "$BASE/products"
  hit "$BASE/calculations/divide?dividend=$1&divisor=2"
  hit "$BASE/calculations/multiply?multiplicand=$1&multiplier=3"
}

end=$(( $(date +%s) + MINUTES * 60 ))
round=0
echo "Cenário '$SCENARIO' por $MINUTES min contra $BASE"

while [ "$(date +%s)" -lt "$end" ]; do
  round=$(( round + 1 ))
  healthy "$round"
  case "$SCENARIO" in
    pr)
      hit "$BASE/calculations/divide?dividend=10&divisor=0"
      hit "$BASE/calculations/multiply"
      ;;
    issue)
      hit "$BASE/products/database"
      ;;
    externo)
      hit "$BASE/quotes/dollar"
      ;;
  esac
  printf '\rRodada %d | ' "$round"
  for k in $(printf '%s\n' "${!count[@]}" | sort); do printf 'HTTP %s: %d  ' "$k" "${count[$k]}"; done
  sleep "$INTERVAL"
done
echo
