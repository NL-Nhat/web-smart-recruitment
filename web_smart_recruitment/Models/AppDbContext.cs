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


    public virtual DbSet<KetQuaAi> KetQuaAis { get; set; }

    public virtual DbSet<LichHenPhongVan> LichHenPhongVans { get; set; }

    public virtual DbSet<NhaTuyenDung> NhaTuyenDungs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TinTuyenDung> TinTuyenDungs { get; set; }

    public virtual DbSet<UngVien> UngViens { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietKyNangTinTuyenDung>(entity =>
        {
            entity.HasKey(e => new { e.MaTin, e.MaKyNang }).HasName("PK__ChiTietK__D6DFCCEFA1802241");

            entity.ToTable("ChiTietKyNang_TinTuyenDung");

            entity.Property(e => e.CapDoYeuCau)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("KhongBatBuoc");

            entity.HasOne(d => d.MaKyNangNavigation).WithMany(p => p.ChiTietKyNangTinTuyenDungs)
                .HasForeignKey(d => d.MaKyNang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaKyN__5BE2A6F2");

            entity.HasOne(d => d.MaTinNavigation).WithMany(p => p.ChiTietKyNangTinTuyenDungs)
                .HasForeignKey(d => d.MaTin)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaTin__5AEE82B9");
        });

        modelBuilder.Entity<ChiTietKyNangUngVien>(entity =>
        {
            entity.HasKey(e => new { e.MaUngVien, e.MaKyNang }).HasName("PK__ChiTietK__684D67732DB0845F");

            entity.ToTable("ChiTietKyNang_UngVien");

            entity.HasOne(d => d.MaKyNangNavigation).WithMany(p => p.ChiTietKyNangUngViens)
                .HasForeignKey(d => d.MaKyNang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaKyN__4D94879B");

            entity.HasOne(d => d.MaUngVienNavigation).WithMany(p => p.ChiTietKyNangUngViens)
                .HasForeignKey(d => d.MaUngVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKy__MaUng__4CA06362");
        });

        modelBuilder.Entity<DanhMucKyNang>(entity =>
        {
            entity.HasKey(e => e.MaKyNang).HasName("PK__DanhMucK__796CFDAF7F065284");

            entity.ToTable("DanhMucKyNang");

            entity.HasIndex(e => e.TenKyNang, "UQ__DanhMucK__89D6F06DBABF0F47").IsUnique();

            entity.Property(e => e.PhanLoai)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenKyNang).HasMaxLength(100);
        });

        modelBuilder.Entity<DonUngTuyen>(entity =>
        {
            entity.HasKey(e => e.MaDon).HasName("PK__DonUngTu__3D89F5682ED3B2CF");

            entity.ToTable("DonUngTuyen");

            entity.HasIndex(e => new { e.MaTin, e.TrangThai }, "IX_DonUngTuyen_Tin_TrangThai");

            entity.HasIndex(e => e.MaUngVien, "IX_DonUngTuyen_UngVien");

            entity.HasIndex(e => new { e.MaTin, e.MaUngVien }, "UQ_DonUngTuyen_Tin_UngVien").IsUnique();

            entity.Property(e => e.TenFile).HasMaxLength(255);
            entity.Property(e => e.DuongDanFile).HasMaxLength(500);
            entity.Property(e => e.DinhDang)
                .HasMaxLength(20)
                .IsUnicode(false);
            
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

            entity.HasOne(d => d.MaTinNavigation).WithMany(p => p.DonUngTuyens)
                .HasForeignKey(d => d.MaTin)
                .HasConstraintName("FK__DonUngTuy__MaTin__6383C8BA");

            entity.HasOne(d => d.MaUngVienNavigation).WithMany(p => p.DonUngTuyens)
                .HasForeignKey(d => d.MaUngVien)
                .HasConstraintName("FK__DonUngTuy__MaUng__6477ECF3");
        });


        modelBuilder.Entity<KetQuaAi>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("PK__KetQua_A__D5B3102A890F0299");

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
                .HasConstraintName("FK__KetQua_AI__MaDon__6C190EBB");
        });

        modelBuilder.Entity<LichHenPhongVan>(entity =>
        {
            entity.HasKey(e => e.MaLichHen).HasName("PK__LichHenP__150F264FA02A08B9");

            entity.ToTable("LichHenPhongVan");

            entity.HasIndex(e => e.MaDon, "IX_LichHen_MaDon");

            entity.HasIndex(e => e.NgayPhuongVan, "IX_LichHen_Ngay");

            entity.Property(e => e.DiaDiem).HasMaxLength(255);
            entity.Property(e => e.HinhThuc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Online");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayPhuongVan).HasColumnType("date");
            entity.Property(e => e.GioPhuongVan).HasColumnType("time");
            entity.Property(e => e.LinkHop).HasMaxLength(500);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("ChoXacNhan");

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.LichHenPhongVans)
                .HasForeignKey(d => d.MaDon)
                .HasConstraintName("FK__LichHenPh__MaDon__72C60C4A");
        });

        modelBuilder.Entity<NhaTuyenDung>(entity =>
        {
            entity.HasKey(e => e.MaNhaTuyenDung).HasName("PK__NhaTuyen__2BDEB6A6C4AEBD79");

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
                .HasConstraintName("FK__NhaTuyenD__MaNha__412EB0B6");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529CDF87740");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.MaVaiTro, "IX_TaiKhoan_VaiTro");

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D10534803AAFA1").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.MatKhauHash).HasMaxLength(255);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaVaiTro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiKhoan__MaVaiT__3E52440B");
        });

        modelBuilder.Entity<TinTuyenDung>(entity =>
        {
            entity.HasKey(e => e.MaTin).HasName("PK__TinTuyen__3149033513AB985A");

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
                .HasConstraintName("FK__TinTuyenD__MaNha__5070F446");
        });

        modelBuilder.Entity<UngVien>(entity =>
        {
            entity.HasKey(e => e.MaUngVien).HasName("PK__UngVien__8FDBA8A9939F8E79");

            entity.ToTable("UngVien");

            entity.HasIndex(e => e.HoTen, "IX_UngVien_HoTen");

            entity.HasIndex(e => e.SoDienThoai, "UQ__UngVien__0389B7BD33C97281").IsUnique();

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
                .HasConstraintName("FK__UngVien__MaUngVi__45F365D3");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF6B74BB6A");

            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.TenVaiTro, "UQ__VaiTro__1DA55814A32830E8").IsUnique();

            entity.Property(e => e.TenVaiTro)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
