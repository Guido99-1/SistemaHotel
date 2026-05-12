# MANUAL DE USUARIO
## Sistema de Gestión Hotelera

---

# PORTADA

| Campo | Detalle |
|---|---|
| **Nombre del Sistema** | Sistema de Gestión Hotelera — Hostería Agoyán |
| **Versión** | 1.0 |
| **Fecha de Publicación** | Mayo de 2026 |
| **Plataforma** | Aplicación Web (Blazor WebAssembly + .NET 7) |
| **Empresa** | Hostería Agoyán |
| **RUC** | 1804742532001 |
| **Contacto** | agoyanhosteria@gmail.com — Tel. 0962213000 |
| **Documento** | Manual de Usuario por Roles |

> **Aviso:** Este documento es propiedad de Hostería Agoyán. Su contenido está dirigido exclusivamente al personal autorizado del establecimiento.

---

# 1. INTRODUCCIÓN

## 1.1 Objetivo del Sistema
El Sistema de Gestión Hotelera permite administrar de manera centralizada todas las operaciones diarias del hotel: registro de huéspedes, control de habitaciones, reservas individuales y múltiples, check-ins, check-outs, cobros, métodos de pago y emisión de reportes ejecutivos.

## 1.2 Alcance
El sistema cubre los siguientes procesos:
- Administración de **catálogos**: pisos, categorías y habitaciones.
- Gestión de **clientes** (huéspedes).
- Operaciones de **recepción**: check-in directo y check-in desde reserva.
- **Reservas** estándar, **múltiples** (varias habitaciones) y **gerenciales** (tarifa especial).
- **Salidas (check-out)** con cálculo automático de penalidades.
- **Dashboard** ejecutivo con KPIs, gráficos e indicadores en tiempo real.
- **Reportes** exportables a Excel y PDF.
- **Seguridad** con autenticación por roles y contraseñas encriptadas.

## 1.3 Público Objetivo
- Administradores del hotel.
- Personal de recepción.
- Personal de reservas.
- Personal de gerencia.
- Empleados operativos del hotel.

---

# 2. REQUISITOS DEL SISTEMA

## 2.1 Requisitos Mínimos de Hardware

| Componente | Mínimo | Recomendado |
|---|---|---|
| Procesador | Intel Core i3 / AMD equivalente | Intel Core i5 o superior |
| Memoria RAM | 4 GB | 8 GB |
| Almacenamiento libre | 1 GB | 2 GB |
| Resolución | 1366 × 768 px | 1920 × 1080 px |
| Conexión a Internet | 5 Mbps | 20 Mbps o superior |

## 2.2 Requisitos de Software
- Sistema operativo: Windows 10/11, macOS 11+, Linux Ubuntu 20.04+.
- Navegador web actualizado.
- JavaScript habilitado.
- Cookies habilitadas.

## 2.3 Navegadores Compatibles

| Navegador | Versión Mínima | Estado |
|---|---|---|
| Google Chrome | 110+ | ✅ Recomendado |
| Microsoft Edge | 110+ | ✅ Recomendado |
| Mozilla Firefox | 110+ | ✅ Compatible |
| Safari | 16+ | ✅ Compatible |
| Internet Explorer | — | ❌ No soportado |

## 2.4 Recomendaciones Técnicas
> **💡 Consejo:** Use siempre el navegador en su versión más reciente para garantizar la mejor experiencia y seguridad.

> **⚠ Advertencia:** No abra el sistema en varias pestañas simultáneamente con el mismo usuario; puede generar inconsistencias en sesión.

---

# 3. ACCESO AL SISTEMA

## 3.1 Cómo Iniciar Sesión

**Pasos:**
1. Abra el navegador web.
2. Ingrese a la dirección del sistema proporcionada por el Administrador.
3. Aparecerá la pantalla de **Inicio de Sesión**.
4. Escriba su **Correo electrónico** y **Contraseña**.
5. Haga clic en **Iniciar sesión**.

> 📷 **[Captura sugerida 1]** Pantalla de login con campos de correo y contraseña, logo del hotel a la izquierda.

## 3.2 Recuperación de Contraseña
Por motivos de seguridad, **el sistema no permite que el usuario reestablezca su contraseña directamente**. Debe contactar al **Administrador** o **Gerencia** para que la restablezca desde el módulo *Mantenimiento → Cambiar Contraseñas*.

