# 📦 Layered ASP.NET Core .NET 10 Service Template

> Mẫu khung dự án backend chuẩn dành cho các Microservices thuộc dự án **FPT Alumni Connect** (PRN232).

---

## 🏛️ Cấu trúc Kiến trúc (Layered Architecture)

Template áp dụng mô hình 3 tầng phân lớp rõ ràng:

```text
Template.Api (REST API, Controllers, Middleware, Health)
    │
    ▼ (Reference duy nhất)
Template.Services (Business Logic, Validation, DTOs, Exceptions)
    │
    ▼ (Reference duy nhất)
Template.Repositories (Data Access, EF Core/In-Memory, Entities)
```

> ⚠️ **Quy tắc bất di bất dịch**: `Template.Api` không được reference hay inject bất kỳ Repository nào trực tiếp. Mọi truy vấn và nghiệp vụ phải đi qua tầng `Template.Services`.

---

## 🚀 Hướng dẫn Sử dụng Nhanh (Quickstart)

### 1. Khôi phục packages & Biên dịch dự án
```bash
# Khôi phục dependencies
dotnet restore

# Biên dịch toàn bộ Solution
dotnet build
```

### 2. Chạy Kiểm thử (xUnit Unit & Integration Tests)
```bash
dotnet test
```

### 3. Khởi chạy Web API cục bộ
```bash
dotnet run --project src/Template.Api/Template.Api.csproj
```
Mặc định ứng dụng sẽ lắng nghe trên cổng HTTP: `http://localhost:5000` (hoặc cổng được chỉ định trong `launchSettings.json`).

---

## 📡 Danh sách Endpoints Mẫu (Sample Endpoints)

| Phương thức | Đường dẫn URL | Mô tả | Mã trạng thái HTTP |
| :--- | :--- | :--- | :--- |
| `GET` | `/health` | Kiểm tra tình trạng hoạt động (Health Check) | `200 OK` (`"Healthy"`) |
| `GET` | `/api/v1/sample-items` | Lấy danh sách toàn bộ các items | `200 OK` (JSON List) |
| `GET` | `/api/v1/sample-items/{id}` | Lấy chi tiết 1 item theo GUID | `200 OK` hoặc `404 Not Found` |
| `POST` | `/api/v1/sample-items` | Tạo mới 1 sample item | `201 Created` (Kèm Location header) |
| `DELETE` | `/api/v1/sample-items/{id}` | Xóa 1 sample item theo GUID | `204 NoContent` hoặc `404 Not Found` |

---

## 🛠️ Hướng dẫn Sao chép cho Service Mới

Khi bắt đầu một service mới trong danh sách (`f-alumni-user-service`, `f-alumni-chat-service`, v.v.):

1. Copy toàn bộ nội dung thư mục này vào thư mục repository của service.
2. Đổi tên solution `Template.sln` thành `FAlumni.<ServiceName>.sln`.
3. Đổi tên các thư mục `src/Template.*` thành `src/FAlumni.<ServiceName>.*`.
4. Tìm và thay thế chuỗi `Template` thành `FAlumni.<ServiceName>` trong tất cả các file `.csproj` và `.cs`.
5. Chạy `dotnet test` để đảm bảo hệ thống biên dịch và chạy kiểm thử thành công trước khi bắt đầu viết code nghiệp vụ.
