/**
 * Đăng nhập demo — sessionStorage (không thay thế backend; khớp VaiTro TaiKhoan)
 */
(function (global) {
  const KEY = "smartrecruiter_demo_session";
  const REG_KEY = "smartrecruiter_demo_registered";

  function getRegistered() {
    try {
      return JSON.parse(localStorage.getItem(REG_KEY)) || [];
    } catch {
      return [];
    }
  }

  function allUsers() {
    const base = (global.SR_DEMO && global.SR_DEMO.taiKhoan) || [];
    return base.concat(getRegistered());
  }

  function getSession() {
    try {
      const raw = sessionStorage.getItem(KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  function setSession(obj) {
    sessionStorage.setItem(KEY, JSON.stringify(obj));
  }

  function clearSession() {
    sessionStorage.removeItem(KEY);
  }

  /**
   * @param {string} email
   * @param {string} password — demo: bất kỳ chuỗi khác rỗng
   * @param {string} vaiTro UngVien | NhaTuyenDung | Admin
   */
  function login(email, password, vaiTro) {
    const em = (email || "").trim().toLowerCase();
    if (!em) return { ok: false, message: "Vui lòng nhập email." };
    if (!password) return { ok: false, message: "Vui lòng nhập mật khẩu." };
    if (!vaiTro) return { ok: false, message: "Chọn vai trò đăng nhập." };

    const user = allUsers().find(
      (u) => u.email.toLowerCase() === em && u.vaiTro === vaiTro
    );
    if (!user)
      return {
        ok: false,
        message:
          "Không tìm thấy tài khoản với email và vai trò này trong bộ dữ liệu demo.",
      };
    if (!user.trangThaiHoatDong)
      return { ok: false, message: "Tài khoản đã bị khóa (TrangThaiHoatDong = 0)." };

    const tenHienThi = user.hoTen || user.email.split("@")[0];

    setSession({
      maTaiKhoan: user.maTaiKhoan,
      email: user.email,
      vaiTro: user.vaiTro,
      tenHienThi,
    });
    return { ok: true };
  }

  function registerCandidate(email, password, hoTen) {
    const em = (email || "").trim().toLowerCase();
    if (!em || !password || !hoTen)
      return { ok: false, message: "Điền đủ họ tên, email và mật khẩu." };
    if (allUsers().some((u) => u.email.toLowerCase() === em))
      return { ok: false, message: "Email đã tồn tại trong demo." };

    const list = getRegistered();
    const nextId =
      Math.max(0, ...allUsers().map((u) => u.maTaiKhoan || 0), 999) + 1;
    list.push({
      maTaiKhoan: nextId,
      email: em,
      matKhauHash: "(demo)",
      vaiTro: "UngVien",
      trangThaiHoatDong: true,
      hoTen: hoTen.trim(),
      isRegisteredDemo: true,
    });
    localStorage.setItem(REG_KEY, JSON.stringify(list));
    return { ok: true, message: "Đăng ký thành công. Vui lòng đăng nhập." };
  }

  function logout() {
    clearSession();
  }

  function redirectByRole() {
    const s = getSession();
    if (!s) return global.location.pathname.includes("login") ? null : "login";
    if (s.vaiTro === "UngVien") return "candidate/jobs.html";
    if (s.vaiTro === "NhaTuyenDung") return "hr/dashboard.html";
    if (s.vaiTro === "Admin") return "admin/dashboard.html";
    return "login.html";
  }

  /**
   * @param {string|string[]} allowed - vai trò được phép
   * @param {string} loginPath - đường dẫn tới login từ thư mục hiện tại
   */
  function requireAuth(allowed, loginPath) {
    const s = getSession();
    const roles = Array.isArray(allowed) ? allowed : [allowed];
    if (!s) {
      global.location.href =
        loginPath +
        "?next=" +
        encodeURIComponent(global.location.pathname.split("/").pop() || "");
      return null;
    }
    if (!roles.includes(s.vaiTro)) {
      global.location.href = loginPath;
      return null;
    }
    return s;
  }

  function updateNavPlaceholders() {
    const el = document.getElementById("sr-user-slot");
    if (!el) return;
    const s = getSession();
    if (s) {
      const profilePath = s.vaiTro === "UngVien" 
        ? (location.pathname.includes("/Candidate/") ? "profile" : "Candidate/profile")
        : (location.pathname.includes("/Hr/") ? "profile" : "Hr/profile");
      
      el.innerHTML = `
        <a href="${profilePath}" class="sr-nav-user-link" style="text-decoration: none; color: var(--el-black); font-weight: 600; margin-right: 20px;">
          ${SR_UI.escapeHtml(s.tenHienThi || s.email)}
        </a>
        <a class="el-btn-white" style="font-size: 13px; padding: 6px 16px; border: 1px solid var(--el-border); text-decoration: none;" href="#" id="sr-logout-btn">Đăng xuất</a>
      `;
      
      const btn = document.getElementById("sr-logout-btn");
      if (btn)
        btn.addEventListener("click", (e) => {
          e.preventDefault();
          logout();
          const root = el.dataset.logoutHref || (location.pathname.includes("/demo/") ? "../index.html" : "index.html");
          global.location.href = root;
        });
    }
  }

  global.SR_AUTH = {
    KEY,
    getSession,
    setSession,
    clearSession,
    login,
    logout,
    registerCandidate,
    requireAuth,
    updateNavPlaceholders,
    redirectByRole,
    allUsers,
  };
})(typeof window !== "undefined" ? window : globalThis);
