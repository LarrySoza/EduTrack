#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <Keypad.h>
#include <Adafruit_Fingerprint.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>

const char* ssid = "DEV";             // Nombre de tu red Wi-Fi
const char* claveWifi = "147852369";  // Contraseña de tu red Wi-Fi

// En ESP32, usamos HardwareSerial para los puertos seriales.
HardwareSerial mySerial(1);

// Definimos los pines para Serial1
#define RX_PIN 16  // Pin 16 como RX
#define TX_PIN 17  // Pin 17 como TX

Adafruit_Fingerprint finger = Adafruit_Fingerprint(&mySerial);

// Configuración del LCD
LiquidCrystal_I2C lcd(0x27, 20, 4);  // set the LCD address to 0x27 for a 16 chars and 2 line display

// Definir las conexiones del teclado 4x4
const byte ROW_NUM = 4;     // Número de filas
const byte COLUMN_NUM = 4;  // Número de columnas
char keys[ROW_NUM][COLUMN_NUM] = {
  { '1', '2', '3', 'A' },
  { '4', '5', '6', 'B' },
  { '7', '8', '9', 'C' },
  { '*', '0', '#', 'D' }
};

enum VentanaActiva {
  Home = 1,
  Clave = 2,
  Enrrolar = 3,
  Asistencia = 4
};

byte pin_rows[ROW_NUM] = { 12, 14, 27, 26 };       // Conectar las filas a los pines 12, 14, 27, 26
byte pin_column[COLUMN_NUM] = { 33, 32, 25, 19 };  // Conectar las columnas a los pines 33, 32, 25, 19

Keypad keypad = Keypad(makeKeymap(keys), pin_rows, pin_column, ROW_NUM, COLUMN_NUM);  // Inicializar el teclado

//La Capacidad de registros disponibles en el marcado
uint16_t capacidad = 0;

VentanaActiva myVentanaActiva;

// Declarar la variable para almacenar la contraseña ingresada
String password = "";
String correctPassword = "12345";  // Contraseña predefinida

String idHuellaStr = "";

//Se usa en el menu clave de administrador
boolean errorClaveAdmin = false;

boolean errorIdHuella = false;

boolean limpiarHuellas;

//leerHuella() siempre devolvera 9999 en caso de error
uint16_t huellaIdLector = 9999;

void viewHome(char key);

void viewClave(char key);

void viewEnrrolar(char key);

void viewAsistencia(char key);

void fnEnrrolar(uint16_t id);

void mostrarMensaje(String mensaje);

uint16_t leerHuella();

void fnRegistrarOnline(uint16_t id);

void fnLimpiarHuellas();

void setup() {
  lcd.init();
  lcd.backlight();  // Activar la retroiluminación del LCD

  //Inicilizamos el puerto serial para depuracion
  Serial.begin(9600);
  while (!Serial)
    ;

  // Conectar al Wi-Fi
  WiFi.begin(ssid, claveWifi);

  // Esperar hasta que esté conectado
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    mostrarMensaje("Conectando a Wi-Fi..");
  }

  delay(100);
  Serial.println("\n\n Prueba del sensor de huella dactilar R307S");

  // Inicializa el puerto Serial1 con los pines RX y TX configurados
  mySerial.begin(57600, SERIAL_8N1, RX_PIN, TX_PIN);

  delay(5);
  if (finger.verifyPassword()) {
    Serial.println("¡Sensor de huella dactilar encontrado!");
    // Mensajes iniciales en el LCD
    viewHome(' ');

    //Leer Parametros del sensor y mostrarlos en pantalla
    Serial.println(F("Leyendo parámetros del sensor"));
    finger.getParameters();
    Serial.print(F("Estado: 0x"));
    Serial.println(finger.status_reg, HEX);
    Serial.print(F("ID del sistema: 0x"));
    Serial.println(finger.system_id, HEX);
    capacidad = finger.capacity;
    Serial.print(F("Capacidad: "));
    Serial.println(finger.capacity);
    Serial.print(F("Nivel de seguridad: "));
    Serial.println(finger.security_level);
    Serial.print(F("Dirección del dispositivo: "));
    Serial.println(finger.device_addr, HEX);
    Serial.print(F("Longitud de paquete: "));
    Serial.println(finger.packet_len);
    Serial.print(F("Tasa de baudios: "));
    Serial.println(finger.baud_rate);

  } else {
    Serial.println("No se encontró el sensor de huella dactilar :(");
    //Detiene la ejecución si no se encuentra el sensor
    lcd.setCursor(2, 1);
    lcd.print("ERROR DE SENSOR!");
    while (1) {
      delay(1);
    }
  }
}