## 3.3 Cierre de Sesión
1. Haga clic en su **nombre de usuario** en la parte superior derecha.
2. Seleccione **Cerrar sesión**.
3. Será redirigido al login.

> **🔒 Consejo de Seguridad:** Cierre siempre la sesión al terminar su jornada, especialmente si comparte el equipo.

---

# 4. DESCRIPCIÓN GENERAL DEL SISTEMA

## 4.1 Menú Principal
El menú lateral izquierdo muestra solo los módulos a los que su rol tiene acceso. Los iconos siguen un código visual estándar.

> 📷 **[Captura sugerida 2]** Menú lateral con todos los módulos (vista de Administrador).

## 4.2 Panel de Control (Dashboard)
Al ingresar verá el **Dashboard**, un tablero ejecutivo con:
- **KPIs principales**: ingresos del mes, ocupación, reservas activas, etc.
- **Gráficos** de ingresos, métodos de pago y ocupación por categoría.
- **Indicadores** de habitaciones disponibles, en limpieza y ocupadas.

## 4.3 Navegación General
- **Encabezado superior:** datos del usuario y cierre de sesión.
- **Menú lateral:** acceso a módulos según rol.
- **Área central:** contenido del módulo activo.
- **Cabecera de cada página:** título, subtítulo y acciones principales (Actualizar, Nuevo, Exportar).
- **KPIs:** tarjetas con bordes coloreados (azul, verde, naranja, gris).
- **Tablas:** búsqueda, filtros, paginación y acciones por fila.

---

# 5. ROLES Y PERMISOS

## 5.1 Tabla General de Permisos

| Módulo | Administrador | Gerencia | Recepcionista | Reservas | Empleado |
|---|:---:|:---:|:---:|:---:|:---:|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ |
| Usuarios | ✅ | ✅ | ❌ | ❌ | ❌ |
| Mantenimiento (Categoría/Piso/Habitación) | ✅ | ✅ | ❌ | ❌ | ❌ |
| Cambiar Contraseñas | ✅ | ✅ | ❌ | ❌ | ❌ |
| Clientes | ✅ | ✅ | ✅ | ✅ | ✅ |
| Recepción | ✅ | ✅ | ✅ | ❌ | ❌ |
| Reservas | ✅ | ✅ | ✅ | ✅ | ❌ |
| Reserva Múltiple | ✅ | ✅ | ✅ | ✅ | ❌ |
| Reservas Gerencia | ✅ | ✅ | ✅ | ✅ | ❌ |
| Salida (Check-out) | ✅ | ✅ | ✅ | ❌ | ✅ |
| Reportes | ✅ | ✅ | ✅ | ❌ | ✅ |

> **Leyenda:** ✅ Acceso completo · ❌ Sin acceso

---

# 6. FUNCIONALIDADES DEL SISTEMA

## 6.1 Módulo: Dashboard
**Objetivo:** ofrecer una vista general inmediata del estado del hotel.

**Pasos de uso:**
1. Al iniciar sesión, el sistema lo lleva automáticamente al Dashboard.
2. Revise las **tarjetas KPI** en la parte superior.
3. Use las **pestañas** internas para alternar entre Ingresos, Ocupación, Métodos de pago y Reservas.

**Ejemplo práctico:** Una recepcionista al iniciar el turno verifica en el Dashboard cuántas habitaciones están disponibles, cuántos check-ins están programados y cuál es la ocupación del día.

> 📷 **[Captura sugerida 3]** Dashboard con KPIs y gráficos.

---

## 6.2 Módulo: Usuarios
**Disponible para:** Administrador, Gerencia.
**Objetivo:** crear, modificar y eliminar usuarios del sistema y asignarles roles.

### Crear un Usuario
1. Menú → **Usuarios**.
2. Clic en **Nuevo Usuario**.
3. Complete: Nombre, Correo, Teléfono, Rol y Contraseña inicial.
4. Clic en **Guardar**.

### Editar un Usuario
1. En la tabla, presione el ícono de lápiz (editar).
2. Modifique los campos requeridos.
3. Guarde los cambios.

### Eliminar un Usuario
1. Presione el ícono de papelera.
2. Confirme en el cuadro de diálogo.

> **⚠ Advertencia:** No elimine un usuario que tenga recepciones o reservas históricas asociadas. Mejor cambie su contraseña o desactive el rol.

