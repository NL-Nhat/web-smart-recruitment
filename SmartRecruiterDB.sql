-- Kiem tra xem database da ton tai hay chua, ton tai thi xoa
IF EXISTS (SELECT * FROM sys.databases WHERE name = N'SmartRecruiterDB')
BEGIN
    -- Dong tat ca cac ket noi den co so du lieu
    EXECUTE sp_MSforeachdb 'IF ''?'' = ''SmartRecruiterDB''
    BEGIN
        DECLARE @sql AS NVARCHAR(MAX) = ''USE [?]; ALTER DATABASE [?] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;''
        EXEC (@sql)
    END'
    -- Xoa tat ca cac ket noi toi co so du lieu (thuc hien qua he thong master)
    USE MASTER
    -- Xoa co so du lieu neu ton tai
    DROP DATABASE SmartRecruiterDB
END
CREATE DATABASE SmartRecruiterDB
GO
USE SmartRecruiterDB
GO

-- =========================================================================
-- HỆ THỐNG SMART RECRUITER - DATABASE SCHEMA (INT IDENTITY)
-- =========================================================================

-- 1. VAI TRÒ
CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro VARCHAR(50) NOT NULL UNIQUE,
    MoTa NVARCHAR(100)
);

-- 2. TÀI KHOẢN (Dùng chung cho Authentication)
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    MatKhauHash NVARCHAR(255) NOT NULL,
    MaVaiTro INT NOT NULL, 
    TrangThaiHoatDong BIT DEFAULT 1, -- 1: Hoạt động, 0: Khóa
    NgayTao DATETIME DEFAULT GETDATE(),
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);

-- 2. HỒ SƠ NHÀ TUYỂN DỤNG
-- Không dùng IDENTITY vì là quan hệ 1-1, lấy ID từ TaiKhoan
CREATE TABLE NhaTuyenDung (
    MaNhaTuyenDung INT PRIMARY KEY, 
    TenCongTy NVARCHAR(150),
    SoDienThoai VARCHAR(20),
    Website NVARCHAR(255),
    DiaChi NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    Logo NVARCHAR(500),
    AnhBia NVARCHAR(500),
    FOREIGN KEY (MaNhaTuyenDung) REFERENCES TaiKhoan(MaTaiKhoan)
);

-- 3. HỒ SƠ ỨNG VIÊN
-- Không dùng IDENTITY vì là quan hệ 1-1, lấy ID từ TaiKhoan
CREATE TABLE UngVien (
    MaUngVien INT PRIMARY KEY, 
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(20) unique,
    LinkLinkedIn NVARCHAR(255),
    ChucDanhHienTai NVARCHAR(150),
    SoNamKinhNghiem INT DEFAULT 0,
    AnhDaiDien NVARCHAR(500),
    GioiThieu NVARCHAR(MAX),
    FOREIGN KEY (MaUngVien) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CHK_UngVien_SoNamKN CHECK (SoNamKinhNghiem >= 0)
);

-- 4. DANH MỤC KỸ NĂNG (Master Data)
CREATE TABLE DanhMucKyNang (
    MaKyNang INT IDENTITY(1,1) PRIMARY KEY,
    TenKyNang NVARCHAR(100) NOT NULL UNIQUE, -- VD: 'C#', 'ReactJS', 'SQL Server'
    PhanLoai VARCHAR(50) -- VD: 'Language', 'Framework', 'Database', 'SoftSkill'
);

-- 5. KỸ NĂNG CỦA ỨNG VIÊN (Mapping)
CREATE TABLE ChiTietKyNang_UngVien (
    MaUngVien INT,
    MaKyNang INT,
    SoNamKinhNghiem INT, 
    PRIMARY KEY (MaUngVien, MaKyNang),
    FOREIGN KEY (MaUngVien) REFERENCES UngVien(MaUngVien),
    FOREIGN KEY (MaKyNang) REFERENCES DanhMucKyNang(MaKyNang)
);

-- 6. TIN TUYỂN DỤNG (Job Description - JD)
CREATE TABLE TinTuyenDung (
    MaTin INT IDENTITY(1,1) PRIMARY KEY,
    MaNhaTuyenDung INT FOREIGN KEY REFERENCES NhaTuyenDung(MaNhaTuyenDung),
    TieuDe NVARCHAR(200) NOT NULL,
    PhongBan NVARCHAR(100), 
    DiaDiem NVARCHAR(200),
    HinhThucLamViec VARCHAR(50), 
    MucLuongToiThieu DECIMAL(18,2),
    MucLuongToiDa DECIMAL(18,2),
    MoTaCongViec NVARCHAR(MAX) NOT NULL, 
    YeuCauCongViec NVARCHAR(MAX) NOT NULL, 
    QuyenLoi NVARCHAR(MAX),
    TrangThai VARCHAR(50) DEFAULT 'DangMo', 
    HanNopCV DATETIME not null,
    DaXoa BIT DEFAULT 0, 
    NgayTao DATETIME DEFAULT GETDATE() not null,
    NgayCapNhat DATETIME DEFAULT GETDATE() not null,
    CONSTRAINT CHK_TinTuyenDung_TrangThai CHECK (TrangThai IN ('DangMo', 'DaDong')),
    CONSTRAINT CHK_TinTuyenDung_HinhThuc CHECK (HinhThucLamViec IN ('FullTime', 'PartTime', 'Online', 'Intern')),
    CONSTRAINT CHK_TinTuyenDung_Luong CHECK (MucLuongToiThieu >= 0 AND MucLuongToiDa >= MucLuongToiThieu)
);
CREATE INDEX IX_TinTuyenDung_TrangThai_HanNop ON TinTuyenDung(TrangThai, HanNopCV);

-- 7. KỸ NĂNG YÊU CẦU CỦA TIN TUYỂN DỤNG (Mapping)
CREATE TABLE ChiTietKyNang_TinTuyenDung (
    MaTin INT,
    MaKyNang INT,
    CapDoYeuCau VARCHAR(50) DEFAULT 'KhongBatBuoc', -- 'BatBuoc', 'UuTien', 'KhongBatBuoc'
    PRIMARY KEY (MaTin, MaKyNang),
    FOREIGN KEY (MaTin) REFERENCES TinTuyenDung(MaTin),
    FOREIGN KEY (MaKyNang) REFERENCES DanhMucKyNang(MaKyNang),
    CONSTRAINT CHK_ChiTietKyNang_CapDo CHECK (CapDoYeuCau IN ('BatBuoc', 'UuTien', 'KhongBatBuoc'))
);

-- 8. ĐƠN ỨNG TUYỂN (Gộp từ HoSoCV)
CREATE TABLE DonUngTuyen (
    MaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaTin INT FOREIGN KEY REFERENCES TinTuyenDung(MaTin),
    MaUngVien INT FOREIGN KEY REFERENCES UngVien(MaUngVien),
    TenFile NVARCHAR(255) NOT NULL,
    DuongDanFile NVARCHAR(500) NOT NULL, 
    DinhDang VARCHAR(20) NOT NULL,
    NoiDungTrichXuat NVARCHAR(MAX), 
    TrangThai VARCHAR(50) DEFAULT 'DaNop', 
    NgayNop DATETIME DEFAULT GETDATE(),
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_DonUngTuyen_TrangThai CHECK (TrangThai IN ('DaNop', 'AIDaLoc', 'DaChapNhan', 'PhongVan', 'TrungTuyen', 'TuChoi'))
);
-- Ngăn 1 ứng viên apply 1 job nhiều lần tại cùng 1 thời điểm
CREATE UNIQUE INDEX UQ_DonUngTuyen_Tin_UngVien ON DonUngTuyen(MaTin, MaUngVien);

