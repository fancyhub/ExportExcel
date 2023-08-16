@echo off
cd /d %~dp0

set SRC_DIR=Output\Client\CS
set DST_DIR=..\Client\EGG_Unity\Assets\Scripts\Table\Gen
 
echo ========== copy cs files ===================
xcopy /S /Y %SRC_DIR%\*.cs %DST_DIR%\
echo.

set SRC_DIR=Output\Client\Lua
set DST_DIR=..\Client\ProjDll\lua\Gen\TableScripts

echo ========== copy lua files ===================
xcopy /S /Y %SRC_DIR%\*.lua %DST_DIR%\
move /Y %DST_DIR%\Lua_StructDef.lua %DST_DIR%\..\..\TypeDef\TableTypeDef.lua
echo.


echo "All Done"
pause