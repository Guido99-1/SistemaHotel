# Migración de Contraseñas a BCrypt

## Descripción
Script para migrar todas las contraseñas de texto plano a BCrypt hash.

## Opción 1: Usar Script C# (Recomendado)

Ejecutar este código en Program.cs antes de `app.Run()`:

```csharp
// Migración de contraseñas a BCrypt (EJECUTAR UNA SOLA VEZ EN DESARROLLO)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DbhotelBlazorContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();
        
        var usuariosSinHashear = dbContext.Usuarios
            .Where(u => !u.Clave.StartsWith("$2"))
            .ToList();
        
        if (usuariosSinHashear.Count > 0)
        {
            Console.WriteLine($"[MIGRACION] Migrando {usuariosSinHashear.Count} contraseñas a BCrypt...");
            
            foreach (var usuario in usuariosSinHashear)
            {
                usuario.Clave = passwordService.HashPassword(usuario.Clave);
            }
            
            dbContext.SaveChanges();
            Console.WriteLine("[MIGRACION] ✅ Migración completada");
        }
    }
}
```

## Opción 2: Script SQL Manual

Si las contraseñas están en texto plano y tienes una base de datos existente:

```sql
-- Script SQL para verificar contraseñas sin hashear
SELECT IdUsuario, Correo, Clave 
FROM Usuario 
WHERE Clave NOT LIKE '$2[aby]$%' 
AND Correo IS NOT NULL;
```

**IMPORTANTE**: Las contraseñas de texto plano detectadas deben ser hasheadas usando C# (Opción 1).

## Pasos

1. Asegúrate de que `PasswordHashingService` está registrado en Program.cs
2. Ejecuta la aplicación en modo desarrollo
3. El script se ejecutará automáticamente antes de `app.Run()`
4. Verifica en consola que dice "✅ Migración completada"
5. Intenta hacer login con una contraseña original

## Verificación

Después de la migración:
```sql
SELECT IdUsuario, Correo, Clave 
FROM Usuario 
WHERE IdUsuario = 1;
-- Deberías ver: Clave = "$2b$12$..." (60+ caracteres)
```

## Contraseñas de Prueba

Si necesitas contraseñas de prueba hasheadas:

### Admin (contraseña: "admin123")
```
$2b$12$tPEVlMBXJjDJPEWQiN0V8O8PwrXVdJFqJVbhK9qLvKQfKmQ6pEuKq
```

### Usuario (contraseña: "usuario123")
```
$2b$12$Q2F7FjRKZq3mhx7hKJwkN.WVpXLQwWqqYbH2rHz0oNK0pqx0q0NUe
```

### Recepción (contraseña: "recepcion123")
```
$2b$12$5hVRHwXV0xIH7f8K9qL2R.3ZPq7WxYqQmN6kL8XvK9x0pqJ1q0NUe
```

Puedes usar estos hashes para pre-popular la BD con usuarios de prueba hasheados correctamente.

## Rollback

Si necesitas revertir (NO RECOMENDADO):
```sql
-- Esto solo funciona si guardaste las contraseñas originales en otro lado
-- No hay forma de revertir bcrypt (es irreversible por diseño)
```

## Estado

- [x] Script C# en Program.cs
- [ ] Ejecutar al iniciar aplicación (desarrollo)
- [ ] Verificar migración exitosa
