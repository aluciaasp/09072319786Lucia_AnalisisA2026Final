# Diagrama de Flujo — Proceso de un Envío

Diagrama del proceso completo, desde el registro hasta un estado final
(`Entregado` o `Devuelto`), incluyendo el cálculo de tarifa, el descuento por
NIT y la regla de los 3 intentos de entrega.

> GitHub renderiza Mermaid automáticamente. Para una imagen, pegar el bloque en
> https://mermaid.live

```mermaid
flowchart TD
    A([Inicio]) --> B[Agente registra envío:\nremitente, destinatario, peso, depto.]
    B --> C{¿Peso > 0?}
    C -- No --> C1[/Error: peso inválido/] --> Z([Fin])
    C -- Sí --> D[Calcular tarifa base según peso\n≤1kg=Q25 · 1.01-5=Q45 · 5.01-10=Q75 · >10=Q100]
    D --> E{¿Remitente o destinatario\ncon NIT válido?}
    E -- Sí --> F[Aplicar 5% de descuento]
    E -- No --> G[Tarifa sin descuento]
    F --> H
    G --> H[Generar código ENV-YYYYMMDD-XXXX\nEstado = Registrado\nRegistrar en historial]
    H --> I[EnTransito]
    I --> J[EnReparto]
    J --> K{¿Entrega exitosa?}
    K -- Sí --> L[Entregado]
    K -- No --> M[Registrar intento fallido\nintentos = intentos + 1]
    M --> N{¿intentos = 3?}
    N -- No --> J
    N -- Sí --> O[EnDevolucion\nautomático]
    O --> P[Devuelto]
    L --> Z
    P --> Z

    classDef estado fill:#dbeafe,stroke:#1e40af,color:#1e3a8a;
    classDef final fill:#dcfce7,stroke:#166534,color:#14532d;
    class I,J,O estado;
    class L,P final;
```

## Diagrama de Estados (Regla 3)

Refuerza que los estados solo avanzan en una dirección.

```mermaid
stateDiagram-v2
    [*] --> Registrado
    Registrado --> EnTransito
    EnTransito --> EnReparto
    EnReparto --> Entregado: entrega exitosa
    EnReparto --> Devuelto: devolución directa
    EnReparto --> EnDevolucion: 3er intento fallido (automático)
    EnDevolucion --> Devuelto
    Entregado --> [*]
    Devuelto --> [*]
```
