using Microsoft.EntityFrameworkCore;
using BlazorBday.Data;
using BlazorBday.Models;

namespace BlazorBday.Repositories;

public class AgencyRepository : IAgencyRepository
{
    private readonly MarketShopDbContext _context;

    public AgencyRepository(MarketShopDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Agency>> GetAllAsync()
    {
        return await _context.Agencies
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Agency?> GetByIdAsync(int id)
    {
        return await _context.Agencies.FindAsync(id);
    }

    public async Task AddAsync(Agency agency)
    {
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Agency agency)
    {
        // Load existing entity to avoid navigation property conflicts
        var existingAgency = await _context.Agencies.FindAsync(agency.Id);
        if (existingAgency == null) return;

        // Update only scalar properties
        _context.Entry(existingAgency).CurrentValues.SetValues(agency);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var agency = await _context.Agencies.FindAsync(id);
        if (agency != null)
        {
            _context.Agencies.Remove(agency);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Agencies.AnyAsync(e => e.Id == id);
    }
}
