# Proyecto Taller .NET <br> "Sistema de Gestión de Eventos Deportivos" <br> 🏃‍♀️‍➡️🏃‍➡️🏃‍♂️‍➡️🥇

Sistema desarrollado en el ecosistema de .NET - Aspire, encargado de gestionar aspectos operativos de las carreras deportivas y enriquecer la expericencia de los corredores/espectadores.

## 🧩 Funcionalidades principales

| Funcionalidad | Descripción Breve | 
|---------------|---|
| Registro de Carreras | Alta y configuración de nuevos eventos deportivos. |
| Inscripción de Participantes | Registro de corredores en carreras disponibles. |
| Monitoreo en Tiempo Real | Display de progreso y datos de la carrera en vivo. |
| Gestión de Equipamiento | Administración de la entrega de kits a corredores. | 

## 📊 Casos de Uso Principales

A continuación se describen los casos de usos principales en base a las funcionalidades. Haz clic en cada uno para ver los detalles y los **diagramas de secuencia**.

<details>
<summary><strong>📝 1. Registrar carrera</strong></summary>

- **Actor:** `Administrador`
- **Descripción:** Alta de una nueva carrera en el sistema, especificando sus detalles, recorrido y puntos de entrega de equipamiento.
- **Flujo Principal:**
  1. El administrador accede al panel de administración.
  2. Completa el formulario de registro de carrera.
  3. Envía el formulario y recibe una confirmación.
- **Diagrama de secuencia:**
<img width="1295" height="719" alt="image" src="https://github.com/user-attachments/assets/932849e0-bda5-474f-a42a-f4669310e0c4" />


</details>

<details>
<summary><strong>📋 2. Listar carreras</strong></summary>

- **Actor:** `Usuario`
- **Descripción:** Visualización del listado de todas las carreras disponibles.
- **Flujo Principal:**
  1. El usuario accede a la página principal.
  2. Navega a la sección de carreras.
  3. Visualiza la lista de carreras disponibles con detalles básicos.
<img width="909" height="451" alt="imagen" src="https://github.com/user-attachments/assets/a6b37618-f96a-42d7-abc7-335ec0d285c3" />

</details>

<details>
<summary><strong>✍️ 3. Inscribirse a carrera</strong></summary>

- **Actor:** `Usuario`
- **Descripción:** Inscripción de un corredor a una carrera disponible.
- **Flujo Principal:**
  1. El usuario navega a la sección de carreras disponibles.
  2. Selecciona una carrera y completa el formulario de inscripción.
  3. Envía el formulario y recibe una confirmación.
  <img width="1036" height="473" alt="{59F9F232-E737-41D1-9168-7F99823D02BC}" src="https://github.com/user-attachments/assets/70460236-c935-49f5-87d7-f4af189d45ab" />


</details>

<details>
<summary><strong>ℹ️ 4. Consultar datos de carrera (no iniciada)</strong></summary>

- **Actor:** `Usuario`
- **Descripción:** Visualización de los detalles completos de una carrera que aún no ha comenzado.
- **Flujo Principal:**
  1. El usuario selecciona una carrera no iniciada.
  2. Visualiza detalles como fecha, hora, lugar y participantes inscritos.
<img width="1019" height="451" alt="imagen" src="https://github.com/user-attachments/assets/418190fd-b97f-4c2c-8c98-f0fc74df07b5" />

</details>

<details>
<summary><strong>📈 5. Monitorear carrera (en curso)</strong></summary>

- **Actor:** `Usuario`
- **Descripción:** Visualización de datos en tiempo real de una carrera, incluyendo el progreso de un participante específico.
- **Flujo Principal:**
  1. El usuario selecciona una carrera en curso.
  2. Visualiza datos en tiempo real como posiciones, tiempos y estadísticas.
<img width="1434" height="570" alt="{D1DC018F-5EBD-475D-BAFF-90E6C5960C45}" src="https://github.com/user-attachments/assets/b90a24ad-5596-4ec0-9329-ba9f18bdd596" />

</details>

<details>
<summary><strong>📦 6. Entrega de equipamiento</strong></summary>

- **Actor:** `Administrador`, `Usuario`
- **Descripción:** Gestión y registro de la entrega de equipamiento a los corredores inscritos.
- **Flujo Principal:**
  1. Los puntos de entrega se definen en el registro de la carrera.
  2. El usuario se inscribe a la carrera y elige el punto de entrega.
  3. El sistema actualiza un `JSON` con los datos de entrega del nuevo participante (incluye link de confirmación de entrega y pago).
<img width="1088" height="544" alt="{EAF45A97-C3DF-45E6-8DDC-D24A33F708E1}" src="https://github.com/user-attachments/assets/5d593d8e-2fac-471a-9705-204724de2e66" />

</details>

<details>
<summary><strong>🤖 7. Simular carrera</strong></summary>

- **Actor:** `Sistema`, `Administrador`
- **Descripción:** Simulación de lectura de chips RFID para generar eventos de tiempo y enviarlos al bus de mensajes.
- **Flujo Principal:**
  1. El administrador da por iniciada la carrera y la simulación comienza.
  2. El servicio en segundo plano genera eventos de tiempo para corredores.
  3. Los eventos son enviados al bus de mensajes para su procesamiento.
  4. El sistema actualiza los datos de la carrera en tiempo real.
<img width="796" height="379" alt="{B2AB4E83-87F6-43D2-A923-FE776F652F39}" src="https://github.com/user-attachments/assets/22183344-dcfa-426e-b96d-803e731d27c5" />

