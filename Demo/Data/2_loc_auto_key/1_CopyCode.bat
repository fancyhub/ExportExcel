@echo off
cd /d %~dp0


 
echo ========== copy client cs files ===================
::xcopy /S /Y Output\Client\CS\*.cs ..\..\TestLoadCS\gen\
echo.

echo ========== copy client Lua files ===================
::xcopy /S /Y Output\Client\Lua\*.lua ..\..\TestLoadLua\table\gen\
echo.

echo ========== copy client Cpp files ===================
xcopy /S /Y Output\Client\Cpp\*.cpp ..\..\TestLoadCpp\gen\
xcopy /S /Y Output\Client\Cpp\*.h ..\..\TestLoadCpp\gen\
echo.

echo ========== copy sever Go files ===================
::xcopy /S /Y Output\Server\Go\*.go ..\..\TestLoadGo\config\
echo.

echo "All Done"
pause