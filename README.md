# 🎬 HubCinema – Auto Test Project

Dự án kiểm thử tự động (automated testing) cho website bán vé xem phim **HubCinema**. Dự án sử dụng mô hình **Page Object Model (POM)** kết hợp với bộ dữ liệu test được quản lý bằng file Excel, cho phép chạy và ghi nhận kết quả kiểm thử một cách có hệ thống.

---

## 📌 Mục tiêu

- Tự động hóa các kịch bản kiểm thử chức năng (functional testing) trên giao diện web của HubCinema.
- Ghi nhận kết quả thực tế, trạng thái (Pass/Fail) và ảnh chụp màn hình trực tiếp vào file Excel.
- Đảm bảo chất lượng các tính năng chính như đăng ký, đăng nhập, đặt vé, thanh toán, quản lý rạp chiếu, phòng chiếu, lịch chiếu, thực phẩm, tin tức và nhiều chức năng quản trị khác.

---

## 🛠️ Công nghệ sử dụng

| Công nghệ | Phiên bản | Mục đích |
|---|---|---|
| **C# / .NET** | .NET 8.0 | Ngôn ngữ và nền tảng thực thi chính |
| **NUnit** | 3.14.0 | Framework kiểm thử đơn vị và tích hợp |
| **NUnit3TestAdapter** | 4.5.0 | Adapter chạy NUnit trong Visual Studio / dotnet test |
| **Selenium WebDriver** | 4.27.0 | Tự động hóa tương tác trình duyệt |
| **Selenium.Support** | 4.27.0 | Các tiện ích hỗ trợ Selenium (waits, helpers) |
| **ChromeDriver** | 146.x | Điều khiển trình duyệt Google Chrome |
| **ClosedXML** | 0.102.3 | Đọc dữ liệu test và ghi kết quả vào file Excel (.xlsx) |
| **Microsoft.NET.Test.Sdk** | 17.8.0 | SDK chạy và phát hiện test case |
| **coverlet.collector** | 6.0.0 | Thu thập độ bao phủ code (code coverage) |
| **System.Drawing.Common** | 7.0.0 | Hỗ trợ chụp ảnh màn hình (screenshot) |

---

## 📁 Cấu trúc dự án

```
sqa-testing-report/
├── Data/
│   ├── DataTest.xlsx          # File Excel chứa test case, dữ liệu test và kết quả
│   └── Screenshots/           # Ảnh chụp màn hình được lưu sau khi chạy test
├── Models/
│   └── TestCaseStep.cs        # Model đại diện cho một bước trong test case
├── Pages/                     # Page Object Model – ánh xạ các trang giao diện
├── Tests/                     # Các lớp test case theo từng chức năng
├── Utilities/
│   ├── DriverFactory.cs       # Khởi tạo và cấu hình ChromeDriver
│   ├── ExcelTestCaseHelper.cs # Đọc/ghi test case từ/vào file Excel
│   ├── PathHelper.cs          # Quản lý đường dẫn file
│   └── ScreenshotHelper.cs    # Chụp và lưu ảnh màn hình
└── sqa-testing-report.csproj
```

---

## 🧪 Phạm vi kiểm thử

Dự án bao gồm các test case tự động cho các chức năng sau:

- **Xác thực người dùng**: Đăng ký, Đăng nhập, Cập nhật hồ sơ
- **Đặt vé & Thanh toán**: Chọn phim, chọn ghế, đặt vé, thanh toán
- **Quản lý rạp chiếu**: Tạo/Cập nhật rạp chiếu, phòng chiếu, sơ đồ ghế
- **Quản lý lịch chiếu**: Tạo và quản lý suất chiếu
- **Quản lý phim**: Danh sách phim (admin)
- **Quản lý thực phẩm**: Tạo và chỉnh sửa thực phẩm
- **Quản lý tin tức**: Tạo tin tức
- **Quản lý tài khoản admin**: Tạo và cập nhật tài khoản admin
- **Đăng nhập admin**: Kiểm thử xác thực phía admin
- **Vé đã đặt**: Xem lịch sử vé

---

## 👥 Thành viên nhóm

| Thành viên | Số test case |
|---|---|
| Nguyễn Đàm Khá | 40 |
| Nguyễn Xuân Bắc | 40 |
| Lâm Tấn Thành | 40 |
| Trần Duy Khoa | 40 |

**Tổng cộng: 160 test case tự động**

---

## 🚀 Hướng dẫn chạy

### Yêu cầu
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Google Chrome (phiên bản tương thích với ChromeDriver đã cài)

### Chạy toàn bộ test

```bash
dotnet test
```

### Chạy một test cụ thể

```bash
dotnet test --filter "FullyQualifiedName~<TênTestClass>"
```

> **Lưu ý:** Mặc định trình duyệt sẽ hiển thị khi chạy test. Để chạy ở chế độ ẩn (headless), mở comment dòng `options.AddArgument("--headless")` trong file `Utilities/DriverFactory.cs`.

---

## 📊 Kết quả kiểm thử

Sau khi chạy, kết quả sẽ được ghi lại tự động vào file `Data/DataTest.xlsx` bao gồm:
- **Actual Result**: Kết quả thực tế
- **Status**: Pass / Fail
- **Screenshots**: Tên file ảnh chụp màn hình tương ứng