**Errores comunes:**
| Mensaje | Causa | Solución |
|---|---|---|
| "El correo ya existe" | Correo duplicado | Use otro correo |
| "Contraseña muy corta" | Menos de 6 caracteres | Use al menos 6 caracteres |

---

## 6.3 Módulo: Mantenimiento

### 6.3.1 Categorías
**Objetivo:** definir tipos de habitación (Simple, Doble, Suite, etc.).
1. Menú → **Mantenimiento → Categoría**.
2. **Nueva Categoría** → ingrese descripción → **Guardar**.
3. Edite o elimine con los íconos de la columna *Acciones*.

### 6.3.2 Pisos
**Objetivo:** registrar los niveles del hotel.
1. Menú → **Mantenimiento → Piso**.
2. **Nuevo Piso** → ingrese descripción (ej. "Planta Baja", "Primer Piso").
3. Guarde y verifique en la tabla.

### 6.3.3 Habitaciones
**Objetivo:** registrar las habitaciones físicas.
1. Menú → **Mantenimiento → Habitación**.
2. **Nueva Habitación** → ingrese: Número, Detalle, Precio, Piso y Categoría.
3. **Guardar**.

> 📷 **[Captura sugerida 4]** Tabla de Habitaciones con KPIs (Total, Categorías, Pisos, Precio Promedio).

**Ejemplo práctico:** Habitación 101, Detalle "Vista al jardín", Precio 35.00, Piso "Primer Piso", Categoría "Doble".

### 6.3.4 Cambiar Contraseñas
1. Menú → **Mantenimiento → Cambiar Contraseñas**.
2. Seleccione un usuario de la lista.
3. Ingrese **Nueva Contraseña** y **Confirmar Contraseña**.
4. Clic en **Guardar**.

> **🔒 Buena Práctica:** Nunca anote contraseñas en papel ni las envíe por correo sin cifrar.

---

## 6.4 Módulo: Clientes
**Objetivo:** mantener un registro de huéspedes con sus datos.

### Crear Cliente
1. Menú → **Clientes** → **Nuevo Cliente**.
2. Complete: Tipo Documento (Cédula/RUC/Pasaporte), Documento, Nombre Completo, Correo, Teléfono, Dirección.
3. **Guardar**.

### Buscar / Editar / Eliminar
- Use el campo **Buscar** por nombre, documento o tipo.
- Use los íconos de cada fila.

> **💡 Consejo:** Los clientes con RUC corresponden a empresas; verifique el dato antes de emitir un comprobante.

---

## 6.5 Módulo: Recepción
**Disponible para:** Administrador, Gerencia, Recepcionista.
**Objetivo:** controlar diariamente las habitaciones y procesar check-ins.

Tiene dos pestañas:

### Pestaña 1 — Habitaciones
Muestra el mapa de habitaciones por piso con código de colores:
- 🟢 **Verde** — Disponible
- 🟠 **Naranja** — Reservada para hoy (no permite check-in directo)
- 🔵 **Azul** — En limpieza
- ⚫ **Gris** — Ocupada / no disponible

**Acciones:**
- Clic en una habitación **disponible** → ir a la página de Check-in.
- Clic en una habitación **en limpieza** → confirmar y pasarla a *Disponible*.
- Clic en una habitación con **reserva pendiente** → el sistema bloquea el check-in directo y le indica ir a *Reservas*.

### Pestaña 2 — Reservas
Listado de reservas con filtros por fecha. Permite hacer **Check-in** sólo en reservas cuya fecha de entrada sea hoy.

> **⚠ Advertencia:** Si una habitación tiene reserva activa, no podrá hacer check-in directo. Debe usar el botón **Check-in** desde la pestaña *Reservas*.

### Realizar un Check-in (Detalle Recepción)

**Pasos:**
1. En *Recepción → Habitaciones*, clic en una habitación verde.
2. En el panel **Huésped**, **Buscar cliente** existente o **Nuevo cliente**.
3. Verifique fecha de entrada y modifique fecha de salida si aplica.
4. Ingrese **Precio por noche** y **Adelanto** (si lo hay).
5. Clic en **Seleccionar método de pago** y elija EFECTIVO, TRANSFERENCIA, OTRO o MIXTO.
6. Ingrese **Observación** opcional.
7. Clic en **Registrar check-in**.

> 📷 **[Captura sugerida 5]** Pantalla de Check-in con resumen del saldo a cobrar.