-- 10. KẾT QUẢ AI PHÂN TÍCH
CREATE TABLE KetQua_AI (
    MaKetQua INT IDENTITY(1,1) PRIMARY KEY,
    MaDon INT FOREIGN KEY REFERENCES DonUngTuyen(MaDon),
    TrangThaiXuLy VARCHAR(50) DEFAULT 'DangXuLy', 
    DiemPhuHop DECIMAL(5,2), 
    TomTatUngVien NVARCHAR(MAX), 
    KyNangPhuHop_Json NVARCHAR(MAX), 
    KyNangThieu_Json NVARCHAR(MAX), 
    DiemManh NVARCHAR(MAX), 
    DiemYeu NVARCHAR(MAX),
    DeXuat VARCHAR(50), 
    PhanHoiGocTuAI NVARCHAR(MAX), 
    NgayPhanTich DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_KetQuaAI_TrangThai CHECK (TrangThaiXuLy IN ('DangXuLy', 'HoanThanh', 'Loi')),
    CONSTRAINT CHK_KetQuaAI_DeXuat CHECK (DeXuat IN ('TuyenNhanh', 'CoTheCanNhac', 'LoaiBo'))
);
CREATE INDEX IX_KetQuaAI_DiemPhuHop ON KetQua_AI(DiemPhuHop DESC);

-- 11. INDEX BỔ SUNG ĐỂ TỐI ƯU
CREATE INDEX IX_TinTuyenDung_NhaTuyenDung ON TinTuyenDung(MaNhaTuyenDung);
CREATE INDEX IX_DonUngTuyen_Tin_TrangThai ON DonUngTuyen(MaTin, TrangThai);
CREATE INDEX IX_DonUngTuyen_UngVien ON DonUngTuyen(MaUngVien);
CREATE INDEX IX_UngVien_HoTen ON UngVien(HoTen);
CREATE INDEX IX_TaiKhoan_VaiTro ON TaiKhoan(MaVaiTro);

-- 12. LỊCH HẸN PHỎNG VẤN
CREATE TABLE LichHenPhongVan (
    MaLichHen INT IDENTITY(1,1) PRIMARY KEY,
    MaDon INT FOREIGN KEY REFERENCES DonUngTuyen(MaDon),
    NgayPhuongVan DATE NOT NULL,
    GioPhuongVan TIME NOT NULL,
    DiaDiem NVARCHAR(255),
    LinkHop NVARCHAR(500),
    HinhThuc VARCHAR(50) DEFAULT 'Online', -- 'Online', 'Offline'
    GhiChu NVARCHAR(MAX),
    TrangThai VARCHAR(50) DEFAULT 'ChoXacNhan' not null, 
    NgayTao DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_LichHen_HinhThuc CHECK (HinhThuc IN ('Online', 'Offline')),
    CONSTRAINT CHK_LichHen_TrangThai CHECK (TrangThai IN ('ChoXacNhan', 'DaXacNhan', 'DaHuy', 'HoanThanh')),
    CONSTRAINT CHK_LichHen_DiaDiem_LinkHop CHECK (
        (HinhThuc = 'Online' AND LinkHop IS NOT NULL AND DiaDiem IS NULL) OR 
        (HinhThuc = 'Offline' AND DiaDiem IS NOT NULL AND LinkHop IS NULL)
    )
);
CREATE INDEX IX_LichHen_MaDon ON LichHenPhongVan(MaDon);
CREATE INDEX IX_LichHen_Ngay ON LichHenPhongVan(NgayPhuongVan);




-- =========================================================================
-- SCRIPT MOCK DATA - SMART RECRUITER (DÙNG IDENTITY_INSERT)
-- =========================================================================

-- ---------------------------------------------------------
-- 0. VAI TRÒ
-- ---------------------------------------------------------
SET IDENTITY_INSERT VaiTro ON;
INSERT INTO VaiTro (MaVaiTro, TenVaiTro) VALUES 
(1, 'Admin'),
(2, 'NhaTuyenDung'),
(3, 'UngVien');
SET IDENTITY_INSERT VaiTro OFF;

-- ---------------------------------------------------------
-- 1. TÀI KHOẢN (2 Admin, 4 HR, 8 Ứng viên)
-- ---------------------------------------------------------
SET IDENTITY_INSERT TaiKhoan ON;
INSERT INTO TaiKhoan (MaTaiKhoan, Email, MatKhauHash, MaVaiTro, TrangThaiHoatDong, NgayTao, NgayCapNhat) VALUES 
-- Admins (ID: 1-2)
(1, 'admin1@smartrecruit.vn', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 1, 1, '2023-01-01 08:00', '2023-01-01 08:00'),
(2, 'admin2@smartrecruit.vn', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 1, 1, '2023-01-02 08:00', '2023-01-02 08:00'),
-- HRs (ID: 3-6)
(3, 'tuyendung@fpt.com.vn', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 2, 1, '2023-05-10 09:00', '2023-05-10 09:00'),
(4, 'hr.vng@vng.com.vn', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 2, 1, '2023-06-15 10:00', '2023-06-15 10:00'),
(5, 'recruitment@viettel.vn', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 2, 1, '2023-07-20 08:30', '2023-07-20 08:30'),
(6, 'hr@momo.vn', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 2, 1, '2023-08-05 14:00', '2023-08-05 14:00'),
-- Candidates (ID: 7-14)
(7, 'nguyenvana@gmail.com', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 3, 1, '2024-01-10 09:15', '2024-01-10 09:15'),
(8, 'tranthingoc@gmail.com', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 3, 1, '2024-01-12 10:20', '2024-01-12 10:20'),
(9, 'lehoanghai.dev@gmail.com', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 3, 1, '2024-02-05 14:00', '2024-02-05 14:00'),
(10, 'phamminhtuan@gmail.com', '$2a$11$DFsfGKhBLo8Ez7W33DxuX.GGO/QGv1R48f16zqjgSSwrnRWRUgPl2', 3, 1, '2024-02-20 16:45', '2024-02-20 16:45'),
(11, 'hoangthanhmai@gmail.com', 'hash_uv5', 3, 1, '2024-03-01 08:30', '2024-03-01 08:30'),
(12, 'vuminhduc.ai@gmail.com', 'hash_uv6', 3, 1, '2024-03-15 11:11', '2024-03-15 11:11'),
(13, 'doanvanhau.it@gmail.com', 'hash_uv7', 3, 1, '2024-03-20 09:00', '2024-03-20 09:00'),
(14, 'ngotienhiep@gmail.com', 'hash_uv8', 3, 0, '2024-04-01 10:00', '2024-04-05 10:00'); -- Bị khóa
SET IDENTITY_INSERT TaiKhoan OFF;

