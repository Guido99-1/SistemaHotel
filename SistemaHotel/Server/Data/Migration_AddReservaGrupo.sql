-- ==========================================
-- Migration: Add ReservaGrupo support
-- ==========================================

-- 1. Add columns to Reserva table
BEGIN TRANSACTION;

-- Add IdReservaGrupo column to Reserva if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reserva' AND COLUMN_NAME = 'IdReservaGrupo')
BEGIN
    ALTER TABLE Reserva ADD IdReservaGrupo INT NULL;
END

-- Add CantidadPersonas column to Reserva if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reserva' AND COLUMN_NAME = 'CantidadPersonas')
BEGIN
    ALTER TABLE Reserva ADD CantidadPersonas INT NOT NULL DEFAULT 1;
END

-- Add FechaCreacion column to Reserva if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reserva' AND COLUMN_NAME = 'FechaCreacion')
BEGIN
    ALTER TABLE Reserva ADD FechaCreacion DATETIME NULL DEFAULT GETDATE();
END

-- 2. Create ReservaGrupo table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ReservaGrupo')
BEGIN
    CREATE TABLE ReservaGrupo (
        IdReservaGrupo INT PRIMARY KEY IDENTITY(1,1),
        IdCliente INT NULL,
        FechaEntrada DATETIME NULL,
        FechaSalida DATETIME NULL,
        MontoTotal DECIMAL(10,2) NULL,
        PrecioPersona DECIMAL(10,2) NULL,
        TotalPersonas INT NULL,
        Observacion VARCHAR(500) NULL,
        Estado BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ReservaGrupo_Cliente FOREIGN KEY (IdCliente)
            REFERENCES Cliente(IdCliente)
    );
END

-- 3. Add Foreign Key from Reserva to ReservaGrupo if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
              WHERE CONSTRAINT_NAME = 'FK_Reserva_ReservaGrupo')
BEGIN
    ALTER TABLE Reserva
    ADD CONSTRAINT FK_Reserva_ReservaGrupo
        FOREIGN KEY (IdReservaGrupo) REFERENCES ReservaGrupo(IdReservaGrupo);
END

-- 4. Create index on ReservaGrupo for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReservaGrupo_IdCliente' AND object_id = OBJECT_ID('ReservaGrupo'))
BEGIN
    CREATE INDEX IX_ReservaGrupo_IdCliente ON ReservaGrupo(IdCliente);
END

COMMIT TRANSACTION;

PRINT 'Migration completed successfully!';
