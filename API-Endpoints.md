# Tansiqy API Endpoints Documentation

## Base URL
```
https://tansiqy.runasp.net/api
```

---

## 🔐 Authentication
Most endpoints require authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

---

## 📚 University Endpoints

### Get All Universities
- **Method:** `GET`
- **Endpoint:** `/api/Universities`
- **Auth:** Public
- **Response:** List of all universities

### Get University Types
- **Method:** `GET`
- **Endpoint:** `/api/Universities/types`
- **Auth:** Public
- **Response:** List of university types

### Get Universities by Type
- **Method:** `GET`
- **Endpoint:** `/api/Universities/type/{type}`
- **Auth:** Public
- **Parameters:**
  - `type` (path): University type ID (1=Governmental, 2=Private, 3=National, 4=HigherInstitute, 5=Foreign, 6=Technological)
- **Response:** List of universities of specified type

### Get University by ID
- **Method:** `GET`
- **Endpoint:** `/api/Universities/{id}`
- **Auth:** Public
- **Parameters:**
  - `id` (path): University ID
- **Response:** Single university details

### Create University
- **Method:** `POST`
- **Endpoint:** `/api/Universities`
- **Auth:** Admin only
- **Content-Type:** `multipart/form-data`
- **Body:** CreateUniversityDto
- **Response:** Created university

### Update University
- **Method:** `PUT`
- **Endpoint:** `/api/Universities/{id}`
- **Auth:** Admin only
- **Content-Type:** `multipart/form-data`
- **Parameters:**
  - `id` (path): University ID
- **Body:** UpdateUniversityDto
- **Response:** Updated university

### Patch University
- **Method:** `PATCH`
- **Endpoint:** `/api/Universities/{id}`
- **Auth:** Admin only
- **Content-Type:** `multipart/form-data`
- **Parameters:**
  - `id` (path): University ID
- **Body:** PatchUniversityDto
- **Response:** Updated university

### Delete University
- **Method:** `DELETE`
- **Endpoint:** `/api/Universities/{id}`
- **Auth:** Admin only
- **Parameters:**
  - `id` (path): University ID
- **Response:** Success message

---

## 🎓 College Endpoints

### Get Colleges by University
- **Method:** `GET`
- **Endpoint:** `/api/Universities/{id}/colleges`
- **Auth:** Public
- **Parameters:**
  - `id` (path): University ID
- **Response:** List of colleges for the specified university

### Get College by ID
- **Method:** `GET`
- **Endpoint:** `/api/Universities/colleges/{id}`
- **Auth:** Public
- **Parameters:**
  - `id` (path): College ID
- **Response:** Single college details

### Create College
- **Method:** `POST`
- **Endpoint:** `/api/Universities/colleges`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Body:** CreateCollegeDto
- **Response:** Created college

### Update College
- **Method:** `PUT`
- **Endpoint:** `/api/Universities/colleges`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Body:** UpdateCollegeDto
- **Response:** Updated college

### Patch College
- **Method:** `PATCH`
- **Endpoint:** `/api/Universities/colleges/{id}`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Parameters:**
  - `id` (path): College ID
- **Body:** UpdateCollegeDto
- **Response:** Updated college

### Delete College
- **Method:** `DELETE`
- **Endpoint:** `/api/Universities/colleges/{id}`
- **Auth:** Admin only
- **Parameters:**
  - `id` (path): College ID
- **Response:** Success message

---

## 🏢 Department Endpoints

### Get Department by ID
- **Method:** `GET`
- **Endpoint:** `/api/Universities/departments/{id}`
- **Auth:** Public
- **Parameters:**
  - `id` (path): Department ID
- **Response:** Single department details

### Create Department
- **Method:** `POST`
- **Endpoint:** `/api/Universities/departments`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Body:** CreateDepartmentDto
- **Response:** Created department

### Update Department
- **Method:** `PUT`
- **Endpoint:** `/api/Universities/departments`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Body:** UpdateDepartmentDto
- **Response:** Updated department

### Patch Department
- **Method:** `PATCH`
- **Endpoint:** `/api/Universities/departments/{id}`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Parameters:**
  - `id` (path): Department ID
