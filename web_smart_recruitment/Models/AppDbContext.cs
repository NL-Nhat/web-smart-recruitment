using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace web_smart_recruitment.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietKyNangTinTuyenDung> ChiTietKyNangTinTuyenDungs { get; set; }

    public virtual DbSet<ChiTietKyNangUngVien> ChiTietKyNangUngViens { get; set; }

    public virtual DbSet<DanhMucKyNang> DanhMucKyNangs { get; set; }

    public virtual DbSet<DonUngTuyen> DonUngTuyens { get; set; }

    public virtual DbSet<HoSoCv> HoSoCvs { get; set; }

    public virtual DbSet<KetQuaAi> KetQuaAis { get; set; }

    public virtual DbSet<LichHenPhongVan> LichHenPhongVans { get; set; }

    public virtual DbSet<NhaTuyenDung> NhaTuyenDungs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TinTuyenDung> TinTuyenDungs { get; set; }

    public virtual DbSet<UngVien> UngViens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietKyNangTinTuyenDung>(entity =>
        {
            entity.HasKey(e => new { e.MaTin, e.MaKyNang }).HasName("PK__ChiTietK__D6DFCCEF0196E1BF");

            entity.ToTable("ChiTietKyNang_TinTuyenDung");

            entity.Property(e => e.CapDoYeuCau)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("KhongBatBuoc");

            entity.HasOne(d => d.MaKyNangNavigation).WithMany(p => p.ChiTietKyNangTinTuyenDungs)
                .HasForeignKey(d => d.MaKyNang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaKyN__59063A47");

            entity.HasOne(d => d.MaTinNavigation).WithMany(p => p.ChiTietKyNangTinTuyenDungs)
                .HasForeignKey(d => d.MaTin)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaTin__5812160E");
        });

        modelBuilder.Entity<ChiTietKyNangUngVien>(entity =>
        {
            entity.HasKey(e => new { e.MaUngVien, e.MaKyNang }).HasName("PK__ChiTietK__684D6773B482168D");

            entity.ToTable("ChiTietKyNang_UngVien");

            entity.HasOne(d => d.MaKyNangNavigation).WithMany(p => p.ChiTietKyNangUngViens)
                .HasForeignKey(d => d.MaKyNang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaKyN__4AB81AF0");

            entity.HasOne(d => d.MaUngVienNavigation).WithMany(p => p.ChiTietKyNangUngViens)
                .HasForeignKey(d => d.MaUngVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaUng__49C3F6B7");
        });

        modelBuilder.Entity<DanhMucKyNang>(entity =>
        {
            entity.HasKey(e => e.MaKyNang).HasName("PK__DanhMucK__796CFDAF116C440E");

            entity.ToTable("DanhMucKyNang");

            entity.HasIndex(e => e.TenKyNang, "UQ__DanhMucK__89D6F06D2B3C797F").IsUnique();

            entity.Property(e => e.PhanLoai)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenKyNang).HasMaxLength(100);
        });

        modelBuilder.Entity<DonUngTuyen>(entity =>
        {
            entity.HasKey(e => e.MaDon).HasName("PK__DonUngTu__3D89F568EF68CDFC");

            entity.ToTable("DonUngTuyen");

            entity.HasIndex(e => new { e.MaTin, e.TrangThai }, "IX_DonUngTuyen_Tin_TrangThai");

            entity.HasIndex(e => e.MaUngVien, "IX_DonUngTuyen_UngVien");

            entity.HasIndex(e => new { e.MaTin, e.MaUngVien }, "UQ_DonUngTuyen_Tin_UngVien").IsUnique();

            entity.Property(e => e.MaCv).HasColumnName("MaCV");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DaNop");

            entity.HasOne(d => d.MaCvNavigation).WithMany(p => p.DonUngTuyens)
                .HasForeignKey(d => d.MaCv)
                .HasConstraintName("FK__DonUngTuye__MaCV__628FA481");

            entity.HasOne(d => d.MaTinNavigation).WithMany(p => p.DonUngTuyens)
                .HasForeignKey(d => d.MaTin)
                .HasConstraintName("FK__DonUngTuy__MaTin__60A75C0F");

            entity.HasOne(d => d.MaUngVienNavigation).WithMany(p => p.DonUngTuyens)
                .HasForeignKey(d => d.MaUngVien)
                .HasConstraintName("FK__DonUngTuy__MaUng__619B8048");
        });

        modelBuilder.Entity<HoSoCv>(entity =>
        {
            entity.HasKey(e => e.MaCv).HasName("PK__HoSoCV__27258E768F04840A");

            entity.ToTable("HoSoCV");

            entity.Property(e => e.MaCv).HasColumnName("MaCV");
            entity.Property(e => e.DinhDang)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DuongDanFile).HasMaxLength(500);
            entity.Property(e => e.NgayTaiLen)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenFile).HasMaxLength(255);

            entity.HasOne(d => d.MaUngVienNavigation).WithMany(p => p.HoSoCvs)
                .HasForeignKey(d => d.MaUngVien)
                .HasConstraintName("FK__HoSoCV__MaUngVie__5CD6CB2B");
        });

        modelBuilder.Entity<KetQuaAi>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("PK__KetQua_A__D5B3102A683C62EA");

            entity.ToTable("KetQua_AI");

            entity.HasIndex(e => e.DiemPhuHop, "IX_KetQuaAI_DiemPhuHop").IsDescending();

            entity.Property(e => e.DeXuat)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DiemPhuHop).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.KyNangPhuHopJson).HasColumnName("KyNangPhuHop_Json");
            entity.Property(e => e.KyNangThieuJson).HasColumnName("KyNangThieu_Json");
            entity.Property(e => e.NgayPhanTich)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhanHoiGocTuAi).HasColumnName("PhanHoiGocTuAI");
            entity.Property(e => e.TrangThaiXuLy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DangXuLy");

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.KetQuaAis)
                .HasForeignKey(d => d.MaDon)
                .HasConstraintName("FK__KetQua_AI__MaDon__693CA210");
        });

        modelBuilder.Entity<LichHenPhongVan>(entity =>
        {
            entity.HasKey(e => e.MaLichHen).HasName("PK__LichHenP__150F264F0303B328");

            entity.ToTable("LichHenPhongVan");

            entity.HasIndex(e => e.MaDon, "IX_LichHen_MaDon");

            entity.HasIndex(e => e.ThoiGian, "IX_LichHen_ThoiGian");

            entity.Property(e => e.DiaDiem).HasMaxLength(255);
            entity.Property(e => e.HinhThuc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Online");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThoiGian).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("ChoXacNhan");

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.LichHenPhongVans)
                .HasForeignKey(d => d.MaDon)
                .HasConstraintName("FK__LichHenPh__MaDon__6FE99F9F");
        });

        modelBuilder.Entity<NhaTuyenDung>(entity =>
        {
            entity.HasKey(e => e.MaNhaTuyenDung).HasName("PK__NhaTuyen__2BDEB6A6440E4710");

            entity.ToTable("NhaTuyenDung");

            entity.Property(e => e.MaNhaTuyenDung).ValueGeneratedNever();
            entity.Property(e => e.AnhBia).HasMaxLength(500);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.Logo).HasMaxLength(500);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenCongTy).HasMaxLength(150);
            entity.Property(e => e.Website).HasMaxLength(255);

            entity.HasOne(d => d.MaNhaTuyenDungNavigation).WithOne(p => p.NhaTuyenDung)
                .HasForeignKey<NhaTuyenDung>(d => d.MaNhaTuyenDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhaTuyenD__MaNha__3E52440B");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529BFF4CD05");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.VaiTro, "IX_TaiKhoan_VaiTro");

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D105340B86D02B").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.MatKhauHash).HasMaxLength(255);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);
            entity.Property(e => e.VaiTro)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TinTuyenDung>(entity =>
        {
            entity.HasKey(e => e.MaTin).HasName("PK__TinTuyen__314903351B490A01");

            entity.ToTable("TinTuyenDung");

            entity.HasIndex(e => e.MaNhaTuyenDung, "IX_TinTuyenDung_NhaTuyenDung");

            entity.HasIndex(e => new { e.TrangThai, e.HanNopCv }, "IX_TinTuyenDung_TrangThai_HanNop");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.DiaDiem).HasMaxLength(200);
            entity.Property(e => e.HanNopCv)
                .HasColumnType("datetime")
                .HasColumnName("HanNopCV");
            entity.Property(e => e.HinhThucLamViec)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MucLuongToiDa).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MucLuongToiThieu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhongBan).HasMaxLength(100);
            entity.Property(e => e.TieuDe).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DangMo");

            entity.HasOne(d => d.MaNhaTuyenDungNavigation).WithMany(p => p.TinTuyenDungs)
                .HasForeignKey(d => d.MaNhaTuyenDung)
                .HasConstraintName("FK__TinTuyenD__MaNha__4D94879B");
        });

        modelBuilder.Entity<UngVien>(entity =>
        {
            entity.HasKey(e => e.MaUngVien).HasName("PK__UngVien__8FDBA8A9DA12DE26");

            entity.ToTable("UngVien");

            entity.HasIndex(e => e.HoTen, "IX_UngVien_HoTen");

            entity.HasIndex(e => e.SoDienThoai, "UQ__UngVien__0389B7BDD1DB4975").IsUnique();

            entity.Property(e => e.MaUngVien).ValueGeneratedNever();
            entity.Property(e => e.AnhDaiDien).HasMaxLength(500);
            entity.Property(e => e.ChucDanhHienTai).HasMaxLength(150);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.LinkLinkedIn).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoNamKinhNghiem).HasDefaultValue(0);

            entity.HasOne(d => d.MaUngVienNavigation).WithOne(p => p.UngVien)
                .HasForeignKey<UngVien>(d => d.MaUngVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UngVien__MaUngVi__4316F928");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