void loop() {
  char key = keypad.getKey();

  if (key) {
    //Si estoy en modo clave envio la tecla precionada
    if (myVentanaActiva == Clave) {
      viewClave(key);
      return;
    }

    if (myVentanaActiva == Enrrolar) {
      viewEnrrolar(key);
      return;
    }

    if (myVentanaActiva == Asistencia) {
      viewAsistencia(key);
      return;
    }

    if (myVentanaActiva == Home) {
      viewHome(key);
      return;
    }
  }
}

void viewHome(char key) {
  myVentanaActiva = Home;

  if (key == 'A') {
    viewAsistencia(' ');
    return;
  }

  if (key == 'B') {
    viewClave(' ');
    return;
  }

  if (key == 'C') {
    limpiarHuellas = true;
    viewClave(' ');
    return;
  }

  lcd.clear();
  lcd.setCursor(6, 0);
  lcd.print("EduTrack");
  lcd.setCursor(0, 1);
  lcd.print("A: MARCAR ASISTENCIA");
  lcd.setCursor(0, 2);
  lcd.print("B: ENRROLAR");
  lcd.setCursor(0, 3);
  lcd.print("C: LIMPIAR HUELLAS");
}

void viewClave(char key) {
  myVentanaActiva = Clave;

  //Si no existe error en la clave del admin entonces se procesa la tecla pulsada
  if (!errorClaveAdmin) {
    if (key == '#') {  // Si se presiona el botón '#', validar la contraseña
      if (password == correctPassword) {
        password = ""; 
        if (limpiarHuellas) {
          Serial.println("Limpiando Huellas");
          fnLimpiarHuellas();
          delay(2000);
          limpiarHuellas = false;
          viewHome(' ');
          return;
        } else {
          Serial.println("Enrrolando");
          viewEnrrolar(' ');
          return;
        }
      } else {
        lcd.clear();
        lcd.setCursor(6, 0);
        lcd.print("EduTrack");
        lcd.setCursor(2, 1);
        lcd.print("Clave incorrecta");
        lcd.setCursor(1, 3);
        lcd.print("PRECIONE UNA TECLA");
        errorClaveAdmin = true;
      }
      password = "";  // Limpiar la variable de la contraseña ingresada
      return;
    }

    if (key == '*') {  // Si se presiona '*', eliminar el último carácter
      password = password.substring(0, password.length() - 1);
      if (password.length() <= 0) {
        viewHome(' ');
        return;
      }
    } else {
      if (key != ' ') {
        password += key;  // Agregar el carácter a la contraseña
      }
    }
  } else {
    //limpiamos la variable de error
    errorClaveAdmin = false;
  }

  lcd.clear();
  lcd.setCursor(6, 0);
  lcd.print("EduTrack");
  lcd.setCursor(0, 1);
  lcd.print("INGRESE CLAVE ADMIN:");
  lcd.setCursor(0, 2);

  int largoPasword = password.length();
  String claveOculta = "";

  for (int i = 0; i < largoPasword; i++) {
    claveOculta += '*';
  }

  lcd.print(claveOculta);
  lcd.setCursor(0, 3);
  lcd.print("#: ENTER , *: ATRAS");
}

