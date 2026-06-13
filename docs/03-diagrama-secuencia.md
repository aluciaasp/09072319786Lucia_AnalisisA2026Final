# Diagrama de Secuencia

Criterio de rúbrica: **Diagrama de secuencia (5 pts) — exactitud y coherencia.**

## Secuencia: Registrar un envío

Muestra la interacción entre el cliente HTTP, el controlador, los servicios de
negocio y la base de datos al registrar un envío, incluyendo el cálculo de la
tarifa, el descuento por NIT y la generación del código de rastreo.

![Diagrama de secuencia - Registrar envío](img/diagrama-secuencia.png)

### Código fuente (Mermaid)

> GitHub renderiza Mermaid automáticamente. Para regenerar la imagen, pegar el
> bloque en https://mermaid.live

```mermaid
sequenceDiagram
    actor Agente as Agente de oficina
    participant API as EnviosController
    participant Svc as EnvioService
    participant Tar as TarifaService
    participant Nit as NitValidator
    participant DB as SQLite (EF Core)

    Agente->>API: POST /api/envios (remitente, destinatario, peso, depto)
    API->>Svc: RegistrarEnvioAsync(dto)
    Svc->>Tar: CalcularTarifaBase(peso)
    Tar-->>Svc: tarifaBase (Q25/45/75/100)
    Svc->>Tar: AplicaDescuento(nitRem, nitDest)
    Tar->>Nit: EsValido(nit)
    Nit-->>Tar: true/false
    Tar-->>Svc: aplicaDescuento
    Svc->>Tar: AplicarDescuentoNit(tarifaBase, nits)
    Tar-->>Svc: tarifaFinal
    Svc->>DB: COUNT envíos del día (correlativo)
    DB-->>Svc: n
    Note over Svc: Genera ENV-YYYYMMDD-XXXX\nEstado = Registrado\nAgrega registro al historial
    Svc->>DB: SaveChanges (Envio + Historial)
    DB-->>Svc: OK
    Svc-->>API: Envio
    API-->>Agente: 201 Created + EnvioDto
```
