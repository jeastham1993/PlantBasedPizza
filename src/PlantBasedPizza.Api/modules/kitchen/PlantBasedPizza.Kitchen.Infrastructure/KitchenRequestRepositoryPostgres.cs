using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class KitchenRequestRepositoryPostgres : IKitchenRequestRepository
{
    private readonly KitchenDbContext _context;

    public KitchenRequestRepositoryPostgres(KitchenDbContext context)
    {
        _context = context;
    }

    public async Task AddNew(KitchenRequest kitchenRequest)
    {
        await _context.KitchenRequests.AddAsync(kitchenRequest);
        await _context.SaveChangesAsync();
    }

    public async Task Update(KitchenRequest kitchenRequest)
    {
        _context.KitchenRequests.Update(kitchenRequest);
        var rowsAffected = await _context.SaveChangesAsync();
        
        // Adding telemetry (adapting from MongoDB implementation)
        Activity.Current?.AddTag("postgres.rowsAffected", rowsAffected);
    }

    public async Task<KitchenRequest> Retrieve(string orderIdentifier)
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .FirstOrDefaultAsync(k => k.OrderIdentifier == orderIdentifier);
    }

    public async Task<IEnumerable<KitchenRequest>> GetNew()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.NEW)
            .ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetPrep()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.PREPARING)
            .ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetBaking()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.BAKING)
            .ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.QUALITYCHECK)
            .ToListAsync();
    }
} 