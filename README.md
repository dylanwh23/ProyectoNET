# Proyecto Taller .NET <br> "Sistema de Gestión de Eventos Deportivos" <br> 🏃‍♀️‍➡️🏃‍➡️🏃‍♂️‍➡️🥇
Sistema desarrollado en el ecosistema de .NET - Aspire, encargado de gestionar aspectos operativos de las carreras deportivas y enriquecer la expericencia de los corredores/espectadores.

## 🎯 Objetivo del sistema
Desarrollar una plataforma que permita:
- Registro de nuevas carreras.
- Inscripción de participantes.
- Monitoreo en tiempo real de carreras.
- Gestión de entrega de equipamiento.

## 🧩 Funcionalidades principales
| Funcionalidad | Descripción Breve| Actores | 
|---------------|-------------|-------------|
| Registro de Carreras | Alta de nuevas carreras | Administrador  |
| Inscripción de Participantes | Registro de corredores en carreras disponibles | Usuarios
| Monitoreo en Tiempo Real | Display de progreso y datos de la carrera | Usuarios |

## 🛠️ Arquitectura
El siguiente diagrama ilustra la arquitectura del sistema, destacando los componentes clave y sus interacciones:
<img width="1422" height="681" alt="Diagrama-arquitectura drawio" src="https://github.com/user-attachments/assets/9c76c584-d83c-43d1-aecb-b9e3b30d626f" />
### Decisiones Clave de Arquitectura 🎯

El diseño del sistema se fundamenta en las siguientes elecciones tecnológicas y arquitectónicas:

* **Escalabilidad y Orquestación:** Se adopta una arquitectura de **microservicios** orquestada por **.NET Aspire** para optimizar el despliegue, la gestión y la escalabilidad de cada servicio de forma independiente.
* **Resiliencia y Concurrencia:** Se garantiza la resiliencia y el manejo eficiente de eventos masivos (como la toma de tiempos en la largada) mediante un **bus de mensajes asíncrono** (**RabbitMQ**), que desacopla la recepción del procesamiento.
* **Experiencia del Usuario (UX):** Se implementan interfaces web modernas utilizando **Blazor** para ofrecer una experiencia interactiva y en tiempo real, permitiendo a los usuarios acceder a información actualizada al instante.
* **Simulación de Hardware:** Se utiliza un **Servicio en Segundo Plano** (*Worker Service*) para simular la lectura de chips RFID y la inyección de eventos de tiempo al Bus de Mensajes, emulando el entorno de carrera real.
* **Persistencia de Datos sencilla:** Se emplea **SQLite** para el almacenamiento local durante el desarrollo, con la flexibilidad de migrar a bases de datos más robustas en producción gracias a **Entity Framework Core**.

## 📊 Casos de Uso Principales
### 1. Registrar carrera
- **Actor:** Administrador
- **Descripción:** Alta de carrera.
- **Flujo Principal:**
  1. El administrador accede al panel de administración.
  2. Completa el formulario de registro de carrera.
  3. Envía el formulario y recibe una confirmación.
### 2. Inscribirse a carrera
- **Actor:** Usuario
- **Descripción:** Inscripción de corredor a una carrera.
- **Flujo Principal:**
    1. El usuario navega a la sección de carreras disponibles.
    2. Selecciona una carrera y completa el formulario de inscripción.
    3. Envía el formulario y recibe una confirmación.
### 3. Listar carreras
- **Actor:** Usuario
- **Descripción:** Visualización de carreras disponibles.
- **Flujo Principal:**
    1. El usuario accede a la página principal.
    2. Navega a la sección de carreras.
    3. Visualiza la lista de carreras disponibles con detalles básicos.
### 4. Listar datos carrera (en curso)
- **Actor:** Usuario
- **Descripción:** Visualización de datos en tiempo real de una carrera, incluyendo datos de un participante en especifico.
- **Flujo Principal:**
    1. El usuario selecciona una carrera en curso.
    2. Visualiza datos en tiempo real como posiciones, tiempos y estadísticas.   
### 5. Listar datos carrera (no iniciada)
- **Actor:** Usuario
- **Descripción:** Visualización de datos de una carrera no iniciada.
- **Flujo Principal:**
    1. El usuario selecciona una carrera no iniciada.
    2. Visualiza detalles como fecha, hora, lugar y participantes inscritos.
### 6. Simular carrera
- **Actor:** Sistema (Servicio en Segundo Plano), Administrador
- **Descripción:** Simulación de lectura de chip RFID y envío de eventos al bus de mensajes.
- **Flujo Principal:**
    1. El administrador da por iniciada la carrera y la simulación comienza.
    2. El servicio en segundo plano genera eventos de tiempo para corredores.
    3. Los eventos son enviados al bus de mensajes para su procesamiento.
    4. El sistema actualiza los datos de la carrera en tiempo real.
###  7. Entrega de equipamiento
- **Actor:** Administrador, Usuario
- **Descripción:** Gestión de la entrega de equipamiento a los corredores.
- **Flujo Principal:**
    1. Los puntos de entrega se definen en el registro de la carrera.
    2. El usuario se inscribe a la carrera y elige el punto de entrega.
    3. El sistema actualiza JSON con los datos de entrega del nuevo participante.  


## 🖼️ Maquetado de la Interfaz de Usuario y Administrador
### Interfaz de Usuario
![alt text](Interfaz-Usuario.png)
### Interfaz de Administrador
![alt text](Interfaz-Administrador.png)

## 🛠️ Tecnologías Utilizadas
- **.NET 8**: Plataforma principal de desarrollo.
- **ASP.NET Core**: Framework para construir aplicaciones web.
- **Blazor**: Framework para interfaces web interactivas.
- **Entity Framework Core**: ORM para acceso a datos.
- **SQLite**: Base de datos ligera para desarrollo.
- **RabbitMQ**: Sistema de mensajería para comunicación asíncrona.
- **Aspire**: Orquestador de microservicios y dashboard de monitoreo.



