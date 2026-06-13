# Informe de Uso de Inteligencia Artificial

Criterio de rúbrica: **Uso de IA (20 pts) — documentación y reflexión.**

Este documento describe cómo se utilizó una herramienta de IA (asistente de
programación) durante el desarrollo del prototipo, qué prompts se enviaron, qué
correcciones fueron necesarias y una reflexión final sobre la experiencia.

---

## 1. Herramienta utilizada

- **Asistente de IA de programación** (modelo tipo LLM con acceso a terminal).
- Uso: generación de código base, redacción de documentación, creación de
  pruebas y verificación de la API.
- **Criterio personal:** la IA se usó como apoyo, validando cada salida
  ejecutando el código realmente (compilación, pruebas y llamadas HTTP), no
  aceptando el resultado "a ciegas".

---

## 2. Prompts enviados (resumen cronológico)

A continuación los prompts representativos enviados durante el desarrollo:

1. **Prompt inicial (análisis del enunciado):**
   > "Necesito realizar este proyecto, analízalo y hazlo por partes que cumplan
   > con lo requerido." (acompañado de la foto del enunciado.)

2. **Definición de arquitectura:**
   > "Crea una solución .NET con un proyecto Web API y otro de pruebas xUnit,
   > usando EF Core con SQLite."

3. **Modelado del dominio:**
   > "Define los modelos Cliente, Envio e HistorialEstado y un enum de estados
   > según el flujo Registrado → EnTransito → EnReparto → Entregado / Devuelto /
   > EnDevolucion."

4. **Lógica de tarifa y descuento:**
   > "Implementa el cálculo de tarifa por peso (Q25/Q45/Q75/Q100) y un descuento
   > del 5% cuando el remitente o el destinatario tienen un NIT válido."

5. **Máquina de estados:**
   > "Implementa una máquina de estados que solo permita avanzar en una
   > dirección y que rechace retrocesos o saltos."

6. **Regla de intentos:**
   > "Al registrar el tercer intento de entrega fallido, el envío debe pasar
   > automáticamente a EnDevolucion."

7. **Pruebas:**
   > "Escribe pruebas unitarias xUnit que cubran los límites de la tarifa, la
   > validación de NIT, la máquina de estados y la regla de los 3 intentos."

8. **Despliegue:**
   > "Crea un Dockerfile y un render.yaml para desplegar en Render.com leyendo el
   > puerto desde la variable PORT."

9. **Documentación:**
   > "Redacta 10+ historias de usuario en formato de clase, un diagrama de flujo
   > y uno de secuencia en Mermaid, y el README con instrucciones."

---

## 3. Correcciones y decisiones realizadas

Durante el proceso fue necesario revisar y corregir varias salidas de la IA:

| # | Situación detectada | Corrección aplicada |
|---|---------------------|---------------------|
| 1 | El validador de NIT debía coincidir con el algoritmo **real** del dígito verificador guatemalteco (módulo 11), no una validación superficial de longitud. | Se implementó el algoritmo módulo 11 y se verificó que `1234567-9` resultara válido de forma consistente, usándolo en las pruebas. |
| 2 | Los **límites** de la tarifa (1.0 vs 1.01, 5.0 vs 5.01, 10.0 vs 10.01) son el punto típico de error. | Se usó `switch` con `<=` y se agregaron pruebas `[Theory]` explícitas en cada frontera. |
| 3 | El uso de `DateTime.Now` haría las pruebas no deterministas (el código de rastreo depende de la fecha). | Se introdujo una abstracción `IClock` con un `FakeClock` para los tests, fijando la fecha a 2026-06-13. |
| 4 | El descuento debía redondearse a 2 decimales para representar quetzales correctamente. | Se aplicó `Math.Round(..., 2, MidpointRounding.AwayFromZero)`. |
| 5 | Render asigna el puerto vía variable de entorno `PORT`; por defecto la app no lo escuchaba. | Se leyó `PORT` en `Program.cs` y se configuró `UseUrls("http://0.0.0.0:{PORT}")`. |
| 6 | Las excepciones de regla de negocio devolvían 500 en vez de 400. | Se agregó un middleware (`ManejadorErroresMiddleware`) que traduce `ReglaNegocioException` a HTTP 400. |
| 7 | Se debía garantizar que un intento fallido solo aplicara en estado `EnReparto`. | Se agregó la validación de estado antes de incrementar el contador de intentos. |

---

## 4. Verificación (no solo confiar en la IA)

Para no aceptar el código sin comprobarlo, se ejecutó realmente:

- `dotnet build` → **compilación correcta, 0 errores**.
- `dotnet test` → **49 pruebas correctas**.
- `dotnet run` + llamadas `curl` reales que confirmaron:
  - Tarifa Q45 con peso 2 kg y descuento NIT → **Q42.75**.
  - Código generado `ENV-20260613-0001`.
  - Tercer intento fallido → estado `EnDevolucion` automático.
  - Transición inválida (`Registrado → Entregado`) → **HTTP 400** con mensaje claro.

---

## 5. Reflexión

El uso de IA aceleró notablemente las tareas repetitivas: andamiaje del
proyecto, redacción de DTOs, documentación y casos de prueba base. Sin embargo,
la calidad final dependió del **criterio humano** para:

- Traducir correctamente las **reglas de negocio** del enunciado (especialmente
  los límites de la tarifa y el flujo de estados, que son fáciles de equivocar).
- **Verificar ejecutando**: varias suposiciones de la IA solo se validaron al
  correr las pruebas y la API real.
- Tomar decisiones de **diseño y testabilidad** (inyección de `IClock`, manejo
  centralizado de errores) que la IA no propone por defecto.

**Conclusión:** la IA es una herramienta de productividad valiosa, pero no
sustituye el entendimiento del problema ni la validación. El mayor valor se
obtuvo combinando la generación rápida con una revisión crítica y pruebas
automatizadas que respaldan cada regla de negocio.