- **Body:** UpdateDepartmentDto
- **Response:** Updated department

### Delete Department
- **Method:** `DELETE`
- **Endpoint:** `/api/Universities/departments/{id}`
- **Auth:** Admin only
- **Parameters:**
  - `id` (path): Department ID
- **Response:** Success message

---

## 🌳 Branch Endpoints

### Create Branch
- **Method:** `POST`
- **Endpoint:** `/api/Universities/{universityId}/branches`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Parameters:**
  - `universityId` (path): University ID
- **Body:** CreateBranchDto
- **Response:** Created branch

### Update Branch
- **Method:** `PUT`
- **Endpoint:** `/api/Universities/{universityId}/branches`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Parameters:**
  - `universityId` (path): University ID
- **Body:** UpdateBranchDto
- **Response:** Updated branch

### Patch Branch
- **Method:** `PATCH`
- **Endpoint:** `/api/Universities/{universityId}/branches/{id}`
- **Auth:** Admin only
- **Content-Type:** `application/json`
- **Parameters:**
  - `universityId` (path): University ID
  - `id` (path): Branch ID
- **Body:** UpdateBranchDto
- **Response:** Updated branch

### Delete Branch
- **Method:** `DELETE`
- **Endpoint:** `/api/Universities/branches/{id}`
- **Auth:** Admin only
- **Parameters:**
  - `id` (path): Branch ID
- **Response:** Success message

---

## 🔍 Search Endpoints

### Intelligent Search
- **Method:** `GET`
- **Endpoint:** `/api/Universities/search/intelligent`
- **Auth:** Public
- **Query Parameters:**
  - `searchTerm` (optional): Search term
  - `type` (optional): University type filter
  - `governorate` (optional): Governorate filter
  - `studyType` (optional): Study type filter
  - `minFees` (optional): Minimum fees filter
  - `maxFees` (optional): Maximum fees filter
  - `minCoordination` (optional): Minimum coordination filter
  - `maxCoordination` (optional): Maximum coordination filter
  - `collegeName` (optional): College name filter
- **Response:** List of universities or colleges based on filters

### Intelligent Name Search
- **Method:** `GET`
- **Endpoint:** `/api/Universities/search/name/intelligent`
- **Auth:** Public
- **Query Parameters:**
  - `searchTerm` (required): Search term
- **Response:** SearchResultViewModel containing matching universities and colleges

---

## 📝 Response Codes

- **200 OK:** Request successful
- **201 Created:** Resource created successfully
- **400 Bad Request:** Invalid request data
- **401 Unauthorized:** Authentication required or invalid token
- **404 Not Found:** Resource not found
- **500 Internal Server Error:** Server error

---

## 📄 Content Types

### JSON Endpoints
- Most endpoints use `application/json`
- Include in request header: `Content-Type: application/json`

### Form Data Endpoints
- University create/update/patch use `multipart/form-data`
- Include in request header: `Content-Type: multipart/form-data`
- Used for file uploads (images)

---

## 🧪 Testing Examples

### Get All Universities
```bash
curl -X GET "https://tansiqy.runasp.net/api/Universities"
```

### Get College by ID
```bash
curl -X GET "https://tansiqy.runasp.net/api/Universities/colleges/11"
```

### Search Universities
```bash
curl -X GET "https://tansiqy.runasp.net/api/Universities/search/intelligent?searchTerm=قاهرة&type=1"
```

### Create College (Admin)
```bash
curl -X POST "https://tansiqy.runasp.net/api/Universities/colleges" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "universityId": 1,
    "nameAr": "كلية التجارة",
    "nameEn": "Faculty of Commerce"
  }'
```

### Update College (Admin)
```bash
curl -X PATCH "https://tansiqy.runasp.net/api/Universities/colleges/11" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 11,
    "universityId": 1,
    "nameAr": "كلية التجارة",
    "nameEn": "Faculty of Commerce"
  }'
```

---

## 📚 Additional Resources

- **Swagger Documentation:** `https://tansiqy.runasp.net/swagger`
- **API Base URL:** `https://tansiqy.runasp.net/api`
- **Frontend Dashboard:** `https://dashboard-tansiqy.vercel.app`

---

*Last Updated: April 2026*
