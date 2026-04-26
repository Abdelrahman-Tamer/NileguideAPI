using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public class CityService : ICityService
    {
        private readonly AppDbContext _context;

        public CityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CityDto>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .AsNoTracking()
                .OrderBy(c => c.CityName)
                .Select(c => new CityDto
                {
                    CityID = c.CityID,
                    CityName = c.CityName ?? string.Empty
                })
                .ToListAsync();
        }
    }
}
