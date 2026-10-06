#!/usr/bin/env bash
# Busca alertas ativos no Grafana. Para cada alerta ainda não analisado:
#  1. o Claude Code investiga traces, logs, métricas e o código e aplica uma correção
#  2. abre uma Issue com o diagnóstico e o percentual de confiança
#  3. se houver alteração e confiança >= MIN_CONFIDENCE, abre um PR em draft
set -uo pipefail

BASE="${BASE_BRANCH:-main}"
MIN_CONF="${MIN_CONFIDENCE:-70}"
FILTER="${ALERT_NAME_REGEX:-.*}"
LABEL="alert-agent"
MCP_CONFIG="$RUNNER_TEMP/mcp.json"

git config user.name  "Alert Agent"
git config user.email "alert-agent@users.noreply.github.com"
gh label create "$LABEL" --color D93F0B --description "Criado pelo agente de alertas" --force >/dev/null

GRAFANA_URL="${GRAFANA_URL%/}"   # remove barra final, se houver

alerts=$(curl -fsS \
  -H "Authorization: Bearer $GRAFANA_SERVICE_ACCOUNT_TOKEN" \
  "$GRAFANA_URL/api/alertmanager/grafana/api/v2/alerts?active=true&silenced=false&inhibited=false") \
  || { echo "::error::Falha ao consultar o Grafana"; exit 1; }

if ! jq -e 'type == "array"' >/dev/null 2>&1 <<<"$alerts"; then
  echo "::error::O Grafana não retornou uma lista de alertas. Início da resposta:"
  head -c 500 <<<"$alerts"; echo
  exit 1
fi

total=$(jq --arg re "$FILTER" '[.[] | select(.labels.alertname | test($re))] | length' <<<"$alerts")
echo "Alertas ativos que passam no filtro: $total"

# Marcadores dos alertas já analisados (ficam no corpo das issues)
known=$(gh issue list --label "$LABEL" --state all --limit 300 --json body -q '.[].body' \
        | grep -oE 'agentid[0-9a-f]{12}' || true)

jq -c --arg re "$FILTER" '.[] | select(.labels.alertname | test($re))' <<<"$alerts" | while read -r alert; do
  hash=$(jq -r '.fingerprint + "-" + .startsAt' <<<"$alert" | sha1sum | cut -c1-12)
  marker="agentid$hash"
  name=$(jq -r '.labels.alertname' <<<"$alert" | tr -c 'A-Za-z0-9_-' '_' | cut -c1-40)

  if grep -qx "$marker" <<<"$known"; then
    echo "Já analisado: $name ($hash)"; continue
  fi
  echo "::group::Analisando $name ($hash)"

  branch="alert-fix/${name}-${hash}"
  file="$RUNNER_TEMP/diag-$hash.md"
  git reset --hard -q && git clean -fdq
  git fetch -q origin "$BASE" && git checkout -q -B "$branch" "origin/$BASE"

  prompt=$(cat <<PROMPT
Você é um engenheiro de confiabilidade analisando um alerta de produção
de uma aplicação .NET instrumentada com OpenTelemetry e monitorada no Grafana Cloud.

Alerta (JSON do Alertmanager do Grafana):
$alert

Passos:
1. Use as ferramentas do Grafana para buscar, numa janela de 15 minutos
   em torno de startsAt: métricas relacionadas (Prometheus), logs de erro
   (Loki) e traces com erro (Tempo). Correlacione pelo trace_id.
2. Extraia exceções e stack traces e localize os arquivos e linhas
   correspondentes no código-fonte do diretório atual.
3. Se as evidências apontarem com clareza para um defeito no código,
   aplique a MENOR correção possível diretamente nos arquivos.
   Pode adicionar ou ajustar testes unitários relacionados.
   Não altere a pasta .github, configurações de ambiente, segredos ou dependências.
   Se a causa for infraestrutura, dados ou estiver incerta, NÃO altere código.

Responda somente em Markdown, com estas seções:
# <título curto do problema>
## Resumo
## Evidências (trace_ids, consultas usadas, trechos de log)
## Causa provável
## Código envolvido (arquivo:linha)
## Alterações realizadas (ou motivo de não alterar)
## Como validar a correção
## Confiança no diagnóstico e justificativa

A última linha da resposta deve ser exatamente: CONFIANCA=<0 a 100>
PROMPT
)

  if ! claude -p "$prompt" --mcp-config "$MCP_CONFIG" \
        --allowedTools "mcp__grafana,Read,Grep,Glob,Edit,Write" > "$file"; then
    echo "::warning::Falha na análise de $name"; echo "::endgroup::"; continue
  fi

  # Proteção: descarta qualquer mudança em .github
  git checkout -q "origin/$BASE" -- .github 2>/dev/null; git clean -fdq -- .github

  conf=$(grep -oE 'CONFIANCA=[0-9]+' "$file" | tail -1 | cut -d= -f2); conf=${conf:-0}
  headline=$(grep -m1 '^# ' "$file" | sed 's/^# //'); headline=${headline:-$name}
  title="[Alerta ${conf}%] $headline"

  { echo; echo "---"; echo "Alerta: \`$name\` | Início: $(jq -r .startsAt <<<"$alert") | Confiança: **${conf}%**";
    echo "<!-- $marker -->"; } >> "$file"
  issue_url=$(gh issue create --title "$title" --label "$LABEL" --body-file "$file")
  issue_num=${issue_url##*/}
  echo "Issue criada: $issue_url"

  if [ -z "$(git status --porcelain)" ]; then
    echo "Sem alteração de código."
  elif [ "$conf" -lt "$MIN_CONF" ]; then
    echo "Confiança ${conf}% abaixo de ${MIN_CONF}%: diff anexado à issue, sem PR."
    { echo "Confiança abaixo do mínimo (${MIN_CONF}%). Diff proposto, não enviado:";
      echo '```diff'; git diff | head -c 60000; echo '```'; } > "$RUNNER_TEMP/diff-$hash.md"
    gh issue comment "$issue_num" --body-file "$RUNNER_TEMP/diff-$hash.md"
  else
    git add -A
    git commit -q -m "$title" -m "Relacionado a #$issue_num"
    if git push -q -u origin "$branch"; then
      pr_url=$(gh pr create --draft --base "$BASE" --head "$branch" --title "$title" \
               --body "Correção proposta pelo agente para #$issue_num (confiança ${conf}%). Veja o diagnóstico completo na issue.")
      gh issue comment "$issue_num" --body "PR aberto: $pr_url"
      echo "PR aberto: $pr_url"
    else
      echo "::warning::Falha no push da branch $branch"
    fi
  fi
  echo "::endgroup::"
done