**Ejemplo práctico:** Cliente paga con $50 efectivo + $30 transferencia → use método **MIXTO** y registre el desglose.

**Errores comunes:**
| Mensaje | Causa | Solución |
|---|---|---|
| "Falta cliente" | No se seleccionó/creó cliente | Buscar o crear uno |
| "Falta método de pago" | No se seleccionó | Elija un método |
| "El adelanto no puede ser mayor al total" | Error de captura | Corrija el adelanto |

---

## 6.6 Módulo: Reservas
**Disponible para:** Administrador, Gerencia, Recepcionista, Reservas.
**Objetivo:** gestionar reservas de habitaciones individuales.

### Pestaña 1 — Habitaciones (crear reserva)
1. Seleccione el **Piso**.
2. Indique las fechas **Desde** y **Hasta**.
3. Clic en **Aplicar** para que el sistema bloquee las habitaciones ya reservadas en ese rango.
4. Clic en una habitación disponible → se abre la página **Detalle de Reserva**.

### Detalle de Reserva (crear reserva individual)
1. Confirme/seleccione el **Cliente** (buscar o nuevo).
2. Verifique las fechas; el sistema calcula automáticamente las noches y el **Precio estimado**.
3. Ingrese el **Adelanto** (obligatorio, mayor a 0).
4. Agregue **Observación** opcional.
5. Clic en **Registrar reserva**.

> **💡 Consejo:** El adelanto debe ser menor o igual al precio estimado. Si la fecha de salida es igual a la de entrada, no se permite (mínimo 1 noche).

### Pestaña 2 — Listado
Listado de reservas con filtros y exportación a Excel/PDF.

**Filtros disponibles:**
- Rango de fechas (Desde/Hasta).
- Cliente (autocompletado).
- Habitación.
- Filtros rápidos: Hoy, Esta semana, Este mes, Mes anterior.
- "Solo con saldo pendiente".

---

## 6.7 Módulo: Reserva Múltiple
**Disponible para:** Administrador, Gerencia, Recepcionista, Reservas.
**Objetivo:** crear una reserva que involucra varias habitaciones (ideal para grupos).

**Pasos:**
1. Menú → **Reserva Múltiple**.
2. **Seleccionar Cliente**.
3. Indique **Fecha Entrada**, **Fecha Salida**, **Monto Total** y **Anticipo**.
4. Agregue habitaciones una por una indicando **Personas**.
5. Verifique el **Resumen Financiero** (total personas, precio por persona, saldo pendiente).
6. Clic en **Crear Reserva**.

> 📷 **[Captura sugerida 6]** Reserva Múltiple con tabla de habitaciones y resumen.

**Ejemplo práctico:** Grupo de 8 personas en 3 habitaciones por 2 noches. Monto total $400. Anticipo $200. El sistema reparte montos por habitación automáticamente.

> **⚠ Advertencia:** El sistema verifica que cada habitación esté libre en el rango. Si una está ocupada, no se permite agregarla.

---

## 6.8 Módulo: Reservas Gerencia
**Disponible para:** Administrador, Gerencia, Recepcionista, Reservas.
**Objetivo:** bloquear habitaciones con tarifa especial:
- **Día normal:** $15 por persona.
- **Día festivo:** $20 por persona.

**Pasos:**
1. Menú → **Reservas Gerencia**.
2. Seleccione el **Piso**.
3. Clic en una habitación disponible → **Bloquear**.
4. En el modal indique: cliente, fecha entrada/salida, cantidad de personas, tipo de día (Normal/Festivo), método de pago.
5. El monto total se calcula automáticamente: *personas × tarifa × noches*.
6. Confirme.

> **💡 Consejo:** Las reservas Gerencia se pagan en su totalidad al momento de crearlas. Al hacer check-in no se requerirá cobrar nuevamente.

---

## 6.9 Módulo: Salida (Check-out)
**Disponible para:** Administrador, Gerencia, Recepcionista, Empleado.
**Objetivo:** registrar la salida del huésped y calcular penalidades si aplican.

**Pasos:**
1. Menú → **Salida**.
2. Localice la recepción activa del huésped.
3. Clic en **Detalle** → revise fechas, total y método de pago.
4. Si la salida es en una fecha distinta a la pactada, el sistema calcula la **penalidad** automáticamente.
5. Clic en **Registrar salida**.

