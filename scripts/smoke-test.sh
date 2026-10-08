#!/usr/bin/env bash
# End-to-end smoke test for the monolith -> order-service extraction.
# Expects the docker-compose stack to be running: docker compose up --build -d
set -euo pipefail

MONOLITH_URL="${MONOLITH_URL:-http://localhost:8080}"
ORDER_SERVICE_URL="${ORDER_SERVICE_URL:-http://localhost:5003}"
SMOKE_USERNAME="${SMOKE_USERNAME:-admin}"
SMOKE_PASSWORD="${SMOKE_PASSWORD:-tempP@ss123}"
WAIT_SECONDS="${WAIT_SECONDS:-180}"

command -v jq >/dev/null || { echo "jq is required" >&2; exit 2; }

BODY="$(mktemp)"
trap 'rm -f "$BODY"' EXIT

pass() { echo "PASS  $*"; }
fail() { echo "FAIL  $*" >&2; [[ -s "$BODY" ]] && cat "$BODY" >&2 && echo >&2; exit 1; }

# request METHOD URL [curl args...] -> prints status code, body in $BODY
request() {
  local method="$1" url="$2"; shift 2
  curl -sS -o "$BODY" -w '%{http_code}' -X "$method" "$url" "$@"
}

expect() {
  local expected="$1" actual="$2" what="$3"
  [[ "$actual" == "$expected" ]] || fail "$what: expected HTTP $expected, got $actual"
  pass "$what ($actual)"
}

wait_for() {
  local url="$1" deadline=$((SECONDS + WAIT_SECONDS))
  until [[ "$(curl -s -o /dev/null -w '%{http_code}' "$url" || true)" == "200" ]]; do
    (( SECONDS < deadline )) || fail "timed out waiting for $url"
    sleep 3
  done
  pass "ready: $url"
}

wait_for "$ORDER_SERVICE_URL/healthz"
wait_for "$ORDER_SERVICE_URL/readyz"
wait_for "$MONOLITH_URL/"

expect 401 "$(request GET "$MONOLITH_URL/api/orders")" "monolith /api/orders requires authentication"

status="$(request POST "$MONOLITH_URL/connect/token" \
  -d grant_type=password -d client_id=quickapp_spa -d scope="openid profile roles" \
  --data-urlencode "username=$SMOKE_USERNAME" --data-urlencode "password=$SMOKE_PASSWORD")"
expect 200 "$status" "login as $SMOKE_USERNAME"
TOKEN="$(jq -r .access_token "$BODY")"
AUTH=(-H "Authorization: Bearer $TOKEN")
JSON=(-H "Content-Type: application/json")

expect 200 "$(request GET "$MONOLITH_URL/api/customer")" "list customers via monolith"
CUSTOMER_ID="$(jq -r '.[0].id // empty' "$BODY")"
[[ -n "$CUSTOMER_ID" ]] || fail "no seeded customers found"
SEEDED_ORDERS="$(jq '[.[].orders | length] | add' "$BODY")"
pass "customer $CUSTOMER_ID found; customers carry $SEEDED_ORDERS order(s) from order-service"

CORRELATION_ID="smoke-$(date +%s)"
status="$(request POST "$MONOLITH_URL/api/orders" "${AUTH[@]}" "${JSON[@]}" -H "X-Correlation-ID: $CORRELATION_ID" \
  -d "{\"customerId\":$CUSTOMER_ID,\"discount\":10,\"comments\":\"smoke test\",\"items\":[{\"productId\":1,\"unitPrice\":100,\"quantity\":2,\"discount\":5},{\"productId\":2,\"unitPrice\":50,\"quantity\":1}]}")"
expect 201 "$status" "create order via monolith"
ORDER_ID="$(jq -r .id "$BODY")"
[[ "$(jq -r .total "$BODY")" == "235" ]] || fail "unexpected total for order $ORDER_ID"
[[ -n "$(jq -r '.cashierId // empty' "$BODY")" ]] || fail "cashierId was not set from the authenticated user"
pass "order $ORDER_ID total=235 cashierId set"

expect 200 "$(request GET "$ORDER_SERVICE_URL/api/orders/$ORDER_ID")" "order $ORDER_ID persisted in order-service"
[[ "$(jq -r .customerId "$BODY")" == "$CUSTOMER_ID" ]] || fail "order-service has wrong customerId"

expect 200 "$(request GET "$MONOLITH_URL/api/orders/$ORDER_ID" "${AUTH[@]}")" "read order via monolith"

expect 200 "$(request GET "$MONOLITH_URL/api/customer")" "customers via monolith"
jq -e --argjson c "$CUSTOMER_ID" --argjson o "$ORDER_ID" \
  '.[] | select(.id == $c) | .orders | any(.id == $o)' "$BODY" >/dev/null \
  || fail "customer $CUSTOMER_ID does not list order $ORDER_ID"
pass "customer $CUSTOMER_ID lists order $ORDER_ID"

expect 200 "$(request PUT "$MONOLITH_URL/api/orders/$ORDER_ID" "${AUTH[@]}" "${JSON[@]}" -d '{"discount":0,"comments":"updated"}')" "update order via monolith"
[[ "$(jq -r .total "$BODY")" == "245" ]] || fail "unexpected total after update"
pass "updated total=245"

expect 400 "$(request POST "$MONOLITH_URL/api/orders" "${AUTH[@]}" "${JSON[@]}" -d "{\"customerId\":$CUSTOMER_ID,\"items\":[]}")" "reject order without items"

expect 204 "$(request DELETE "$MONOLITH_URL/api/orders/$ORDER_ID" "${AUTH[@]}")" "delete order via monolith"
expect 404 "$(request GET "$ORDER_SERVICE_URL/api/orders/$ORDER_ID")" "order $ORDER_ID removed from order-service"

echo "Smoke test passed"
