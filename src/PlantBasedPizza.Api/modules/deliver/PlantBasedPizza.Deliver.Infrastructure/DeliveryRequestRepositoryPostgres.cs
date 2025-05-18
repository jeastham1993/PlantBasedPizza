using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public class DeliveryRequestRepositoryPostgres : IDeliveryRequestRepository
    {
        private readonly DeliveryDbContext _context;

        public DeliveryRequestRepositoryPostgres(DeliveryDbContext context)
        {
            _context = context;
        }
        
        public async Task AddNewDeliveryRequest(DeliveryRequest request)
        {
            await _context.DeliveryRequests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeliveryRequest(DeliveryRequest request)
        {
            _context.DeliveryRequests.Update(request);
            var rowsAffected = await _context.SaveChangesAsync();
            
            // Adding telemetry
            Activity.Current?.AddTag("postgres.rowsAffected", rowsAffected);
        }

        public async Task<DeliveryRequest?> GetDeliveryStatusForOrder(string orderIdentifier)
        {
            return await _context.DeliveryRequests
                .Include(d => d.DeliveryAddress)
                .FirstOrDefaultAsync(d => d.OrderIdentifier == orderIdentifier);
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            var requests = await _context.DeliveryRequests
                .Include(d => d.DeliveryAddress)
                .Where(d => d.DriverCollectedOn == null)
                .ToListAsync();

            Activity.Current?.AddTag("postgres.findCount", requests.Count);

            return requests;
        }

        public async Task<List<DeliveryRequest>> GetOrdersWithDriver(string driverName)
        {
            var requests = await _context.DeliveryRequests
                .Include(d => d.DeliveryAddress)
                .Where(d => d.DeliveredOn == null && 
                           d.DriverCollectedOn != null && 
                           d.Driver == driverName)
                .ToListAsync();
            
            Activity.Current?.AddTag("postgres.findCount", requests.Count);

            return requests;
        }
    }
} 