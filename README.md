# API - NFC

Sistema de gestión de inventario y procesos utilizando tecnología NFC (Near Field Communication) desarrollado con ASP.NET Core.

## 📋 Descripción General

API - NFC es un sistema integral que permite la gestión y seguimiento de elementos de inventario mediante etiquetas NFC. El sistema facilita el control de acceso, registro de procesos y administración de usuarios en un entorno académico o institucional.

## ✨ Características Principales

### 🔐 Sistema de Autenticación
- Autenticación basada en JWT (JSON Web Tokens)
- Login para funcionarios con validación de documentos y contraseñas
- Sesiones con cookies y expiración configurable (60 minutos por defecto)
- Control de acceso basado en roles

### 📡 Integración NFC con SignalR
El sistema utiliza **SignalR** para comunicación en tiempo real entre:
- **Agente NFC** (aplicación de escritorio/dispositivo lector NFC)
- **Cliente Web** (dashboard y panel de administración)

#### Modos de Operación NFC:
1. **Modo Lectura**: Lectura continua de etiquetas NFC para el dashboard
2. **Modo Escritura**: Escritura de datos en etiquetas NFC
3. **Modo Limpieza**: Borrado de datos de etiquetas NFC

#### Eventos SignalR Disponibles:
- `TransmitirDatosTag`: Transmite datos leídos de una etiqueta NFC
- `SetAgentModeToWrite`: Solicita al agente cambiar a modo escritura
- `SetAgentModeToClean`: Solicita al agente cambiar a modo limpieza
- `SetAgentModeToRead`: Solicita al agente volver a modo lectura
- `SendStatusUpdate`: Envía actualizaciones de estado del agente
- `SendOperationSuccess`: Notifica operación exitosa
- `SendOperationFailure`: Notifica fallo en operación

### 👥 Gestión de Usuarios

El sistema maneja diferentes tipos de usuarios:

#### Aprendiz
- Estudiantes o aprendices del sistema
- Asociados a una ficha académica
- Información: Nombre, Documento, Ficha

#### Funcionario
- Personal administrativo o instructores
- Credenciales de acceso al sistema
- Información: Nombre, Documento, Contraseña, Estado

#### Guardia
- Personal de seguridad
- Control de acceso y salida
- Información: Nombre, Documento, Estado

#### Usuario
- Entidad unificada que puede ser Aprendiz o Funcionario
- Gestión centralizada de permisos
- Relación con elementos y procesos

### 📦 Gestión de Inventario

#### Elementos
- Registro de elementos físicos con etiquetas NFC
- Propiedades:
  - Tipo de elemento
  - Serial único
  - Características técnicas y físicas
  - Detalles adicionales
  - Marca
  - Imagen (URL)
  - Propietario (Usuario)
  - Estado de etiqueta NFC

#### Tipos de Elemento
- Categorización de elementos (laptops, tablets, equipos, etc.)
- Facilita la organización del inventario

### 🔄 Gestión de Procesos

#### Procesos
El sistema registra y rastrea diferentes tipos de procesos:
- **Entrada/Salida** de elementos
- **Préstamos** de equipos
- **Devoluciones**
- **Transferencias** entre usuarios
- Timestamp de operaciones
- Portador del elemento
- Relación con otros procesos

#### Tipos de Proceso
- Definición de diferentes categorías de procesos
- Personalización según necesidades institucionales

### 🎓 Módulo Académico

#### Programas
- Gestión de programas de formación
- Información: Nombre, Código, Descripción

#### Fichas
- Agrupación de aprendices por cohortes
- Asociadas a programas académicos
- Información: Número de ficha, Programa

### 🖥️ Interfaces Web

El sistema incluye páginas Razor para:

1. **Login** (`/Login`)
   - Autenticación de usuarios
   - Validación de credenciales

2. **Terminal** (`/Terminal`)
   - Dashboard en tiempo real
   - Visualización de lecturas NFC
   - Control de operaciones NFC

3. **Panel de Administración** (`/Admin`)
   - Gestión de datos
   - Gestión NFC
   - Configuración del sistema

## 🛠️ Tecnologías Utilizadas

- **Framework**: ASP.NET Core 8.0
- **Base de Datos**: SQL Server (Entity Framework Core)
- **Comunicación en Tiempo Real**: SignalR
- **Autenticación**: JWT + Cookies
- **Encriptación**: BCrypt.Net-Next
- **API Documentation**: Swagger/OpenAPI
- **Frontend**: Razor Pages

## 📚 API Endpoints

### Autenticación
```
POST /api/auth/login
```

### Usuarios
```
GET    /api/usuario
GET    /api/usuario/{id}
POST   /api/usuario
PUT    /api/usuario/{id}
DELETE /api/usuario/{id}
```

### Aprendices
```
GET    /api/aprendiz
GET    /api/aprendiz/{id}
POST   /api/aprendiz
PUT    /api/aprendiz/{id}
DELETE /api/aprendiz/{id}
```

