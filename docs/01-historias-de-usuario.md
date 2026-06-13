# Historias de Usuario — Envíos Rápidos GT

Formato utilizado (aprendido en clase):

> **Como** [rol], **quiero** [funcionalidad], **para** [beneficio/valor].
> **Criterios de aceptación** (medibles, formato Dado/Cuando/Entonces).

Roles del sistema:
- **Agente de oficina:** registra y actualiza envíos en una de las 18 oficinas departamentales.
- **Repartidor:** registra intentos de entrega y entregas.
- **Cliente (remitente/destinatario):** consulta el estado de su paquete.
- **Supervisor/Gerente:** consulta reportes de eficiencia.

---

## HU-01: Registrar un envío

**Como** agente de oficina,
**quiero** registrar un nuevo envío con los datos del remitente, destinatario, peso y departamento de destino,
**para** ingresar el paquete al sistema y darle seguimiento.

**Criterios de aceptación:**
- Dado un peso, nombre de remitente y destinatario válidos, cuando registro el envío, entonces el sistema crea el envío con estado inicial `Registrado`.
- Dado un peso menor o igual a 0, cuando intento registrar, entonces el sistema rechaza la operación con un error.
- Cuando el envío se registra, entonces se genera automáticamente un código de rastreo y se calcula la tarifa.

---

## HU-02: Calcular la tarifa automáticamente según el peso

**Como** agente de oficina,
**quiero** que la tarifa se calcule automáticamente según el peso del paquete,
**para** evitar errores de cobro manuales.

**Criterios de aceptación (Regla 1):**
- Dado un peso ≤ 1 kg, entonces la tarifa base es **Q25.00**.
- Dado un peso entre 1.01 y 5 kg, entonces la tarifa base es **Q45.00**.
- Dado un peso entre 5.01 y 10 kg, entonces la tarifa base es **Q75.00**.
- Dado un peso mayor a 10 kg, entonces la tarifa base es **Q100.00**.

---

## HU-03: Aplicar descuento por NIT válido

**Como** cliente con NIT válido (remitente o destinatario),
**quiero** recibir un 5% de descuento sobre la tarifa,
**para** beneficiarme por estar registrado fiscalmente.

**Criterios de aceptación (Regla 7):**
- Dado que el remitente **o** el destinatario tiene un NIT válido, cuando se calcula la tarifa, entonces se aplica un **5% de descuento** sobre la tarifa base.
- Dado un NIT inválido, vacío o "CF" (consumidor final), entonces **no** se aplica descuento.
- El descuento se valida con el algoritmo de dígito verificador del NIT guatemalteco (módulo 11).

---

## HU-04: Generar código de rastreo automático

**Como** agente de oficina,
**quiero** que cada envío reciba un código de rastreo único automáticamente,
**para** que el cliente pueda consultar su paquete sin ambigüedad.

**Criterios de aceptación (Regla 5):**
- Cuando se registra un envío, entonces se genera un código con formato **`ENV-YYYYMMDD-XXXX`**.
- `XXXX` es un correlativo de 4 dígitos por día (0001, 0002, ...).
- El código de rastreo es único en todo el sistema.

---

## HU-05: Rastrear un envío por su código

**Como** cliente,
**quiero** consultar el estado actual de mi paquete usando su código de rastreo,
**para** saber dónde está sin llamar a la oficina.

**Criterios de aceptación:**
- Dado un código de rastreo válido, cuando consulto, entonces obtengo el estado actual, la ubicación y el historial completo.
- Dado un código inexistente, entonces el sistema responde "no encontrado" (HTTP 404).

---

## HU-06: Actualizar el estado de un envío respetando el flujo

**Como** agente de oficina,
**quiero** actualizar el estado del envío únicamente hacia adelante,
**para** mantener la integridad del flujo logístico.

**Criterios de aceptación (Regla 3):**
- Las transiciones válidas son:
  `Registrado → EnTransito → EnReparto → Entregado`, además `EnReparto → Devuelto`, `EnReparto → EnDevolucion → Devuelto`.
