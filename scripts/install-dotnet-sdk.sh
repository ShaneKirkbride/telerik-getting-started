#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GLOBAL_JSON="$REPO_ROOT/global.json"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
DOTNET_BIN="$DOTNET_ROOT/dotnet"

if [[ ! -f "$GLOBAL_JSON" ]]; then
  echo "global.json not found at $GLOBAL_JSON" >&2
  exit 1
fi

SDK_VERSION="$(GLOBAL_JSON="$GLOBAL_JSON" python - <<'PY'
import json
import os
from pathlib import Path
config = json.loads(Path(os.environ["GLOBAL_JSON"]).read_text())
print(config["sdk"]["version"])
PY
)"

if [[ -x "$DOTNET_BIN" ]] && "$DOTNET_BIN" --list-sdks | grep -q "^${SDK_VERSION} "; then
  echo ".NET SDK ${SDK_VERSION} already installed at $DOTNET_ROOT"
  exit 0
fi

INSTALL_SCRIPT="${DOTNET_INSTALL_SCRIPT:-/tmp/dotnet-install.sh}"
curl -sSL https://dot.net/v1/dotnet-install.sh -o "$INSTALL_SCRIPT"
bash "$INSTALL_SCRIPT" --version "$SDK_VERSION" --install-dir "$DOTNET_ROOT"

echo "Installed .NET SDK ${SDK_VERSION} to $DOTNET_ROOT"
PYTHONPATH="" "$DOTNET_BIN" --version