void viewEnrrolar(char key) {
  myVentanaActiva = Enrrolar;

  if (!errorIdHuella) {
    if (key == '#' && idHuellaStr.length() > 0) {  // Si se presiona el botón '#', se debe intentar registrar la firma
      uint8_t idHuella = idHuellaStr.toInt();

      if (idHuella <= 0 || idHuella > capacidad) {
        lcd.clear();
        lcd.setCursor(6, 0);
        lcd.print("EduTrack");
        lcd.setCursor(2, 1);
        lcd.print("Clave incorrecta");
        lcd.setCursor(1, 3);
        lcd.print("PRECIONE UNA TECLA");
        errorIdHuella = true;
        idHuellaStr = "";
        return;
      } else {
        errorIdHuella = false;
        fnEnrrolar(idHuella);
        idHuellaStr = "";
        goto menuEnrrolar;
      }
    }

    if (key == '*') {  // Si se presiona '*', eliminar el último carácter
      idHuellaStr = idHuellaStr.substring(0, idHuellaStr.length() - 1);
      if (idHuellaStr.length() <= 0) {
        viewHome(' ');
        return;
      }
    } else {
      if (isDigit(key)) {
        idHuellaStr += key;
      }
    }
  }

menuEnrrolar:
  finger.getTemplateCount();

  uint16_t totalHuellas = finger.templateCount;
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("EduTrack - Reg: " + String(totalHuellas));
  lcd.setCursor(0, 1);
  lcd.print("ID (1 al " + String(capacidad) + "):");
  lcd.setCursor(0, 2);
  lcd.print(idHuellaStr);
  lcd.setCursor(0, 3);
  lcd.print("#: ENTER , *: ATRAS");
}

void viewAsistencia(char key) {
  myVentanaActiva = Asistencia;

ProcesarAsistencia:
  lcd.clear();
  lcd.setCursor(6, 0);
  lcd.print("EduTrack");
  lcd.setCursor(2, 1);
  lcd.print("ESPERANDO HUELLA");
  lcd.setCursor(0, 3);
  lcd.print("* SIN SOLTAR IR HOME");

  while (huellaIdLector == 9999) {
    key = keypad.getKey();

    if (key) {
      if (key == '*') {
        viewHome(' ');
        return;
      }
    }

    huellaIdLector = leerHuella();
    lcd.setCursor(0, 1);
    lcd.print("..ESPERANDO HUELLA..");
  }

  String mensaje = "ID encontrado # " + String(huellaIdLector);

  //Codigo para registrar en el servidor
  mostrarMensaje(mensaje);
  delay(500);
  mostrarMensaje("REGISTRANDO SERVIDOR");
  delay(500);
  fnRegistrarOnline(huellaIdLector);
  delay(500);
  mostrarMensaje("ASISTENCIA OK");
  delay(500);
  //Resateamos el huellaIdLector
  huellaIdLector = 9999;
  goto ProcesarAsistencia;
}

