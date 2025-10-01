# Proyecto Taller .NET <br> "Sistema de Gestión de Eventos Deportivos" <br> 🏃‍♀️‍➡️🏃‍➡️🏃‍♂️‍➡️🥇

Sistema desarrollado en el ecosistema de .NET - Aspire, encargado de gestionar aspectos operativos de las carreras deportivas y enriquecer la expericencia de los corredores/espectadores.

## 🧩 Funcionalidades principales

| Funcionalidad | Descripción Breve | Actores |
|---------------|---|---|
| Registro de Carreras | Alta y configuración de nuevos eventos deportivos. | Administrador |
| Inscripción de Participantes | Registro de corredores en carreras disponibles. | Usuarios |
| Monitoreo en Tiempo Real | Display de progreso y datos de la carrera en vivo. | Usuarios |
| Gestión de Equipamiento | Administración de la entrega de kits a corredores. | Administrador |

## 📊 Casos de Uso Principales

A continuación se describen los casos de usos principales en base a las funcionalidades.

### 1\. Registrar carrera

  - **Actor:** Administrador
  - **Descripción:** Alta de una nueva carrera en el sistema, especificando sus detalles, recorrido y puntos de entrega de equipamiento.
  - **Flujo Principal:**
    1.  El administrador accede al panel de administración.
    2.  Completa el formulario de registro de carrera.
    3.  Envía el formulario y recibe una confirmación.

### 2\. Listar carreras

  - **Actor:** Usuario
  - **Descripción:** Visualización del listado de todas las carreras disponibles.
  - **Flujo Principal:**
    1.  El usuario accede a la página principal.
    2.  Navega a la sección de carreras.
    3.  Visualiza la lista de carreras disponibles con detalles básicos.

### 3\. Inscribirse a carrera

  - **Actor:** Usuario
  - **Descripción:** Inscripción de un corredor a una carrera disponible.
  - **Flujo Principal:**
    1.  El usuario navega a la sección de carreras disponibles.
    2.  Selecciona una carrera y completa el formulario de inscripción.
    3.  Envía el formulario y recibe una confirmación.

### 4\. Consultar datos de carrera (no iniciada)

  - **Actor:** Usuario
  - **Descripción:** Visualización de los detalles completos de una carrera que aún no ha comenzado.
  - **Flujo Principal:**
    1.  El usuario selecciona una carrera no iniciada.
    2.  Visualiza detalles como fecha, hora, lugar y participantes inscritos.

### 5\. Monitorear carrera (en curso)

  - **Actor:** Usuario
  - **Descripción:** Visualización de datos en tiempo real de una carrera, incluyendo el progreso de un participante específico.
  - **Flujo Principal:**
    1.  El usuario selecciona una carrera en curso.
    2.  Visualiza datos en tiempo real como posiciones, tiempos y estadísticas.

### 6\. Entrega de equipamiento

  - **Actor:** Administrador, Usuario
  - **Descripción:** Gestión y registro de la entrega de equipamiento a los corredores inscritos.
  - **Flujo Principal:**
    1.  Los puntos de entrega se definen en el registro de la carrera.
    2.  El usuario se inscribe a la carrera y elige el punto de entrega.
    3.  El sistema actualiza un JSON con los datos de entrega del nuevo participante.

### 7\. Simular carrera

  - **Actor:** Sistema (Servicio en Segundo Plano), Administrador
  - **Descripción:** Simulación de lectura de chips RFID para generar eventos de tiempo y enviarlos al bus de mensajes.
  - **Flujo Principal:**
    1.  El administrador da por iniciada la carrera y la simulación comienza.
    2.  El servicio en segundo plano genera eventos de tiempo para corredores.
    3.  Los eventos son enviados al bus de mensajes para su procesamiento.
    4.  El sistema actualiza los datos de la carrera en tiempo real.

## 🛠️ Arquitectura y Tecnologías

El siguiente diagrama ilustra la arquitectura del sistema, destacando los componentes clave y sus interacciones:
<img width="1422" height="681" alt="Diagrama-arquitectura drawio" src="https://github.com/user-attachments/assets/352c0ff3-8782-4255-85b3-a1183add27b4" />


### Decisiones Clave y Stack Tecnológico 🎯

El diseño del sistema se fundamenta en las siguientes elecciones, que definen tanto la arquitectura como el stack tecnológico utilizado:

  * **Escalabilidad y Orquestación:** Se adopta una arquitectura de **microservicios** orquestada por **.NET Aspire** para optimizar el despliegue, la gestión y la escalabilidad de cada servicio de forma independiente.
  * **Resiliencia y Concurrencia:** Se garantiza la resiliencia y el manejo eficiente de eventos masivos (como la toma de tiempos) mediante un bus de mensajes asíncrono (**RabbitMQ**), que desacopla la recepción del procesamiento.
  * **Experiencia del Usuario (UX):** Se implementan interfaces web modernas utilizando **Blazor** sobre **ASP.NET Core** para ofrecer una experiencia interactiva y en tiempo real.
  * **Simulación de Hardware:** Se utiliza un **Servicio en Segundo Plano** (*Worker Service*) para simular la lectura de chips RFID y la inyección de eventos de tiempo al Bus de Mensajes.
  * **Persistencia de Datos:** Se emplea **SQLite** para el almacenamiento local durante el desarrollo, con la flexibilidad de migrar a bases de datos más robustas en producción gracias a **Entity Framework Core**.
  * **Plataforma de Desarrollo:** Todo el ecosistema está construido sobre **.NET 8**.

## 🖼️ Maquetado de la Interfaz de Usuario y Administrador

### Interfaz de Usuario

### Interfaz de Administrador
