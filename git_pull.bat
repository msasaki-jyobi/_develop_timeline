@echo off
:: バッチファイルが存在するディレクトリを取得
set scriptdir=%~dp0

:: プロジェクトディレクトリに移動
cd /d "%scriptdir%"

:: ローカル変更の取り消し
git fetch origin
git reset --hard origin/main

:: リモートの最新の変更を取得
git pull origin main

:: 終了メッセージ
echo Pull completed. Your repository is up to date.
pause