void fnEnrrolar(uint16_t id) {
  int p = -1;
  Serial.print("Esperando una huella válida para inscribir con ID #");
  Serial.println(id);
  mostrarMensaje("Posiciona el dedo");
  while (p != FINGERPRINT_OK) {
    p = finger.getImage();  // Captura una imagen de la huella
    switch (p) {
      case FINGERPRINT_OK:
        Serial.println("Imagen tomada");
        break;
      case FINGERPRINT_NOFINGER:
        Serial.print(".");  // Si no se detecta huella, espera
        break;
      case FINGERPRINT_PACKETRECIEVEERR:
        Serial.println("Error de comunicación");
        break;
      case FINGERPRINT_IMAGEFAIL:
        Serial.println("Error en la toma de imagen");
        break;
      default:
        Serial.println("Error desconocido");
        break;
    }
  }

  p = finger.image2Tz(1);  // Convierte la imagen en una plantilla de huella
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Imagen convertida");
      break;
    case FINGERPRINT_IMAGEMESS:
      Serial.println("Imagen demasiado desordenada");
      return;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Error de comunicación");
      return;
    case FINGERPRINT_FEATUREFAIL:
      Serial.println("No se encontraron características de huella");
      return;
    case FINGERPRINT_INVALIDIMAGE:
      Serial.println("No se pudo encontrar una imagen válida");
      return;
    default:
      Serial.println("Error desconocido");
      return;
  }

  Serial.println("Retira el dedo");
  mostrarMensaje("Retira el dedo");
  delay(2000);
  p = 0;
  while (p != FINGERPRINT_NOFINGER) {  // Espera hasta que no haya huella en el sensor
    p = finger.getImage();
  }
  Serial.print("ID ");
  Serial.println(id);
  p = -1;
  Serial.println("Coloca el mismo dedo nuevamente");
  mostrarMensaje("Posiciona el dedo");
  while (p != FINGERPRINT_OK) {  // Espera hasta que se tome la huella correctamente
    p = finger.getImage();
    switch (p) {
      case FINGERPRINT_OK:
        Serial.println("Imagen tomada");
        break;
      case FINGERPRINT_NOFINGER:
        Serial.print(".");
        break;
      case FINGERPRINT_PACKETRECIEVEERR:
        Serial.println("Error de comunicación");
        break;
      case FINGERPRINT_IMAGEFAIL:
        Serial.println("Error en la toma de imagen");
        break;
      default:
        Serial.println("Error desconocido");
        break;
    }
  }

  p = finger.image2Tz(2);  // Convierte la segunda imagen
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Imagen convertida");
      break;
    case FINGERPRINT_IMAGEMESS:
      Serial.println("Imagen demasiado desordenada");
      mostrarMensaje("Imagen desordenada");
      delay(2000);
      return;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Error de comunicación");
      mostrarMensaje("Error comunicacion");
      delay(2000);
      return;
    case FINGERPRINT_FEATUREFAIL:
      Serial.println("No se encontraron características de huella");
      mostrarMensaje("Huella no encontrada");
      delay(2000);
      return;
    case FINGERPRINT_INVALIDIMAGE:
      Serial.println("No se pudo encontrar una imagen válida");
      mostrarMensaje("Imagen invalida");
      delay(2000);
      return;
    default:
      Serial.println("Error desconocido");
      mostrarMensaje("Error desconocido");
      delay(2000);
      return;
  }

  Serial.print("Creando modelo para el ID #");
  Serial.println(id);

  mostrarMensaje("Creando imagen...");
  delay(2000);

  p = finger.createModel();  // Crea el modelo de huella
  if (p == FINGERPRINT_OK) {
    Serial.println("Las huellas coinciden!");
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    Serial.println("Error de comunicación");
    mostrarMensaje("Error comunicacion");
    delay(2000);
    return;
  } else if (p == FINGERPRINT_ENROLLMISMATCH) {
    Serial.println("Las huellas no coinciden");
    mostrarMensaje("Huellas diferentes");
    delay(2000);
    return;
  } else {
    Serial.println("Error desconocido");
    mostrarMensaje("Error desconocido");
    delay(2000);
    return;
  }

  Serial.print("ID ");
  Serial.println(id);
  p = finger.storeModel(id);  // Almacena el modelo de huella
  if (p == FINGERPRINT_OK) {
    Serial.println("¡Guardado!");
    mostrarMensaje("Guardado!");
    delay(2000);
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    Serial.println("Error de comunicación");
    mostrarMensaje("Error comunicacion");
    delay(2000);
    return;
  } else if (p == FINGERPRINT_BADLOCATION) {
    Serial.println("No se pudo guardar en esa ubicación");
    mostrarMensaje("Error guardardo");
    delay(2000);
    return;
  } else if (p == FINGERPRINT_FLASHERR) {
    Serial.println("Error al escribir en la memoria");
    mostrarMensaje("Error memoria");
    delay(2000);
    return;
  } else {
    Serial.println("Error desconocido");
    mostrarMensaje("Error desconocido");
    delay(2000);
    return;
  }
}

void mostrarMensaje(String mensaje) {
  lcd.clear();
  lcd.setCursor(6, 0);
  lcd.print("EduTrack");
  lcd.setCursor(0, 1);
  lcd.print(mensaje);
}

