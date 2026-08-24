# 📦 Hướng dẫn Khôi phục Cơ sở dữ liệu (Database Restore)

Tài liệu này hướng dẫn cách khôi phục (restore) cơ sở dữ liệu từ file `LMS_Backup.bak` để chạy dự án.

---

## 🚀 Các bước thực hiện trong Visual Studio

### Bước 1: Mở SQL Server Object Explorer
1. Mở Visual Studio.
2. Chọn **View** > **SQL Server Object Explorer** (Phím tắt: `Ctrl + \`, sau đó `Ctrl + S`).

### Bước 2: Mở cửa sổ New Query
1. Mở rộng mục **SQL Server** > `(localdb)\MSSQLLocalDB`.
2. Nhấp chuột phải vào thư mục **Databases** và chọn **New Query...**

### Bước 3: Chạy lệnh Restore tự động
Sao chép và dán đoạn mã T-SQL dưới đây vào cửa sổ Query:

> ⚠️ **Quan trọng:** Hãy thay đổi đường dẫn `'C:\path\to\LMS_Backup.bak'` thành **đường dẫn thực tế** tới file `LMS_Backup.bak` trên máy tính của bạn.

```sql
RESTORE DATABASE "aspnet-LMS_DotNETCore_MVC-58a71cd6-45ae-4548-b7ae-07f6573c8c95" 
FROM DISK = 'C:\path\to\LMS_Backup.bak' 
WITH REPLACE;