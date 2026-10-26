@echo off
cd /d "%~dp0"
echo ==============================================
echo   Actualizando AwakeWin en GitHub (erikrtz-crypto)
echo ==============================================
echo.
git push -u origin main --force
echo.
if %ERRORLEVEL% equ 0 (
    echo [EXITO] AwakeWin actualizado con tu usuario oficial erikrtz-crypto!
) else (
    echo [ERROR] No se pudo subir. Revisa tus credenciales o conexion.
)
echo.
pause