> 📷 **[Captura sugerida 7]** Detalle de Salida con KPIs y total destacado.

**Ejemplo práctico:** Si un huésped extiende su estadía un día más, el sistema cobra el valor de una noche adicional como penalidad.

---

## 6.10 Módulo: Reportes
**Disponible para:** Administrador, Gerencia, Recepcionista, Empleado.
**Objetivo:** generar reportes de recepciones con totales, ingresos y penalidades.

### Generar un Reporte
1. Menú → **Reportes**.
2. Seleccione **Fecha Inicio** y **Fecha Fin**.
3. Filtros opcionales: cliente, habitación, tipo de documento, método de pago, "solo con penalidad".
4. Clic en **Buscar**.

### Exportar a Excel
- Clic en el botón **Excel** (verde).
- Se descarga un archivo `.xlsx` con encabezados, datos, totales y formato profesional.

### Exportar a PDF
- Clic en el botón **PDF** (naranja).
- Se descarga un archivo `.pdf` en formato horizontal con totales destacados.

> **💡 Consejo:** Los reportes incluyen información detallada del método de pago, incluyendo el desglose en pagos MIXTOS.

---

# 7. GESTIÓN DE USUARIOS Y ROLES

## 7.1 Crear y Editar Usuarios
Vea la sección **6.2 Módulo: Usuarios**.

## 7.2 Roles del Sistema
| Rol | Descripción |
|---|---|
| **Administrador** | Control total del sistema. Crea usuarios, configura catálogos, accede a todos los módulos. |
| **Gerencia** | Mismas capacidades que el Administrador. Foco en supervisión y reportes. |
| **Recepcionista** | Atiende huéspedes: check-ins, reservas, salidas. |
| **Reservas** | Gestiona reservas (individuales, múltiples y gerenciales). No procesa check-ins/salidas. |
| **Empleado** | Personal operativo con acceso limitado a Salidas, Clientes, Dashboard y Reportes. |

> **⚠ Importante:** No comparta credenciales entre personas. Cada usuario debe tener su propia cuenta para garantizar trazabilidad.

---

# 8. REPORTES — GUÍA RÁPIDA

| Acción | Cómo se hace |
|---|---|
| Generar reporte de recepciones | Menú **Reportes** → seleccionar fechas → **Buscar** |
| Filtrar por cliente | Use el autocompletado de cliente |
| Exportar Excel | Botón **Excel** (verde) |
| Exportar PDF | Botón **PDF** (naranja) |
| Ver totales | Tarjetas KPI en la parte superior |
| Filtros adicionales | Tipo documento, Método pago, Solo con penalidad |

---

# 9. BUENAS PRÁCTICAS DE USO

## 9.1 Seguridad
- **Cambie su contraseña** al menos cada 90 días.
- Nunca **comparta** su correo y contraseña.
- **Cierre sesión** al ausentarse del puesto.
- Reporte cualquier acceso sospechoso al Administrador.

## 9.2 Recomendaciones Operativas
- Verifique siempre los **datos del cliente** antes de registrar el check-in.
- Confirme el **método de pago** y el monto antes de guardar.
- En caso de **MIXTO**, ingrese el desglose exacto.
- Revise el **Dashboard al inicio del turno** para conocer el estado del día.

## 9.3 Respaldo de Información
- El sistema guarda los datos en una base de datos central. El Administrador es responsable del respaldo periódico.
- **Recomendado:** respaldo diario automático y respaldo semanal manual a un disco externo.

> **⚠ Advertencia:** Nunca elimine registros sin antes confirmarlo con el Administrador. La eliminación es definitiva.

---

# 10. PREGUNTAS FRECUENTES (FAQ)

**P1. Olvidé mi contraseña, ¿qué hago?**
R: Solicite al Administrador o Gerencia que la restablezca desde *Mantenimiento → Cambiar Contraseñas*.

**P2. No puedo ver el menú "Mantenimiento". ¿Por qué?**
R: Su rol no tiene acceso. Solo Administrador y Gerencia ven ese menú.

**P3. Intento hacer check-in y aparece "Habitación reservada". ¿Por qué?**
R: La habitación tiene una reserva activa para hoy. Vaya a *Recepción → Reservas* y use el botón **Check-in** de esa reserva.

**P4. ¿Cómo cobro un pago dividido entre efectivo y tarjeta?**
R: Al registrar el pago elija el método **MIXTO** e ingrese el desglose.

