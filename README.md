# Envíos Rápidos GT — API REST

Examen Final · Análisis de Sistemas 2026

Sistema de gestión y rastreo de envíos para **Envíos Rápidos GT**, empresa de
logística y paquetería con cobertura en 18 departamentos de Guatemala y ≈500
envíos diarios en temporada alta. Resuelve el seguimiento manual, los paquetes
"perdidos" sin estado actualizado, el control de intentos de entrega y la
generación de reportes de eficiencia.

> **Reemplaza `0907-23-19786` por tu carné** al crear el repositorio:
> `https://github.com/aluciaasp/09072319786Lucia_AnalisisA2026Final.git`.

---

## 🧱 Tecnologías

| Componente | Tecnología |
|------------|-----------|
| Lenguaje / Framework | C# · ASP.NET Core Web API (.NET 9) |
| Persistencia | Entity Framework Core + **SQLite** |
| Documentación API | Swagger / OpenAPI |
| Pruebas | **xUnit** + EF Core InMemory |
| Despliegue | **Render.com** (Docker) |

---

## 📁 Estructura del proyecto

```
EnviosRapidosGT.sln
├── src/EnviosRapidosGT.Api/
│   ├── Models/        # Cliente, Envio, HistorialEstado, EstadoEnvio (enum)
│   ├── Data/          # AppDbContext (EF Core + SQLite)
│   ├── Services/      # TarifaService, MaquinaEstados, NitValidator, EnvioService
│   ├── Dtos/          # Contratos de entrada/salida + mapper
│   ├── Controllers/   # EnviosController (endpoints REST)
│   └── Program.cs     # Configuración, DI, Swagger, manejo de errores
├── tests/EnviosRapidosGT.Tests/   # Pruebas unitarias xUnit (49 pruebas)
├── docs/              # Historias de usuario, diagramas, informe de IA
├── Dockerfile         # Imagen para Render
└── render.yaml        # Blueprint de despliegue
```

---

## ▶️ Cómo ejecutar localmente

Requisito: **.NET SDK 9** (o superior).

```bash
# 1. Restaurar y compilar
dotnet build

# 2. Ejecutar la API
dotnet run --project src/EnviosRapidosGT.Api

# 3. Abrir Swagger en el navegador
#    http://localhost:5xxx/swagger   (el puerto aparece en la consola)
```

La base de datos SQLite (`envios.db`) se crea automáticamente al arrancar.

---

## 🧪 Cómo ejecutar las pruebas

```bash
dotnet test
```

Resultado esperado: **49 pruebas correctas**. Cubren:

- Cálculo de tarifa en todos los rangos y sus **límites** (1.0, 1.01, 5.0, 5.01, 10.0, 10.01 kg).
- Validación de NIT (válido, inválido, "CF", vacío) y aplicación del 5%.
- Máquina de estados: transiciones válidas, retrocesos y saltos rechazados, estados finales.
- Generación y correlativo del código `ENV-YYYYMMDD-XXXX`.
- Regla de 3 intentos → devolución automática a `EnDevolucion`.
- Reporte de eficiencia.

---

## 🌐 Endpoints

Base: `/api/envios`

| # | Método | Ruta | Descripción |
|---|--------|------|-------------|
| 1 | POST | `/api/envios` | Registrar envío (calcula tarifa + genera código) |
| 2 | GET | `/api/envios?estado={estado}` | Listar envíos (filtro opcional por estado) |
| 3 | GET | `/api/envios/{id}` | Obtener envío por Id |
| 4 | GET | `/api/envios/rastreo/{codigo}` | Rastrear por código de rastreo |
| 5 | GET | `/api/envios/{id}/historial` | Historial de cambios de estado |
| 6 | PUT | `/api/envios/{id}/estado` | Actualizar estado (valida máquina de estados) |
| 7 | POST | `/api/envios/{id}/intento-fallido` | Registrar intento fallido |
| 8 | GET | `/api/reportes/eficiencia` | Reporte de eficiencia de entrega |

Adicionales: `GET /` (info), `GET /health` (health check), `GET /swagger` (docs).

### Ejemplos con `curl`

**Registrar un envío** (remitente con NIT válido → 5% de descuento):

```bash
curl -X POST http://localhost:5080/api/envios \
  -H "Content-Type: application/json" \
  -d '{
        "remitente":    {"nombre":"Carlos López","nit":"1234567-9"},
        "destinatario": {"nombre":"Ana Díaz","nit":"CF"},
        "pesoKg": 2.0,
        "departamentoDestino": "Quetzaltenango"
      }'
# → tarifaBase: 45, descuentoNitAplicado: true, tarifaFinal: 42.75
#   codigoRastreo: "ENV-YYYYMMDD-0001", estadoActual: "Registrado"
```

**Avanzar el estado** (los valores de estado son: 0=Registrado, 1=EnTransito,
2=EnReparto, 3=Entregado, 4=EnDevolucion, 5=Devuelto):

```bash
curl -X PUT http://localhost:5080/api/envios/1/estado \
  -H "Content-Type: application/json" \
  -d '{"nuevoEstado":1,"ubicacion":"Bodega Central GT"}'
```

**Registrar intento fallido** (al 3.º pasa automáticamente a `EnDevolucion`):

```bash
curl -X POST http://localhost:5080/api/envios/1/intento-fallido \
  -H "Content-Type: application/json" \
  -d '{"ubicacion":"Oficina Xela","notas":"Cliente ausente"}'
```

**Rastrear:**

```bash
curl http://localhost:5080/api/envios/rastreo/ENV-20260613-0001
```

---

## 📜 Reglas de negocio implementadas

| Regla | Descripción | Dónde |
|-------|-------------|-------|
| 1 | Tarifa por peso (Q25 / Q45 / Q75 / Q100) | `TarifaService` |
| 2 | Máximo 3 intentos → `EnDevolucion` automático | `EnvioService` |
| 3 | Estados avanzan en una sola dirección | `MaquinaEstados` |
| 4 | Cada cambio registra la ubicación (oficina) | `EnvioService` / DTOs |
| 5 | Código `ENV-YYYYMMDD-XXXX` automático | `EnvioService` |
| 6 | Historial: estado, ubicación, timestamp, notas | `HistorialEstado` |
| 7 | 5% de descuento con NIT válido (remitente o destinatario) | `TarifaService` + `NitValidator` |

---

## 🚀 Despliegue en Render.com

Este proyecto se despliega con **Docker** (incluye `Dockerfile` y `render.yaml`).

1. Sube el repositorio a GitHub.
2. En Render: **New + → Web Service** → conecta el repositorio.
3. Render detecta el `Dockerfile` (o usa el Blueprint `render.yaml`).
   - Runtime: **Docker**
   - Health Check Path: `/health`
   - Plan: Free
4. Render asigna la variable `PORT`; la aplicación la lee en `Program.cs` y
   escucha en `0.0.0.0:$PORT`.
5. Al terminar, la API queda disponible en
   `https://<tu-servicio>.onrender.com` y Swagger en `/swagger`.

> Alternativa sin Docker: crear un Web Service con build
> `dotnet publish src/EnviosRapidosGT.Api -c Release -o out` y start
> `dotnet out/EnviosRapidosGT.Api.dll`.

---

## 📚 Documentación adicional

- [Historias de usuario](docs/01-historias-de-usuario.md) (12)
- [Diagrama de flujo y de estados](docs/02-diagrama-flujo.md)
- [Diagrama de secuencia](docs/03-diagrama-secuencia.md)
- [Informe de uso de IA](docs/04-informe-uso-ia.md)
