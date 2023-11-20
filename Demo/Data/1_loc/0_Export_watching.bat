@echo off
cd /d %~dp0

echo ===========================0.clear Output============================
del /F /S /Q Output\*.cs
del /F /S /Q Output\*.go
del /F /S /Q Output\*.csv
del /F /S /Q Output\*.lua
echo.
echo.

:: Disable Console Quick Edit Mode
echo ===========================1.gen data and code ===============

..\..\Tool\WinCmdTool.exe ..\..\Tool\ExportExcel.exe -watch
echo.
echo.