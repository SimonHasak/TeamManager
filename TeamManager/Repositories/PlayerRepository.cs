using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Repositories
{
    public class PlayerRepository
    {
        private readonly TeamManagerContext _context;

        public PlayerRepository(TeamManagerContext context)
        {
            _context = context;
        }

        public async Task<List<Player>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }

        public async Task<List<Team>> GetTeams()
        {
            return await _context.Teams.ToListAsync();
        }

        public async Task AddPlayer(Player player)
        {
            _context.Players.Add(player);
            await this.SaveChanges();
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Remove(TeamAssignment team)
        {
            _context.Remove(team);
            await this.SaveChanges();
        }

        public async Task Remove(Player player)
        {
            _context.Players.Remove(player);
            await this.SaveChanges();
        }

        public async Task<Player> GetPlayerWithTeams(int? id)
        {
            return await _context.Players
                .Include(t => t.TeamAssignments)
                    .ThenInclude(t => t.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == id);
        }
    }
}