</details>

## 🛠️ Arquitectura y Tecnologías

El siguiente diagrama ilustra la arquitectura del sistema, destacando los componentes clave y sus interacciones:

<img width="1422" height="681" alt="Diagrama-arquitectura drawio" src="https://github.com/user-attachments/assets/352c0ff3-8782-4255-85b3-a1183add27b4" />

### Stack Tecnológico y Decisiones Clave 🎯

La arquitectura se basa en un conjunto de tecnologías y patrones seleccionados para garantizar escalabilidad, resiliencia y una excelente experiencia de usuario. A continuación, se detalla el stack tecnológico:

| Área Clave | Tecnología / Patrón | Propósito y Justificación |
| :--- | :--- | :--- |
| **Plataforma Base** | `.NET 8` | Ecosistema de desarrollo unificado, moderno y de alto rendimiento para todos los componentes del sistema. |
| **Arquitectura** | `Microservicios` | Permite que cada servicio (carreras, inscripciones, etc.) evolucione, se despliegue y escale de forma independiente. |
| **Orquestación** | `.NET Aspire` | Orquestación nativa para simplificar el desarrollo, la configuración y el despliegue de la arquitectura de microservicios. |
| **Frontend (UX)** | `Blazor` sobre `ASP.NET Core` | Creación de interfaces web interactivas y en tiempo real con C#, ofreciendo una experiencia de usuario fluida y moderna. |
| **Mensajería Asíncrona**| `RabbitMQ` (Bus de Mensajes) | Desacopla los servicios y garantiza la resiliencia en el manejo de eventos masivos (ej. tiempos de carrera en tiempo real). |
| **Persistencia de Datos** | `Entity Framework Core` + `SQLite` | Abstracción de la base de datos que facilita el desarrollo (con SQLite) y la migración a sistemas robustos en producción (ej. PostgreSQL). |
| **Tareas en Segundo Plano**| `Worker Service` | Simulación de hardware (lectura de chips RFID) y generación de datos en tiempo real de forma asíncrona y desacoplada de la UI. |

## 🖼️ Maquetado de la Interfaz de Usuario y Administrador

### Interfaz de Usuario y Administrador
<a href="https://wireframe.cc/xaQ82H" target="_blank">Ver Diagrama</a>


# 🗓️ Planificación de Iteraciones

> **Duración total:** 6 semanas  
> **Metodología:** Iterativa e incremental (inspirada en Scrum)  
> **Objetivo:** Desarrollar un sistema de gestión y monitoreo de carreras con arquitectura distribuida, comunicación por eventos y visualización en tiempo real.

---

## 🔍 Iteración 0 – Estudio, análisis y diseño
**Duración:** Semana 1  
**Objetivo:** Comprender las tecnologías base y definir la arquitectura del sistema.

### Actividades principales
- Investigar **.NET Aspire**, **RabbitMQ**, **SignalR** y **Worker Services**.  
- Diseñar la arquitectura de microservicios y la comunicación por eventos.  
- Definir modelos de datos, diagramas de secuencia y casos de uso.  
- Configurar entorno base de desarrollo (repositorios, Docker, Aspire).  

### Entregables
- Documento de análisis y diseño.  
- Entorno base funcionando con contenedores y configuración inicial.

---

## ⚙️ Iteración 1 – Prueba de eventos y simulación base
**Duración:** Semanas 2–3  
**Objetivo:** Implementar la base técnica de eventos y la simulación inicial de carrera.

### Actividades principales
- Configurar **Event Bus (RabbitMQ)** y probar comunicación entre servicios.  
- Desarrollar el **WorkerService** que genera eventos simulados (lecturas RFID).  
- Implementar **SignalR** para comunicación en tiempo real.  
- Crear endpoints para recibir y mostrar eventos de carrera.  

### Casos de uso implementados
- 🤖 **Simular carrera (versión inicial)**  

### Entregables
- Comunicación por eventos funcionando entre servicios.  
- Simulación básica en tiempo real visible desde la interfaz.  
- Base sólida para el resto del sistema.

---

## 🏗️ Iteración 2 – Gestión de carreras e inscripciones
**Duración:** Semanas 4–5  
**Objetivo:** Implementar las funcionalidades principales del sistema.

### Actividades principales
- Crear microservicio `Carrera API` con operaciones CRUD.  
- Desarrollar `Inscripción API` y formularios en Blazor Web App.  
- Integrar los eventos con las carreras e inscripciones.  
- Permitir monitoreo en tiempo real con **SignalR** y datos del bus.  

### Casos de uso implementados
- 📝 Registrar carrera  
- 📋 Listar carreras  
- ✍️ Inscribirse a carrera  
- 📈 Monitorear carrera  

### Entregables
- Carreras reales con inscripciones activas.  
- Interfaz que permite registrar, listar e inscribirse a carreras.  
- Monitoreo en tiempo real de la simulación vinculada a cada carrera.

---

## 🎯 Iteración 3 – Entrega de equipamiento y cierre del sistema
**Duración:** Semana 6  
**Objetivo:** Completar funcionalidades complementarias y preparar la entrega final.

### Actividades principales
- Implementar caso de uso **📦 Entrega de equipamiento**.  
- Actualizar datos de entrega (JSON y confirmaciones).  
- Incorporar métricas y observabilidad con **Aspire Dashboard**.  
- Pruebas de integración y mejoras finales en la interfaz.  

### Casos de uso implementados
- 📦 Entrega de equipamiento  
- ℹ️ Consultar datos de carrera (versión final)  