- Dado un intento de retroceder o saltar etapas, entonces el sistema rechaza el cambio con un error.
- Dado un envío en estado final (`Entregado` o `Devuelto`), entonces no se permite ningún cambio adicional.

---

## HU-07: Registrar la ubicación en cada cambio de estado

**Como** agente de oficina,
**quiero** indicar la oficina donde ocurre cada actualización,
**para** rastrear por dónde ha pasado el paquete.

**Criterios de aceptación (Regla 4):**
- Dado un cambio de estado, cuando no se indica la ubicación, entonces el sistema rechaza la operación.
- Cuando se actualiza el estado, entonces la ubicación queda registrada como ubicación actual del envío.

---

## HU-08: Registrar intentos de entrega y devolución automática

**Como** repartidor,
**quiero** registrar cada intento de entrega fallido,
**para** que tras el tercer intento el envío pase automáticamente a devolución.

**Criterios de aceptación (Regla 2):**
- Solo se pueden registrar intentos cuando el envío está en estado `EnReparto`.
- Dado el **tercer** intento fallido, entonces el envío cambia automáticamente a `EnDevolucion`.
- No se permiten más de 3 intentos de entrega.

---

## HU-09: Consultar el historial de cambios de un envío

**Como** cliente o agente,
**quiero** ver el historial completo de estados de un envío,
**para** entender la trazabilidad del paquete.

**Criterios de aceptación (Regla 6):**
- Cada cambio de estado registra: **nuevo estado, ubicación, timestamp automático y notas opcionales**.
- El historial se devuelve ordenado cronológicamente.

---

## HU-10: Generar reporte de eficiencia de entrega

**Como** supervisor,
**quiero** un reporte con el porcentaje de envíos entregados vs. devueltos,
**para** medir la eficiencia operativa.

**Criterios de aceptación:**
- El reporte muestra: total de envíos, entregados, devueltos, en proceso y porcentajes.
- Identifica cuántos envíos tuvieron intentos de entrega fallidos.

---

## HU-11: Listar y filtrar envíos por estado

**Como** agente de oficina,
**quiero** listar los envíos y filtrarlos por estado,
**para** gestionar la carga de trabajo diaria (≈500 envíos en temporada alta).

**Criterios de aceptación:**
- Cuando consulto sin filtro, entonces obtengo todos los envíos.
- Cuando filtro por un estado (p. ej. `EnReparto`), entonces obtengo solo los envíos en ese estado.
- Los envíos se ordenan del más reciente al más antiguo.

---

## HU-12: Identificar envíos con múltiples intentos fallidos

**Como** supervisor,
**quiero** identificar rápidamente los envíos con intentos de entrega fallidos,
**para** atender los casos problemáticos antes de que se devuelvan.

**Criterios de aceptación:**
- El sistema mantiene el contador de intentos de entrega por envío.
- El reporte de eficiencia indica cuántos envíos tienen al menos un intento fallido.

---

### Trazabilidad Historia ↔ Regla ↔ Endpoint

| Historia | Regla de negocio | Endpoint principal |
|----------|------------------|--------------------|
| HU-01 | Registro | `POST /api/envios` |
| HU-02 | Regla 1 (tarifa) | `POST /api/envios` |
| HU-03 | Regla 7 (NIT 5%) | `POST /api/envios` |
| HU-04 | Regla 5 (código) | `POST /api/envios` |
| HU-05 | Rastreo | `GET /api/envios/rastreo/{codigo}` |
| HU-06 | Regla 3 (estados) | `PUT /api/envios/{id}/estado` |
| HU-07 | Regla 4 (ubicación) | `PUT /api/envios/{id}/estado` |
| HU-08 | Regla 2 (3 intentos) | `POST /api/envios/{id}/intento-fallido` |
| HU-09 | Regla 6 (historial) | `GET /api/envios/{id}/historial` |
| HU-10 | Reporte | `GET /api/reportes/eficiencia` |
| HU-11 | Gestión | `GET /api/envios?estado=` |
| HU-12 | Identificación | `GET /api/reportes/eficiencia` |
