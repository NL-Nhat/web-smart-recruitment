/**
 * Nhãn hiển thị khớp CHECK constraint SmartRecruiterDB
 */
(function (global) {
  const TRANG_THAI_TIN = {
    DangMo: { label: "Đang mở nhận CV", cls: "sr-badge--dangmo" },
    DaDong: { label: "Đã đóng", cls: "sr-badge--dadong" },
    DaHuy: { label: "Đã hủy", cls: "sr-badge--dahuy" },
  };

  const TRANG_THAI_DON = {
    DaNop: { label: "Mới nộp", cls: "sr-badge--danop" },
    AIDaLoc: { label: "AI đã lọc", cls: "sr-badge--aidaloc" },
    TrungTuyen: { label: "Trúng tuyển", cls: "sr-badge--trungtuyen" },
    TuChoi: { label: "Từ chối", cls: "sr-badge--tuchoi" },
    PhongVan: { label: "Phỏng vấn", cls: "sr-badge--phongvan" },
  };

  const DE_XUAT_AI = {
    TuyenNhanh: { label: "Tuyển nhanh", cls: "sr-badge--tuyennhanh" },
    CoTheCanNhac: { label: "Cân nhắc", cls: "sr-badge--canhnac" },
    LoaiBo: { label: "Loại bỏ", cls: "sr-badge--loaibo" },
  };

  const TRANG_THAI_AI = {
    DangXuLy: { label: "Đang xử lý", cls: "sr-badge--dangxuly" },
    HoanThanh: { label: "Hoàn thành", cls: "sr-badge--hoanthanh" },
    Loi: { label: "Lỗi", cls: "sr-badge--loi" },
  };

  const CAP_DO_KN = {
    BatBuoc: "Bắt buộc",
    UuTien: "Ưu tiên",
    KhongBatBuoc: "Không bắt buộc",
  };

  const HINH_THUC = {
    FullTime: "Toàn thời gian",
    PartTime: "Bán thời gian",
    Online: "Làm việc online",
    Intern: "Thực tập",
  };

  function formatVnd(n) {
    if (n == null || n === "") return "—";
    return (
      new Intl.NumberFormat("vi-VN").format(Number(n)) + " đ"
    );
  }

  function badgeHtml(map, key, fallbackText) {
    if (!key && fallbackText)
      return `<span class="sr-badge sr-badge--inactive">${escapeHtml(
        fallbackText
      )}</span>`;
    const x = map[key];
    if (!x)
      return `<span class="sr-badge sr-badge--inactive">${escapeHtml(
        key || "—"
      )}</span>`;
    return `<span class="sr-badge ${x.cls}">${escapeHtml(x.label)}</span>`;
  }

  function escapeHtml(s) {
    if (s == null) return "";
    return String(s)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  global.SR_UI = {
    TRANG_THAI_TIN,
    TRANG_THAI_DON,
    DE_XUAT_AI,
    TRANG_THAI_AI,
    CAP_DO_KN,
    HINH_THUC,
    formatVnd,
    badgeHtml,
    escapeHtml,
    tinBadge: (k) => badgeHtml(TRANG_THAI_TIN, k),
    donBadge: (k) => badgeHtml(TRANG_THAI_DON, k),
    aiDeXuatBadge: (k) => badgeHtml(DE_XUAT_AI, k, "Chưa có"),
    aiTrangThaiBadge: (k) => badgeHtml(TRANG_THAI_AI, k, "—"),
    hinhThucLabel: (k) => HINH_THUC[k] || k || "—",
  };
})(typeof window !== "undefined" ? window : globalThis);
