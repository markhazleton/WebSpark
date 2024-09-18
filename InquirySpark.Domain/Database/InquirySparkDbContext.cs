using Microsoft.EntityFrameworkCore;

namespace InquirySpark.Domain.Database;

public partial class InquirySparkDbContext : DbContext
{
    public InquirySparkDbContext()
    {
    }

    public InquirySparkDbContext(DbContextOptions<InquirySparkDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppProperty> AppProperties { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ApplicationSurvey> ApplicationSurveys { get; set; }

    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public virtual DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

    public virtual DbSet<ChartSetting> ChartSettings { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<ImportHistory> ImportHistories { get; set; }

    public virtual DbSet<LuApplicationType> LuApplicationTypes { get; set; }

    public virtual DbSet<LuQuestionType> LuQuestionTypes { get; set; }

    public virtual DbSet<LuReviewStatus> LuReviewStatuses { get; set; }

    public virtual DbSet<LuSurveyResponseStatus> LuSurveyResponseStatuses { get; set; }

    public virtual DbSet<LuSurveyType> LuSurveyTypes { get; set; }

    public virtual DbSet<LuUnitOfMeasure> LuUnitOfMeasures { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionAnswer> QuestionAnswers { get; set; }

    public virtual DbSet<QuestionGroup> QuestionGroups { get; set; }

    public virtual DbSet<QuestionGroupMember> QuestionGroupMembers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SiteAppMenu> SiteAppMenus { get; set; }

    public virtual DbSet<SiteRole> SiteRoles { get; set; }

    public virtual DbSet<Survey> Surveys { get; set; }

    public virtual DbSet<SurveyEmailTemplate> SurveyEmailTemplates { get; set; }

    public virtual DbSet<SurveyResponse> SurveyResponses { get; set; }

    public virtual DbSet<SurveyResponseAnswer> SurveyResponseAnswers { get; set; }

    public virtual DbSet<SurveyResponseAnswerError> SurveyResponseAnswerErrors { get; set; }

    public virtual DbSet<SurveyResponseAnswerReview> SurveyResponseAnswerReviews { get; set; }

    public virtual DbSet<SurveyResponseHistory> SurveyResponseHistories { get; set; }

    public virtual DbSet<SurveyResponseSequence> SurveyResponseSequences { get; set; }

    public virtual DbSet<SurveyResponseState> SurveyResponseStates { get; set; }

    public virtual DbSet<SurveyReviewStatus> SurveyReviewStatuses { get; set; }

    public virtual DbSet<SurveyStatus> SurveyStatuses { get; set; }

    public virtual DbSet<TblFile> TblFiles { get; set; }

    public virtual DbSet<UserAppProperty> UserAppProperties { get; set; }

    public virtual DbSet<UserMessage> UserMessages { get; set; }

    public virtual DbSet<WebPortal> WebPortals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppProperty>(entity =>
        {
            entity.ToTable("AppProperty");

            entity.Property(e => e.Key)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.SiteAppId).HasColumnName("SiteAppID");
            entity.Property(e => e.Value)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");

            entity.HasOne(d => d.SiteApp).WithMany(p => p.AppProperties)
                .HasForeignKey(d => d.SiteAppId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Application");

            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.ApplicationCd)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("ApplicationCD");
            entity.Property(e => e.ApplicationDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("ApplicationDS");
            entity.Property(e => e.ApplicationFolder)
                .HasDefaultValue("SurveyAdmin")
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(150)");
            entity.Property(e => e.ApplicationNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(250)")
                .HasColumnName("ApplicationNM");
            entity.Property(e => e.ApplicationShortNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("ApplicationShortNM");
            entity.Property(e => e.ApplicationTypeId).HasColumnName("ApplicationTypeID");
            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.DefaultPageId)
                .HasDefaultValue(63)
                .HasColumnName("DefaultPageID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");

            entity.HasOne(d => d.ApplicationType).WithMany(p => p.Applications)
                .HasForeignKey(d => d.ApplicationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.Applications).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<ApplicationSurvey>(entity =>
        {
            entity.ToTable("ApplicationSurvey");

            entity.HasIndex(e => new { e.ApplicationId, e.SurveyId }, "ApplicationSurvey_UK_ApplicationSurvey")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.ApplicationSurveyId).HasColumnName("ApplicationSurveyID");
            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.DefaultRoleId).HasColumnName("DefaultRoleID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId).HasColumnName("ModifiedID");
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationSurveys)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DefaultRole).WithMany(p => p.ApplicationSurveys)
                .HasForeignKey(d => d.DefaultRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Survey).WithMany(p => p.ApplicationSurveys)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("ApplicationUser");

            entity.HasIndex(e => e.AccountNm, "ApplicationUser_UK_ApplicationUser_AccountNM")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.ApplicationUserId).HasColumnName("ApplicationUserID");
            entity.Property(e => e.AccountNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("AccountNM");
            entity.Property(e => e.CommentDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("CommentDS");
            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.DisplayName)
                .HasDefaultValue("Display CompanyNm")
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(150)");
            entity.Property(e => e.EMailAddress)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("eMailAddress");
            entity.Property(e => e.EmailVerified).HasColumnType("bit");
            entity.Property(e => e.FirstNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("FirstNM");
            entity.Property(e => e.LastLoginDt)
                .HasColumnType("datetime")
                .HasColumnName("LastLoginDT");
            entity.Property(e => e.LastLoginLocation)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.LastNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("LastNM");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.Password)
                .HasDefaultValue("password")
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(150)");
            entity.Property(e => e.RoleId)
                .HasDefaultValue(4)
                .HasColumnName("RoleID");
            entity.Property(e => e.SupervisorAccountNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("SupervisorAccountNM");
            entity.Property(e => e.UserKey).HasColumnType("guid");
            entity.Property(e => e.UserLogin)
                .HasDefaultValue("User Login")
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(150)");
            entity.Property(e => e.VerifyCode)
                .HasDefaultValue("Verify Code")
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");

            entity.HasOne(d => d.Company).WithMany(p => p.ApplicationUsers).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Role).WithMany(p => p.ApplicationUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ApplicationUserRole>(entity =>
        {
            entity.ToTable("ApplicationUserRole");

            entity.HasIndex(e => new { e.ApplicationId, e.ApplicationUserId }, "ApplicationUserRole_UK_ApplicationUserRole")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.ApplicationUserRoleId).HasColumnName("ApplicationUserRoleID");
            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.ApplicationUserId).HasColumnName("ApplicationUserID");
            entity.Property(e => e.IsDemo).HasColumnType("bit");
            entity.Property(e => e.IsMonthlyPrice).HasColumnType("bit");
            entity.Property(e => e.IsUserAdmin).HasColumnType("bit");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.Price).HasColumnType("numeric");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.StartUpDate)
                .UseCollation("NOCASE")
                .HasColumnType("datetime");
            entity.Property(e => e.UserInRolled).HasColumnType("bit");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationUserRoles)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ApplicationUser).WithMany(p => p.ApplicationUserRoles)
                .HasForeignKey(d => d.ApplicationUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Role).WithMany(p => p.ApplicationUserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ChartSetting>(entity =>
        {
            entity.ToTable("ChartSetting");

            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.LastUpdated).HasColumnType("datetime");
            entity.Property(e => e.SettingName)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.SettingType)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.SettingValue)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.SettingValueEnhanced)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.SiteAppId).HasColumnName("SiteAppID");
            entity.Property(e => e.SiteUserId).HasColumnName("SiteUserID");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company");

            entity.HasIndex(e => e.CompanyCd, "Company_UK_CompanyCD")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.ActiveFl)
                .HasColumnType("bit")
                .HasColumnName("ActiveFL");
            entity.Property(e => e.Address1)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)");
            entity.Property(e => e.Address2)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)");
            entity.Property(e => e.City)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.CompanyCd)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("CompanyCD");
            entity.Property(e => e.CompanyDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("CompanyDS");
            entity.Property(e => e.CompanyNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("CompanyNM");
            entity.Property(e => e.Component)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.Country)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.DefaultInvoiceDescription)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.DefaultPaymentTerms)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)");
            entity.Property(e => e.DefaultTheme)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)");
            entity.Property(e => e.FaxNumber)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(30)");
            entity.Property(e => e.FromEmail)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.GalleryFolder)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.PhoneNumber)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(30)");
            entity.Property(e => e.PostalCode)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)");
            entity.Property(e => e.SiteUrl)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("SiteURL");
            entity.Property(e => e.Smtp)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("SMTP");
            entity.Property(e => e.State)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)");
            entity.Property(e => e.Theme)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)");
            entity.Property(e => e.Title)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)");
        });

        modelBuilder.Entity<ImportHistory>(entity =>
        {
            entity.ToTable("ImportHistory");

            entity.Property(e => e.ImportHistoryId).HasColumnName("ImportHistoryID");
            entity.Property(e => e.FileName)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(150)");
            entity.Property(e => e.ImportLog)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ImportType)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
        });

        modelBuilder.Entity<LuApplicationType>(entity =>
        {
            entity.HasKey(e => e.ApplicationTypeId);

            entity.ToTable("lu_ApplicationType");

            entity.HasIndex(e => e.ApplicationTypeNm, "lu_ApplicationType_UK_lu_ApplicationType_ApplicationTypeNM")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.ApplicationTypeId).HasColumnName("ApplicationTypeID");
            entity.Property(e => e.ApplicationTypeDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("ApplicationTypeDS");
            entity.Property(e => e.ApplicationTypeNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("ApplicationTypeNM");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
        });

        modelBuilder.Entity<LuQuestionType>(entity =>
        {
            entity.HasKey(e => e.QuestionTypeId);

            entity.ToTable("lu_QuestionType");

            entity.Property(e => e.QuestionTypeId).HasColumnName("QuestionTypeID");
            entity.Property(e => e.AnswerDataType)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)");
            entity.Property(e => e.ControlName)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.QuestionTypeCd)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("QuestionTypeCD");
            entity.Property(e => e.QuestionTypeDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("QuestionTypeDS");
        });

        modelBuilder.Entity<LuReviewStatus>(entity =>
        {
            entity.HasKey(e => e.ReviewStatusId);

            entity.ToTable("lu_ReviewStatus");

            entity.Property(e => e.ReviewStatusId).HasColumnName("ReviewStatusID");
            entity.Property(e => e.ApprovedFl)
                .HasColumnType("bit")
                .HasColumnName("ApprovedFL");
            entity.Property(e => e.CommentFl)
                .HasColumnType("bit")
                .HasColumnName("CommentFL");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.ReviewStatusDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("ReviewStatusDS");
            entity.Property(e => e.ReviewStatusNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("ReviewStatusNM");
        });

        modelBuilder.Entity<LuSurveyResponseStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId);

            entity.ToTable("lu_SurveyResponseStatus");

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.EmailTemplate)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.NextStatusId).HasColumnName("NextStatusID");
            entity.Property(e => e.PreviousStatusId).HasColumnName("PreviousStatusID");
            entity.Property(e => e.StatusDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("StatusDS");
            entity.Property(e => e.StatusNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("StatusNM");
        });

        modelBuilder.Entity<LuSurveyType>(entity =>
        {
            entity.HasKey(e => e.SurveyTypeId);

            entity.ToTable("lu_SurveyType");

            entity.HasIndex(e => e.SurveyTypeNm, "lu_SurveyType_UK_lu_SurveyType_SurveyTypeNM")
                .IsUnique()
                .IsDescending();

            entity.HasIndex(e => e.SurveyTypeShortNm, "lu_SurveyType_UK_lu_SurveyType_SurveyTypeShortNM")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.SurveyTypeId).HasColumnName("SurveyTypeID");
            entity.Property(e => e.ApplicationTypeId).HasColumnName("ApplicationTypeID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.MutiSequenceFl)
                .HasColumnType("bit")
                .HasColumnName("MutiSequenceFL");
            entity.Property(e => e.ParentSurveyTypeId).HasColumnName("ParentSurveyTypeID");
            entity.Property(e => e.SurveyTypeComment)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.SurveyTypeDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("SurveyTypeDS");
            entity.Property(e => e.SurveyTypeNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("SurveyTypeNM");
            entity.Property(e => e.SurveyTypeShortNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("SurveyTypeShortNM");
        });

        modelBuilder.Entity<LuUnitOfMeasure>(entity =>
        {
            entity.HasKey(e => e.UnitOfMeasureId);

            entity.ToTable("lu_UnitOfMeasure");

            entity.Property(e => e.UnitOfMeasureId).HasColumnName("UnitOfMeasureID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.UnitOfMeasureDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("UnitOfMeasureDS");
            entity.Property(e => e.UnitOfMeasureNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("UnitOfMeasureNM");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Question");

            entity.HasIndex(e => e.QuestionShortNm, "Question_UK_QuestionShortName")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.CommentFl)
                .HasColumnType("bit")
                .HasColumnName("CommentFL");
            entity.Property(e => e.Keywords)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.QuestionDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("QuestionDS");
            entity.Property(e => e.QuestionNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("QuestionNM");
            entity.Property(e => e.QuestionShortNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(75)")
                .HasColumnName("QuestionShortNM");
            entity.Property(e => e.QuestionTypeId).HasColumnName("QuestionTypeID");
            entity.Property(e => e.SurveyTypeId).HasColumnName("SurveyTypeID");
            entity.Property(e => e.UnitOfMeasureId).HasColumnName("UnitOfMeasureID");

            entity.HasOne(d => d.QuestionType).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SurveyType).WithMany(p => p.Questions)
                .HasForeignKey(d => d.SurveyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.Questions)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuestionAnswer>(entity =>
        {
            entity.ToTable("QuestionAnswer");

            entity.HasIndex(e => new { e.QuestionId, e.QuestionAnswerShortNm }, "QuestionAnswer_UK_QuestionAnswer_ShortNMQuestionID")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.QuestionAnswerId).HasColumnName("QuestionAnswerID");
            entity.Property(e => e.ActiveFl)
                .HasColumnType("bit")
                .HasColumnName("ActiveFL");
            entity.Property(e => e.CommentFl)
                .HasColumnType("bit")
                .HasColumnName("CommentFL");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.QuestionAnswerDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("QuestionAnswerDS");
            entity.Property(e => e.QuestionAnswerNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("QuestionAnswerNM");
            entity.Property(e => e.QuestionAnswerShortNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("QuestionAnswerShortNM");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuestionGroup>(entity =>
        {
            entity.ToTable("QuestionGroup");

            entity.Property(e => e.QuestionGroupId).HasColumnName("QuestionGroupID");
            entity.Property(e => e.DependentMaxScore).HasColumnType("numeric");
            entity.Property(e => e.DependentMinScore).HasColumnType("numeric");
            entity.Property(e => e.DependentQuestionGroupId).HasColumnName("DependentQuestionGroupID");
            entity.Property(e => e.GroupFooter)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.GroupHeader)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.QuestionGroupDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("QuestionGroupDS");
            entity.Property(e => e.QuestionGroupNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("QuestionGroupNM");
            entity.Property(e => e.QuestionGroupShortNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("QuestionGroupShortNM");
            entity.Property(e => e.QuestionGroupWeight)
                .HasDefaultValue(1)
                .HasColumnType("numeric");
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");

            entity.HasOne(d => d.Survey).WithMany(p => p.QuestionGroups)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuestionGroupMember>(entity =>
        {
            entity.ToTable("QuestionGroupMember");

            entity.Property(e => e.QuestionGroupMemberId).HasColumnName("QuestionGroupMemberID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.QuestionGroupId).HasColumnName("QuestionGroupID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.QuestionWeight).HasColumnType("numeric");

            entity.HasOne(d => d.QuestionGroup).WithMany(p => p.QuestionGroupMembers)
                .HasForeignKey(d => d.QuestionGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionGroupMembers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.ReadFl)
                .HasColumnType("bit")
                .HasColumnName("ReadFL");
            entity.Property(e => e.ReviewLevel).HasDefaultValue(1);
            entity.Property(e => e.RoleCd)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("RoleCD");
            entity.Property(e => e.RoleDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("RoleDS");
            entity.Property(e => e.RoleNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("RoleNM");
            entity.Property(e => e.UpdateFl)
                .HasColumnType("bit")
                .HasColumnName("UpdateFL");
        });

        modelBuilder.Entity<SiteAppMenu>(entity =>
        {
            entity.ToTable("SiteAppMenu");

            entity.HasIndex(e => new { e.MenuText, e.SiteAppId }, "SiteAppMenu_UK_SiteAppMenu")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.GlyphName)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.MenuText)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.SiteAppId).HasColumnName("SiteAppID");
            entity.Property(e => e.SiteRoleId).HasColumnName("SiteRoleID");
            entity.Property(e => e.TartgetPage)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ViewInMenu).HasColumnType("bit");

            entity.HasOne(d => d.SiteApp).WithMany(p => p.SiteAppMenus)
                .HasForeignKey(d => d.SiteAppId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SiteRole>(entity =>
        {
            entity.ToTable("SiteRole");

            entity.Property(e => e.Active).HasColumnType("bit");
            entity.Property(e => e.RoleName)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(150)");
        });

        modelBuilder.Entity<Survey>(entity =>
        {
            entity.ToTable("Survey");

            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");
            entity.Property(e => e.AutoAssignFilter)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.CompletionMessage)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.EndDt)
                .UseCollation("NOCASE")
                .HasColumnType("datetime")
                .HasColumnName("EndDT");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.ParentSurveyId).HasColumnName("ParentSurveyID");
            entity.Property(e => e.ResponseNmtemplate)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("ResponseNMTemplate");
            entity.Property(e => e.ReviewerAccountNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("ReviewerAccountNM");
            entity.Property(e => e.StartDt)
                .UseCollation("NOCASE")
                .HasColumnType("datetime")
                .HasColumnName("StartDT");
            entity.Property(e => e.SurveyDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("SurveyDS");
            entity.Property(e => e.SurveyNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("SurveyNM");
            entity.Property(e => e.SurveyShortNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("SurveyShortNM");
            entity.Property(e => e.SurveyTypeId).HasColumnName("SurveyTypeID");
            entity.Property(e => e.UseQuestionGroupsFl)
                .HasColumnType("bit")
                .HasColumnName("UseQuestionGroupsFL");

            entity.HasOne(d => d.SurveyType).WithMany(p => p.Surveys)
                .HasForeignKey(d => d.SurveyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyEmailTemplate>(entity =>
        {
            entity.ToTable("SurveyEmailTemplate");

            entity.Property(e => e.SurveyEmailTemplateId).HasColumnName("SurveyEmailTemplateID");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnType("bit");
            entity.Property(e => e.EmailTemplate)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.EndDt)
                .HasColumnType("datetime")
                .HasColumnName("EndDT");
            entity.Property(e => e.FilterCriteria)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.FromEmailAddress)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(150)");
            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId).HasColumnName("ModifiedID");
            entity.Property(e => e.SendToSupervisor).HasColumnType("bit");
            entity.Property(e => e.StartDt)
                .HasColumnType("datetime")
                .HasColumnName("StartDT");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.SubjectTemplate)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.SurveyEmailTemplateNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(250)")
                .HasColumnName("SurveyEmailTemplateNM");
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyEmailTemplates)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyResponse>(entity =>
        {
            entity.ToTable("SurveyResponse");

            entity.HasIndex(e => new { e.SurveyResponseNm, e.AssignedUserId }, "SurveyResponse_SurveyResponse_UK")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.SurveyResponseId).HasColumnName("SurveyResponseID");
            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.AssignedUserId).HasColumnName("AssignedUserID");
            entity.Property(e => e.DataSource)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(250)");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");
            entity.Property(e => e.SurveyResponseNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(250)")
                .HasColumnName("SurveyResponseNM");

            entity.HasOne(d => d.Application).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.SurveyResponses).HasForeignKey(d => d.AssignedUserId);

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyResponseAnswer>(entity =>
        {
            entity.HasKey(e => e.SurveyAnswerId);

            entity.ToTable("SurveyResponseAnswer");

            entity.Property(e => e.SurveyAnswerId).HasColumnName("SurveyAnswerID");
            entity.Property(e => e.AnswerComment)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.AnswerDate).HasColumnType("datetime");
            entity.Property(e => e.AnswerQuantity).HasColumnType("float");
            entity.Property(e => e.AnswerType)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)");
            entity.Property(e => e.ModifiedComment)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.QuestionAnswerId).HasColumnName("QuestionAnswerID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.SequenceNumber).HasDefaultValue(1);
            entity.Property(e => e.SurveyResponseId).HasColumnName("SurveyResponseID");

            entity.HasOne(d => d.QuestionAnswer).WithMany(p => p.SurveyResponseAnswers)
                .HasForeignKey(d => d.QuestionAnswerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Question).WithMany(p => p.SurveyResponseAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SequenceNumberNavigation).WithMany(p => p.SurveyResponseAnswerSequenceNumberNavigations)
                .HasForeignKey(d => d.SequenceNumber)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SurveyResponse).WithMany(p => p.SurveyResponseAnswerSurveyResponses)
                .HasForeignKey(d => d.SurveyResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyResponseAnswerError>(entity =>
        {
            entity.HasKey(e => e.SurveyAnswerErrorId);

            entity.ToTable("SurveyResponseAnswer_Error");

            entity.Property(e => e.SurveyAnswerErrorId).HasColumnName("SurveyAnswer_ErrorID");
            entity.Property(e => e.AnswerComment)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.AnswerDate)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.AnswerQuantity)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.AnswerType)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ErrorCode)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ErrorMessage)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.ProgramName)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.QuestionAnswerId).HasColumnName("QuestionAnswerID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.SequenceNumber).HasDefaultValue(1);
            entity.Property(e => e.SurveyResponseId).HasColumnName("SurveyResponseID");
        });

        modelBuilder.Entity<SurveyResponseAnswerReview>(entity =>
        {
            entity.ToTable("SurveyResponseAnswerReview");

            entity.Property(e => e.SurveyResponseAnswerReviewId).HasColumnName("SurveyResponseAnswerReviewID");
            entity.Property(e => e.ApplicationUserRoleId).HasColumnName("ApplicationUserRoleID");
            entity.Property(e => e.ModifiedComment)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.ReviewLevel).HasDefaultValue(1);
            entity.Property(e => e.ReviewStatusId).HasColumnName("ReviewStatusID");
            entity.Property(e => e.SurveyAnswerId).HasColumnName("SurveyAnswerID");

            entity.HasOne(d => d.ApplicationUserRole).WithMany(p => p.SurveyResponseAnswerReviews)
                .HasForeignKey(d => d.ApplicationUserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SurveyAnswer).WithMany(p => p.SurveyResponseAnswerReviews)
                .HasForeignKey(d => d.SurveyAnswerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyResponseHistory>(entity =>
        {
            entity.ToTable("SurveyResponseHistory");

            entity.Property(e => e.SurveyResponseHistoryId).HasColumnName("SurveyResponseHistoryID");
            entity.Property(e => e.Answers)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ApplicationUserId).HasColumnName("ApplicationUserID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.QuestionGroupId).HasColumnName("QuestionGroupID");
            entity.Property(e => e.StatusId)
                .HasDefaultValue(1)
                .HasColumnName("StatusID");
            entity.Property(e => e.SurveyResponseId).HasColumnName("SurveyResponseID");
            entity.Property(e => e.SurveyResponseNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("SurveyResponseNM");
            entity.Property(e => e.UserNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("UserNM");

            entity.HasOne(d => d.SurveyResponse).WithMany(p => p.SurveyResponseHistories)
                .HasForeignKey(d => d.SurveyResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyResponseSequence>(entity =>
        {
            entity.ToTable("SurveyResponseSequence");

            entity.HasIndex(e => new { e.SurveyResponseId, e.SequenceNumber }, "SurveyResponseSequence_UK_SurveyResponseSequence")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.SurveyResponseSequenceId).HasColumnName("SurveyResponseSequenceID");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.SequenceNumber).HasDefaultValue(1);
            entity.Property(e => e.SequenceText)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)");
            entity.Property(e => e.SurveyResponseId).HasColumnName("SurveyResponseID");

            entity.HasOne(d => d.SurveyResponse).WithMany(p => p.SurveyResponseSequences)
                .HasForeignKey(d => d.SurveyResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyResponseState>(entity =>
        {
            entity.ToTable("SurveyResponseState");

            entity.Property(e => e.SurveyResponseStateId).HasColumnName("SurveyResponseStateID");
            entity.Property(e => e.Active).HasColumnType("bit");
            entity.Property(e => e.AssignedUserId).HasColumnName("AssignedUserID");
            entity.Property(e => e.EmailBody)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.EmailSent).HasColumnType("bit");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId).HasColumnName("ModifiedID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.SurveyResponseId).HasColumnName("SurveyResponseID");

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.SurveyResponseStates)
                .HasForeignKey(d => d.AssignedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Status).WithMany(p => p.SurveyResponseStates)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SurveyResponse).WithMany(p => p.SurveyResponseStates)
                .HasForeignKey(d => d.SurveyResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyReviewStatus>(entity =>
        {
            entity.ToTable("SurveyReviewStatus");

            entity.HasIndex(e => new { e.SurveyId, e.ReviewStatusId }, "SurveyReviewStatus_UK_SurveyReviewStatus_SurveyStatus")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.SurveyReviewStatusId).HasColumnName("SurveyReviewStatusID");
            entity.Property(e => e.ApprovedFl)
                .HasColumnType("bit")
                .HasColumnName("ApprovedFL");
            entity.Property(e => e.CommentFl)
                .HasColumnType("bit")
                .HasColumnName("CommentFL");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.ReviewStatusDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("ReviewStatusDS");
            entity.Property(e => e.ReviewStatusId).HasColumnName("ReviewStatusID");
            entity.Property(e => e.ReviewStatusNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("ReviewStatusNM");
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyReviewStatuses)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SurveyStatus>(entity =>
        {
            entity.ToTable("SurveyStatus");

            entity.HasIndex(e => new { e.SurveyId, e.StatusId }, "SurveyStatus_UK_SurveyStatus_SurveyStatus")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.SurveyStatusId).HasColumnName("SurveyStatusID");
            entity.Property(e => e.EmailSubjectTemplate)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.EmailTemplate)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId).HasColumnName("ModifiedID");
            entity.Property(e => e.NextStatusId).HasColumnName("NextStatusID");
            entity.Property(e => e.PreviousStatusId).HasColumnName("PreviousStatusID");
            entity.Property(e => e.StatusDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("StatusDS");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.StatusNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("StatusNM");
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyStatuses)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });


        modelBuilder.Entity<TblFile>(entity =>
        {
            entity.ToTable("tblFiles");

            entity.HasIndex(e => new { e.Name, e.ContentType }, "tblFiles_UK_tblFiles_Type_Name")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentType)
                .UseCollation("NOCASE")
                .HasColumnType("varchar(50)");
            entity.Property(e => e.Name)
                .UseCollation("NOCASE")
                .HasColumnType("varchar(50)");
        });

        modelBuilder.Entity<UserAppProperty>(entity =>
        {
            entity.ToTable("UserAppProperty");

            entity.HasIndex(e => new { e.AppId, e.UserId }, "UserAppProperty_UK_UserAppProperty")
                .IsUnique()
                .IsDescending();

            entity.Property(e => e.AppId).HasColumnName("AppID");
            entity.Property(e => e.Key)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Value)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");

            entity.HasOne(d => d.App).WithMany(p => p.UserAppProperties)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.UserAppProperties)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserMessage>(entity =>
        {
            entity.Property(e => e.AppId).HasColumnName("AppID");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.Deleted).HasColumnType("bit");
            entity.Property(e => e.FromApp).HasColumnType("bit");
            entity.Property(e => e.FromUserId).HasColumnName("FromUserID");
            entity.Property(e => e.Message)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar");
            entity.Property(e => e.Opened).HasColumnType("bit");
            entity.Property(e => e.Subject)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)");
            entity.Property(e => e.ToUserId).HasColumnName("ToUserID");

            entity.HasOne(d => d.FromUser).WithMany(p => p.UserMessageFromUsers).HasForeignKey(d => d.FromUserId);

            entity.HasOne(d => d.ToUser).WithMany(p => p.UserMessageToUsers).HasForeignKey(d => d.ToUserId);
        });

        modelBuilder.Entity<WebPortal>(entity =>
        {
            entity.ToTable("WebPortal");

            entity.Property(e => e.WebPortalId).HasColumnName("WebPortalID");
            entity.Property(e => e.ActiveFl)
                .HasDefaultValue(true)
                .HasColumnType("bit")
                .HasColumnName("ActiveFL");
            entity.Property(e => e.ModifiedDt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");
            entity.Property(e => e.ModifiedId)
                .HasDefaultValue(1)
                .HasColumnName("ModifiedID");
            entity.Property(e => e.WebPortalDs)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar")
                .HasColumnName("WebPortalDS");
            entity.Property(e => e.WebPortalNm)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("WebPortalNM");
            entity.Property(e => e.WebPortalUrl)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(250)")
                .HasColumnName("WebPortalURL");
            entity.Property(e => e.WebServiceUrl)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(250)")
                .HasColumnName("WebServiceURL");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
