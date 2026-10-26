@echo off
echo ============================================
echo   Compilando AwakeWin v1.3.0 (Fluent UI)
echo ============================================

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Error: No se encontro el compilador C# en %CSC%
    pause
    exit /b 1
)

if not exist "bin" mkdir bin

"%CSC%" /nologo /target:winexe /optimize+ /win32icon:src\app.ico /out:"bin\AwakeWin.exe" /reference:System.dll,System.Drawing.dll,System.Windows.Forms.dll /recurse:src\*.cs

if %ERRORLEVEL% equ 0 (
    echo.
    echo Compilacion exitosa: bin\AwakeWin.exe con Fluent UI y Anti-Slop
    echo.
) else (
    echo.
    echo Error durante la compilacion.
    echo.
)

pause
