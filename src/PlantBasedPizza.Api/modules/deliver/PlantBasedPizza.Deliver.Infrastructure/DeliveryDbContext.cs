// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Infrastructure;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options)
    {
    }

    public DbSet<DeliveryRequest> DeliveryRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeliveryRequest>()
            .HasKey(d => d.OrderIdentifier);
            
        modelBuilder.Entity<Address>()
            .HasKey(d => d.AddressId);

        modelBuilder.Entity<DeliveryRequest>()
            .ToTable("DeliveryRequests")
            .HasOne<Address>()
            .WithOne()
            .HasForeignKey<DeliveryRequest>(a => a.AddressId) // Specify the entity type and use a lambda
            .OnDelete(DeleteBehavior.Cascade);;
    }
}