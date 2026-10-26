# AwakeWin ⚡

> **Modo de fondo persistente, control multimedia inteligente y streaming remoto a 60 FPS para Windows.**  
> *Inspirado y basado en la idea original de [Awake para iOS](https://alex-louder.github.io/Awake/) de Alex Louder.*

---

## 📖 Acerca del Proyecto

**AwakeWin** es una adaptación y evolución para Windows de la aplicación de código abierto **Awake** creada originalmente para iOS por **Alex Louder** ([alex-louder.github.io/Awake](https://alex-louder.github.io/Awake/)).

Mientras que en iOS la app mantiene tareas activas burlando las restricciones de suspensión móvil, **AwakeWin** traslada este concepto a portátiles y PCs con Windows, resolviendo los problemas críticos que sufren los usuarios al cerrar la tapa de su laptop o dejar tareas pesadas desatendidas:

* 🤖 **Agentes de IA y tareas pesadas sin interrupción:** Mantiene tus modelos locales (Ollama, LM Studio), descargas, renders y compilaciones al 100% de velocidad sin que el sistema reduzca la potencia de la CPU/GPU ni entre en suspensión profunda.
* 🖥️ **Streaming remoto a 60 FPS (AnyDesk / Sunshine / Parsec):** Permite controlar tu equipo a toda velocidad incluso con la pantalla apagada o la laptop cerrada y guardada, sin caídas de señal ni congelamientos de DirectX/DWM.
* 🎵 **Pausa y silencio multimedia automático:** Al bajar la tapa, pausa automáticamente tu música o videos (Spotify, YouTube, VLC, podcasts) y silencia los altavoces; al levantarla, restaura el sonido y reanuda la reproducción exactamente donde la dejaste.
* 🌙 **Falsa suspensión instantánea (Blackout):** Apaga la pantalla en negro puro (`#000000`) con el atajo global **`Ctrl + Alt + B`** sin desconectar el compositor gráfico. Toca cualquier tecla para volver en 0 ms.
* 🎒 **Mochila Segura (Protección Térmica):** Si la laptop está cerrada en la mochila y funcionando en batería, vigila que procesos descontrolados no sobrecalienten el equipo.
* ⏱️ **Temporizador de Reposo Personalizado:** Permite programar el apagado automático en 30m, 1h, 2h o un tiempo exacto en horas y minutos.
* 🎨 **Diseño Fluent Windows 11 (Anti-AI-Slop):** Interfaz minimalista y táctil con interruptores deslizantes nativos, paleta Charcoal/Zinc de alto contraste y apertura cómoda en la esquina inferior derecha sobre la bandeja del sistema.

---

## 📥 Instalación y Uso Rápido

### Opción 1: Ejecutar directamente (Sin instalación de runtimes)
AwakeWin está compilado como un ejecutable nativo autónomo de solo **~71 KB** que funciona de fábrica en cualquier Windows 10 u 11 sin instalar dependencias:

1. Descarga el repositorio o ve a la carpeta `bin/`.
2. Haz doble clic en **`AwakeWin.exe`** (o en `run.bat`).
3. La aplicación se abrirá en la esquina inferior derecha junto al reloj. Al cerrarla (`X`), permanecerá activa y protegida en la bandeja del sistema.

### Opción 2: Recompilar desde el código fuente
Incluye un script de compilación de un solo clic que utiliza el compilador nativo de C# incluido en Windows (`csc.exe`):
1. Haz doble clic en **`build.bat`**.
2. El ejecutable se generará de inmediato en `bin/AwakeWin.exe` con su icono vectorial multirresolución incrustado.

---

## ⚙️ Configuración Recomendada para Portátiles

Para aprovechar al máximo el uso de la laptop con la tapa cerrada y acceso remoto:
1. En la aplicación, haz clic en **`⚙️ Configurar acción de la tapa en Windows`** (o abre *Opciones de energía* en el Panel de Control).
2. En la opción **"Al cerrar la tapa"**, selecciona **"No hacer nada"** (tanto con batería como con corriente alterna).
3. Con esto, al cerrar la laptop:
   * La pantalla se apagará para no gastar energía.
   * La música se pausará y el sonido se silenciará.
   * La laptop seguirá 100% activa para acceso remoto por AnyDesk o ejecución de agentes de IA.

---

## ⌨️ Atajos de Teclado

* **`Ctrl + Alt + B`**: Activa o desactiva la pantalla negra instantánea (Blackout Mode).
* **Cualquier tecla o ratón**: Despierta la pantalla inmediatamente al instante.

---

## 📁 Estructura del Código

```text
AwakeWin/
├── bin/
│   └── AwakeWin.exe              # Ejecutable autónomo nativo (~71 KB)
├── src/
│   ├── Controls/
│   │   ├── ToggleSwitch.cs       # Interruptor deslizante Fluent Windows 11
│   │   ├── SettingCard.cs        # Fila de ajuste con delimitación inteligente
│   │   ├── HeroCard.cs           # Tarjeta de control maestro con indicador LED
│   │   └── FluentComboBox.cs     # Menú desplegable con tema oscuro nativo
│   ├── PowerManager.cs           # Control de energía y modo ausente de Windows
│   ├── DisplayWakeController.cs  # Despertar de pantalla y supresión de bucle de teclas
│   ├── BlackoutOverlay.cs        # Capa negra pura excluida de capturas DXGI
│   ├── LidWatcher.cs             # Receptor de eventos de energía y bisagra (winnt.h)
│   ├── AudioController.cs        # Control de volumen multicanal con CoreAudio
│   ├── MediaController.cs        # Envío de comandos Play/Pause multimedia globales
│   ├── ThermalGuard.cs           # Algoritmo de protección térmica para mochilas
│   ├── SleepTimer.cs             # Temporizador regresivo en segundo plano
│   ├── AutoStartManager.cs       # Registro de inicio automático con Windows
│   ├── CustomTimeDialog.cs       # Selector de horas (0-12) y minutos (0-60)
│   ├── MainForm.cs               # Ventana principal y bandeja del sistema
│   ├── Program.cs                # Instancia única y arranque minimizado
│   └── app.ico                   # Icono nativo multiescala (16 a 256 px)
├── build.bat                     # Compilador nativo
├── run.bat                       # Lanzador rápido
└── README.md
```

---

## 🙏 Créditos y Atribución

Este proyecto está inspirado y basado en el concepto original de **Awake** para iOS desarrollado por **Alex Louder**:
* Proyecto original: [https://alex-louder.github.io/Awake/](https://alex-louder.github.io/Awake/)
* Desarrollado con permiso y adaptado para el ecosistema Windows por la comunidad de código abierto.

---

## 📄 Licencia

Este proyecto es de código abierto bajo la licencia [MIT](LICENSE).
