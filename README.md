# TansiqyV1 - Intelligent Arabic Search System

A comprehensive university, college, and department management system with intelligent Arabic text search capabilities.

## Overview

TansiqyV1 is an ASP.NET Core Web API that provides management and search functionality for Egyptian universities, their colleges, and departments. The system features an intelligent Arabic search that handles letter variations, diacritics, and morphological forms.

## Key Features

### 1. University Management
- CRUD operations for universities, colleges, and departments
- Support for university branches
- Fee structures and coordination scores
- Governorate and study type filtering

### 2. Intelligent Arabic Search
The system implements a sophisticated Arabic text normalization system that:
- **Handles letter variations**: ة→ه, أ/إ/آ→ا, ى→ي, ؤ→و, ئ→ي
- **Removes diacritics** (tashkeel) automatically
- **Supports morphological variations**: singular/plural forms
- **Handles definite articles**: searches work with or without "ال" prefix
- **Root extraction**: Matches related word forms

### 3. Search Endpoints

#### Standard Search (Original)
```
GET /api/universities/search?name={searchTerm}
GET /api/universities/search/name?searchTerm={searchTerm}
```

#### Intelligent Arabic Search (New)
```
GET /api/universities/search/intelligent?searchTerm={term}&type={type}&governorate={gov}
GET /api/universities/search/name/intelligent?searchTerm={term}
```

**Query Parameters for Intelligent Search:**
- `searchTerm` - Arabic or English university name (intelligent matching)
- `type` - University type ID (1=Governmental, 2=Private, 3=National, 4=HigherInstitute, 5=Foreign, 6=Technological)
- `governorate` - Governorate ID
- `studyType` - Study type ID (1=Math, 2=Science, 3=Literary, 4=Industrial, 5=American)
- `minFees` / `maxFees` - Fee range filter
- `minCoordination` / `maxCoordination` - Coordination score range
- `collegeName` - College name search (intelligent Arabic matching)

## Architecture

### Project Structure
```
TansiqyV1/
├── TansiqyV1.API/           # Web API controllers and startup
├── TansiqyV1.BLL/           # Business logic layer (Services)
├── TansiqyV1.DAL/           # Data access layer
│   ├── Database/            # DbContext and configuration
│   ├── Entities/            # Domain entities
│   ├── Enums/               # Enumerations
│   ├── Helpers/             # ArabicTextNormalizer
│   └── Repo/                # Repository implementations
└── Migrations/              # SQL migration scripts
```

### Core Components

#### ArabicTextNormalizer
Located in `TansiqyV1.DAL/Helpers/ArabicTextNormalizer.cs`

Provides:
- `Normalize(string)` - Normalizes Arabic text for comparison
- `ExtractRoot(string)` - Extracts root form for morphological matching
- `GenerateSearchVariations(string)` - Creates multiple search patterns
- `Contains(string, string)` - Normalized containment check
- `CalculateRelevance(string, string)` - Relevance scoring

#### Database Schema
Key tables:
- **Universities** - University information with `NormalizedNameAr` column
- **Colleges** - College information with `NormalizedNameAr` column  
- **Departments** - Department information with `NormalizedNameAr` column
- **UniversityBranches** - Branch locations

## Database Setup

### Option 1: Using SQL Script (Recommended for Production)
Run the migration script to add normalized columns and indexes:

```sql
-- Execute: Migrations/AddArabicSearchNormalization.sql
```

This script:
1. Adds `NormalizedNameAr` columns to Universities, Colleges, and Departments
2. Creates a SQL function for normalization
3. Populates existing data with normalized values
4. Creates optimized search indexes

### Option 2: Using EF Core Migrations
```bash
dotnet ef database update --project TansiqyV1.DAL --startup-project TansiqyV1.API
```

**Note:** If columns already exist from the SQL script, remove conflicting migrations first.

## Usage Examples

### Search for Engineering Colleges
```
GET /api/universities/search/intelligent?collegeName=هندسه
```
Matches: "هندسة", "الهندسة", "هندسه", "الهندسه"

### Search Universities in Cairo
```
GET /api/universities/search/name/intelligent?searchTerm=القاهره
```
Matches: "القاهرة", "قاهرة", "القاهره", "قاهره"

### Combined Search with Filters
```
GET /api/universities/search/intelligent?searchTerm=جامعه&type=1&governorate=1&collegeName=طب
```
Finds governmental universities in Cairo with medical colleges.

## API Response Format

```json
{
  "id": 1,
  "nameAr": "جامعة القاهرة",
  "nameEn": "Cairo University",
  "type": 1,
  "typeAr": "حكومي",
  "location": "الجيزة",
  "governorate": 1,
  "governorateAr": "القاهرة",
  "fees": 1000.00,
  "lastYearCoordination": 395.50,
  "collegesCount": 20,
  "branchesCount": 3,
  "colleges": [
    {
      "id": 1,
      "nameAr": "كلية الهندسة",
      "nameEn": "Faculty of Engineering",
      "departments": [...]
    }
  ]
}
```

## Performance Optimizations

1. **Database Indexes**: Optimized indexes on `NormalizedNameAr` columns for fast searches
2. **Response Caching**: 120-second cache on search endpoints
3. **AsNoTracking**: Read-only queries for better performance
4. **Covering Indexes**: Include frequently accessed columns

## Backward Compatibility

All existing API endpoints remain unchanged:
- `/api/universities/search` - Still works as before
- `/api/universities/search/name` - Still works as before

The new intelligent search endpoints are additive:
- `/api/universities/search/intelligent` - New
- `/api/universities/search/name/intelligent` - New

## Development

### Prerequisites
- .NET 9.0 SDK
- SQL Server (2016 or later)
- Visual Studio 2022 or VS Code

### Build
```bash
dotnet build TansiqyV1.API/TansiqyV1.API.csproj
```

### Run
```bash
dotnet run --project TansiqyV1.API/TansiqyV1.API.csproj
```

### Populate Normalized Data
After adding new universities/colleges/departments, ensure `NormalizedNameAr` is populated:

```csharp
// This happens automatically in Create/Update methods
entity.NormalizedNameAr = ArabicTextNormalizer.Normalize(entity.NameAr);
```

For existing data, use the SQL script's update statements or run:
```sql
UPDATE Universities SET NormalizedNameAr = dbo.NormalizeArabicText(NameAr) WHERE NormalizedNameAr IS NULL;
UPDATE Colleges SET NormalizedNameAr = dbo.NormalizeArabicText(NameAr) WHERE NormalizedNameAr IS NULL;
UPDATE Departments SET NormalizedNameAr = dbo.NormalizeArabicText(NameAr) WHERE NormalizedNameAr IS NULL;
```

## Deployment Notes

1. **Pre-deployment**: Run SQL migration script to add columns and indexes
2. **Data migration**: Script auto-populates existing data
3. **Zero-downtime**: New columns are nullable; existing API continues working
4. **Rollback**: Simply remove the columns if needed

## License

[Your License Here]

## Support

For issues or questions about the intelligent Arabic search system, refer to the code comments in:
- `TansiqyV1.DAL/Helpers/ArabicTextNormalizer.cs`
- `TansiqyV1.DAL/Repo/Implementation/UniversityRepository.cs`