-- ---------------------------------------------------------
-- 2. HỒ SƠ NHÀ TUYỂN DỤNG (Không có IDENTITY, map ID: 3-6)
-- ---------------------------------------------------------
INSERT INTO NhaTuyenDung (MaNhaTuyenDung, TenCongTy, SoDienThoai, Website, DiaChi, MoTa, Logo) VALUES 
(3, N'FPT Software', '0901111222', 'https://fptsoftware.com', N'Duy Tân, Cầu Giấy, Hà Nội', N'Công ty xuất khẩu phần mềm hàng đầu Việt Nam...', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR6f9YQZ1I0v-t3zP8i_zY6N-V8u6RjXjP8wQ&s'),
(4, N'VNG Corporation', '0903333444', 'https://vng.com.vn', N'VNG Campus, Quận 7, TP.HCM', N'Kỳ lân công nghệ đầu tiên tại Việt Nam...', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTzR6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s'),
(5, N'Viettel Group', '0988888999', 'https://viettel.vn', N'Giang Văn Minh, Ba Đình, Hà Nội', N'Tập đoàn Viễn thông và Công nghệ hàng đầu...', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR1-X1N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s'),
(6, N'MoMo', '0912222333', 'https://momo.vn', N'Lầu 6, Phú Mỹ Hưng, Quận 7, TP.HCM', N'Ví điện tử số 1 Việt Nam...', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT3R6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s');

-- ---------------------------------------------------------
-- 3. HỒ SƠ ỨNG VIÊN (Không có IDENTITY, map ID: 7-14)
-- ---------------------------------------------------------
INSERT INTO UngVien (MaUngVien, HoTen, SoDienThoai, LinkLinkedIn, ChucDanhHienTai, SoNamKinhNghiem, AnhDaiDien, GioiThieu) VALUES 
(7, N'Nguyễn Văn A', '0971112223', 'linkedin.com/in/nguyenvana', N'Backend .NET Developer', 4, '/avatars/uv7.jpg', N'Tôi là một lập trình viên .NET với 4 năm kinh nghiệm...'),
(8, N'Trần Thị Ngọc', '0972223334', 'linkedin.com/in/tranthingoc', N'Frontend ReactJS', 2, '/avatars/uv8.jpg', N'Yêu thích xây dựng giao diện người dùng hiện đại...'),
(9, N'Lê Hoàng Hải', '0973334445', 'linkedin.com/in/lehoanghai', N'Fullstack Developer', 5, '/avatars/uv9.jpg', N'Kinh nghiệm thực chiến với cả Backend và Frontend...'),
(10, N'Phạm Minh Tuấn', '0974445556', 'linkedin.com/in/phamminhtuan', N'Java Backend', 3, '/avatars/uv10.jpg', N'Chuyên gia về Java Spring Boot và Microservices...'),
(11, N'Hoàng Thanh Mai', '0975556667', 'linkedin.com/in/hoangthanhmai', N'Data Analyst', 2, '/avatars/uv11.jpg', N'Đam mê phân tích dữ liệu và đưa ra các insight...'),
(12, N'Vũ Minh Đức', '0976667778', 'linkedin.com/in/vuminhduc', N'AI Engineer', 4, '/avatars/uv12.jpg', N'Nghiên cứu và triển khai các mô hình Machine Learning...'),
(13, N'Đoàn Văn Hậu', '0977778889', 'linkedin.com/in/doanvanhau', N'DevOps Engineer', 3, '/avatars/uv13.jpg', N'Tối ưu hóa quy trình triển khai phần mềm...'),
(14, N'Ngô Tiến Hiệp', '0978889990', '', N'Fresher Web', 0, '/avatars/uv14.jpg', N'Sinh viên mới tốt nghiệp, ham học hỏi...');

-- ---------------------------------------------------------
-- 4. DANH MỤC KỸ NĂNG (15 Kỹ năng)
-- ---------------------------------------------------------
SET IDENTITY_INSERT DanhMucKyNang ON;
INSERT INTO DanhMucKyNang (MaKyNang, TenKyNang, PhanLoai) VALUES 
(1, 'C#', 'Language'),
(2, 'ASP.NET Core', 'Framework'),
(3, 'SQL Server', 'Database'),
(4, 'Java', 'Language'),
(5, 'Spring Boot', 'Framework'),
(6, 'ReactJS', 'Framework'),
(7, 'JavaScript', 'Language'),
(8, 'TypeScript', 'Language'),
(9, 'Python', 'Language'),
(10, 'Machine Learning', 'AI'),
(11, 'Docker', 'DevOps'),
(12, 'Kubernetes', 'DevOps'),
(13, 'AWS', 'Cloud'),
(14, 'Agile/Scrum', 'SoftSkill'),
(15, 'English', 'Language');
SET IDENTITY_INSERT DanhMucKyNang OFF;

-- ---------------------------------------------------------
-- 5. KỸ NĂNG CỦA ỨNG VIÊN
-- ---------------------------------------------------------
INSERT INTO ChiTietKyNang_UngVien (MaUngVien, MaKyNang, SoNamKinhNghiem) VALUES 
(7, 1, 4), (7, 2, 4), (7, 3, 3), (7, 14, 2), -- UV 7: .NET (4 năm)
(8, 6, 2), (8, 7, 2), (8, 8, 1),             -- UV 8: React (2 năm)
(9, 1, 5), (9, 2, 5), (9, 6, 3), (9, 11, 2), -- UV 9: Fullstack .NET + React (5 năm)
(10, 4, 3), (10, 5, 3), (10, 3, 2),          -- UV 10: Java (3 năm)
(12, 9, 4), (12, 10, 3), (12, 13, 2),        -- UV 12: AI/Python (4 năm)
(13, 11, 3), (13, 12, 2), (13, 13, 3);       -- UV 13: DevOps (3 năm)

-- ---------------------------------------------------------
-- 6. TIN TUYỂN DỤNG (6 Jobs)
-- Ràng buộc: TrangThai (DangMo, DaDong, DaHuy), HinhThuc (FullTime, PartTime, Online, Intern)
-- ---------------------------------------------------------
SET IDENTITY_INSERT TinTuyenDung ON;
INSERT INTO TinTuyenDung (MaTin, MaNhaTuyenDung, TieuDe, PhongBan, DiaDiem, HinhThucLamViec, MucLuongToiThieu, MucLuongToiDa, MoTaCongViec, YeuCauCongViec, QuyenLoi, TrangThai, HanNopCV, DaXoa, NgayTao, NgayCapNhat) VALUES 
(1, 3, N'Senior .NET Developer', 'FSU1', N'Hà Nội', 'FullTime', 30000000, 50000000, N'Phát triển core banking...', N'Ít nhất 4 năm C#, ASP.NET Core', N'Lương tháng 13, BHYT', 'DangMo', '2026-12-31', 0, '2024-04-01 08:00', '2026-04-01 08:00'),
(2, 4, N'Frontend ReactJS (Middle)', 'ZaloPay', N'TP.HCM', 'FullTime', 20000000, 35000000, N'Làm UI/UX cho ví điện tử...', N'Tối thiểu 2 năm ReactJS, Redux, TS', N'Ăn trưa miễn phí', 'DangMo', '2026-11-30', 0, '2026-04-05 09:00', '2026-04-05 09:00'),
(3, 5, N'Java Backend Engineer', 'Viettel Digital', N'Hà Nội', 'FullTime', 25000000, 45000000, N'Xây dựng hệ thống High availability', N'Có kinh nghiệm Spring Boot, Microservices', N'Thưởng dự án', 'DangMo', '2026-10-15', 0, '2026-04-10 10:00', '2024-04-10 10:00'),
(4, 6, N'AI / Machine Learning Engineer', 'Data Team', N'TP.HCM', 'FullTime', 40000000, 70000000, N'Xây dựng model recommend...', N'Thành thạo Python, Tensorflow/PyTorch', N'Cấp Macbook Pro', 'DangMo', '2026-06-01', 0, '2026-04-15 11:00', '2026-04-15 11:00'),
(5, 3, N'Fresher .NET', 'FSU2', N'Đà Nẵng', 'Intern', 5000000, 10000000, N'Đào tạo từ đầu', N'Biết cơ bản C#', N'Được mentor kèm cặp', 'DaDong', '2026-03-30', 0, '2026-02-01 08:00', '2026-03-30 23:59'),
(6, 4, N'Remote DevOps Engineer', 'Cloud Team', N'Toàn quốc', 'Online', 35000000, 55000000, N'Quản lý hệ thống AWS', N'Cứng Docker, K8s, AWS', N'Làm việc tại nhà', 'DaDong', '2026-12-31', 1, '2026-04-20 08:00', '2026-04-22 09:00');
SET IDENTITY_INSERT TinTuyenDung OFF;

-- ---------------------------------------------------------
-- 7. KỸ NĂNG YÊU CẦU CỦA TIN TUYỂN DỤNG
-- Ràng buộc: CapDoYeuCau (BatBuoc, UuTien, KhongBatBuoc)
-- ---------------------------------------------------------
INSERT INTO ChiTietKyNang_TinTuyenDung (MaTin, MaKyNang, CapDoYeuCau) VALUES 
-- Job 1: .NET
(1, 1, 'BatBuoc'), (1, 2, 'BatBuoc'), (1, 3, 'BatBuoc'), (1, 14, 'UuTien'),
-- Job 2: React
(2, 6, 'BatBuoc'), (2, 7, 'BatBuoc'), (2, 8, 'UuTien'),
-- Job 3: Java
(3, 4, 'BatBuoc'), (3, 5, 'BatBuoc'), (3, 3, 'UuTien'),
-- Job 4: AI
(4, 9, 'BatBuoc'), (4, 10, 'BatBuoc'), (4, 15, 'UuTien'),
-- Job 5: Fresher .NET
(5, 1, 'BatBuoc'), (5, 3, 'KhongBatBuoc'),
-- Job 6: DevOps
(6, 11, 'BatBuoc'), (6, 12, 'BatBuoc'), (6, 13, 'BatBuoc');

-- ---------------------------------------------------------
-- 8. ĐƠN ỨNG TUYỂN
-- Ràng buộc: TrangThai (DaNop, AIDaLoc, TrungTuyen, TuChoi)
-- ---------------------------------------------------------
SET IDENTITY_INSERT DonUngTuyen ON;
INSERT INTO DonUngTuyen (MaDon, MaTin, MaUngVien, TenFile, DuongDanFile, DinhDang, NoiDungTrichXuat, TrangThai, NgayNop, NgayCapNhat) VALUES 
-- Job 1 (Senior .NET): UV 7 & UV 9 apply
(1, 1, 7, 'NguyenVanA_NET_CV.pdf', '/cvs/nguyenvana_1.pdf', 'PDF', N'Senior Backend .NET. Kỹ năng: C#, ASP.NET Core, SQL. 4 năm KN.', 'TrungTuyen', '2024-04-10 08:30', '2024-04-15 10:00'), -- UV7 pass
(2, 1, 9, 'LeHoangHai_Fullstack.docx', '/cvs/lehoanghai.docx', 'DOCX', N'Lập trình viên Fullstack. Backend .NET Core 5 năm, Frontend ReactJS 3 năm. Biết dùng Docker.', 'AIDaLoc', '2024-04-11 09:00', '2024-04-11 09:05'),   -- UV9 đang chờ HR xem

-- Job 2 (ReactJS): UV 8 & UV 9 apply
(3, 2, 8, 'TranNgoc_ReactJS.pdf', '/cvs/tranngoc.pdf', 'PDF', N'Frontend React Developer. Có kinh nghiệm với ReactJS, JS, HTML/CSS.', 'AIDaLoc', '2024-04-12 10:00', '2024-04-12 10:05'),   -- UV8
(4, 2, 9, 'LeHoangHai_Fullstack.docx', '/cvs/lehoanghai.docx', 'DOCX', N'Lập trình viên Fullstack. Backend .NET Core 5 năm, Frontend ReactJS 3 năm. Biết dùng Docker.', 'DaNop', '2024-04-13 14:00', '2024-04-13 14:00'),     -- UV9 vừa nộp, AI chưa chạy xong

-- Job 3 (Java): UV 10 apply, UV 8 nộp nhầm
(5, 3, 10, 'PhamTuan_Java.pdf', '/cvs/phamtuan.pdf', 'PDF', N'Java Dev. Tech stack: Java, Spring Boot, MySQL. 3 years experience.', 'TrungTuyen', '2024-04-14 09:00', '2024-04-20 16:00'), -- UV10 pass
(6, 3, 8, 'TranNgoc_ReactJS.pdf', '/cvs/tranngoc.pdf', 'PDF', N'Frontend React Developer. Có kinh nghiệm với ReactJS, JS, HTML/CSS.', 'TuChoi', '2024-04-15 10:00', '2024-04-15 10:10'),    -- UV8 tạch vì sai skill

-- Job 4 (AI): UV 12 apply
(7, 4, 12, 'VuMinhDuc_AI.pdf', '/cvs/vuminhduc.pdf', 'PDF', N'AI/ML Engineer. Python, PyTorch, Scikit-learn. NLP projects.', 'AIDaLoc', '2024-04-16 11:00', '2024-04-16 11:05');    -- UV12

SET IDENTITY_INSERT DonUngTuyen OFF;

-- ---------------------------------------------------------
-- 10. KẾT QUẢ AI PHÂN TÍCH
-- Ràng buộc: TrangThaiXuLy (DangXuLy, HoanThanh, Loi), DeXuat (TuyenNhanh, CoTheCanNhac, LoaiBo)
-- ---------------------------------------------------------
SET IDENTITY_INSERT KetQua_AI ON;
INSERT INTO KetQua_AI (MaKetQua, MaDon, TrangThaiXuLy, DiemPhuHop, TomTatUngVien, KyNangPhuHop_Json, KyNangThieu_Json, DiemManh, DiemYeu, DeXuat, PhanHoiGocTuAI, NgayPhanTich) VALUES 

-- MaDon 1 (UV7 -> Job .NET)
(1, 1, 'HoanThanh', 96.50, 
N'Ứng viên hoàn hảo cho vị trí Senior .NET. Có đủ 4 năm kinh nghiệm.', 
'["C#", "ASP.NET Core", "SQL Server"]', '[]', 
N'Nền tảng .NET vững vàng.', N'Chưa thấy bằng cấp tiếng Anh.', 'TuyenNhanh', '{"status":"success"}', '2024-04-10 08:31'),

-- MaDon 2 (UV9 -> Job .NET)
(2, 2, 'HoanThanh', 90.00, 
N'Ứng viên Fullstack, kỹ năng .NET Core rất tốt (5 năm).', 
'["C#", "ASP.NET Core"]', '["SQL Server"]', 
N'Biết cả Docker là điểm cộng.', N'CV ghi chung chung phần DB.', 'TuyenNhanh', '{"status":"success"}', '2024-04-11 09:02'),

-- MaDon 3 (UV8 -> Job React)
(3, 3, 'HoanThanh', 82.00, 
N'Ứng viên Middle Frontend, đáp ứng đủ yêu cầu cốt lõi.', 
'["ReactJS", "JavaScript"]', '["TypeScript"]', 
N'Kinh nghiệm thực chiến 2 năm.', N'Thiếu TypeScript theo yêu cầu ưu tiên.', 'CoTheCanNhac', '{"status":"success"}', '2024-04-12 10:02'),

-- MaDon 4 (UV9 -> Job React) - AI ĐANG XỬ LÝ (Nên DiemPhuHop = NULL)
(4, 4, 'DangXuLy', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2024-04-13 14:00'),

-- MaDon 5 (UV10 -> Job Java)
(5, 5, 'HoanThanh', 92.00, 
N'Ứng viên chuyên Java Backend, kinh nghiệm khớp với JD.', 
'["Java", "Spring Boot"]', '[]', 
N'Kinh nghiệm 3 năm chuẩn chỉ.', N'Không có.', 'TuyenNhanh', '{"status":"success"}', '2024-04-14 09:03'),

-- MaDon 6 (UV8 -> Job Java - Rớt)
(6, 6, 'HoanThanh', 10.00, 
N'Ứng viên làm ReactJS, ứng tuyển nhầm vị trí Java Backend.', 
'[]', '["Java", "Spring Boot"]', 
N'Không có.', N'Sai hoàn toàn định hướng chuyên môn.', 'LoaiBo', '{"status":"success"}', '2024-04-15 10:02'),

-- MaDon 7 (UV12 -> Job AI)
(7, 7, 'HoanThanh', 95.00, 
N'AI Engineer với kinh nghiệm Python và PyTorch mạnh.', 
'["Python", "Machine Learning"]', '[]', 
N'Có project NLP thực tế.', N'Chưa rõ kỹ năng deploy model.', 'TuyenNhanh', '{"status":"success"}', '2024-04-16 11:03');

SET IDENTITY_INSERT KetQua_AI OFF;

--=================================================
--==================================================

-- =========================================================================
-- SCRIPT MỞ RỘNG DỮ LIỆU - SMART RECRUITER (PHẦN 2)
-- =========================================================================

-- ---------------------------------------------------------
-- 1. THÊM TÀI KHOẢN MỚI (6 HR, 15 Ứng viên)
-- Tiếp nối ID từ 15 đến 35
-- ---------------------------------------------------------
SET IDENTITY_INSERT TaiKhoan ON;
INSERT INTO TaiKhoan (MaTaiKhoan, Email, MatKhauHash, MaVaiTro, TrangThaiHoatDong, NgayTao) VALUES 
-- HRs mới (ID: 15-20)
(15, 'hr@shopee.vn', 'hash_hr5', 2, 1, '2024-05-01 08:00'),
(16, 'tuyendung@tiki.vn', 'hash_hr6', 2, 1, '2024-05-02 08:00'),
(17, 'talent@vnpay.vn', 'hash_hr7', 2, 1, '2024-05-03 08:00'),
(18, 'hr@nashtech.com', 'hash_hr8', 2, 1, '2024-05-04 08:00'),
(19, 'recruitment@katalon.com', 'hash_hr9', 2, 1, '2024-05-05 08:00'),
(20, 'careers@vinai.io', 'hash_hr10', 2, 1, '2024-05-06 08:00'),

-- Candidates mới (ID: 21-35)
(21, 'lethanh.ba@gmail.com', 'hash_uv21', 3, 1, '2024-06-01 09:00'),
(22, 'tranquoctoan.qa@gmail.com', 'hash_uv22', 3, 1, '2024-06-02 10:00'),
(23, 'nguyenha.mobile@gmail.com', 'hash_uv23', 3, 1, '2024-06-03 11:00'),
(24, 'phamhung.ios@gmail.com', 'hash_uv24', 3, 1, '2024-06-04 14:00'),
(25, 'dovan.data@gmail.com', 'hash_uv25', 3, 1, '2024-06-05 15:00'),
(26, 'lynhan.uxui@gmail.com', 'hash_uv26', 3, 1, '2024-06-06 16:00'),
(27, 'vuongdinh.php@gmail.com', 'hash_uv27', 3, 1, '2024-06-07 09:30'),
(28, 'caothang.go@gmail.com', 'hash_uv28', 3, 1, '2024-06-08 10:30'),
(29, 'dinhbao.vue@gmail.com', 'hash_uv29', 3, 1, '2024-06-09 11:30'),
(30, 'truonggiang.ruby@gmail.com', 'hash_uv30', 3, 1, '2024-06-10 14:30'),
(31, 'ngocmai.tester@gmail.com', 'hash_uv31', 3, 1, '2024-06-11 15:30'),
(32, 'hoangnam.sysadmin@gmail.com', 'hash_uv32', 3, 1, '2024-06-12 16:30'),
(33, 'tuananh.fullstack@gmail.com', 'hash_uv33', 3, 1, '2024-06-13 09:00'),
(34, 'minhthu.marketing@gmail.com', 'hash_uv34', 3, 1, '2024-06-14 10:00'),
(35, 'bichtram.hr@gmail.com', 'hash_uv35', 3, 1, '2024-06-15 11:00');
SET IDENTITY_INSERT TaiKhoan OFF;

-- ---------------------------------------------------------
-- 2. HỒ SƠ NHÀ TUYỂN DỤNG MỚI
-- ---------------------------------------------------------
INSERT INTO NhaTuyenDung (MaNhaTuyenDung, TenCongTy, SoDienThoai, Website, DiaChi, MoTa, Logo) VALUES 
(15, N'Shopee Vietnam', '0911223344', 'https://shopee.vn', N'Capital Place, Liễu Giai, Hà Nội', N'Nền tảng thương mại điện tử lớn nhất...', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT4R6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s'),
(16, N'Tiki', '0922334455', 'https://tiki.vn', N'Phổ Quang, Tân Bình, TP.HCM', N'Sàn thương mại điện tử uy tín...', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5R6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s'),
(17, N'VNPay', '0933445566', 'https://vnpay.vn', N'Láng Hạ, Đống Đa, Hà Nội', N'Giải pháp thanh toán điện tử...', NULL),
(18, N'NashTech', '0944556677', 'https://nashtechglobal.com', N'Etown, Tân Bình, TP.HCM', N'Tư vấn và giải pháp công nghệ...', NULL),
(19, N'Katalon', '0955667788', 'https://katalon.com', N'Quận 4, TP.HCM', N'Nền tảng kiểm thử tự động...', NULL),
(20, N'VinAI', '0966778899', 'https://vinai.io', N'Times City, Hai Bà Trưng, Hà Nội', N'Viện nghiên cứu trí tuệ nhân tạo...', NULL);

-- ---------------------------------------------------------
-- 3. HỒ SƠ ỨNG VIÊN MỚI
-- ---------------------------------------------------------
INSERT INTO UngVien (MaUngVien, HoTen, SoDienThoai, LinkLinkedIn, ChucDanhHienTai, SoNamKinhNghiem, AnhDaiDien, GioiThieu) VALUES 
(21, N'Lê Thanh', '0811112222', 'linkedin.com/in/lethanh-ba', N'Senior Business Analyst', 5, NULL, N'Chuyên viên phân tích nghiệp vụ cao cấp...'),
(22, N'Trần Quốc Toản', '0822223333', 'linkedin.com/in/toantq', N'QA/QC Engineer', 3, NULL, N'Đảm bảo chất lượng sản phẩm phần mềm...'),
(23, N'Nguyễn Hà', '0833334444', 'linkedin.com/in/nguyenha-android', N'Android Developer', 4, NULL, N'Lập trình viên di động Android...'),
(24, N'Phạm Hùng', '0844445555', 'linkedin.com/in/phamhung-ios', N'iOS Developer', 2, NULL, N'Lập trình viên di động iOS...'),
(25, N'Đỗ Văn', '0855556666', 'linkedin.com/in/dovan-data', N'Data Engineer', 4, NULL, N'Kỹ sư dữ liệu với kinh nghiệm AWS...'),
(26, N'Lý Nhân', '0866667777', 'linkedin.com/in/lynhan-design', N'UI/UX Designer', 3, NULL, N'Thiết kế trải nghiệm người dùng...'),
(27, N'Vương Đình', '0877778888', 'linkedin.com/in/vuongdinh', N'PHP Developer', 5, NULL, N'Chuyên gia PHP và Laravel...'),
(28, N'Cao Thắng', '0888889999', 'linkedin.com/in/caothang', N'Golang Developer', 3, NULL, N'Lập trình viên Golang hiệu năng cao...'),
(29, N'Đinh Bảo', '0899990000', 'linkedin.com/in/dinhbao', N'Frontend VueJS', 2, NULL, N'Lập trình viên Frontend với VueJS...'),
(30, N'Trường Giang', '0900001111', 'linkedin.com/in/truonggiang', N'Ruby on Rails Dev', 4, NULL, N'Lập trình viên Ruby on Rails...'),
(31, N'Ngọc Mai', '0911110000', 'linkedin.com/in/ngocmai', N'Manual Tester', 1, NULL, N'Kiểm thử viên thủ công...'),
(32, N'Hoàng Nam', '0922221111', 'linkedin.com/in/hoangnam', N'System Admin', 6, NULL, N'Quản trị hệ thống và mạng...'),
(33, N'Tuấn Anh', '0933332222', 'linkedin.com/in/tuananh', N'Fullstack MERN', 3, NULL, N'Lập trình viên Fullstack MERN...'),
(34, N'Minh Thư', '0944443333', '', N'Digital Marketing', 2, NULL, N'Chuyên viên tiếp thị kỹ thuật số...'),
(35, N'Bích Trâm', '0955554444', '', N'HR Executive', 3, NULL, N'Chuyên viên nhân sự tổng hợp...');

-- ---------------------------------------------------------
-- 4. BỔ SUNG DANH MỤC KỸ NĂNG (Thêm 20 Kỹ năng)
-- ID tiếp nối từ 16 đến 35
-- ---------------------------------------------------------
SET IDENTITY_INSERT DanhMucKyNang ON;
INSERT INTO DanhMucKyNang (MaKyNang, TenKyNang, PhanLoai) VALUES 
(16, 'PHP', 'Language'), (17, 'Laravel', 'Framework'),
(18, 'Golang', 'Language'), (19, 'Ruby on Rails', 'Framework'),
(20, 'VueJS', 'Framework'), (21, 'Angular', 'Framework'),
(22, 'Swift', 'Language'), (23, 'Kotlin', 'Language'),
(24, 'Flutter', 'Framework'), (25, 'React Native', 'Framework'),
(26, 'MySQL', 'Database'), (27, 'PostgreSQL', 'Database'),
(28, 'MongoDB', 'Database'), (29, 'Redis', 'Database'),
(30, 'Figma', 'Tool'), (31, 'Selenium', 'Tool'),
(32, 'Appium', 'Tool'), (33, 'Business Analysis', 'SoftSkill'),
(34, 'UML', 'Tool'), (35, 'Linux', 'DevOps');
SET IDENTITY_INSERT DanhMucKyNang OFF;

-- ---------------------------------------------------------
-- 5. KỸ NĂNG CỦA ỨNG VIÊN MỚI
-- ---------------------------------------------------------
INSERT INTO ChiTietKyNang_UngVien (MaUngVien, MaKyNang, SoNamKinhNghiem) VALUES 
(21, 33, 5), (21, 34, 4), (21, 14, 3), -- BA
(22, 31, 3), (22, 32, 2), (22, 26, 3), -- QA
(23, 23, 4), (23, 24, 2), (23, 11, 1), -- Android/Flutter
(24, 22, 2), (24, 25, 1),              -- iOS/React Native
(25, 9, 4), (25, 27, 4), (25, 13, 2),  -- Data (Python, Postgres, AWS)
(26, 30, 3), (26, 7, 1),               -- UI/UX (Figma, JS)
(27, 16, 5), (27, 17, 4), (27, 26, 5), -- PHP/Laravel
(28, 18, 3), (28, 11, 2), (28, 29, 2), -- Golang/Redis
(29, 20, 2), (29, 7, 2),               -- VueJS
(33, 28, 3), (33, 6, 3), (33, 7, 3);   -- MERN Stack (MongoDB, React, JS)

-- ---------------------------------------------------------
-- 6. THÊM TIN TUYỂN DỤNG MỚI (10 Jobs)
-- Tiếp nối ID từ 7 đến 16
-- ---------------------------------------------------------
SET IDENTITY_INSERT TinTuyenDung ON;
INSERT INTO TinTuyenDung (MaTin, MaNhaTuyenDung, TieuDe, PhongBan, DiaDiem, HinhThucLamViec, MucLuongToiThieu, MucLuongToiDa, MoTaCongViec, YeuCauCongViec, QuyenLoi, TrangThai, HanNopCV) VALUES 
(7, 15, N'Senior Golang Developer', 'Backend', N'TP.HCM', 'FullTime', 40000000, 60000000, N'Xây dựng core system e-commerce', N'3+ năm Golang, Microservices, Redis', N'Thưởng cổ phiếu', 'DangMo', '2026-12-31'),
(8, 16, N'PHP/Laravel Dev', 'Product', N'Hà Nội', 'FullTime', 15000000, 25000000, N'Bảo trì hệ thống ERP nội bộ', N'2 năm PHP Laravel, MySQL', N'BHXH full lương', 'DangMo', '2024-10-30'),
(9, 17, N'Mobile Dev (Flutter)', 'Mobile App', N'Hà Nội', 'FullTime', 20000000, 35000000, N'Làm app ví điện tử', N'Thành thạo Flutter, biết Native là lợi thế', N'Môi trường trẻ trung', 'DangMo', '2026-11-15'),
(10, 18, N'QA Automation Engineer', 'Testing', N'Đà Nẵng', 'FullTime', 18000000, 28000000, N'Viết script auto test cho Web & App', N'Selenium, Appium, Java/Python', N'Làm việc với khách Âu', 'DangMo', '2026-12-01'),
(11, 19, N'Senior Business Analyst', 'Consulting', N'TP.HCM', 'FullTime', 30000000, 45000000, N'Lấy yêu cầu khách hàng, vẽ flow', N'Kinh nghiệm BA 4 năm+, UML, SQL', N'Cơ hội Onsite', 'DangMo', '2026-11-20'),
(12, 20, N'Data Engineer', 'Data Lab', N'Hà Nội', 'FullTime', 35000000, 55000000, N'Xây dựng Data Warehouse', N'Python, Postgres, AWS/GCP', N'Chế độ chuyên gia', 'DangMo', '2024-12-31'),
(13, 15, N'UI/UX Designer', 'Design Team', N'TP.HCM', 'FullTime', 15000000, 25000000, N'Thiết kế giao diện App, Web', N'Thành thạo Figma, tư duy UX tốt', N'Cấp Mac Studio', 'DangMo', '2026-10-10'),
(14, 16, N'VueJS Frontend Dev', 'Storefront', N'Toàn quốc', 'Online', 18000000, 30000000, N'Phát triển trang bán hàng', N'Kinh nghiệm VueJS 2 năm+', N'Remote linh hoạt', 'DangMo', '2026-11-11'),
(15, 17, N'iOS Developer (Fresher)', 'Mobile App', N'Hà Nội', 'Intern', 5000000, 8000000, N'Học việc iOS', N'Nắm vững Swift cơ bản', N'Trợ cấp thực tập', 'DaDong', '2026-05-30'),
(16, 18, N'System Administrator', 'IT Helpdesk', N'TP.HCM', 'FullTime', 20000000, 30000000, N'Quản trị server, mạng nội bộ', N'Linux, Windows Server, Network', N'Bảo hiểm PVI', 'DaDong', '2026-06-01');
SET IDENTITY_INSERT TinTuyenDung OFF;

-- ---------------------------------------------------------
-- 7. KỸ NĂNG YÊU CẦU CỦA TIN TUYỂN DỤNG MỚI
-- ---------------------------------------------------------
INSERT INTO ChiTietKyNang_TinTuyenDung (MaTin, MaKyNang, CapDoYeuCau) VALUES 
-- Job 7: Golang
(7, 18, 'BatBuoc'), (7, 29, 'BatBuoc'), (7, 11, 'UuTien'),
-- Job 8: PHP
(8, 16, 'BatBuoc'), (8, 17, 'BatBuoc'), (8, 26, 'BatBuoc'),
-- Job 9: Mobile Flutter
(9, 24, 'BatBuoc'), (9, 22, 'UuTien'), (9, 23, 'UuTien'),
-- Job 10: QA
(10, 31, 'BatBuoc'), (10, 32, 'UuTien'), (10, 4, 'KhongBatBuoc'),
-- Job 11: BA
(11, 33, 'BatBuoc'), (11, 34, 'BatBuoc'), (11, 3, 'UuTien'),
-- Job 12: Data Engineer
(12, 9, 'BatBuoc'), (12, 27, 'BatBuoc'), (12, 13, 'BatBuoc'),
-- Job 13: UI/UX
(13, 30, 'BatBuoc'), (13, 7, 'KhongBatBuoc'),
-- Job 14: VueJS
(14, 20, 'BatBuoc'), (14, 7, 'BatBuoc');

-- ---------------------------------------------------------
-- 9. ĐƠN ỨNG TUYỂN MỚI
-- Tiếp nối ID từ 8 đến 20
-- ---------------------------------------------------------
SET IDENTITY_INSERT DonUngTuyen ON;
INSERT INTO DonUngTuyen (MaDon, MaTin, MaUngVien, TenFile, DuongDanFile, DinhDang, NoiDungTrichXuat, TrangThai, NgayNop) VALUES 
(8, 7, 28, 'CaoThang_Golang.pdf', '/cvs/caothang_go.pdf', 'PDF', N'Golang Developer. Chuyên làm hệ thống high concurrency. Redis, Docker, gRPC.', 'TrungTuyen', '2024-06-10 09:00'), -- Thắng nộp Go (Match cao)
(9, 7, 27, 'VuongDinh_PHP.pdf', '/cvs/vuongdinh_php.pdf', 'PDF', N'Backend Dev. 5 năm PHP, 4 năm sử dụng Laravel framework, MySQL optimization.', 'TuChoi', '2024-06-11 10:00'),    -- Đình (PHP) nộp Go (Fail)
(10, 8, 27, 'VuongDinh_PHP.pdf', '/cvs/vuongdinh_php.pdf', 'PDF', N'Backend Dev. 5 năm PHP, 4 năm sử dụng Laravel framework, MySQL optimization.', 'AIDaLoc', '2024-06-12 11:00'),   -- Đình (PHP) nộp PHP (Match cao)
(11, 9, 23, 'NguyenHa_Mobile.pdf', '/cvs/nguyenha_mobile.pdf', 'PDF', N'Mobile Developer. 4 năm Native Android (Kotlin), 2 năm cross-platform Flutter.', 'DaNop', '2024-06-13 14:00'),      -- Hà nộp Flutter
(12, 10, 22, 'TranToan_QA.pdf', '/cvs/trantoan_qa.pdf', 'PDF', N'Automation QA. Sử dụng Selenium, Cypress. Có 3 năm kinh nghiệm test Web và Mobile app.', 'TrungTuyen', '2024-06-14 15:00'),-- Toản nộp QA
(13, 11, 21, 'LeThanh_BA_CV.pdf', '/cvs/lethanh_ba.pdf', 'PDF', N'Business Analyst với 5 năm kinh nghiệm. Kỹ năng lấy requirements, vẽ UML, viết tài liệu. Có hiểu biết về SQL Server.', 'AIDaLoc', '2024-06-15 16:00'),   -- Thanh nộp BA
(14, 12, 25, 'DoVan_DataEng.pdf', '/cvs/dovan_data.pdf', 'PDF', N'Data Engineer. Tech stack: Python, PostgreSQL, AWS Redshift, Airflow.', 'AIDaLoc', '2024-06-16 09:00'),  -- Văn nộp Data
(15, 13, 26, 'LyNhan_UIUX.pdf', '/cvs/lynhan_uiux.pdf', 'PDF', N'UI/UX Designer. Sử dụng thành thạo Figma, Adobe XD. Đã thiết kế app e-commerce.', 'DaNop', '2024-06-17 10:00'),    -- Nhân nộp UI/UX
(16, 14, 29, 'DinhBao_Vue.pdf', '/cvs/dinhbao_vue.pdf', 'PDF', N'Frontend Dev 2 năm kinh nghiệm. Framework chính là VueJS, Vuex, NuxtJS.', 'AIDaLoc', '2024-06-18 11:00'),  -- Bảo nộp VueJS
(17, 14, 33, 'TuanAnh_MERN.pdf', '/cvs/tuananh_mern.pdf', 'PDF', N'MERN Stack. NodeJS, ReactJS, MongoDB. Có thể làm fullstack cho các dự án SME.', 'TuChoi', '2024-06-19 14:00'),   -- Tuấn Anh (React) nộp VueJS (Mismatch framework)
(18, 15, 24, 'PhamHung_iOS.docx', '/cvs/phamhung_ios.docx', 'DOCX', N'iOS Dev, 2 năm kinh nghiệm với Swift, UIKit, SwiftUI. Từng thử nghiệm React Native 1 dự án nhỏ.', 'TuChoi', '2024-05-20 15:00'),   -- Hùng nộp Fresher iOS nhưng job đã đóng
(19, 12, 28, 'CaoThang_Golang.pdf', '/cvs/caothang_go.pdf', 'PDF', N'Golang Developer. Chuyên làm hệ thống high concurrency. Redis, Docker, gRPC.', 'TuChoi', '2024-06-20 09:00'),   -- Thắng (Go) nộp Data Engineer (Fail)
(20, 2, 33, 'TuanAnh_MERN.pdf', '/cvs/tuananh_mern.pdf', 'PDF', N'MERN Stack. NodeJS, ReactJS, MongoDB. Có thể làm fullstack cho các dự án SME.', 'DaNop', '2024-06-21 10:00');     -- Tuấn Anh nộp React (Job cũ số 2)
SET IDENTITY_INSERT DonUngTuyen OFF;

-- ---------------------------------------------------------
-- 10. KẾT QUẢ AI PHÂN TÍCH MỚI
-- Tiếp nối ID từ 8 đến 20
-- ---------------------------------------------------------
SET IDENTITY_INSERT KetQua_AI ON;
INSERT INTO KetQua_AI (MaKetQua, MaDon, TrangThaiXuLy, DiemPhuHop, TomTatUngVien, KyNangPhuHop_Json, KyNangThieu_Json, DiemManh, DiemYeu, DeXuat) VALUES 
(8, 8, 'HoanThanh', 95.00, N'Golang Developer 3 năm, phù hợp hệ thống high concurrency.', '["Golang", "Redis", "Docker"]', '[]', N'Nắm vững Redis', N'Chưa rõ kĩ năng Cloud', 'TuyenNhanh'),
(9, 9, 'HoanThanh', 20.00, N'Ứng viên làm PHP/Laravel, không có kỹ năng Golang.', '[]', '["Golang", "Redis"]', N'Kinh nghiệm backend lâu năm', N'Sai ngôn ngữ cốt lõi', 'LoaiBo'),
(10, 10, 'HoanThanh', 98.00, N'Ứng viên PHP/Laravel 5 năm, hoàn toàn đáp ứng công việc.', '["PHP", "Laravel", "MySQL"]', '[]', N'Dày dặn kinh nghiệm', N'Không có', 'TuyenNhanh'),
(11, 11, 'DangXuLy', NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(12, 12, 'HoanThanh', 90.00, N'Automation QA tốt, mạnh về Selenium.', '["Selenium"]', '["Appium", "Java/Python"]', N'Kinh nghiệm test đa nền tảng', N'Thiếu Appium theo JD', 'TuyenNhanh'),
(13, 13, 'HoanThanh', 92.00, N'BA Senior, kỹ năng BA và UML xuất sắc.', '["Business Analysis", "UML", "SQL Server"]', '[]', N'Profile rất đẹp', N'Không có', 'TuyenNhanh'),
(14, 14, 'HoanThanh', 96.00, N'Data Engineer có kinh nghiệm AWS và Postgres.', '["Python", "PostgreSQL", "AWS"]', '[]', N'Tech stack hiện đại', N'Không', 'TuyenNhanh'),
(15, 15, 'DangXuLy', NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(16, 16, 'HoanThanh', 85.00, N'Frontend VueJS đáp ứng đủ số năm kinh nghiệm.', '["VueJS", "JavaScript"]', '[]', N'Framework đúng JD', N'Chỉ mới 2 năm', 'CoTheCanNhac'),
(17, 17, 'HoanThanh', 40.00, N'Ứng viên chuyên ReactJS, không có kinh nghiệm VueJS.', '["JavaScript"]', '["VueJS"]', N'Biết JS', N'Sai Framework yêu cầu', 'LoaiBo'),
(18, 18, 'Loi', NULL, NULL, NULL, NULL, NULL, NULL, NULL), -- Lỗi xử lý AI hoặc Job đã đóng
(19, 19, 'HoanThanh', 15.00, N'Ứng viên Backend Golang apply nhầm vị trí Data Engineer.', '[]', '["Python", "PostgreSQL", "AWS"]', N'Giỏi Go', N'Sai định hướng', 'LoaiBo'),
(20, 20, 'DangXuLy', NULL, NULL, NULL, NULL, NULL, NULL, NULL);
SET IDENTITY_INSERT KetQua_AI OFF;

-- ---------------------------------------------------------
-- 11. MOCK LỊCH HẸN PHỎNG VẤN
INSERT INTO LichHenPhongVan (MaDon, NgayPhuongVan, GioPhuongVan, DiaDiem, LinkHop, HinhThuc, GhiChu, TrangThai) VALUES 
(1, '2024-04-12', '09:00', N'Văn phòng FPT, Duy Tân', NULL, 'Offline', N'Phỏng vấn vòng 1 với Tech Lead', 'HoanThanh'),
(5, '2024-04-18', '14:00', NULL, 'https://zoom.us/j/123456789', 'Online', N'Phỏng vấn kỹ thuật trực tuyến', 'HoanThanh'),
(8, '2024-06-15', '10:00', N'Văn phòng Shopee, Liễu Giai', NULL, 'Offline', N'Gặp mặt trực tiếp Bộ phận Nhân sự', 'DaXacNhan'),
(12, '2024-06-20', '15:30', NULL, 'https://meet.google.com/abc-xyz', 'Online', N'Phỏng vấn với Project Manager', 'ChoXacNhan'),
(10, '2024-06-25', '09:00', N'Văn phòng VNPay, Láng Hạ', NULL, 'Offline', N'Phỏng vấn chuyên sâu kỹ thuật', 'ChoXacNhan');


select *from TaiKhoan
select *from UngVien
select *from NhaTuyenDung
select *from TinTuyenDung
select *from ChiTietKyNang_TinTuyenDung
select *from ChiTietKyNang_UngVien
select *from DanhMucKyNang
select *from DonUngTuyen
select *from KetQua_AI
select *from LichHenPhongVan

UPDATE NhaTuyenDung SET Logo = '/img/fpt.png' WHERE MaNhaTuyenDung = 3;
UPDATE NhaTuyenDung SET Logo = '/img/vng.png' WHERE MaNhaTuyenDung = 4;
UPDATE NhaTuyenDung SET Logo = '/img/viettel.png' WHERE MaNhaTuyenDung = 5;
UPDATE NhaTuyenDung SET Logo = '/img/momo.png' WHERE MaNhaTuyenDung = 6;