uint16_t leerHuella() {
  uint8_t p = finger.getImage();  // Captura la imagen de la huella
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Imagen tomada");
      break;
    case FINGERPRINT_NOFINGER:
      Serial.println("No se detectó huella");
      return 9999;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Error de comunicación");
      return 9999;
    case FINGERPRINT_IMAGEFAIL:
      Serial.println("Error en la toma de imagen");
      return 9999;
    default:
      Serial.println("Error desconocido");
      return 9999;
  }

  // Éxito
  p = finger.image2Tz();  // Convierte la imagen en plantilla
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Imagen convertida");
      break;
    case FINGERPRINT_IMAGEMESS:
      Serial.println("Imagen demasiado desordenada");
      return 9999;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Error de comunicación");
      return 9999;
    case FINGERPRINT_FEATUREFAIL:
      Serial.println("No se encontraron características de huella");
      return 9999;
    case FINGERPRINT_INVALIDIMAGE:
      Serial.println("No se encontró una imagen válida");
      return 9999;
    default:
      Serial.println("Error desconocido");
      return 9999;
  }

  mostrarMensaje("LEIDA Y BUSCANDO");
  delay(1000);

  // Imagen convertida correctamente
  p = finger.fingerSearch();  // Busca una huella en la base de datos
  if (p == FINGERPRINT_OK) {
    Serial.println("¡Se encontró una coincidencia de huella!");
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    Serial.println("Error de comunicación");
    mostrarMensaje("Error de comunicacion");
    delay(1000);
    return 9999;
  } else if (p == FINGERPRINT_NOTFOUND) {
    Serial.println("No se encontró una coincidencia");
    mostrarMensaje("Huella no encontrada");
    delay(1000);
    return 9999;
  } else {
    Serial.println("Error desconocido");
    mostrarMensaje("Error desconocido");
    delay(1000);
    return 9999;
  }

  // Se encontró una coincidencia
  Serial.print("ID encontrado # ");
  Serial.print(finger.fingerID);
  Serial.print(" con una confianza de ");
  Serial.println(finger.confidence);

  return finger.fingerID;
}

void fnRegistrarOnline(uint16_t id) {
  String token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJtYXJjYWRvcjEiLCJqdGkiOiI5MDJhODczNi0wNzIxLTRmN2EtYTBjZi04N2VhYzk3NDUzMWEiLCJpYXQiOjE3NjQ1MDYyMTIsInVzdWFyaW9JZCI6IjE4MzUxMjIyLWM5ZWEtNGY0NC1iMGRkLTRjYjVjMmI0YmNiMCIsInN0YW1wU2VjdXJpdHkiOiI2NzAxMDY4OC03NmFmLTQ1MWQtOTZmMy1kMjkyYTg3MjA1OGYiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJNQVJDQURPUiIsImV4cCI6MTc2NTExMTAxMiwiaXNzIjoiR3VhcmRpYXMiLCJhdWQiOiJodHRwczovL2dhc3BlcnNvZnQuY29tIn0.dkjWUvJehHcCsYq40qLiZsbqS6RQJ6VdLqArsorhNsk";
  String gradoId = "fdad7a70-40a9-4972-9c51-57e737f75712";

  // Crear el JSON que enviarás en el cuerpo de la solicitud
  StaticJsonDocument<200> doc;
  doc["grado_id"] = gradoId;
  doc["biometrico_id"] = id;

  String jsonString;
  serializeJson(doc, jsonString);  // Serializa el objeto JSON a un string

  HTTPClient http;

  // Configurar la URL para la solicitud POST
  String url = "https://edutrack.gaspersoft.com/api/asistencia";
  http.begin(url);  // Inicia la conexión HTTP

  // Establecer el encabezado de autorización con el token
  http.addHeader("Content-Type", "application/json");  // Tipo de contenido
  http.addHeader("Authorization", "Bearer " + token);  // Token Bearer

  // Realizar la solicitud POST
  int httpResponseCode = http.POST(jsonString);

  // Verificar si la solicitud fue exitosa
  if (httpResponseCode > 0) {
    Serial.print("Código de respuesta: ");
    Serial.println(httpResponseCode);
    String response = http.getString();  // Obtener la respuesta del servidor
    Serial.println("Respuesta del servidor: ");
    Serial.println(response);  // Mostrar respuesta
  } else {
    Serial.print("Error en la solicitud POST. Código: ");
    Serial.println(httpResponseCode);  // Mostrar error si no se pudo hacer la solicitud
  }

  // Finalizar la conexión
  http.end();
}

void fnLimpiarHuellas() {
  mostrarMensaje("LIMPIANDO HUELLAS...");
  finger.emptyDatabase();
}
