using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace prjTravel.Models;

public partial class TravelDbContext : DbContext
{
    public TravelDbContext()
    {
    }

    public TravelDbContext(DbContextOptions<TravelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Advertise> Advertises { get; set; }

    public virtual DbSet<Classify> Classifies { get; set; }

    public virtual DbSet<Folder> Folders { get; set; }

    public virtual DbSet<FolderPicture> FolderPictures { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=I:\\其他電腦\\家裡電腦\\C#練習\\網頁練習\\緯育\\ASP.NET Core MVC\\成品\\slnTravel\\prjTravel\\App_Data\\dbProject.mdf;Integrated Security=True;Trusted_Connection=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Advertise>(entity =>
        {
            entity.HasKey(e => e.Aid);

            entity.ToTable("Advertise");

            entity.Property(e => e.Aid).HasColumnName("AId");
            entity.Property(e => e.Acid).HasColumnName("ACid");
            entity.Property(e => e.AfolderId)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("AFolderId");
            entity.Property(e => e.Apictures)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("APictures");
            entity.Property(e => e.Arow).HasColumnName("ARow");
            entity.Property(e => e.Astatus).HasColumnName("AStatus");
        });

        modelBuilder.Entity<Classify>(entity =>
        {
            entity.HasKey(e => e.Cid).HasName("PK__tmp_ms_x__C1F8DC390024029E");

            entity.ToTable("Classify");

            entity.Property(e => e.Cid).HasColumnName("CId");
            entity.Property(e => e.Cname)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("CName");
            entity.Property(e => e.Cstatus)
                .HasDefaultValueSql("((1))")
                .HasColumnName("CStatus");
        });

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasKey(e => e.FfolderId).HasName("PK__tmp_ms_x__6ABFCD717C835F2D");

            entity.ToTable("Folder");

            entity.Property(e => e.FfolderId)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("FFolderId");
            entity.Property(e => e.Fcid).HasColumnName("FCid");
            entity.Property(e => e.FcreateTime)
                .HasColumnType("datetime")
                .HasColumnName("FCreateTime");
            entity.Property(e => e.FcreateUser)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("FCreateUser");
            entity.Property(e => e.Fdescription)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("FDescription");
            entity.Property(e => e.FeditTime)
                .HasColumnType("datetime")
                .HasColumnName("FEditTime");
            entity.Property(e => e.FeditUser)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("FEditUser");
            entity.Property(e => e.Fpicture)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("FPicture");
            entity.Property(e => e.Fstatus)
                .HasDefaultValueSql("((1))")
                .HasColumnName("FStatus");
            entity.Property(e => e.Ftitle)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("FTitle");
        });

        modelBuilder.Entity<FolderPicture>(entity =>
        {
            entity.HasKey(e => e.Pid);

            entity.ToTable("FolderPicture");

            entity.Property(e => e.Pid).HasColumnName("PId");
            entity.Property(e => e.PcontentClassify)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("PContentClassify");
            entity.Property(e => e.Pfid)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("PFId");
            entity.Property(e => e.Ppicture)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("PPicture");
            entity.Property(e => e.Prow).HasColumnName("PRow");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Muid).HasName("PK__Member__D02F83610ED4D452");

            entity.ToTable("Member");

            entity.Property(e => e.Muid)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("MUid");
            entity.Property(e => e.Mmail)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("MMail");
            entity.Property(e => e.Mname)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("MName");
            entity.Property(e => e.Mpwd)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("MPwd");
            entity.Property(e => e.Mrole)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("MRole");
            entity.Property(e => e.Mstatus)
                .HasDefaultValueSql("((1))")
                .HasColumnName("MStatus");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Rid).HasName("PK__Role__CAFF40D2E1B91017");

            entity.ToTable("Role");

            entity.Property(e => e.Rid)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("RId");
            entity.Property(e => e.Rname)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnName("RName");
            entity.Property(e => e.Rstatus)
                .HasDefaultValueSql("((1))")
                .HasColumnName("RStatus");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
