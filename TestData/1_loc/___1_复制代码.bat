@echo off
cd /d %~dp0


 
echo ========== copy cs files ===================
xcopy /S /Y Output\Client\CS\*.cs ..\..\TestLoadCS\gen\
echo.


echo ========== copy sever Go files ===================
xcopy /S /Y Output\Server\Go\*.go ..\..\TestLoadGo\config\

echo.
echo "All Done"
pause