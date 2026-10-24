# 06. Quy chuẩn Thiết kế REST API & OpenAPI (REST API & Contract Standards)

> **Môn học**: PRN232 — Final Assignment  
> **Dự án**: FPT Alumni Connect  
> **Áp dụng cho**: 3 Backend Developers, 1 Web Developer, 1 Flutter Developer.  
> **Mục tiêu**: Chuẩn hóa toàn diện thiết kế REST API, mã trạng thái HTTP, định dạng phản hồi lỗi, phân trang, lọc dữ liệu và tài liệu OpenAPI/Swagger giữa Backend và Frontend/Mobile.

---

## 📑 Mục lục
1. [Quy chuẩn Định tuyến URL (URL Routing Conventions)](#1-quy-chuẩn-định-tuyến-url-url-routing-conventions)
2. [Các Phương thức HTTP & Mục đích Sử dụng](#2-các-phương-thức-http--mục-đích-sử-dụng)
3. [Mã Trạng thái HTTP Chuẩn (Standard HTTP Status Codes)](#3-mã-trạng-thái-http-chuẩn-standard-http-status-codes)
4. [Định dạng Phản hồi Lỗi Chuẩn RFC 7807 (ProblemDetails)](#4-định-dạng-phản-hồi-lỗi-chuẩn-rfc-7807-problemdetails)
5. [Quy chuẩn DTO & Validation (Data Annotations)](#5-quy-chuẩn-dto--validation-data-annotations)
6. [Tiêu chuẩn Phân trang, Tìm kiếm & Sắp xếp (Pagination, Filtering & Sorting)](#6-tiêu-chuẩn-phân-trang-tìm-kiếm--sắp-xếp-pagination-filtering--sorting)
7. [Quy chuẩn Tài liệu OpenAPI / Swagger](#7-quy-chuẩn-tài-liệu-openapi--swagger)
8. [Ví dụ Thực tế Minh họa từ Template Endpoint](#8-ví-dụ-thực-tế-minh-họa-từ-template-endpoint)

---

## 1. Quy chuẩn Định tuyến URL (URL Routing Conventions)

Mọi API công khai trong hệ thống phải tuân theo cấu trúc URL chuẩn mực:

```text
https://api.domain.com/api/v1/{resource-name}[/{id}][/{sub-resource}]
```

### Nguyên tắc đặt tên:
- **Tiền tố phiên bản**: Luôn bắt đầu bằng `/api/v1/`.
- **Danh từ số nhiều (Plural Nouns)**: Đại diện cho tập hợp tài nguyên, ví dụ:
  - `/api/v1/sample-items` (Thay vì `/api/v1/sampleItem` hay `/api/v1/getSampleItem`)
  - `/api/v1/users`
  - `/api/v1/posts`
  - `/api/v1/conversations`
- **Kebab-case cho tên ghép**: Sử dụng chữ thường ngăn cách bằng dấu gạch ngang (ví dụ: `sample-items`, `verification-tokens`).
- **Không chứa động từ trong URL**: Hành động được biểu thị thông qua phương thức HTTP (`GET`, `POST`, `PUT`, `DELETE`).
- **Tài nguyên con phân cấp**: Biểu thị mối quan hệ phụ thuộc cha - con:
  - `/api/v1/posts/{postId}/comments`: Lấy hoặc tạo bình luận của bài viết `{postId}`.

---

## 2. Các Phương thức HTTP & Mục đích Sử dụng

| HTTP Method | Mục đích | Tính Lũy đẳng (Idempotent) | Ví dụ Endpoint |
| :--- | :--- | :---: | :--- |
| **`GET`** | Đọc dữ liệu (danh sách hoặc chi tiết 1 bản ghi). Không làm thay đổi dữ liệu server. | Có | `GET /api/v1/sample-items` |
| **`POST`** | Tạo mới một tài nguyên hoặc kích hoạt tác vụ nghiệp vụ (đăng nhập, verify OTP). | Không | `POST /api/v1/sample-items` |
| **`PUT`** | Cập nhật / Thay thế toàn bộ nội dung của một tài nguyên hiện có. | Có | `PUT /api/v1/sample-items/{id}` |
| **`PATCH`**| Cập nhật một phần thuộc tính (Partial update), ví dụ đổi trạng thái, sửa 1 trường. | Không | `PATCH /api/v1/users/{id}/status` |
| **`DELETE`**| Xóa một tài nguyên theo định danh. | Có | `DELETE /api/v1/sample-items/{id}` |

---

## 3. Mã Trạng thái HTTP Chuẩn (Standard HTTP Status Codes)

Mọi phản hồi từ Controller phải trả về mã HTTP chính xác theo ngữ cảnh:

| Mã HTTP | Tên mã | Khi nào sử dụng? |
| :---: | :--- | :--- |
| **`200 OK`** | Success | Request `GET`, `PUT`, `PATCH` thành công và có dữ liệu trả về trong body. |
| **`201 Created`** | Created | Request `POST` tạo mới tài nguyên thành công. **Bắt buộc kèm Header `Location`** trỏ tới URL của tài nguyên vừa tạo. |
| **`204 NoContent`** | No Content | Request `DELETE` hoặc xử lý thành công nhưng không cần trả về nội dung body. |
| **`400 BadRequest`** | Bad Request | Dữ liệu đầu vào không hợp lệ (sai định dạng JSON, vi phạm DataAnnotations validation). |
| **`401 Unauthorized`** | Unauthorized | Thiếu JWT Token hoặc Token không hợp lệ / đã hết hạn. |
| **`403 Forbidden`** | Forbidden | Token hợp lệ nhưng tài khoản không có quyền truy cập (ví dụ: Student gọi API của Admin). |
| **`404 NotFound`** | Not Found | Không tìm thấy tài nguyên theo ID yêu cầu (`NotFoundException`). |
| **`409 Conflict`** | Conflict | Xung đột dữ liệu (ví dụ: Đăng ký với Email đã tồn tại trong hệ thống). |
| **`500 InternalServerError`** | Server Error | Lỗi không mong muốn phát sinh trong mã nguồn hoặc hệ thống hạ tầng. |

---

## 4. Định dạng Phản hồi Lỗi Chuẩn RFC 7807 (ProblemDetails)

Khi xảy ra lỗi (4xx, 5xx), mọi service phải trả về định dạng chuẩn quốc tế **RFC 7807 (`application/problem+json`)**:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Resource 'SampleItem' with identifier '11111111-1111-1111-1111-111111111111' was not found.",
  "instance": "/api/v1/sample-items/11111111-1111-1111-1111-111111111111",
  "traceId": "0HN12AB34CD:00000001"
}
```

### Lợi ích:
- Frontend (React) và Mobile (Flutter) chỉ cần viết **một hàm xử lý lỗi duy nhất** cho toàn bộ hệ thống bằng cách đọc trường `detail` và `title`.
- Có `traceId` để lập trình viên Backend tra cứu ngay log tương ứng trong terminal hoặc console.

---

## 5. Quy chuẩn DTO & Validation (Data Annotations)

Toàn bộ dữ liệu đầu vào nhận từ Client phải được định nghĩa bằng DTO và kiểm tra tính hợp lệ qua **System.ComponentModel.DataAnnotations**:

```csharp
public class CreateSampleItemRequest
{
    [Required(ErrorMessage = "Tên không được để trống.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên phải từ 2 đến 100 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string Description { get; set; } = string.Empty;
}
```

- Khi validation thất bại, ASP.NET Core tự động trả về `400 Bad Request` dạng `ValidationProblemDetails` liệt kê rõ ràng danh sách các trường vi phạm mà không cần lập trình viên phải viết code if-else thủ công.

---

## 6. Tiêu chuẩn Phân trang, Tìm kiếm & Sắp xếp (Pagination, Filtering & Sorting)

Khi API trả về danh sách có số lượng bản ghi lớn, bắt buộc phải hỗ trợ phân trang để tránh tràn bộ nhớ:

### 6.1 Tham số Query Parameters chuẩn:
- `pageNumber`: Số trang hiện tại (bắt đầu từ `1`, mặc định `1`).
- `pageSize`: Số bản ghi mỗi trang (mặc định `10`, giới hạn tối đa `100`).
- `search`: Từ khóa tìm kiếm (tìm kiếm theo tên hoặc mô tả).
- `sortBy`: Tên trường cần sắp xếp (ví dụ: `createdAt`, `name`).
- `sortOrder`: Hướng sắp xếp (`asc` hoặc `desc`, mặc định `desc`).

Ví dụ URL:
```text
GET /api/v1/sample-items/paged?search=alumni&pageNumber=1&pageSize=10&sortBy=createdAt&sortOrder=desc
```

### 6.2 Cấu trúc Phản hồi Phân trang chuẩn (`PagedResponse<T>`):
```json
{
  "items": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Initial Sample Item",
      "description": "Seeded item for testing",
      "isActive": true,
      "createdAtUtc": "2026-10-24T09:17:53Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

## 7. Quy chuẩn Tài liệu OpenAPI / Swagger

Mọi Action trong Controller phải được trang bị đầy đủ XML Doc Comments và Swagger attributes:

```csharp
/// <summary>
/// Tạo mới một sample item resource.
/// </summary>
/// <param name="request">Payload dữ liệu bao gồm tên và mô tả.</param>
/// <param name="cancellationToken">Token hủy tác vụ.</param>
/// <response code="201">Tạo thành công, trả về resource kèm Header Location.</response>
/// <response code="400">Dữ liệu đầu vào vi phạm validation.</response>
[HttpPost]
[ProducesResponseType(typeof(SampleItemResponse), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public async Task<ActionResult<SampleItemResponse>> Create(
    [FromBody] CreateSampleItemRequest request,
    CancellationToken cancellationToken)
{
    var created = await _sampleItemService.CreateAsync(request, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

---

## 8. Ví dụ Thực tế Minh họa từ Template Endpoint

Bảng tra cứu các API mẫu đã được triển khai hoàn chỉnh trong [templates/dotnet-service/](file:///c:/Use/University/PRN232/ASM/guideline/templates/dotnet-service/):

| Phương thức | Đường dẫn URL | Mô tả nghiệp vụ | Mã trạng thái thành công | Mã trạng thái lỗi |
| :--- | :--- | :--- | :---: | :---: |
| `GET` | `/health` | Kiểm tra tình trạng hoạt động của service | `200 OK` | `503 Service Unavailable` |
| `GET` | `/api/v1/sample-items` | Lấy toàn bộ danh sách items | `200 OK` | `500 Server Error` |
| `GET` | `/api/v1/sample-items/paged` | Lấy danh sách có phân trang và tìm kiếm | `200 OK` | `400 Bad Request` |
| `GET` | `/api/v1/sample-items/{id}` | Lấy chi tiết một item theo GUID | `200 OK` | `404 Not Found` |
| `POST` | `/api/v1/sample-items` | Tạo mới một item | `201 Created` | `400 Bad Request` |
| `DELETE` | `/api/v1/sample-items/{id}` | Xóa một item theo GUID | `204 NoContent` | `404 Not Found` |
