#!/usr/bin/env bash
# Copyright (c) Microsoft. All rights reserved.
# Note: Windows Bash doesn't support shebang extra params
set -e

# Sometimes when creating bash scripts in Windows, bash scripts will not have
# the +x flag carried over to Linux/MacOS. This script should help setting the
# permission flags right.

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd .. && cd .. && pwd )/"
cd $APP_HOME

chmod ugo+x ./scripts/build
chmod ugo+x ./scripts/git/setup
chmod ugo+x ./scripts/git/*.sh

git update-index --chmod=+x ./scripts/build
git update-index --chmod=+x ./scripts/git/setup
git update-index --chmod=+x ./scripts/git/*.sh

set +e