// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderManagerDbContext : DbContext
{
    public OrderManagerDbContext(DbContextOptions<OrderManagerDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OutboxItem> OutboxItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasKey(o => o.OrderIdentifier);
    
        modelBuilder.Entity<Order>()
            .ToTable("orders");
    
        // Configure OrderItems collection relationship
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderIdentifier")
            .OnDelete(DeleteBehavior.Cascade);
    
        // Configure OrderHistory collection relationship
        modelBuilder.Entity<Order>()
            .HasMany(o => o.History)
            .WithOne()
            .HasForeignKey("OrderIdentifier")
            .OnDelete(DeleteBehavior.Cascade);
    
        // Configure DeliveryDetails relationship (if it's a navigation property)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.DeliveryDetails)
            .WithOne()
            .HasForeignKey<DeliveryDetails>("OrderIdentifier");
    
        modelBuilder.Entity<DeliveryDetails>()
            .HasKey(o => o.DeliveryDetailsId);
    
        modelBuilder.Entity<OrderItem>()
            .HasKey(o => o.OrderItemId);
    
        modelBuilder.Entity<OrderHistory>()
            .HasKey(o => o.OrderHistoryId);
        
        modelBuilder.Entity<OutboxItem>()
            .HasKey(o => o.EventType);
        
        modelBuilder.Entity<OutboxItem>()
            .ToTable("orders_outboxitems");
    }
}