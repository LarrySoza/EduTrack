# EduTrack

Este proyecto se llama EduTrack. La API (EduTrack.WebApi) y la carpeta WEB son ejemplos muy básicos; están pensados como pruebas para demostrar los mecanismos y las capacidades del ESP32 para comunicarse con servicios en la nube. Estos ejemplos pueden ampliarse para integrar otros servicios de notificación (por ejemplo, WhatsApp o Telegram) u otros consumidores en producción.

Nota: el ejemplo ya incluye una integración de notificaciones por WhatsApp mediante `WhatsappNotificationService` que hace POST a un endpoint de terceros. La integración es de ejemplo y la configuración (URL/usuario/clave) se debe manejar fuera del repositorio (appsettings.Secrets.json, user-secrets o variables de entorno).

Proyecto para control de asistencias escolar (backend .NET) y componentes relacionados.

Resumen
- Backend API: `EduTrack.WebApi` (.NET 9) — expone endpoints REST y un Hub SignalR para notificaciones en tiempo real.
- Librerías y capas: `EduTrack.Core`, `EduTrack.Application`, `EduTrack.Infrastructure`.
- Dispositivo biométrico: carpeta `ESP32` en el directorio raíz contiene el firmware/cliente que se conecta a los endpoints del API para registrar asistencias.
- Interfaz web: carpeta `WEB` en el directorio raíz contiene una UI básica que consume los endpoints y el hub SignalR.

Estructura principal
- `EduTrack.WebApi/` — API ASP.NET Core
  - SignalR Hub: `NotificarHub` mapeado en `/hubs/notificar` internamente; la aplicación usa `UsePathBase("/api")` por lo que la ruta pública es `/api/hubs/notificar`.
  - Configuración sensible: se recomienda usar `appsettings.Secrets.json` (ignorada por git) o `dotnet user-secrets` / variables de entorno para valores sensibles.
  - Servicio de notificaciones WhatsApp: `IWhatsappNotificationService` / `WhatsappNotificationService` que hace POST al endpoint externo. Configuración en sección `Whatsapp`.
- `EduTrack.Infrastructure/` — dependencias y registro de DI
- `EduTrack.Core/` — entidades del dominio (por ejemplo `Alumno`)
- `EduTrack.Application/` — interfaces y casos de uso
- `ESP32/` — código para el dispositivo biométrico (firmware) que llama a los endpoints del API para reportar asistencias.
- `WEB/` — interfaz web que muestra/gestiona asistencias y se conecta al Hub SignalR para recibir eventos `NuevaAsistencia`.

Ejecutar localmente
1. Configurar secretos (opción recomendada):
   - Usar `appsettings.Secrets.json` en `EduTrack.WebApi/` (ya ignorado por git) o
   - Desde la carpeta `EduTrack.WebApi` ejecutar:
     ```bash
     dotnet user-secrets init
     dotnet user-secrets set "Whatsapp:EndpointUrl" "https://..."
     dotnet user-secrets set "Whatsapp:Username" "user"
     dotnet user-secrets set "Whatsapp:Password" "secret"
     ```
   - Alternativamente, exportar variables de entorno `Whatsapp__EndpointUrl`, `Whatsapp__Username`, `Whatsapp__Password`.

2. Restaurar y ejecutar:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project EduTrack.WebApi/EduTrack.WebApi.csproj
   ```

3. Rutas importantes
- API pública base: `http://localhost:5000/api` (si se usa `UsePathBase("/api")`)
- Hub SignalR público: `ws(s)://{host}/api/hubs/notificar`
- Endpoint WhatsApp (interno): configurado en `Whatsapp:EndpointUrl`

SignalR (cliente)
- El Hub emite `NuevaAsistencia` con un objeto `Alumno` JSON con esta estructura:
  ```ts
  interface Alumno {
    id: string;
    nombre_completo: string;
    grado_id: string;
    biometrico_id: number;
    nombre_apoderado: string;
    numero_apoderado: string;
  }
  ```
- Clientes JavaScript deben enviar el JWT al conectar (por ejemplo `accessTokenFactory`) ya que el Hub está protegido con `[Authorize]`.

Seguridad y despliegue
- No incluir secretos en el repo. Usar `appsettings.Secrets.json` local (añadido a `.gitignore`) o variables de entorno/secret manager en producción.
- Si se publica detrás de un reverse proxy que añade un prefijo (ej. `/api`), mantener `UsePathBase` o configurar `ForwardedHeaders` correctamente.
- Para WebSockets en producción, asegúrate que el proxy (NGINX, IIS, ALB) reenvíe correctamente `Upgrade` y `Connection` y que la afinidad de sesión (sticky sessions) esté configurada si hay múltiples instancias; o usar Azure SignalR Service.

Integración con ESP32
- El dispositivo ESP32 en `ESP32/` está preparado para llamar a los endpoints del API para notificar asistencias. Verifica URL base pública (ej. `https://mi-dominio.com/api`) y autenticación requerida.

ESP32 - Pines y conexiones (extraído de `ESP32/EduTrack.ino`)
- UART para sensor de huella (Serial1):
  - `RX_PIN` (ESP32) = GPIO 16  // Conectar TX del sensor a GPIO16
  - `TX_PIN` (ESP32) = GPIO 17  // Conectar RX del sensor to GPIO17
  - Baud rate: 57600
  - Alimentación del sensor: `VCC` -> 3.3V, `GND` -> GND

- Teclado matricial 4x4:
  - Filas (ROW) -> GPIO 12, 14, 27, 26
  - Columnas (COL) -> GPIO 33, 32, 25, 19
  - Usa la librería `Keypad`; asegúrate de tener resistencias pull-up/pull-down según tu montaje (normalmente entradas con pull-ups internas o resistencias externas).

- LCD I2C (LiquidCrystal_I2C):
  - Dirección I2C: `0x27`
  - Tamaño configurado en código: 20x4 (ajustar si tu pantalla es distinta)
  - Conexiones I2C típicas en ESP32 (si no se reconfigura Wire): `SDA` -> GPIO21, `SCL` -> GPIO22
  - Alimentación: `VCC` -> 5V o 3.3V según módulo, `GND` -> GND

- Entradas / botones: el teclado se usa para navegación y no hay botones físicos adicionales en el código.

- Otros detalles relevantes:
  - Sensor de huella usa la librería `Adafruit_Fingerprint` y llamadas a `finger.getImage()`, `image2Tz`, `createModel`, `storeModel`, `fingerSearch()`.
  - El código realiza llamadas HTTP POST al endpoint `https://edutrack.gaspersoft.com/api/asistencia` usando `HTTPClient` y envía `grado_id` y `biometrico_id` en JSON, con un token Bearer hardcoded en el ejemplo.
  - Wi?Fi: el sketch usa SSID/clave en variables `ssid` y `claveWifi` (reemplazar por credenciales reales o usar gestión segura).

Integración con la WEB
- La carpeta `WEB/` contiene una interfaz básica que consume la API y se conecta al Hub SignalR para recibir `NuevaAsistencia` y mostrarla en tiempo real.

Contribuir
- Abre un issue o PR en el repositorio. Mantén las credenciales fuera del repo.

Contacto
- Repo: https://github.com/LarrySoza/EduTrack

Licencia
- Revisa el archivo `LICENSE` si existe.