### Funcionarios
```
GET    /api/funcionario
GET    /api/funcionario/{id}
POST   /api/funcionario
PUT    /api/funcionario/{id}
DELETE /api/funcionario/{id}
```

### Guardias
```
GET    /api/guardia
GET    /api/guardia/{id}
POST   /api/guardia
PUT    /api/guardia/{id}
DELETE /api/guardia/{id}
```

### Elementos (Inventario)
```
GET    /api/elemento
GET    /api/elemento/{id}
POST   /api/elemento
PUT    /api/elemento/{id}
DELETE /api/elemento/{id}
```

### Tipos de Elemento
```
GET    /api/tipoelemento
GET    /api/tipoelemento/{id}
POST   /api/tipoelemento
PUT    /api/tipoelemento/{id}
DELETE /api/tipoelemento/{id}
```

### Procesos
```
GET    /api/proceso
GET    /api/proceso/{id}
POST   /api/proceso
PUT    /api/proceso/{id}
DELETE /api/proceso/{id}
```

### Tipos de Proceso
```
GET    /api/tipoproceso
GET    /api/tipoproceso/{id}
POST   /api/tipoproceso
PUT    /api/tipoproceso/{id}
DELETE /api/tipoproceso/{id}
```

### Programas Académicos
```
GET    /api/programa
GET    /api/programa/{id}
POST   /api/programa
PUT    /api/programa/{id}
DELETE /api/programa/{id}
```

### Fichas
```
GET    /api/ficha
GET    /api/ficha/{id}
POST   /api/ficha
PUT    /api/ficha/{id}
DELETE /api/ficha/{id}
```

## 🔧 Configuración

### Requisitos Previos
- .NET 8.0 SDK
- SQL Server
- Dispositivo lector NFC (para funcionalidad completa)

### Configuración de Base de Datos

Actualiza la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tu-servidor;Database=NFCDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Configuración JWT

Configura las credenciales JWT en `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "tu-clave-secreta-muy-segura-de-al-menos-32-caracteres",
    "Issuer": "API-NFC",
    "Audience": "API-NFC-Client"
  }
}
```

### Migraciones de Base de Datos

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Ejecutar la Aplicación

```bash
cd "API - NFC"
dotnet restore
dotnet run
```

La aplicación estará disponible en:
- **HTTPS**: https://localhost:7xxx
- **HTTP**: http://localhost:5xxx
- **Swagger**: https://localhost:7xxx/swagger

## 🔒 Seguridad

### Características de Seguridad Implementadas:
- Autenticación JWT con expiración de tokens (2 horas)
- Validación de contraseñas (se recomienda hash en producción)
- CORS configurado para permitir credenciales
- HTTPS redirection habilitado
- Protección de rutas con autorización

### Recomendaciones:
⚠️ **IMPORTANTE**: El código actual almacena contraseñas en texto plano. En producción, se debe:
- Implementar hash de contraseñas con BCrypt
- Usar variables de entorno para claves sensibles
- Implementar rate limiting
- Añadir validación de fuerza de contraseñas
- Implementar 2FA para accesos críticos

## 📊 Modelo de Datos

### Esquema Principal

```
Usuario
├── Aprendiz
│   └── Ficha
│       └── Programa
├── Funcionario
└── Relaciones
    ├── Elementos (como Propietario)
    └── Procesos (como Portador)

Elemento
├── TipoElemento
├── Propietario (Usuario)
└── TieneNFCTag

Proceso
├── TipoProceso
├── Elemento
├── Portador (Usuario)
└── RequiereOtroProceso
```

## 🚀 Flujos de Trabajo Comunes

### 1. Registro de Entrada/Salida con NFC
```
1. Usuario coloca elemento con etiqueta NFC en el lector
2. Agente NFC lee la etiqueta → Envía datos vía SignalR
3. Sistema identifica el elemento y usuario
4. Se registra proceso de entrada/salida
5. Dashboard se actualiza en tiempo real
```

### 2. Asignación de Elemento a Usuario
```
1. Administrador crea/selecciona elemento
2. Escribe ID en etiqueta NFC usando modo escritura
3. Asigna propietario en el sistema
4. Elemento queda vinculado al usuario
```

### 3. Préstamo de Equipo
```
1. Funcionario inicia sesión
2. Selecciona usuario destino (aprendiz)
3. Escanea elemento con NFC
4. Sistema crea proceso de préstamo
5. Se notifica al usuario
```

## 🤝 Contribuciones

Este proyecto está en desarrollo activo. Para contribuir:

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Notas de Desarrollo

- El proyecto usa `ReferenceHandler.IgnoreCycles` para evitar ciclos de referencia en JSON
- SignalR Hub está configurado en `/nfcHub`
- Todas las entidades tienen un campo `Estado` para soft-delete
- Las relaciones entre entidades usan Foreign Keys explícitas

## 📄 Licencia

Este proyecto es de código abierto. Consulta con el propietario del repositorio para más información sobre licencias.

## 👨‍💻 Autor

**Cesar Gaviria S.**

---

**Última actualización**: Octubre 2025
