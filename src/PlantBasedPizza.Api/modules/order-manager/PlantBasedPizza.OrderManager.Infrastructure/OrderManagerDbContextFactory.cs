// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantBasedPizza.OrderManager.Infrastructure;

internal class OrderManagerDbContextFactory : IDesignTimeDbContextFactory<OrderManagerDbContext>
{
    public OrderManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderManagerDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=PlantBasedPizza_Orders;Username=postgres;Password=yourpassword");

        return new OrderManagerDbContext(optionsBuilder.Options);
    }
}