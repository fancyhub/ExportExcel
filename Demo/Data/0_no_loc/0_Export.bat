@echo off
cd /d %~dp0

echo ===========================0.clear Output============================
del /F /S /Q Output\*.cs
del /F /S /Q Output\*.go
del /F /S /Q Output\*.csv
del /F /S /Q Output\*.lua
echo.
echo.

echo ===========================1.gen data and code ===============
..\..\Tool\ExportExcel.exe
echo.
echo.