**P5. Las contraseñas, ¿están encriptadas?**
R: Sí, todas las contraseñas se almacenan encriptadas con BCrypt. Ni el Administrador puede verlas; solo restablecerlas.

**P6. ¿Puedo trabajar desde casa?**
R: Sí, siempre que cuente con la dirección del sistema y credenciales válidas. Recomendamos conexión segura (no Wi-Fi público).

**P7. ¿Por qué no puedo reservar el mismo día de la salida de otro huésped?**
R: Sí puede. La regla del sistema es: *una nueva reserva puede comenzar el mismo día en que otra termina*, porque la habitación queda disponible.

**P8. El reporte exportado en Excel sale sin formato. ¿Qué hago?**
R: Asegúrese de abrirlo con Microsoft Excel 2016 o superior. Los reportes incluyen encabezados, totales y formato profesional automático.

---

# 11. SOPORTE TÉCNICO

| Canal | Detalle |
|---|---|
| 📧 **Correo** | agoyanhosteria@gmail.com |
| 📞 **Teléfono** | 0962213000 |
| 🕒 **Horario** | Lunes a Viernes 08:00 – 18:00 |
| 🛠 **Soporte de emergencia** | Contactar al Administrador del sistema |

**Antes de contactar a soporte, tenga a mano:**
- Su nombre de usuario.
- Captura de pantalla del error.
- Descripción de los pasos que realizó.
- Fecha y hora del incidente.

---

# 12. GLOSARIO DE TÉRMINOS

| Término | Definición |
|---|---|
| **Adelanto** | Pago parcial entregado por el cliente antes del check-in. |
| **BCrypt** | Algoritmo de encriptación usado para proteger las contraseñas. |
| **Check-in** | Registro de entrada del huésped al hotel. |
| **Check-out / Salida** | Registro de salida del huésped, cierre de la cuenta. |
| **DTO** | (*Data Transfer Object*) Estructura usada internamente para mover datos entre el sistema y la base de datos. |
| **Habitación bloqueada** | Habitación que no puede reservarse en un rango de fechas. |
| **JWT** | (*JSON Web Token*) Tecnología que mantiene segura la sesión del usuario. |
| **KPI** | (*Key Performance Indicator*) Indicador clave mostrado en el Dashboard. |
| **MIXTO** | Método de pago que combina dos formas (efectivo + transferencia, etc.). |
| **Penalidad** | Cargo adicional cuando el huésped sale en fecha diferente a la pactada. |
| **Reserva Gerencia** | Reserva con tarifa especial fija ($15 normal / $20 festivo por persona). |
| **Reserva Múltiple** | Reserva que involucra varias habitaciones simultáneamente. |
| **Rol** | Nivel de acceso del usuario (Administrador, Gerencia, Recepcionista, Reservas, Empleado). |
| **Saldo pendiente** | Monto que aún debe el cliente al hotel. |

---

# 13. MANUAL POR ROL — RESUMEN OPERATIVO

A continuación se presenta una **guía rápida específica por rol** con las funciones más frecuentes.

---

## 13.1 ROL: ADMINISTRADOR

### Misión del rol
Tiene acceso total al sistema. Es responsable de la configuración inicial, de los usuarios y de la supervisión global.

### Tareas habituales
1. **Crear y gestionar usuarios** (Menú *Usuarios*).
2. **Configurar catálogos**: pisos, categorías, habitaciones (Menú *Mantenimiento*).
3. **Restablecer contraseñas** (Menú *Mantenimiento → Cambiar Contraseñas*).
4. **Supervisar el Dashboard** y los reportes ejecutivos.
5. **Auditar las reservas, recepciones y salidas**.

### Módulos a su disposición
- Dashboard, Usuarios, Mantenimiento (Categoría/Piso/Habitación/Cambiar Contraseñas), Clientes, Recepción, Reservas, Reserva Múltiple, Reservas Gerencia, Salida, Reportes.

### Buenas prácticas
- Crear un usuario por persona; nunca compartir cuentas.
- Realizar respaldos periódicos de la base de datos.
- Revisar reportes semanales y mensuales.

---

## 13.2 ROL: GERENCIA

### Misión del rol
Supervisión estratégica y operativa del hotel. Acceso casi total, igual que Administrador.

