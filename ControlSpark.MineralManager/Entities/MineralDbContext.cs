using Microsoft.EntityFrameworkCore;

namespace ControlSpark.MineralManager.Entities;

public partial class MineralDbContext : DbContext
{
    public MineralDbContext()
    {
    }

    public MineralDbContext(DbContextOptions<MineralDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Collection> Collections { get; set; } = null!;
    public virtual DbSet<CollectionItem> CollectionItems { get; set; } = null!;
    public virtual DbSet<CollectionItemImage> CollectionItemImages { get; set; } = null!;
    public virtual DbSet<CollectionItemMineral> CollectionItemMinerals { get; set; } = null!;
    public virtual DbSet<Company> Companies { get; set; } = null!;
    public virtual DbSet<LocationCity> LocationCities { get; set; } = null!;
    public virtual DbSet<LocationCountry> LocationCountries { get; set; } = null!;
    public virtual DbSet<LocationState> LocationStates { get; set; } = null!;
    public virtual DbSet<Mineral> Minerals { get; set; } = null!;
    public virtual DbSet<VwCollectionItem> VwCollectionItems { get; set; } = null!;
    public virtual DbSet<VwCollectionItemImage> VwCollectionItemImages { get; set; } = null!;
    public virtual DbSet<VwMineralCollectionItem> VwMineralCollectionItems { get; set; } = null!;
    public virtual DbSet<VwSpecimenImage> VwSpecimenImages { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("{Need to get from Secrets}");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collection>(entity =>
        {
            entity.ToTable("Collection");

            entity.Property(e => e.CollectionId).HasColumnName("CollectionID");

            entity.Property(e => e.CollectionDs).HasColumnName("CollectionDS");

            entity.Property(e => e.CollectionNm)
                .HasMaxLength(255)
                .HasColumnName("CollectionNM");

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");
        });

        modelBuilder.Entity<CollectionItem>(entity =>
        {
            entity.ToTable("CollectionItem");

            entity.HasIndex(e => new { e.CollectionId, e.SpecimenNumber }, "UK_CollectionItem_SpecimenKey")
                .IsUnique();

            entity.Property(e => e.CollectionItemId).HasColumnName("CollectionItemID");

            entity.Property(e => e.CollectionId).HasColumnName("CollectionID");

            entity.Property(e => e.ExCollection).HasMaxLength(255);

            entity.Property(e => e.LocationCityId).HasColumnName("LocationCityID");

            entity.Property(e => e.LocationCountryId).HasColumnName("LocationCountryID");

            entity.Property(e => e.LocationStateId).HasColumnName("LocationStateID");

            entity.Property(e => e.MineNm)
                .HasMaxLength(255)
                .HasColumnName("MineNM");

            entity.Property(e => e.MineralVariety).HasMaxLength(255);

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");

            entity.Property(e => e.Nickname).HasMaxLength(255);

            entity.Property(e => e.PrimaryMineralId).HasColumnName("PrimaryMineralID");

            entity.Property(e => e.PurchaseDate).HasColumnType("datetime");

            entity.Property(e => e.PurchasePrice).HasColumnType("money");

            entity.Property(e => e.PurchasedFromCompanyId).HasColumnName("PurchasedFromCompanyID");

            entity.Property(e => e.SaleDt)
                .HasColumnType("datetime")
                .HasColumnName("SaleDT");

            entity.Property(e => e.SalePrice).HasColumnType("money");

            entity.Property(e => e.ShowWherePurchased).HasMaxLength(255);

            entity.Property(e => e.StorageLocation).HasMaxLength(255);

            entity.Property(e => e.Value).HasColumnType("money");

            entity.Property(e => e.WeightGr).HasMaxLength(255);

            entity.Property(e => e.WeightKg).HasMaxLength(255);

            entity.HasOne(d => d.Collection)
                .WithMany(p => p.CollectionItems)
                .HasForeignKey(d => d.CollectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollectionItem_Collection");

            entity.HasOne(d => d.LocationCity)
                .WithMany(p => p.CollectionItems)
                .HasForeignKey(d => d.LocationCityId)
                .HasConstraintName("FK_CollectionItem_LocationCity");

            entity.HasOne(d => d.LocationCountry)
                .WithMany(p => p.CollectionItems)
                .HasForeignKey(d => d.LocationCountryId)
                .HasConstraintName("FK_CollectionItem_LocationCountry");

            entity.HasOne(d => d.LocationState)
                .WithMany(p => p.CollectionItems)
                .HasForeignKey(d => d.LocationStateId)
                .HasConstraintName("FK_CollectionItem_LocationState");

            entity.HasOne(d => d.PrimaryMineral)
                .WithMany(p => p.CollectionItems)
                .HasForeignKey(d => d.PrimaryMineralId)
                .HasConstraintName("FK_CollectionItem_Mineral");

            entity.HasOne(d => d.PurchasedFromCompany)
                .WithMany(p => p.CollectionItems)
                .HasForeignKey(d => d.PurchasedFromCompanyId)
                .HasConstraintName("FK_CollectionItem_Company");
        });

        modelBuilder.Entity<CollectionItemImage>(entity =>
        {
            entity.ToTable("CollectionItemImage");

            entity.HasIndex(e => e.ImageFileNm, "UK_CollectionItemImage")
                .IsUnique();

            entity.HasIndex(e => new { e.CollectionItemId, e.DisplayOrder }, "UK_CollectionItemImageOrder")
                .IsUnique();

            entity.Property(e => e.CollectionItemImageId).HasColumnName("CollectionItemImageID");

            entity.Property(e => e.CollectionItemId).HasColumnName("CollectionItemID");

            entity.Property(e => e.DisplayOrder).HasDefaultValueSql("((1))");

            entity.Property(e => e.ImageDs).HasColumnName("ImageDS");

            entity.Property(e => e.ImageFileNm)
                .HasMaxLength(255)
                .HasColumnName("ImageFileNM");

            entity.Property(e => e.ImageNm)
                .HasMaxLength(255)
                .HasColumnName("ImageNM");

            entity.Property(e => e.ImageType).HasMaxLength(255);

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");

            entity.HasOne(d => d.CollectionItem)
                .WithMany(p => p.CollectionItemImages)
                .HasForeignKey(d => d.CollectionItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollectionItemImage_CollectionItem");
        });

        modelBuilder.Entity<CollectionItemMineral>(entity =>
        {
            entity.ToTable("CollectionItemMineral");

            entity.Property(e => e.CollectionItemMineralId).HasColumnName("CollectionItemMineralID");

            entity.Property(e => e.CollectionItemId).HasColumnName("CollectionItemID");

            entity.Property(e => e.MineralId).HasColumnName("MineralID");

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");

            entity.HasOne(d => d.CollectionItem)
                .WithMany(p => p.CollectionItemMinerals)
                .HasForeignKey(d => d.CollectionItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollectionItemMineral_CollectionItem");

            entity.HasOne(d => d.Mineral)
                .WithMany(p => p.CollectionItemMinerals)
                .HasForeignKey(d => d.MineralId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollectionItemMineral_Mineral");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company");

            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

            entity.Property(e => e.CompanyDs).HasColumnName("CompanyDS");

            entity.Property(e => e.CompanyNm)
                .HasMaxLength(255)
                .HasColumnName("CompanyNM");

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");
        });

        modelBuilder.Entity<LocationCity>(entity =>
        {
            entity.ToTable("LocationCity");

            entity.HasIndex(e => new { e.City, e.LocationStateId, e.LocationCountryId }, "UK_LocationCity_StateCountry")
                .IsUnique();

            entity.Property(e => e.LocationCityId).HasColumnName("LocationCityID");

            entity.Property(e => e.City).HasMaxLength(255);

            entity.Property(e => e.CityDs).HasColumnName("CityDS");

            entity.Property(e => e.County).HasMaxLength(255);

            entity.Property(e => e.Latitude)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.Property(e => e.LocationCountryId).HasColumnName("LocationCountryID");

            entity.Property(e => e.LocationStateId).HasColumnName("LocationStateID");

            entity.Property(e => e.Longitude)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");

            entity.HasOne(d => d.LocationCountry)
                .WithMany(p => p.LocationCities)
                .HasForeignKey(d => d.LocationCountryId)
                .HasConstraintName("FK_LocationCity_LocationCountry");

            entity.HasOne(d => d.LocationState)
                .WithMany(p => p.LocationCities)
                .HasForeignKey(d => d.LocationStateId)
                .HasConstraintName("FK_LocationCity_LocationState");
        });

        modelBuilder.Entity<LocationCountry>(entity =>
        {
            entity.ToTable("LocationCountry");

            entity.HasIndex(e => e.CountryNm, "UK_LocationCountry")
                .IsUnique();

            entity.Property(e => e.LocationCountryId).HasColumnName("LocationCountryID");

            entity.Property(e => e.CountryDs).HasColumnName("CountryDS");

            entity.Property(e => e.CountryNm)
                .HasMaxLength(255)
                .HasColumnName("CountryNM");

            entity.Property(e => e.Latitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.Longitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");
        });

        modelBuilder.Entity<LocationState>(entity =>
        {
            entity.ToTable("LocationState");

            entity.HasIndex(e => new { e.LocationCountryId, e.StateNm }, "UK_LocationState_Country")
                .IsUnique();

            entity.Property(e => e.LocationStateId).HasColumnName("LocationStateID");

            entity.Property(e => e.LocationCountryId).HasColumnName("LocationCountryID");

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");

            entity.Property(e => e.StateCd)
                .HasMaxLength(50)
                .HasColumnName("StateCD");

            entity.Property(e => e.StateDs).HasColumnName("StateDS");

            entity.Property(e => e.StateNm)
                .HasMaxLength(255)
                .HasColumnName("StateNM");

            entity.HasOne(d => d.LocationCountry)
                .WithMany(p => p.LocationStates)
                .HasForeignKey(d => d.LocationCountryId)
                .HasConstraintName("FK_LocationState_LocationCountry");
        });

        modelBuilder.Entity<Mineral>(entity =>
        {
            entity.ToTable("Mineral");

            entity.HasIndex(e => e.MineralNm, "UK_MineralName")
                .IsUnique();

            entity.Property(e => e.MineralId).HasColumnName("MineralID");

            entity.Property(e => e.MineralDs).HasColumnName("MineralDS");

            entity.Property(e => e.MineralNm)
                .HasMaxLength(255)
                .HasColumnName("MineralNM");

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedId)
                .HasColumnName("ModifiedID")
                .HasDefaultValueSql("((999))");

            entity.Property(e => e.WikipediaUrl)
                .HasMaxLength(255)
                .HasColumnName("WikipediaURL");
        });

        modelBuilder.Entity<VwCollectionItem>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("vwCollectionItems");

            entity.Property(e => e.City).HasMaxLength(255);

            entity.Property(e => e.CityDs).HasColumnName("CityDS");

            entity.Property(e => e.CollectionId).HasColumnName("CollectionID");

            entity.Property(e => e.CollectionItemId).HasColumnName("CollectionItemID");

            entity.Property(e => e.CollectionItemImageId).HasColumnName("CollectionItemImageID");

            entity.Property(e => e.CollectionNm)
                .HasMaxLength(255)
                .HasColumnName("CollectionNM");

            entity.Property(e => e.CompanyDs).HasColumnName("CompanyDS");

            entity.Property(e => e.CompanyNm)
                .HasMaxLength(255)
                .HasColumnName("CompanyNM");

            entity.Property(e => e.CountryDs).HasColumnName("CountryDS");

            entity.Property(e => e.CountryNm)
                .HasMaxLength(255)
                .HasColumnName("CountryNM");

            entity.Property(e => e.ExCollection).HasMaxLength(255);

            entity.Property(e => e.ImageDs).HasColumnName("ImageDS");

            entity.Property(e => e.ImageFileNm)
                .HasMaxLength(255)
                .HasColumnName("ImageFileNM");

            entity.Property(e => e.ImageNm)
                .HasMaxLength(255)
                .HasColumnName("ImageNM");

            entity.Property(e => e.ImageType).HasMaxLength(255);

            entity.Property(e => e.Latitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.LocationCityId).HasColumnName("LocationCityID");

            entity.Property(e => e.LocationCountryId).HasColumnName("LocationCountryID");

            entity.Property(e => e.LocationStateId).HasColumnName("LocationStateID");

            entity.Property(e => e.Longitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.MineNm)
                .HasMaxLength(255)
                .HasColumnName("MineNM");

            entity.Property(e => e.MineralDs).HasColumnName("MineralDS");

            entity.Property(e => e.MineralId).HasColumnName("MineralID");

            entity.Property(e => e.MineralNm)
                .HasMaxLength(255)
                .HasColumnName("MineralNM");

            entity.Property(e => e.MineralVariety).HasMaxLength(255);

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");

            entity.Property(e => e.ModifiedId).HasColumnName("ModifiedID");

            entity.Property(e => e.Nickname).HasMaxLength(255);

            entity.Property(e => e.PrimaryMineralDs).HasColumnName("PrimaryMineralDS");

            entity.Property(e => e.PrimaryMineralId).HasColumnName("PrimaryMineralID");

            entity.Property(e => e.PrimaryMineralNm)
                .HasMaxLength(255)
                .HasColumnName("PrimaryMineralNM");

            entity.Property(e => e.PurchaseDate).HasColumnType("datetime");

            entity.Property(e => e.PurchasePrice).HasColumnType("money");

            entity.Property(e => e.PurchasedFromCompanyId).HasColumnName("PurchasedFromCompanyID");

            entity.Property(e => e.SaleDt)
                .HasColumnType("datetime")
                .HasColumnName("SaleDT");

            entity.Property(e => e.SalePrice).HasColumnType("money");

            entity.Property(e => e.ShowWherePurchased).HasMaxLength(255);

            entity.Property(e => e.StateCd)
                .HasMaxLength(50)
                .HasColumnName("StateCD");

            entity.Property(e => e.StateDs).HasColumnName("StateDS");

            entity.Property(e => e.StateNm)
                .HasMaxLength(255)
                .HasColumnName("StateNM");

            entity.Property(e => e.StorageLocation).HasMaxLength(255);

            entity.Property(e => e.ThumbImageFileNm)
                .HasMaxLength(4000)
                .HasColumnName("ThumbImageFileNM");

            entity.Property(e => e.Value).HasColumnType("money");

            entity.Property(e => e.WeightGr).HasMaxLength(255);

            entity.Property(e => e.WeightKg).HasMaxLength(255);
        });

        modelBuilder.Entity<VwCollectionItemImage>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("vwCollectionItemImages");

            entity.Property(e => e.City).HasMaxLength(255);

            entity.Property(e => e.CityDs).HasColumnName("CityDS");

            entity.Property(e => e.CollectionId).HasColumnName("CollectionID");

            entity.Property(e => e.CollectionItemId).HasColumnName("CollectionItemID");

            entity.Property(e => e.CollectionItemImageId).HasColumnName("CollectionItemImageID");

            entity.Property(e => e.CollectionNm)
                .HasMaxLength(255)
                .HasColumnName("CollectionNM");

            entity.Property(e => e.CompanyDs).HasColumnName("CompanyDS");

            entity.Property(e => e.CompanyNm)
                .HasMaxLength(255)
                .HasColumnName("CompanyNM");

            entity.Property(e => e.CountryDs).HasColumnName("CountryDS");

            entity.Property(e => e.CountryNm)
                .HasMaxLength(255)
                .HasColumnName("CountryNM");

            entity.Property(e => e.ExCollection).HasMaxLength(255);

            entity.Property(e => e.ImageDs).HasColumnName("ImageDS");

            entity.Property(e => e.ImageFileNm)
                .HasMaxLength(255)
                .HasColumnName("ImageFileNM");

            entity.Property(e => e.ImageNm)
                .HasMaxLength(255)
                .HasColumnName("ImageNM");

            entity.Property(e => e.ImageType).HasMaxLength(255);

            entity.Property(e => e.Latitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.LocationCityId).HasColumnName("LocationCityID");

            entity.Property(e => e.LocationCountryId).HasColumnName("LocationCountryID");

            entity.Property(e => e.LocationStateId).HasColumnName("LocationStateID");

            entity.Property(e => e.Longitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.MineNm)
                .HasMaxLength(255)
                .HasColumnName("MineNM");

            entity.Property(e => e.MineralDs).HasColumnName("MineralDS");

            entity.Property(e => e.MineralNm)
                .HasMaxLength(255)
                .HasColumnName("MineralNM");

            entity.Property(e => e.MineralVariety).HasMaxLength(255);

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");

            entity.Property(e => e.ModifiedId).HasColumnName("ModifiedID");

            entity.Property(e => e.Nickname).HasMaxLength(255);

            entity.Property(e => e.PrimaryMineralId).HasColumnName("PrimaryMineralID");

            entity.Property(e => e.PurchaseDate).HasColumnType("datetime");

            entity.Property(e => e.PurchasePrice).HasColumnType("money");

            entity.Property(e => e.PurchasedFromCompanyId).HasColumnName("PurchasedFromCompanyID");

            entity.Property(e => e.SaleDt)
                .HasColumnType("datetime")
                .HasColumnName("SaleDT");

            entity.Property(e => e.SalePrice).HasColumnType("money");

            entity.Property(e => e.ShowWherePurchased).HasMaxLength(255);

            entity.Property(e => e.StateCd)
                .HasMaxLength(50)
                .HasColumnName("StateCD");

            entity.Property(e => e.StateDs).HasColumnName("StateDS");

            entity.Property(e => e.StateNm)
                .HasMaxLength(255)
                .HasColumnName("StateNM");

            entity.Property(e => e.StorageLocation).HasMaxLength(255);

            entity.Property(e => e.Value).HasColumnType("money");

            entity.Property(e => e.WeightGr).HasMaxLength(255);

            entity.Property(e => e.WeightKg).HasMaxLength(255);
        });

        modelBuilder.Entity<VwMineralCollectionItem>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("vwMineralCollectionItems");

            entity.Property(e => e.City).HasMaxLength(255);

            entity.Property(e => e.CityDs).HasColumnName("CityDS");

            entity.Property(e => e.CollectionId).HasColumnName("CollectionID");

            entity.Property(e => e.CollectionItemId).HasColumnName("CollectionItemID");

            entity.Property(e => e.CollectionItemImageId).HasColumnName("CollectionItemImageID");

            entity.Property(e => e.CollectionNm)
                .HasMaxLength(255)
                .HasColumnName("CollectionNM");

            entity.Property(e => e.CompanyDs).HasColumnName("CompanyDS");

            entity.Property(e => e.CompanyNm)
                .HasMaxLength(255)
                .HasColumnName("CompanyNM");

            entity.Property(e => e.CountryDs).HasColumnName("CountryDS");

            entity.Property(e => e.CountryNm)
                .HasMaxLength(255)
                .HasColumnName("CountryNM");

            entity.Property(e => e.ExCollection).HasMaxLength(255);

            entity.Property(e => e.ImageDs).HasColumnName("ImageDS");

            entity.Property(e => e.ImageFileNm)
                .HasMaxLength(255)
                .HasColumnName("ImageFileNM");

            entity.Property(e => e.ImageNm)
                .HasMaxLength(255)
                .HasColumnName("ImageNM");

            entity.Property(e => e.ImageType).HasMaxLength(255);

            entity.Property(e => e.Latitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.LocationCityId).HasColumnName("LocationCityID");

            entity.Property(e => e.LocationCountryId).HasColumnName("LocationCountryID");

            entity.Property(e => e.LocationStateId).HasColumnName("LocationStateID");

            entity.Property(e => e.Longitude)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.MineNm)
                .HasMaxLength(255)
                .HasColumnName("MineNM");

