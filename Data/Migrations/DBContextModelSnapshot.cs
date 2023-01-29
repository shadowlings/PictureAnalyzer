﻿// <auto-generated />
using System;
using PictureAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace PictureAnalyzer.Data.Migrations
{
    [DbContext(typeof(DBContext))]
    partial class DBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("PictureAnalyzer.Data.AuthorizedUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("AllowEdit")
                        .HasColumnType("bit");

                    b.Property<string>("FolderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.ToTable("AuthorizedUsers");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.Folder", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.FolderItem", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("BlobId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("FolderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("MimeType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrettyFileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UploadedBy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.ToTable("FolderItems");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.ImageRatings", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("AdultScore")
                        .HasColumnType("float");

                    b.Property<string>("FolderItemId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("GoreScore")
                        .HasColumnType("float");

                    b.Property<bool>("IsAdultContent")
                        .HasColumnType("bit");

                    b.Property<bool>("IsGoryContent")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRacyContent")
                        .HasColumnType("bit");

                    b.Property<double>("RacyScore")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("FolderItemId")
                        .IsUnique()
                        .HasFilter("[FolderItemId] IS NOT NULL");

                    b.ToTable("ImageRatings");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.ImageTag", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("Confidence")
                        .HasColumnType("float");

                    b.Property<string>("FolderItemId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Tag")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("FolderItemId");

                    b.ToTable("ImageTags");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.AuthorizedUser", b =>
                {
                    b.HasOne("PictureAnalyzer.Data.Folder", "Folder")
                        .WithMany("AuthorizedUsers")
                        .HasForeignKey("FolderId");

                    b.Navigation("Folder");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.FolderItem", b =>
                {
                    b.HasOne("PictureAnalyzer.Data.Folder", "Folder")
                        .WithMany("Items")
                        .HasForeignKey("FolderId");

                    b.Navigation("Folder");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.ImageRatings", b =>
                {
                    b.HasOne("PictureAnalyzer.Data.FolderItem", "FolderItem")
                        .WithOne("ImageRatings")
                        .HasForeignKey("PictureAnalyzer.Data.ImageRatings", "FolderItemId");

                    b.Navigation("FolderItem");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.ImageTag", b =>
                {
                    b.HasOne("PictureAnalyzer.Data.FolderItem", "FolderItem")
                        .WithMany("ImageTags")
                        .HasForeignKey("FolderItemId");

                    b.Navigation("FolderItem");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.Folder", b =>
                {
                    b.Navigation("AuthorizedUsers");

                    b.Navigation("Items");
                });

            modelBuilder.Entity("PictureAnalyzer.Data.FolderItem", b =>
                {
                    b.Navigation("ImageRatings");

                    b.Navigation("ImageTags");
                });
#pragma warning restore 612, 618
        }
    }
}
