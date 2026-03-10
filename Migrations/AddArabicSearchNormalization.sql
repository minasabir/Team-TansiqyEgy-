-- ============================================================================
-- Arabic Search Enhancement Migration
-- ============================================================================
-- This migration adds normalized columns for intelligent Arabic search
-- and creates optimized indexes for high-performance querying.
-- ============================================================================

-- ============================================================================
-- STEP 1: Add NormalizedNameAr columns to tables
-- ============================================================================

-- Add to Universities table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Universities' AND COLUMN_NAME = 'NormalizedNameAr'
)
BEGIN
    ALTER TABLE Universities ADD NormalizedNameAr NVARCHAR(200) NULL;
    PRINT 'Added NormalizedNameAr column to Universities';
END

-- Add to Colleges table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Colleges' AND COLUMN_NAME = 'NormalizedNameAr'
)
BEGIN
    ALTER TABLE Colleges ADD NormalizedNameAr NVARCHAR(200) NULL;
    PRINT 'Added NormalizedNameAr column to Colleges';
END

-- Add to Departments table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Departments' AND COLUMN_NAME = 'NormalizedNameAr'
)
BEGIN
    ALTER TABLE Departments ADD NormalizedNameAr NVARCHAR(200) NULL;
    PRINT 'Added NormalizedNameAr column to Departments';
END

GO

-- ============================================================================
-- STEP 2: Create helper function for Arabic normalization
-- ============================================================================

IF OBJECT_ID('dbo.NormalizeArabic', 'FN') IS NOT NULL
    DROP FUNCTION dbo.NormalizeArabic;
GO

CREATE FUNCTION dbo.NormalizeArabic (@input NVARCHAR(200))
RETURNS NVARCHAR(200)
AS
BEGIN
    IF @input IS NULL RETURN NULL;
    
    DECLARE @result NVARCHAR(200) = @input;
    
    -- Remove diacritics (tashkeel)
    SET @result = REPLACE(@result, N'ً', '');
    SET @result = REPLACE(@result, N'ٌ', '');
    SET @result = REPLACE(@result, N'ٍ', '');
    SET @result = REPLACE(@result, N'َ', '');
    SET @result = REPLACE(@result, N'ُ', '');
    SET @result = REPLACE(@result, N'ِ', '');
    SET @result = REPLACE(@result, N'ّ', '');
    SET @result = REPLACE(@result, N'ْ', '');
    SET @result = REPLACE(@result, N'ـ', ''); -- Tatweel
    
    -- Normalize letter variations
    SET @result = REPLACE(@result, N'ة', N'ه'); -- Ta marbuta -> Ha
    SET @result = REPLACE(@result, N'أ', N'ا'); -- Hamza above -> Alif
    SET @result = REPLACE(@result, N'إ', N'ا'); -- Hamza below -> Alif
    SET @result = REPLACE(@result, N'آ', N'ا'); -- Madda -> Alif
    SET @result = REPLACE(@result, N'ى', N'ي'); -- Alif maksura -> Ya
    SET @result = REPLACE(@result, N'ؤ', N'و'); -- Hamza on waw -> Waw
    SET @result = REPLACE(@result, N'ئ', N'ي'); -- Hamza on ya -> Ya
    
    RETURN LTRIM(RTRIM(@result));
END;
GO

PRINT 'Created NormalizeArabic function';

GO

-- ============================================================================
-- STEP 3: Populate existing data with normalized values
-- ============================================================================

-- Update Universities
UPDATE Universities 
SET NormalizedNameAr = dbo.NormalizeArabic(NameAr)
WHERE NormalizedNameAr IS NULL AND IsDeleted = 0;

PRINT CONCAT('Updated ', @@ROWCOUNT, ' Universities with normalized names');

-- Update Colleges
UPDATE Colleges 
SET NormalizedNameAr = dbo.NormalizeArabic(NameAr)
WHERE NormalizedNameAr IS NULL AND IsDeleted = 0;

PRINT CONCAT('Updated ', @@ROWCOUNT, ' Colleges with normalized names');

-- Update Departments
UPDATE Departments 
SET NormalizedNameAr = dbo.NormalizeArabic(NameAr)
WHERE NormalizedNameAr IS NULL AND IsDeleted = 0;

PRINT CONCAT('Updated ', @@ROWCOUNT, ' Departments with normalized names');

GO

-- ============================================================================
-- STEP 4: Create optimized indexes for search
-- ============================================================================

-- Universities indexes
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Universities_NormalizedNameAr' AND object_id = OBJECT_ID('Universities')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Universities_NormalizedNameAr 
    ON Universities(NormalizedNameAr) 
    WHERE IsDeleted = 0;
    PRINT 'Created IX_Universities_NormalizedNameAr index';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Universities_SearchOptimized' AND object_id = OBJECT_ID('Universities')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Universities_SearchOptimized
    ON Universities(NormalizedNameAr, Type, Governorate, IsDeleted)
    INCLUDE (NameAr, NameEn, Location, Fees, LastYearCoordination);
    PRINT 'Created IX_Universities_SearchOptimized covering index';
END

-- Colleges indexes
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Colleges_NormalizedNameAr' AND object_id = OBJECT_ID('Colleges')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Colleges_NormalizedNameAr 
    ON Colleges(NormalizedNameAr) 
    WHERE IsDeleted = 0;
    PRINT 'Created IX_Colleges_NormalizedNameAr index';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Colleges_SearchOptimized' AND object_id = OBJECT_ID('Colleges')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Colleges_SearchOptimized
    ON Colleges(NormalizedNameAr, UniversityId, IsDeleted)
    INCLUDE (NameAr, NameEn, Fees, LastYearCoordination);
    PRINT 'Created IX_Colleges_SearchOptimized covering index';
END

-- Departments indexes
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Departments_NormalizedNameAr' AND object_id = OBJECT_ID('Departments')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Departments_NormalizedNameAr 
    ON Departments(NormalizedNameAr) 
    WHERE IsDeleted = 0;
    PRINT 'Created IX_Departments_NormalizedNameAr index';
END

GO

PRINT 'Migration completed successfully!';