            entity.Property(e => e.MineralDs).HasColumnName("MineralDS");

            entity.Property(e => e.MineralId).HasColumnName("MineralID");

            entity.Property(e => e.MineralNm)
                .HasMaxLength(255)
                .HasColumnName("MineralNM");

            entity.Property(e => e.MineralVariety).HasMaxLength(255);

            entity.Property(e => e.Nickname).HasMaxLength(255);

            entity.Property(e => e.PrimaryMineralDs).HasColumnName("PrimaryMineralDS");

            entity.Property(e => e.PrimaryMineralId).HasColumnName("PrimaryMineralID");

            entity.Property(e => e.PrimaryMineralNm)
                .HasMaxLength(255)
                .HasColumnName("PrimaryMineralNM");

            entity.Property(e => e.PurchaseDate).HasColumnType("datetime");

            entity.Property(e => e.PurchasePrice).HasColumnType("money");

            entity.Property(e => e.PurchasedFromCompanyId).HasColumnName("PurchasedFromCompanyID");

            entity.Property(e => e.SaleDt)
                .HasColumnType("datetime")
                .HasColumnName("SaleDT");

            entity.Property(e => e.SalePrice).HasColumnType("money");

            entity.Property(e => e.ShowWherePurchased).HasMaxLength(255);

