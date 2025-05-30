﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebSpark.Core.Data;

#nullable disable

namespace WebSpark.Core.Migrations
{
    [DbContext(typeof(WebSparkDbContext))]
    [Migration("20240724173747_FixDuplicateDateFields")]
    partial class FixDuplicateDateFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("WebSpark.Core.Data.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Avatar")
                        .HasMaxLength(400)
                        .HasColumnType("TEXT");

                    b.Property<string>("Bio")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<int?>("BlogId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("DATE('now')");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnalyticsListType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnalyticsPeriod")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Cover")
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(450)
                        .HasColumnType("TEXT");

                    b.Property<string>("FooterScript")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<string>("HeaderScript")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IncludeFeatured")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemsPerPage")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Logo")
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<string>("Theme")
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("DATE('now')");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("DATE('now')");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("WebSpark.Core.Data.MailSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BlogId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FromEmail")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<string>("FromName")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<int>("Port")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ToName")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("DATE('now')");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<string>("UserPassword")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("MailSettings");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Menu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Argument")
                        .HasColumnType("TEXT");

                    b.Property<string>("Controller")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DomainId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Icon")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("KeyWords")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("PageContent")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DomainId");

                    b.HasIndex("ParentId");

                    b.ToTable("Menu");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Newsletter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PostId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Success")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("DATE('now')");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("Newsletters");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BlogId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Cover")
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsFeatured")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PostType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PostViews")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Published")
                        .HasColumnType("TEXT");

                    b.Property<double>("Rating")
                        .HasColumnType("REAL");

                    b.Property<bool>("Selected")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("DATE('now')");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("BlogId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("WebSpark.Core.Data.PostCategory", b =>
                {
                    b.Property<int>("PostId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("PostId", "CategoryId");

                    b.HasIndex("CategoryId");

                    b.ToTable("PostCategories");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Recipe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthorName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<double>("AverageRating")
                        .HasColumnType("REAL");

                    b.Property<int>("CommentCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int?>("DomainId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Ingredients")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Instructions")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsApproved")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Keywords")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastViewDt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("TEXT");

                    b.Property<int>("RatingCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RecipeCategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Servings")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ViewCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DomainId");

                    b.HasIndex("RecipeCategoryId");

                    b.ToTable("Recipe");
                });

            modelBuilder.Entity("WebSpark.Core.Data.RecipeCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasMaxLength(1500)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(70)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("RecipeCategory");
                });

            modelBuilder.Entity("WebSpark.Core.Data.RecipeComment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("TEXT");

                    b.Property<int?>("RecipeId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("RecipeComment");
                });

            modelBuilder.Entity("WebSpark.Core.Data.RecipeImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileDescription")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("ImageData")
                        .HasColumnType("BLOB");

                    b.Property<int?>("RecipeId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("RecipeImage");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Subscriber", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BlogId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Country")
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<string>("Ip")
                        .HasMaxLength(80)
                        .HasColumnType("TEXT");

                    b.Property<string>("Region")
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("DATE('now')");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("Subscribers");
                });

            modelBuilder.Entity("WebSpark.Core.Data.WebSite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("Id");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CreatedID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("DomainUrl")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("GalleryFolder")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Style")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Template")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatedID")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseBreadCrumbUrl")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VersionNo")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DomainUrl")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("Title")
                        .IsUnique();

                    b.ToTable("Domain");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Author", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Blog", null)
                        .WithMany("Authors")
                        .HasForeignKey("BlogId");
                });

            modelBuilder.Entity("WebSpark.Core.Data.MailSetting", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Blog", "Blog")
                        .WithMany()
                        .HasForeignKey("BlogId");

                    b.Navigation("Blog");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Menu", b =>
                {
                    b.HasOne("WebSpark.Core.Data.WebSite", "Domain")
                        .WithMany("Menus")
                        .HasForeignKey("DomainId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_Menu_Domain");

                    b.HasOne("WebSpark.Core.Data.Menu", "Parent")
                        .WithMany("InverseParent")
                        .HasForeignKey("ParentId")
                        .HasConstraintName("FK_Menu_ParentMenu_ParentId");

                    b.Navigation("Domain");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Newsletter", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Post", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Author", null)
                        .WithMany("Posts")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebSpark.Core.Data.Blog", "Blog")
                        .WithMany("Posts")
                        .HasForeignKey("BlogId");

                    b.Navigation("Blog");
                });

            modelBuilder.Entity("WebSpark.Core.Data.PostCategory", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Category", "Category")
                        .WithMany("PostCategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebSpark.Core.Data.Post", "Post")
                        .WithMany("PostCategories")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Post");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Recipe", b =>
                {
                    b.HasOne("WebSpark.Core.Data.WebSite", "Domain")
                        .WithMany()
                        .HasForeignKey("DomainId");

                    b.HasOne("WebSpark.Core.Data.RecipeCategory", "RecipeCategory")
                        .WithMany("Recipe")
                        .HasForeignKey("RecipeCategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_Recipe_RecipeCategory");

                    b.Navigation("Domain");

                    b.Navigation("RecipeCategory");
                });

            modelBuilder.Entity("WebSpark.Core.Data.RecipeComment", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Recipe", "Recipe")
                        .WithMany("RecipeComment")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .HasConstraintName("FK_RecipeComment_Recipe");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("WebSpark.Core.Data.RecipeImage", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Recipe", "Recipe")
                        .WithMany("RecipeImage")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .HasConstraintName("FK_RecipeImage_Recipe");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Subscriber", b =>
                {
                    b.HasOne("WebSpark.Core.Data.Blog", "Blog")
                        .WithMany()
                        .HasForeignKey("BlogId");

                    b.Navigation("Blog");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Author", b =>
                {
                    b.Navigation("Posts");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Blog", b =>
                {
                    b.Navigation("Authors");

                    b.Navigation("Posts");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Category", b =>
                {
                    b.Navigation("PostCategories");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Menu", b =>
                {
                    b.Navigation("InverseParent");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Post", b =>
                {
                    b.Navigation("PostCategories");
                });

            modelBuilder.Entity("WebSpark.Core.Data.Recipe", b =>
                {
                    b.Navigation("RecipeComment");

                    b.Navigation("RecipeImage");
                });

            modelBuilder.Entity("WebSpark.Core.Data.RecipeCategory", b =>
                {
                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("WebSpark.Core.Data.WebSite", b =>
                {
                    b.Navigation("Menus");
                });
#pragma warning restore 612, 618
        }
    }
}
