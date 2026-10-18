@echo off
setlocal
dotnet run --project "%~dp0src\Kepler7.Game\Kepler7.Game.csproj"
set "keplerExitCode=%ERRORLEVEL%"
if not "%keplerExitCode%"=="0" (
    echo.
    echo No se pudo iniciar Kepler-7. Revisa el error que aparece arriba.
    echo Se necesita el SDK de .NET 8 para compilar y ejecutar el proyecto.
    pause
)
exit /b %keplerExitCode%