            entity.Property(e => e.StateCd)
                .HasMaxLength(50)
                .HasColumnName("StateCD");

            entity.Property(e => e.StateDs).HasColumnName("StateDS");

            entity.Property(e => e.StateNm)
                .HasMaxLength(255)
                .HasColumnName("StateNM");

            entity.Property(e => e.StorageLocation).HasMaxLength(255);

            entity.Property(e => e.Value).HasColumnType("money");

            entity.Property(e => e.WeightGr).HasMaxLength(255);

            entity.Property(e => e.WeightKg).HasMaxLength(255);
        });

        modelBuilder.Entity<VwSpecimenImage>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("vwSpecimenImage");

            entity.Property(e => e.CollectionItemId).HasColumnName("CollectionItemID");

            entity.Property(e => e.CollectionItemImageId)
                .ValueGeneratedOnAdd()
                .HasColumnName("CollectionItemImageID");

            entity.Property(e => e.ImageDs).HasColumnName("ImageDS");

            entity.Property(e => e.ImageFileNm)
                .HasMaxLength(255)
                .HasColumnName("ImageFileNM");

            entity.Property(e => e.ImageNm)
                .HasMaxLength(255)
                .HasColumnName("ImageNM");

            entity.Property(e => e.ImageType).HasMaxLength(255);

            entity.Property(e => e.ModifiedDt)
                .HasColumnType("datetime")
                .HasColumnName("ModifiedDT");

            entity.Property(e => e.ModifiedId).HasColumnName("ModifiedID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
