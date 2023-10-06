using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using nxLINEadminAPI.Entity;

namespace nxLINEadminAPI.Entity
{
    public partial class nxLINEadminAPIContext : DbContext
    {
        public nxLINEadminAPIContext()
        {
        }

        public nxLINEadminAPIContext(DbContextOptions<nxLINEadminAPIContext> options)
            : base(options)
        {
        }

        
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<LineAccount> LineAccounts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("server=devsv1.japaneast.cloudapp.azure.com;database=nxLINE;user=couponuser;password=M0dnwDnW");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Japanese_CI_AS");

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                
                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserEmail)
                    .HasMaxLength(100)
                    .HasColumnName("user_email");

                entity.Property(e => e.UserLastlogindatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("user_lastlogindatetime");

                entity.Property(e => e.UserLoginId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("user_loginid");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.Property(e => e.UserPwd)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("user_pwd");

                entity.Property(e => e.UserLineAccountID)
                    .HasColumnName("user_lineaccount_id")
                    .HasColumnType("int");
                entity.Property(e => e.UserLineAccountRole)
                    .HasMaxLength(20)
                    .HasColumnName("user_lineaccount_role");

                entity.Property(e => e.UserCreated)
                    .HasColumnName("user_created");
                entity.Property(e => e.UserUpdated)
                    .HasColumnName("user_updated");
            });
            modelBuilder.Entity<LineAccount>(b =>
            {
                b.ToTable("lineaccount");
                b.Property<int>(e => e.LineaccountId)
                    .HasColumnType("int")
                    .HasColumnName("lineaccount_id");

                b.Property<int?>(e => e.EntryPoint)
                    .HasColumnType("int")
                    .HasColumnName("entrypoint");

                b.Property<bool?>(e => e.IsProfile)
                    .HasColumnType("bit")
                    .HasColumnName("isprofile");

                b.Property<bool?>(e => e.IsSmaregi)
                    .HasColumnName("issmaregi")
                    .HasColumnType("bit");

                b.Property<bool?>(e => e.Istalk)
                    .HasColumnName("istalk")
                    .HasColumnType("bit");

                b.Property<string>(e => e.LineChannelAccessToken)
                    .HasColumnName("line_channelaccesstoken")
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>(e => e.LineChannelId)
                    .HasColumnName("line_channelid")
                    .HasMaxLength(10)
                    .HasColumnType("nvarchar(10)");

                b.Property<string>(e => e.LineChannelSecret)
                    .HasColumnName("line_channelsecret")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>(e => e.LineaccountCode)
                    .HasColumnName("lineaccount_code")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>(e => e.LineaccountShortcode)
                    .HasColumnName("lineaccount_shortcode")
                    .IsRequired()
                    .HasMaxLength(5)
                    .HasColumnType("nvarchar(5)");

                b.Property<string>(e => e.LineaccountEmail)
                    .HasColumnName("lineaccount_email")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>(e => e.LineaccountLogoUrl)
                    .HasColumnName("lineaccount_logourl")
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<string>(e => e.LineaccountName)
                    .HasColumnName("lineaccount_name")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<DateTime?>(e => e.LineaccountCreated)
                    .HasColumnName("lineaccount_created")
                    .HasColumnType("datetime2");

                b.Property<DateTime?>(e => e.LineaccountDeleted)
                    .HasColumnName("lineaccount_deleted")
                    .HasColumnType("datetime2");

                b.Property<DateTime?>(e => e.LineaccountUpdated)
                    .HasColumnName("lineaccount_updated")
                    .HasColumnType("datetime2");

                b.Property<string>(e => e.MembersCardColor)
                    .HasColumnName("memberscard_color")
                    .HasMaxLength(10)
                    .HasColumnType("nvarchar(10)");

                b.Property<string>(e => e.MembersCardDesignUrl)
                    .HasColumnName("memberscard_designurl")
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<bool>(e => e.MembersCardIsUseCamera)
                    .HasColumnName("memberscard_isusecamera")
                    .HasColumnType("bit");

                b.Property<string>(e => e.MembersCardLiffId)
                    .HasColumnName("memberscard_liffid")
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<int?>(e => e.PointExpire)
                    .HasColumnName("pointexpire")
                    .HasColumnType("int");

                b.Property<string>(e => e.ProfileSetting)
                    .HasColumnName("profile_setting")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>(e => e.SmaregiContractId)
                    .HasColumnName("smaregi_contractid")
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<int?>(e => e.StartRank)
                    .HasColumnName("startrank")
                    .HasColumnType("int");

                b.Property<string>(e => e.TalkMessage)
                    .HasColumnName("talkmessage")
                    .HasColumnType("nvarchar(max)");

            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public DbSet<nxLINEadminAPI.Entity.Member> Member { get; set; } = default!;
    }
}
