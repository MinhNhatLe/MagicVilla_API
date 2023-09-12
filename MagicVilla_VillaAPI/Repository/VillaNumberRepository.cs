using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
    {
        private readonly ApplicationDbContext _db;

        public VillaNumberRepository(ApplicationDbContext db) : base(db) // base ở đây ta có thể path từ trong Repository ra
        {
            _db = db;
        }


        public async Task<VillaNumber> UpdateAsync(VillaNumber entity)
        {
            entity.UpdatedDate = DateTime.Now;
            _db.VillaNumbers.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
