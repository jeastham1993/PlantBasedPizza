// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class KitchenDbContextFactory : IDesignTimeDbContextFactory<KitchenDbContext>
{
    public KitchenDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KitchenDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=PlantBasedPizza_Kitchen;Username=postgres;Password=yourpassword");

        return new KitchenDbContext(optionsBuilder.Options);
    }
}