### Tareas habituales
1. **Revisar el Dashboard** diariamente.
2. **Generar reportes** mensuales para análisis.
3. **Crear reservas Gerencia** con tarifa especial.
4. **Auditar usuarios y catálogos**.

### Módulos a su disposición
- Mismos que Administrador.

### Buenas prácticas
- Aprobar manualmente las reservas con descuentos especiales.
- Verificar mensualmente la integridad de los reportes financieros.

---

## 13.3 ROL: RECEPCIONISTA

### Misión del rol
Es la cara visible del hotel. Procesa entradas, salidas, cobros y atención al cliente.

### Tareas habituales — Flujo diario
1. **Iniciar sesión** y revisar el Dashboard.
2. **Recepción → Habitaciones**: revisar el estado del piso y los KPIs (Disponibles, Reservadas hoy, En limpieza).
3. Para cada huésped que llega:
   - Si **tiene reserva**: ir a la pestaña *Reservas* → botón **Check-in**.
   - Si **no tiene reserva**: clic en una habitación verde → completar Check-in.
4. **Crear reservas** para huéspedes futuros (módulo *Reservas*).
5. **Crear reservas múltiples** para grupos.
6. **Crear reservas Gerencia** cuando aplique.
7. **Salida → Check-out** del huésped que se marcha.
8. Generar **Reporte diario** al cierre del turno.

### Módulos a su disposición
- Dashboard, Clientes, Recepción, Reservas, Reserva Múltiple, Reservas Gerencia, Salida, Reportes.

### Consejos
> **💡 Antes del check-in:** verifique siempre el documento de identidad del huésped y los datos del cliente.

> **🔒 Al cerrar turno:** genere el **Reporte de Recepciones del día** para confirmar los ingresos.

### Errores comunes
| Situación | Solución |
|---|---|
| Habitación marcada como "Reservada hoy" | Hacer check-in desde la pestaña *Reservas*, no directo |
| Cliente paga con dos métodos | Usar opción **MIXTO** en método de pago |
| El adelanto cubre el total | El sistema asigna **TRANSFERENCIA** automáticamente |

---

## 13.4 ROL: RESERVAS

### Misión del rol
Encargado exclusivamente de las reservas (canal telefónico, correo, presencial). No procesa entradas/salidas.

### Tareas habituales — Flujo diario
1. **Iniciar sesión** y revisar el Dashboard.
2. **Atender solicitudes de reserva**:
   - Reserva individual: módulo *Reservas → Habitaciones*.
   - Reserva grupal: módulo *Reserva Múltiple*.
   - Reserva con tarifa especial: módulo *Reservas Gerencia*.
3. **Verificar disponibilidad** por rango de fechas con el botón **Aplicar**.
4. **Registrar adelanto** y método de pago.
5. **Editar / cancelar reservas** desde el listado.

### Módulos a su disposición
- Dashboard, Clientes, Reservas, Reserva Múltiple, Reservas Gerencia.

### Consejos
> **💡 Tip:** Use los filtros rápidos *Hoy / Esta semana / Este mes* en el Listado para ubicar reservas con rapidez.

> **⚠ Importante:** Antes de confirmar una reserva múltiple, valide que las fechas no choquen con reservas existentes.

---

## 13.5 ROL: EMPLEADO

### Misión del rol
Personal operativo que procesa salidas, consulta clientes y reportes. No crea reservas ni recepciones.

### Tareas habituales
1. **Iniciar sesión** y revisar el Dashboard.
2. **Consultar clientes** para verificar datos.
3. Procesar **Salidas (Check-out)** de huéspedes.
4. Generar **Reportes** del día / semana.

### Módulos a su disposición
- Dashboard, Clientes, Salida, Reportes.

### Consejos
> **💡 Verifique siempre** que la habitación esté en estado correcto al hacer la salida (penalidades, observaciones).

---

# 14. NOTAS FINALES

> **🌟 Recuerde:** Este sistema fue diseñado para simplificar la operación del hotel. Su correcto uso depende de seguir los procedimientos descritos en este manual.

> **📚 Capacitación:** Si es la primera vez que utiliza el sistema, solicite una sesión de capacitación al Administrador.

> **🔄 Actualizaciones:** El sistema se actualiza periódicamente. Las nuevas funciones se anunciarán internamente.

---

**Fin del Manual de Usuario**
*Hostería Agoyán — Sistema de Gestión Hotelera v1.0*
*© 2026 — Todos los derechos reservados*
