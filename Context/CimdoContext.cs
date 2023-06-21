using System;
using System.Collections.Generic;
using CimdoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CimdoApi.Context;

public partial class CimdoContext : DbContext
{
    public CimdoContext()
    {
    }

    public CimdoContext(DbContextOptions<CimdoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BooksGener> BooksGeners { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Gener> Geners { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UsersGener> UsersGeners { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=Cimdo;Username=postgres;password=1812");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.IdAuthor).HasName("authors_pkey");

            entity.ToTable("authors");

            entity.Property(e => e.IdAuthor)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_author");
            entity.Property(e => e.Author1)
                .HasMaxLength(250)
                .HasColumnName("author");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.IdBook).HasName("books_pkey");

            entity.ToTable("books");

            entity.Property(e => e.IdBook)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_book");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.Books)
                .HasForeignKey(d => d.Author)
                .HasConstraintName("books_author_fkey");
        });

        modelBuilder.Entity<BooksGener>(entity =>
        {
            entity.HasKey(e => new { e.IdBook, e.IdGener }).HasName("books_geners_pkey");

            entity.ToTable("books_geners");

            entity.Property(e => e.IdBook).HasColumnName("id_book");
            entity.Property(e => e.IdGener).HasColumnName("id_gener");
            entity.Property(e => e.Note)
                .HasColumnType("character varying")
                .HasColumnName("note");

            entity.HasOne(d => d.IdBookNavigation).WithMany(p => p.BooksGeners)
                .HasForeignKey(d => d.IdBook)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("books_geners_id_book_fkey");

            entity.HasOne(d => d.IdGenerNavigation).WithMany(p => p.BooksGeners)
                .HasForeignKey(d => d.IdGener)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("books_geners_id_gener_fkey");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.IdUser, e.IdBook }).HasName("favorite_pkey");

            entity.ToTable("favorite");

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdBook).HasColumnName("id_book");
            entity.Property(e => e.Note)
                .HasColumnType("character varying")
                .HasColumnName("note");

            entity.HasOne(d => d.IdBookNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdBook)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("favorite_id_book_fkey");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("favorite_id_user_fkey");
        });

        modelBuilder.Entity<Gener>(entity =>
        {
            entity.HasKey(e => e.IdGener).HasName("geners_pkey");

            entity.ToTable("geners");

            entity.Property(e => e.IdGener)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_gener");
            entity.Property(e => e.Gener1)
                .HasMaxLength(250)
                .HasColumnName("gener");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.IdUser)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_user");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .HasColumnName("email");
            entity.Property(e => e.Login)
                .HasMaxLength(250)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(250)
                .HasColumnName("password_");
        });

        modelBuilder.Entity<UsersGener>(entity =>
        {
            entity.HasKey(e => new { e.IdUser, e.IdGener }).HasName("users_geners_pkey");

            entity.ToTable("users_geners");

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdGener).HasColumnName("id_gener");
            entity.Property(e => e.Note)
                .HasColumnType("character varying")
                .HasColumnName("note");

            entity.HasOne(d => d.IdGenerNavigation).WithMany(p => p.UsersGeners)
                .HasForeignKey(d => d.IdGener)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_geners_id_gener_fkey");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.UsersGeners)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_geners_id_user_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
