﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using WebSpark.Prompt.Data;

#nullable disable

namespace WebSpark.Prompt.Migrations
{
    [DbContext(typeof(GPTDbContext))]
    partial class GPTDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("WebSpark.Prompt.Data.GPTDefinition", b =>
                {
                    b.Property<int>("DefinitionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("DefinitionType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("GPTName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("OutputType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Prompt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PromptHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Role")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Temperature")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("DefinitionId");

                    b.HasIndex("GPTName")
                        .IsUnique();

                    b.ToTable("Definitions");
                });

            modelBuilder.Entity("WebSpark.Prompt.Data.GPTDefinitionResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CompletionTokens")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<int>("DefinitionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DefinitionType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("ElapsedMilliseconds")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GPTDescription")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("GPTName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("OutputType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PromptTokens")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ResponseId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SystemPrompt")
                        .HasColumnType("TEXT");

                    b.Property<string>("SystemResponse")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Temperature")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TotalTokens")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserExpectedResponse")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserPrompt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DefinitionId");

                    b.HasIndex("ResponseId");

                    b.ToTable("DefinitionResponses");
                });

            modelBuilder.Entity("WebSpark.Prompt.Data.GPTDefinitionType", b =>
                {
                    b.Property<string>("DefinitionType")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("OutputType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("DefinitionType");

                    b.HasIndex("DefinitionType")
                        .IsUnique();

                    b.ToTable("DefinitionTypes");
                });

            modelBuilder.Entity("WebSpark.Prompt.Data.GPTUserPrompt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("DefinitionType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserExpectedResponse")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserPrompt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserPrompt")
                        .IsUnique();

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("WebSpark.Prompt.Data.GPTDefinitionResponse", b =>
                {
                    b.HasOne("WebSpark.Prompt.Data.GPTDefinition", "Definition")
                        .WithMany("GPTResponses")
                        .HasForeignKey("DefinitionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebSpark.Prompt.Data.GPTUserPrompt", "Response")
                        .WithMany("GPTResponses")
                        .HasForeignKey("ResponseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Definition");

                    b.Navigation("Response");
                });

            modelBuilder.Entity("WebSpark.Prompt.Data.GPTDefinition", b =>
                {
                    b.Navigation("GPTResponses");
                });

            modelBuilder.Entity("WebSpark.Prompt.Data.GPTUserPrompt", b =>
                {
                    b.Navigation("GPTResponses");
                });
#pragma warning restore 612, 618
        }
    }
}
