﻿// <auto-generated />
using System;
using AIDungeonPrompts.Backup.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AIDungeonPrompts.Backup.Persistence.Migrations
{
    [DbContext(typeof(BackupDbContext))]
    [Migration("20210511165741_Add Scripts")]
    partial class AddScripts
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("AIDungeonPrompts.Backup.Persistence.Entities.BackupPrompt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthorsNote")
                        .HasColumnType("TEXT");

                    b.Property<int>("CorrelationId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateEdited")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Memory")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Nsfw")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PromptContent")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PublishDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Quests")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("ScriptZip")
                        .HasColumnType("BLOB");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.ToTable("Prompts");
                });

            modelBuilder.Entity("AIDungeonPrompts.Backup.Persistence.Entities.BackupWorldInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CorrelationId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateEdited")
                        .HasColumnType("TEXT");

                    b.Property<string>("Entry")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Keys")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PromptId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("PromptId");

                    b.ToTable("WorldInfos");
                });

            modelBuilder.Entity("AIDungeonPrompts.Backup.Persistence.Entities.BackupWorldInfo", b =>
                {
                    b.HasOne("AIDungeonPrompts.Backup.Persistence.Entities.BackupPrompt", "Prompt")
                        .WithMany("WorldInfos")
                        .HasForeignKey("PromptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Prompt");
                });

            modelBuilder.Entity("AIDungeonPrompts.Backup.Persistence.Entities.BackupPrompt", b =>
                {
                    b.Navigation("WorldInfos");
                });
#pragma warning restore 612, 618
        }
    }
}
