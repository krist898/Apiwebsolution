# Guía de demostración – GestionAcademica

Ubicación: `C:\Users\krist\.gemini\antigravity\scratch\GestionAcademica`

## 1. Abrir en Visual Studio 2022
1. Abre **`GestionAcademica.sln`** (Archivo → Abrir → Proyecto o solución).
2. Si VS pide restaurar los paquetes NuGet, acepta (o clic derecho en la solución → *Restaurar paquetes NuGet*).
3. Compila con **Ctrl+Shift+B**.

## 2. Migraciones (Consola del Administrador de paquetes)
Herramientas → Administrador de paquetes NuGet → Consola. **Proyecto predeterminado:** `GestionAcademica.API`.

La migración `InitialCreate` ya existe y ya se aplicó en tu LocalDB. Para una base de datos nueva o en otra PC, ejecuta:
```powershell
Update-Database
```
Para demostrar el proceso desde cero en clase:
```powershell
Drop-Database                 # confirma con A
Remove-Migration              # borra la migración existente
Add-Migration InitialCreate
Update-Database
```
Comprueba el resultado: Ver → **Explorador de objetos de SQL Server** → (localdb)\MSSQLLocalDB → Bases de datos → GestionAcademicaDB → Tablas
(Asignaciones, Cursos, Docentes, Estudiantes y __EFMigrationsHistory).

## 3. Ejecutar y abrir Swagger
Elige el perfil **https** o **http** y pulsa **F5**. Se abre `/swagger`. Si no se abre solo, entra a `https://localhost:<puerto>/swagger`.

## 4. JSON de prueba (en este orden)
**POST /api/Estudiantes**
```json
{ "nombres": "Ana María", "apellidos": "López Pérez", "direccion": "Zona 1, Guatemala" }
```
**POST /api/Docentes**
```json
{ "nombres": "Carlos", "apellidos": "Ramírez" }
```
**POST /api/Cursos**
```json
{ "nombre": "Programación III" }
```
**POST /api/Asignaciones** (usa los IDs que devolvieron los POST anteriores)
```json
{ "estudianteId": 1, "cursoId": 1, "docenteId": 1 }
```
**GET /api/Asignaciones** devuelve:
```json
[{ "asignacionId": 1, "estudianteId": 1, "nombreEstudiante": "Ana María López Pérez",
   "cursoId": 1, "nombreCurso": "Programación III", "docenteId": 1, "nombreDocente": "Carlos Ramírez" }]
```
**PUT /api/Cursos/1**
```json
{ "cursoId": 1, "nombre": "Programación III - B" }
```
**PUT /api/Asignaciones/1**
```json
{ "asignacionId": 1, "estudianteId": 1, "cursoId": 1, "docenteId": 1 }
```

## 5. Validaciones que puedes demostrar
| Acción | Respuesta |
|---|---|
| POST correcto | 201 Created |
| PUT o DELETE correcto | 204 No Content |
| Nombre vacío, o ID de la URL distinto al del cuerpo | 400 Bad Request |
| ID inexistente (en GET/PUT/DELETE, o en un FK de una asignación) | 404 Not Found |
| Asignación duplicada | 409 Conflict |
| DELETE de un curso, estudiante o docente con asignaciones | 409 Conflict |

## 6. Si LocalDB no está disponible
Cambia `DefaultConnection` en `appsettings.json`:
- **SQL Server Express:** `Server=.\\SQLEXPRESS;Database=GestionAcademicaDB;Trusted_Connection=True;TrustServerCertificate=True;`
- **SQL Server Developer (instancia predeterminada):** `Server=localhost;Database=GestionAcademicaDB;Trusted_Connection=True;TrustServerCertificate=True;`
- **Autenticación SQL:** `Server=localhost;Database=GestionAcademicaDB;User Id=sa;Password=TuClave;TrustServerCertificate=True;`

Después ejecuta `Update-Database`.

## Alternativa desde la terminal (CLI)
```powershell
dotnet tool restore
dotnet ef database update -p GestionAcademica.API
dotnet run --project GestionAcademica.API
```
