/**
 * Dữ liệu demo bám SmartRecruiterDB.sql (rút gọn văn bản dài; khóa & quan hệ giữ nguyên)
 */
(function (global) {
  const taiKhoan = [
    { maTaiKhoan: 1, email: "admin1@smartrecruit.vn", vaiTro: "Admin", trangThaiHoatDong: true, hoTen: "Admin Hệ thống 1" },
    { maTaiKhoan: 2, email: "admin2@smartrecruit.vn", vaiTro: "Admin", trangThaiHoatDong: true, hoTen: "Admin Hệ thống 2" },
    { maTaiKhoan: 3, email: "tuyendung@fpt.com.vn", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Nguyễn Minh Tâm" },
    { maTaiKhoan: 4, email: "hr.vng@vng.com.vn", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Lê Trọng Đạt" },
    { maTaiKhoan: 5, email: "recruitment@viettel.vn", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Trần Thu Hà" },
    { maTaiKhoan: 6, email: "hr@momo.vn", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Phạm Băng Băng" },
    { maTaiKhoan: 7, email: "nguyenvana@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Nguyễn Văn A" },
    { maTaiKhoan: 8, email: "tranthingoc@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Trần Thị Ngọc" },
    { maTaiKhoan: 9, email: "lehoanghai.dev@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Lê Hoàng Hải" },
    { maTaiKhoan: 10, email: "phamminhtuan@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Phạm Minh Tuấn" },
    { maTaiKhoan: 11, email: "hoangthanhmai@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Hoàng Thanh Mai" },
    { maTaiKhoan: 12, email: "vuminhduc.ai@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Vũ Minh Đức" },
    { maTaiKhoan: 13, email: "doanvanhau.it@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Đoàn Văn Hậu" },
    { maTaiKhoan: 14, email: "ngotienhiep@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: false, hoTen: "Ngô Tiến Hiệp" },
    { maTaiKhoan: 15, email: "hr@shopee.vn", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Đặng Lê Nguyên" },
    { maTaiKhoan: 16, email: "tuyendung@tiki.vn", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Hoàng Yến" },
    { maTaiKhoan: 17, email: "talent@vnpay.vn", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Bùi Thanh Thủy" },
    { maTaiKhoan: 18, email: "hr@nashtech.com", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Ngô Gia Tự" },
    { maTaiKhoan: 19, email: "recruitment@katalon.com", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Trịnh Xuân" },
    { maTaiKhoan: 20, email: "careers@vinai.io", vaiTro: "NhaTuyenDung", trangThaiHoatDong: true, hoTen: "Mai Phương" },
    { maTaiKhoan: 21, email: "lethanh.ba@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Lê Thanh" },
    { maTaiKhoan: 22, email: "tranquoctoan.qa@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Trần Quốc Toản" },
    { maTaiKhoan: 23, email: "nguyenha.mobile@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Nguyễn Hà" },
    { maTaiKhoan: 24, email: "phamhung.ios@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Phạm Hùng" },
    { maTaiKhoan: 25, email: "dovan.data@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Đỗ Văn" },
    { maTaiKhoan: 26, email: "lynhan.uxui@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Lý Nhân" },
    { maTaiKhoan: 27, email: "vuongdinh.php@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Vương Đình" },
    { maTaiKhoan: 28, email: "caothang.go@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Cao Thắng" },
    { maTaiKhoan: 29, email: "dinhbao.vue@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Đinh Bảo" },
    { maTaiKhoan: 30, email: "truonggiang.ruby@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Trường Giang" },
    { maTaiKhoan: 31, email: "ngocmai.tester@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Ngọc Mai" },
    { maTaiKhoan: 32, email: "hoangnam.sysadmin@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Hoàng Nam" },
    { maTaiKhoan: 33, email: "tuananh.fullstack@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Tuấn Anh" },
    { maTaiKhoan: 34, email: "minhthu.marketing@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Minh Thư" },
    { maTaiKhoan: 35, email: "bichtram.hr@gmail.com", vaiTro: "UngVien", trangThaiHoatDong: true, hoTen: "Bích Trâm" },
  ];

  const nhaTuyenDung = [
    { maNhaTuyenDung: 3, hoTen: "Nguyễn Minh Tâm", tenCongTy: "FPT Software", soDienThoai: "0901111222", website: "https://fptsoftware.com", diaChi: "Duy Tân, Cầu Giấy, Hà Nội", quyMo: "30,000+ nhân viên", moTa: "Công ty xuất khẩu phần mềm hàng đầu Việt Nam, chuyên về chuyển đổi số và công nghệ mới.", logo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR6f9YQZ1I0v-t3zP8i_zY6N-V8u6RjXjP8wQ&s" },
    { maNhaTuyenDung: 4, hoTen: "Lê Trọng Đạt", tenCongTy: "VNG Corporation", soDienThoai: "0903333444", website: "https://vng.com.vn", diaChi: "VNG Campus, Quận 7, TP.HCM", quyMo: "5,000+ nhân viên", moTa: "Kỳ lân công nghệ đầu tiên tại Việt Nam, sở hữu Zalo, ZaloPay và nhiều tựa game đình đám.", logo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTzR6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s" },
    { maNhaTuyenDung: 5, hoTen: "Trần Thu Hà", tenCongTy: "Viettel Group", soDienThoai: "0988888999", website: "https://viettel.vn", diaChi: "Giang Văn Minh, Ba Đình, Hà Nội", quyMo: "50,000+ nhân viên", moTa: "Tập đoàn Viễn thông và Công nghệ hàng đầu khu vực, tiên phong về 5G và giải pháp số.", logo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR1-X1N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s" },
    { maNhaTuyenDung: 6, hoTen: "Phạm Băng Băng", tenCongTy: "MoMo", soDienThoai: "0912222333", website: "https://momo.vn", diaChi: "Lầu 6, Tòa nhà Phú Mỹ Hưng, Quận 7, TP.HCM", quyMo: "2,000+ nhân viên", moTa: "Ví điện tử số 1 Việt Nam, cung cấp hệ sinh thái tài chính và thanh toán toàn diện.", logo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT3R6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s" },
    { maNhaTuyenDung: 15, hoTen: "Đặng Lê Nguyên", tenCongTy: "Shopee Vietnam", soDienThoai: "0911223344", website: "https://shopee.vn", diaChi: "Tòa nhà Capital Place, Liễu Giai, Hà Nội", quyMo: "3,000+ nhân viên", moTa: "Nền tảng thương mại điện tử lớn nhất khu vực Đông Nam Á và Đài Loan.", logo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT4R6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s" },
    { maNhaTuyenDung: 16, hoTen: "Hoàng Yến", tenCongTy: "Tiki", soDienThoai: "0922334455", website: "https://tiki.vn", diaChi: "Phổ Quang, Tân Bình, TP.HCM", quyMo: "2,000+ nhân viên", moTa: "Sàn thương mại điện tử uy tín, tập trung vào trải nghiệm khách hàng và dịch vụ giao hàng nhanh.", logo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5R6X-N1xR_m9QY_N_E-K8z1m_x_z_X1xR_g&s" },
  ];

  const ungVien = [
    { maUngVien: 7, hoTen: "Nguyễn Văn A", soDienThoai: "0971112223", linkLinkedIn: "linkedin.com/in/nguyenvana", chucDanhHienTai: "Backend .NET Developer", soNamKinhNghiem: 4 },
    { maUngVien: 8, hoTen: "Trần Thị Ngọc", soDienThoai: "0972223334", linkLinkedIn: "linkedin.com/in/tranthingoc", chucDanhHienTai: "Frontend ReactJS", soNamKinhNghiem: 2 },
    { maUngVien: 9, hoTen: "Lê Hoàng Hải", soDienThoai: "0973334445", linkLinkedIn: "linkedin.com/in/lehoanghai", chucDanhHienTai: "Fullstack Developer", soNamKinhNghiem: 5 },
    { maUngVien: 10, hoTen: "Phạm Minh Tuấn", soDienThoai: "0974445556", linkLinkedIn: "linkedin.com/in/phamminhtuan", chucDanhHienTai: "Java Backend", soNamKinhNghiem: 3 },
    { maUngVien: 11, hoTen: "Hoàng Thanh Mai", soDienThoai: "0975556667", linkLinkedIn: "linkedin.com/in/hoangthanhmai", chucDanhHienTai: "Data Analyst", soNamKinhNghiem: 2 },
    { maUngVien: 12, hoTen: "Vũ Minh Đức", soDienThoai: "0976667778", linkLinkedIn: "linkedin.com/in/vuminhduc", chucDanhHienTai: "AI Engineer", soNamKinhNghiem: 4 },
    { maUngVien: 13, hoTen: "Đoàn Văn Hậu", soDienThoai: "0977778889", linkLinkedIn: "linkedin.com/in/doanvanhau", chucDanhHienTai: "DevOps Engineer", soNamKinhNghiem: 3 },
    { maUngVien: 14, hoTen: "Ngô Tiến Hiệp", soDienThoai: "0978889990", linkLinkedIn: "", chucDanhHienTai: "Fresher Web", soNamKinhNghiem: 0 },
    { maUngVien: 21, hoTen: "Lê Thanh", soDienThoai: "0811112222", linkLinkedIn: "linkedin.com/in/lethanh-ba", chucDanhHienTai: "Senior Business Analyst", soNamKinhNghiem: 5 },
    { maUngVien: 22, hoTen: "Trần Quốc Toản", soDienThoai: "0822223333", linkLinkedIn: "linkedin.com/in/toantq", chucDanhHienTai: "QA/QC Engineer", soNamKinhNghiem: 3 },
    { maUngVien: 23, hoTen: "Nguyễn Hà", soDienThoai: "0833334444", linkLinkedIn: "linkedin.com/in/nguyenha-android", chucDanhHienTai: "Android Developer", soNamKinhNghiem: 4 },
    { maUngVien: 24, hoTen: "Phạm Hùng", soDienThoai: "0844445555", linkLinkedIn: "linkedin.com/in/phamhung-ios", chucDanhHienTai: "iOS Developer", soNamKinhNghiem: 2 },
    { maUngVien: 25, hoTen: "Đỗ Văn", soDienThoai: "0855556666", linkLinkedIn: "linkedin.com/in/dovan-data", chucDanhHienTai: "Data Engineer", soNamKinhNghiem: 4 },
    { maUngVien: 26, hoTen: "Lý Nhân", soDienThoai: "0866667777", linkLinkedIn: "linkedin.com/in/lynhan-design", chucDanhHienTai: "UI/UX Designer", soNamKinhNghiem: 3 },
    { maUngVien: 27, hoTen: "Vương Đình", soDienThoai: "0877778888", linkLinkedIn: "linkedin.com/in/vuongdinh", chucDanhHienTai: "PHP Developer", soNamKinhNghiem: 5 },
    { maUngVien: 28, hoTen: "Cao Thắng", soDienThoai: "0888889999", linkLinkedIn: "linkedin.com/in/caothang", chucDanhHienTai: "Golang Developer", soNamKinhNghiem: 3 },
    { maUngVien: 29, hoTen: "Đinh Bảo", soDienThoai: "0899990000", linkLinkedIn: "linkedin.com/in/dinhbao", chucDanhHienTai: "Frontend VueJS", soNamKinhNghiem: 2 },
    { maUngVien: 33, hoTen: "Tuấn Anh", soDienThoai: "0933332222", linkLinkedIn: "linkedin.com/in/tuananh", chucDanhHienTai: "Fullstack MERN", soNamKinhNghiem: 3 },
  ];

  const danhMucKyNang = [
    { maKyNang: 1, tenKyNang: "C#", phanLoai: "Language" },
    { maKyNang: 2, tenKyNang: "ASP.NET Core", phanLoai: "Framework" },
    { maKyNang: 3, tenKyNang: "SQL Server", phanLoai: "Database" },
    { maKyNang: 4, tenKyNang: "Java", phanLoai: "Language" },
    { maKyNang: 5, tenKyNang: "Spring Boot", phanLoai: "Framework" },
    { maKyNang: 6, tenKyNang: "ReactJS", phanLoai: "Framework" },
    { maKyNang: 7, tenKyNang: "JavaScript", phanLoai: "Language" },
    { maKyNang: 8, tenKyNang: "TypeScript", phanLoai: "Language" },
    { maKyNang: 9, tenKyNang: "Python", phanLoai: "Language" },
    { maKyNang: 10, tenKyNang: "Machine Learning", phanLoai: "AI" },
    { maKyNang: 11, tenKyNang: "Docker", phanLoai: "DevOps" },
    { maKyNang: 12, tenKyNang: "Kubernetes", phanLoai: "DevOps" },
    { maKyNang: 13, tenKyNang: "AWS", phanLoai: "Cloud" },
    { maKyNang: 14, tenKyNang: "Agile/Scrum", phanLoai: "SoftSkill" },
    { maKyNang: 15, tenKyNang: "English", phanLoai: "Language" },
    { maKyNang: 16, tenKyNang: "PHP", phanLoai: "Language" },
    { maKyNang: 17, tenKyNang: "Laravel", phanLoai: "Framework" },
    { maKyNang: 18, tenKyNang: "Golang", phanLoai: "Language" },
    { maKyNang: 19, tenKyNang: "Ruby on Rails", phanLoai: "Framework" },
    { maKyNang: 20, tenKyNang: "VueJS", phanLoai: "Framework" },
    { maKyNang: 21, tenKyNang: "Angular", phanLoai: "Framework" },
    { maKyNang: 22, tenKyNang: "Swift", phanLoai: "Language" },
    { maKyNang: 23, tenKyNang: "Kotlin", phanLoai: "Language" },
    { maKyNang: 24, tenKyNang: "Flutter", phanLoai: "Framework" },
    { maKyNang: 25, tenKyNang: "React Native", phanLoai: "Framework" },
    { maKyNang: 26, tenKyNang: "MySQL", phanLoai: "Database" },
    { maKyNang: 27, tenKyNang: "PostgreSQL", phanLoai: "Database" },
    { maKyNang: 28, tenKyNang: "MongoDB", phanLoai: "Database" },
    { maKyNang: 29, tenKyNang: "Redis", phanLoai: "Database" },
    { maKyNang: 30, tenKyNang: "Figma", phanLoai: "Tool" },
    { maKyNang: 31, tenKyNang: "Selenium", phanLoai: "Tool" },
    { maKyNang: 32, tenKyNang: "Appium", phanLoai: "Tool" },
    { maKyNang: 33, tenKyNang: "Business Analysis", phanLoai: "SoftSkill" },
    { maKyNang: 34, tenKyNang: "UML", phanLoai: "Tool" },
    { maKyNang: 35, tenKyNang: "Linux", phanLoai: "DevOps" },
  ];

  const tinTuyenDung = [
    { maTin: 1, maNhaTuyenDung: 3, tieuDe: "Senior .NET Developer", phongBan: "FSU1", diaDiem: "Hà Nội", hinhThucLamViec: "FullTime", mucLuongToiThieu: 3e7, mucLuongToiDa: 5e7, moTaCongViec: "Phát triển core banking và các dịch vụ tài chính số.", yeuCauCongViec: "Ít nhất 4 năm C#, ASP.NET Core, SQL Server.", quyenLoi: "Lương tháng 13, BHYT đầy đủ.", trangThai: "DangMo", hanNopCV: "2024-12-31", ngayTao: "2024-04-01" },
    { maTin: 2, maNhaTuyenDung: 4, tieuDe: "Frontend ReactJS (Middle)", phongBan: "ZaloPay", diaDiem: "TP.HCM", hinhThucLamViec: "FullTime", mucLuongToiThieu: 2e7, mucLuongToiDa: 3.5e7, moTaCongViec: "Làm UI/UX cho ví điện tử và các sản phẩm thanh toán.", yeuCauCongViec: "Tối thiểu 2 năm ReactJS, Redux, TypeScript.", quyenLoi: "Ăn trưa miễn phí.", trangThai: "DangMo", hanNopCV: "2024-11-30", ngayTao: "2024-04-05" },
    { maTin: 3, maNhaTuyenDung: 5, tieuDe: "Java Backend Engineer", phongBan: "Viettel Digital", diaDiem: "Hà Nội", hinhThucLamViec: "FullTime", mucLuongToiThieu: 2.5e7, mucLuongToiDa: 4.5e7, moTaCongViec: "Xây dựng hệ thống high availability cho dịch vụ số.", yeuCauCongViec: "Spring Boot, microservices.", quyenLoi: "Thưởng dự án.", trangThai: "DangMo", hanNopCV: "2024-10-15", ngayTao: "2024-04-10" },
    { maTin: 4, maNhaTuyenDung: 6, tieuDe: "AI / Machine Learning Engineer", phongBan: "Data Team", diaDiem: "TP.HCM", hinhThucLamViec: "FullTime", mucLuongToiThieu: 4e7, mucLuongToiDa: 7e7, moTaCongViec: "Xây dựng model gợi ý và xử lý dữ liệu lớn.", yeuCauCongViec: "Python, TensorFlow/PyTorch.", quyenLoi: "Cấp MacBook Pro.", trangThai: "DangMo", hanNopCV: "2024-12-01", ngayTao: "2024-04-15" },
    { maTin: 5, maNhaTuyenDung: 3, tieuDe: "Fresher .NET", phongBan: "FSU2", diaDiem: "Đà Nẵng", hinhThucLamViec: "Intern", mucLuongToiThieu: 5e6, mucLuongToiDa: 1e7, moTaCongViec: "Đào tạo từ đầu.", yeuCauCongViec: "Biết cơ bản C#.", quyenLoi: "Mentor kèm cặp.", trangThai: "DaDong", hanNopCV: "2024-03-30", ngayTao: "2024-02-01" },
    { maTin: 6, maNhaTuyenDung: 4, tieuDe: "Remote DevOps Engineer", phongBan: "Cloud Team", diaDiem: "Toàn quốc", hinhThucLamViec: "Online", mucLuongToiThieu: 3.5e7, mucLuongToiDa: 5.5e7, moTaCongViec: "Quản lý hệ thống AWS.", yeuCauCongViec: "Docker, K8s, AWS.", quyenLoi: "Remote.", trangThai: "DaHuy", hanNopCV: "2024-12-31", ngayTao: "2024-04-20" },
    { maTin: 7, maNhaTuyenDung: 15, tieuDe: "Senior Golang Developer", phongBan: "Backend", diaDiem: "TP.HCM", hinhThucLamViec: "FullTime", mucLuongToiThieu: 4e7, mucLuongToiDa: 6e7, moTaCongViec: "Core system e-commerce.", yeuCauCongViec: "Golang, microservices, Redis.", quyenLoi: "Thưởng cổ phiếu.", trangThai: "DangMo", hanNopCV: "2024-12-31", ngayTao: "2024-06-01" },
    { maTin: 8, maNhaTuyenDung: 16, tieuDe: "PHP/Laravel Dev", phongBan: "Product", diaDiem: "Hà Nội", hinhThucLamViec: "FullTime", mucLuongToiThieu: 1.5e7, mucLuongToiDa: 2.5e7, moTaCongViec: "Bảo trì ERP nội bộ.", yeuCauCongViec: "PHP, Laravel, MySQL.", quyenLoi: "BHXH full lương.", trangThai: "DangMo", hanNopCV: "2024-10-30", ngayTao: "2024-06-02" },
    { maTin: 9, maNhaTuyenDung: 17, tieuDe: "Mobile Dev (Flutter)", phongBan: "Mobile App", diaDiem: "Hà Nội", hinhThucLamViec: "FullTime", mucLuongToiThieu: 2e7, mucLuongToiDa: 3.5e7, moTaCongViec: "App ví điện tử.", yeuCauCongViec: "Flutter; native là lợi thế.", quyenLoi: "Môi trường trẻ.", trangThai: "DangMo", hanNopCV: "2024-11-15", ngayTao: "2024-06-03" },
    { maTin: 10, maNhaTuyenDung: 18, tieuDe: "QA Automation Engineer", phongBan: "Testing", diaDiem: "Đà Nẵng", hinhThucLamViec: "FullTime", mucLuongToiThieu: 1.8e7, mucLuongToiDa: 2.8e7, moTaCongViec: "Auto test Web & App.", yeuCauCongViec: "Selenium, Appium, Java/Python.", quyenLoi: "Khách Âu.", trangThai: "DangMo", hanNopCV: "2024-12-01", ngayTao: "2024-06-04" },
    { maTin: 11, maNhaTuyenDung: 19, tieuDe: "Senior Business Analyst", phongBan: "Consulting", diaDiem: "TP.HCM", hinhThucLamViec: "FullTime", mucLuongToiThieu: 3e7, mucLuongToiDa: 4.5e7, moTaCongViec: "Lấy yêu cầu, vẽ flow.", yeuCauCongViec: "BA 4+ năm, UML, SQL.", quyenLoi: "Onsite.", trangThai: "DangMo", hanNopCV: "2024-11-20", ngayTao: "2024-06-05" },
    { maTin: 12, maNhaTuyenDung: 20, tieuDe: "Data Engineer", phongBan: "Data Lab", diaDiem: "Hà Nội", hinhThucLamViec: "FullTime", mucLuongToiThieu: 3.5e7, mucLuongToiDa: 5.5e7, moTaCongViec: "Data warehouse.", yeuCauCongViec: "Python, Postgres, AWS/GCP.", quyenLoi: "Chế độ chuyên gia.", trangThai: "DangMo", hanNopCV: "2024-12-31", ngayTao: "2024-06-06" },
    { maTin: 13, maNhaTuyenDung: 15, tieuDe: "UI/UX Designer", phongBan: "Design Team", diaDiem: "TP.HCM", hinhThucLamViec: "FullTime", mucLuongToiThieu: 1.5e7, mucLuongToiDa: 2.5e7, moTaCongViec: "Thiết kế App/Web.", yeuCauCongViec: "Figma, tư duy UX.", quyenLoi: "Mac Studio.", trangThai: "DangMo", hanNopCV: "2024-10-10", ngayTao: "2024-06-07" },
    { maTin: 14, maNhaTuyenDung: 16, tieuDe: "VueJS Frontend Dev", phongBan: "Storefront", diaDiem: "Toàn quốc", hinhThucLamViec: "Online", mucLuongToiThieu: 1.8e7, mucLuongToiDa: 3e7, moTaCongViec: "Trang bán hàng.", yeuCauCongViec: "VueJS 2+ năm.", quyenLoi: "Remote linh hoạt.", trangThai: "DangMo", hanNopCV: "2024-11-11", ngayTao: "2024-06-08" },
    { maTin: 15, maNhaTuyenDung: 17, tieuDe: "iOS Developer (Fresher)", phongBan: "Mobile App", diaDiem: "Hà Nội", hinhThucLamViec: "Intern", mucLuongToiThieu: 5e6, mucLuongToiDa: 8e6, moTaCongViec: "Học việc iOS.", yeuCauCongViec: "Swift cơ bản.", quyenLoi: "Trợ cấp.", trangThai: "DaDong", hanNopCV: "2024-05-30", ngayTao: "2024-06-09" },
    { maTin: 16, maNhaTuyenDung: 18, tieuDe: "System Administrator", phongBan: "IT Helpdesk", diaDiem: "TP.HCM", hinhThucLamViec: "FullTime", mucLuongToiThieu: 2e7, mucLuongToiDa: 3e7, moTaCongViec: "Server, mạng nội bộ.", yeuCauCongViec: "Linux, Windows Server.", quyenLoi: "Bảo hiểm PVI.", trangThai: "DaHuy", hanNopCV: "2024-06-01", ngayTao: "2024-06-10" },
  ];

  const chiTietKyNangTin = [
    { maTin: 1, maKyNang: 1, capDoYeuCau: "BatBuoc" },
    { maTin: 1, maKyNang: 2, capDoYeuCau: "BatBuoc" },
    { maTin: 1, maKyNang: 3, capDoYeuCau: "BatBuoc" },
    { maTin: 1, maKyNang: 14, capDoYeuCau: "UuTien" },
    { maTin: 2, maKyNang: 6, capDoYeuCau: "BatBuoc" },
    { maTin: 2, maKyNang: 7, capDoYeuCau: "BatBuoc" },
    { maTin: 2, maKyNang: 8, capDoYeuCau: "UuTien" },
    { maTin: 7, maKyNang: 18, capDoYeuCau: "BatBuoc" },
    { maTin: 7, maKyNang: 29, capDoYeuCau: "BatBuoc" },
    { maTin: 7, maKyNang: 11, capDoYeuCau: "UuTien" },
    { maTin: 14, maKyNang: 20, capDoYeuCau: "BatBuoc" },
    { maTin: 14, maKyNang: 7, capDoYeuCau: "BatBuoc" },
  ];

  const hoSoCV = [
    { maCV: 1, maUngVien: 7, tenFile: "NguyenVanA_NET_CV.pdf", duongDanFile: "/cvs/nguyenvana_1.pdf", dinhDang: "PDF" },
    { maCV: 2, maUngVien: 8, tenFile: "TranNgoc_ReactJS.pdf", duongDanFile: "/cvs/tranngoc.pdf", dinhDang: "PDF" },
    { maCV: 3, maUngVien: 9, tenFile: "LeHoangHai_Fullstack.docx", duongDanFile: "/cvs/lehoanghai.docx", dinhDang: "DOCX" },
    { maCV: 4, maUngVien: 10, tenFile: "PhamTuan_Java.pdf", duongDanFile: "/cvs/phamtuan.pdf", dinhDang: "PDF" },
    { maCV: 5, maUngVien: 12, tenFile: "VuMinhDuc_AI.pdf", duongDanFile: "/cvs/vuminhduc.pdf", dinhDang: "PDF" },
    { maCV: 13, maUngVien: 27, tenFile: "VuongDinh_PHP.pdf", duongDanFile: "/cvs/vuongdinh_php.pdf", dinhDang: "PDF" },
    { maCV: 14, maUngVien: 28, tenFile: "CaoThang_Golang.pdf", duongDanFile: "/cvs/caothang_go.pdf", dinhDang: "PDF" },
    { maCV: 15, maUngVien: 29, tenFile: "DinhBao_Vue.pdf", duongDanFile: "/cvs/dinhbao_vue.pdf", dinhDang: "PDF" },
    { maCV: 16, maUngVien: 33, tenFile: "TuanAnh_MERN.pdf", duongDanFile: "/cvs/tuananh_mern.pdf", dinhDang: "PDF" },
  ];

  const donUngTuyen = [
    { maDon: 1, maTin: 1, maUngVien: 7, maCV: 1, trangThai: "TrungTuyen", ngayNop: "2024-04-10" },
    { maDon: 2, maTin: 1, maUngVien: 9, maCV: 3, trangThai: "AIDaLoc", ngayNop: "2024-04-11" },
    { maDon: 3, maTin: 2, maUngVien: 8, maCV: 2, trangThai: "AIDaLoc", ngayNop: "2024-04-12" },
    { maDon: 4, maTin: 2, maUngVien: 9, maCV: 3, trangThai: "DaNop", ngayNop: "2024-04-13" },
    { maDon: 5, maTin: 3, maUngVien: 10, maCV: 4, trangThai: "TrungTuyen", ngayNop: "2024-04-14" },
    { maDon: 6, maTin: 3, maUngVien: 8, maCV: 2, trangThai: "TuChoi", ngayNop: "2024-04-15" },
    { maDon: 7, maTin: 4, maUngVien: 12, maCV: 5, trangThai: "AIDaLoc", ngayNop: "2024-04-16" },
    { maDon: 8, maTin: 7, maUngVien: 28, maCV: 14, trangThai: "PhongVan", ngayNop: "2024-06-10" },
    { maDon: 9, maTin: 7, maUngVien: 27, maCV: 13, trangThai: "TuChoi", ngayNop: "2024-06-11" },
    { maDon: 10, maTin: 8, maUngVien: 27, maCV: 13, trangThai: "PhongVan", ngayNop: "2024-06-12" },
    { maDon: 11, maTin: 9, maUngVien: 23, maCV: 9, trangThai: "PhongVan", ngayNop: "2024-06-13" },
    { maDon: 12, maTin: 10, maUngVien: 22, maCV: 8, trangThai: "PhongVan", ngayNop: "2024-06-14" },
    { maDon: 16, maTin: 14, maUngVien: 29, maCV: 15, trangThai: "PhongVan", ngayNop: "2024-06-18" },
    { maDon: 17, maTin: 14, maUngVien: 33, maCV: 16, trangThai: "TuChoi", ngayNop: "2024-06-19" },
  ];

  const ketQuaAI = [
    { 
      maKetQua: 1, maDon: 1, trangThaiXuLy: "HoanThanh", diemPhuHop: 96.5, 
      tomTatUngVien: "Ứng viên sở hữu nền tảng kỹ thuật cực kỳ vững chắc trong hệ sinh thái .NET. Với hơn 4 năm kinh nghiệm thực chiến tại các dự án lớn, ứng viên thể hiện khả năng thiết kế hệ thống và tối ưu hóa database vượt trội.", 
      kyNangPhuHopJson: '["C#","ASP.NET Core","SQL Server"]', kyNangThieuJson: "[]", 
      diemManh: "Kỹ năng lập trình backend xuất sắc; Am hiểu sâu về SQL Server Performance Tuning; Có kinh nghiệm triển khai Microservices.", 
      diemYeu: "Thiếu các chứng chỉ ngoại ngữ quốc tế (IELTS/TOEIC) dù có khả năng đọc hiểu tài liệu tốt.", 
      deXuat: "TuyenNhanh" 
    },
    { 
      maKetQua: 2, maDon: 2, trangThaiXuLy: "HoanThanh", diemPhuHop: 90, 
      tomTatUngVien: "Lập trình viên Fullstack với xu hướng thiên về Backend .NET Core. Có tư duy logic tốt và khả năng thích nghi nhanh với các công nghệ mới như Docker và CI/CD.", 
      kyNangPhuHopJson: '["C#","ASP.NET Core"]', kyNangThieuJson: '["SQL Server"]', 
      diemManh: "Kỹ năng giải quyết vấn đề tốt; Có kinh nghiệm với Docker và Containerization; Thái độ làm việc chuyên nghiệp.", 
      diemYeu: "Phần mô tả về kinh nghiệm làm việc với Database trên CV còn khá sơ sài, cần phỏng vấn kỹ thêm phần này.", 
      deXuat: "TuyenNhanh" 
    },
    { 
      maKetQua: 3, maDon: 3, trangThaiXuLy: "HoanThanh", diemPhuHop: 82, 
      tomTatUngVien: "Ứng viên có kỹ năng Frontend ổn định, đặc biệt mạnh về ReactJS và tối ưu hóa UI. Tuy nhiên, việc thiếu hụt kiến thức về TypeScript là một điểm trừ nhỏ đối với dự án hiện tại.", 
      kyNangPhuHopJson: '["ReactJS","JavaScript"]', kyNangThieuJson: '["TypeScript"]', 
      diemManh: "Khả năng xây dựng UI/UX tinh tế; Nắm vững React Hooks và State Management; Có sản phẩm thực tế tốt.", 
      diemYeu: "Chưa có kinh nghiệm thực tế với TypeScript; Cần bổ sung thêm kiến thức về Unit Testing cho Frontend.", 
      deXuat: "CoTheCanNhac" 
    },
    { 
      maKetQua: 4, maDon: 4, trangThaiXuLy: "DangXuLy", diemPhuHop: null, 
      tomTatUngVien: null, kyNangPhuHopJson: null, kyNangThieuJson: null, 
      deXuat: null 
    },
    { 
      maKetQua: 5, maDon: 5, trangThaiXuLy: "HoanThanh", diemPhuHop: 92, 
      tomTatUngVien: "Chuyên gia Java Backend với sự am hiểu sâu sắc về Spring Framework. Ứng viên có kinh nghiệm xử lý các bài toán về High Availability và Distributed System.", 
      kyNangPhuHopJson: '["Java","Spring Boot"]', kyNangThieuJson: "[]", 
      diemManh: "Nắm vững Java Core và Design Patterns; Có kinh nghiệm làm việc với hệ thống lớn (High Availability); Tư duy thuật toán tốt.", 
      diemYeu: "Khả năng giao tiếp tiếng Anh ở mức trung bình, cần cải thiện để làm việc trong môi trường quốc tế.", 
      deXuat: "TuyenNhanh" 
    },
    { 
      maKetQua: 6, maDon: 6, trangThaiXuLy: "HoanThanh", diemPhuHop: 10, 
      tomTatUngVien: "Hồ sơ hoàn toàn không phù hợp. Ứng viên là lập trình viên Frontend nhưng lại ứng tuyển vào vị trí Java Backend. Không có sự tương đồng về Techstack yêu cầu.", 
      kyNangPhuHopJson: "[]", kyNangThieuJson: '["Java","Spring Boot"]', 
      diemManh: "Không tìm thấy điểm mạnh phù hợp với vị trí này.", 
      diemYeu: "Thiếu hụt toàn bộ kỹ năng cốt lõi yêu cầu; Sai lệch hoàn toàn về định hướng chuyên môn.", 
      deXuat: "LoaiBo" 
    },
    { 
      maKetQua: 7, maDon: 7, trangThaiXuLy: "HoanThanh", diemPhuHop: 95, 
      tomTatUngVien: "AI Engineer tiềm năng với các dự án NLP ấn tượng. Có khả năng nghiên cứu và triển khai các model phức tạp bằng Python và PyTorch.", 
      kyNangPhuHopJson: '["Python","Machine Learning"]', kyNangThieuJson: "[]", 
      diemManh: "Nền tảng Toán học và Xác suất thống kê tốt; Kinh nghiệm thực chiến với NLP; Thành thạo các thư viện Deep Learning.", 
      diemYeu: "Kỹ năng triển khai model lên môi trường Production (Model Deployment) còn hạn chế.", 
      deXuat: "TuyenNhanh" 
    },
    { 
      maKetQua: 8, maDon: 8, trangThaiXuLy: "HoanThanh", diemPhuHop: 95, 
      tomTatUngVien: "Golang Developer dày dặn kinh nghiệm, chuyên về hệ thống High Concurrency. Đã từng làm việc với Redis và Docker trong môi trường thực tế.", 
      kyNangPhuHopJson: '["Golang","Redis","Docker"]', kyNangThieuJson: "[]", 
      diemManh: "Khả năng tối ưu hóa hiệu suất hệ thống cực tốt; Am hiểu về kiến trúc Microservices; Cứng tay về Docker và Redis.", 
      diemYeu: "Ít kinh nghiệm làm việc với các dịch vụ Cloud như AWS hay Azure.", 
      deXuat: "TuyenNhanh" 
    },
    { 
      maKetQua: 10, maDon: 10, trangThaiXuLy: "HoanThanh", diemPhuHop: 98, 
      tomTatUngVien: "Ứng viên PHP/Laravel kỳ cựu. Khả năng lead team và quản lý dự án tốt. Hệ thống hóa quy trình phát triển phần mềm bài bản.", 
      kyNangPhuHopJson: '["PHP","Laravel","MySQL"]', kyNangThieuJson: "[]", 
      diemManh: "Kinh nghiệm 5 năm PHP chuyên sâu; Khả năng tối ưu hóa database query xuất sắc; Kỹ năng quản lý nhóm tốt.", 
      diemYeu: "Cần cập nhật thêm các xu hướng Frontend hiện đại để phối hợp tốt hơn với team UI.", 
      deXuat: "TuyenNhanh" 
    }
  ];

  const lichHenPhongVan = [
    { maLichHen: 1, maDon: 1, thoiGian: "2024-04-12T09:00", diaDiem: "Văn phòng FPT, Duy Tân", hinhThuc: "Offline", ghiChu: "Phỏng vấn vòng 1 với Tech Lead", trangThai: "HoanThanh" },
    { maLichHen: 2, maDon: 5, thoiGian: "2024-04-18T14:00", diaDiem: "https://zoom.us/j/123456789", hinhThuc: "Online", ghiChu: "Phỏng vấn kỹ thuật trực tuyến", trangThai: "HoanThanh" },
    { maLichHen: 3, maDon: 8, thoiGian: "2024-06-15T10:00", diaDiem: "Văn phòng Shopee, Liễu Giai", hinhThuc: "Offline", ghiChu: "Gặp mặt trực tiếp Bộ phận Nhân sự", trangThai: "DaXacNhan" },
    { maLichHen: 4, maDon: 10, thoiGian: "2024-06-25T09:00", diaDiem: "Văn phòng VNPay, Láng Hạ", hinhThuc: "Offline", ghiChu: "Phỏng vấn chuyên sâu kỹ thuật", trangThai: "ChoXacNhan" },
    { maLichHen: 5, maDon: 16, thoiGian: "2024-06-18T11:00", diaDiem: "https://meet.google.com/abc-xyz", hinhThuc: "Online", ghiChu: "Phỏng vấn với Project Manager", trangThai: "ChoXacNhan" },
  ];

  function skillName(id) {
    const s = danhMucKyNang.find((k) => k.maKyNang === id);
    return s ? s.tenKyNang : "#" + id;
  }

  function tenCongTy(maNha) {
    const n = nhaTuyenDung.find((x) => x.maNhaTuyenDung === maNha);
    return n ? n.tenCongTy : "—";
  }

  function getUngVien(ma) {
    return ungVien.find((u) => u.maUngVien === ma);
  }

  function getTin(maTin) {
    return tinTuyenDung.find((t) => t.maTin === maTin);
  }

  function getDon(maDon) {
    return donUngTuyen.find((d) => d.maDon === maDon);
  }

  function getCV(maCV) {
    return hoSoCV.find((c) => c.maCV === maCV);
  }

  function getKetQuaByDon(maDon) {
    return ketQuaAI.find((k) => k.maDon === maDon);
  }

  function getLichHenByDon(maDon) {
    return lichHenPhongVan.find((l) => l.maDon === maDon);
  }

  function skillsForTin(maTin) {
    return chiTietKyNangTin
      .filter((c) => c.maTin === maTin)
      .map((c) => ({
        ...c,
        tenKyNang: skillName(c.maKyNang),
      }));
  }

  /** Thống kê demo (API: DB chưa có bảng log — hiển thị ước lượng minh họa) */
  const thongKeDemo = {
    jobDangMo: tinTuyenDung.filter((t) => t.trangThai === "DangMo").length,
    cvNopThang: donUngTuyen.filter((d) => d.ngayNop && d.ngayNop.startsWith("2024-06")).length,
    apiAiCallUocLuong: 1842,
  };

  global.SR_DEMO = {
    taiKhoan,
    nhaTuyenDung,
    ungVien,
    danhMucKyNang,
    tinTuyenDung,
    chiTietKyNangTin,
    hoSoCV,
    donUngTuyen,
    ketQuaAI,
    lichHenPhongVan,
    thongKeDemo,
    skillName,
    tenCongTy,
    getUngVien,
    getTin,
    getDon,
    getCV,
    getKetQuaByDon,
    getLichHenByDon,
    skillsForTin,
  };
})(typeof window !== "undefined" ? window : globalThis);
