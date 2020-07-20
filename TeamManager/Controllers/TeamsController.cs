using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TeamManagerContext _context;

        public TeamsController(TeamManagerContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            return View(await _context.Teams.ToListAsync());
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(s => s.PlayerAssignments)
                    .ThenInclude(e => e.Player)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TeamId == id);

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            var team = new Team { PlayerAssignments = new List<TeamAssignment>() };

            ViewData["Players"] = PopulateAssignedPlayerData(team);
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Team team, string[] selectedPlayers)
        {
            if (selectedPlayers != null)
            {
                team.PlayerAssignments = new List<TeamAssignment>();
                foreach (var player in selectedPlayers)
                {
                    var playerToAdd = new TeamAssignment { PlayerId = int.Parse(player), TeamId = team.TeamId };
                    team.PlayerAssignments.Add(playerToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Players"] = PopulateAssignedPlayerData(team);
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(p => p.PlayerAssignments).
                    ThenInclude(p => p.Player)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.TeamId == id);

            if (team == null)
            {
                return NotFound();
            }
            ViewData["Players"] = PopulateAssignedPlayerData(team);
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, string[] selectedPlayers)
        {
            if (id == null)
            {
                return NotFound();
            }
            var teamToUpdate = await _context.Teams
                .Include(p => p.PlayerAssignments)
                    .ThenInclude(p => p.Player)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (await TryUpdateModelAsync<Team>(teamToUpdate, "", s => s.Name, s => s.Description))
            {
                UpdateTeamPlayers(selectedPlayers, teamToUpdate);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"Unable to edit player with id {id}, {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists. ");
                }
                return RedirectToAction(nameof(Index));
            }
            UpdateTeamPlayers(selectedPlayers, teamToUpdate);
            ViewData["Players"] = PopulateAssignedPlayerData(teamToUpdate);
            return View(teamToUpdate);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Delete failed. Try again later.";
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams
                .Include(p => p.PlayerAssignments)
                .SingleAsync(p => p.TeamId == id);

            if (team == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Unable to delete player with id {id}, {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id, saveChangesError = true });
            }
        }
        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamId == id);
        }

        private List<AssignedTeamData> PopulateAssignedPlayerData(Team team)
        {
            var teamPlayers = new HashSet<int>(team.PlayerAssignments.Select(p => p.PlayerId));
            var viewModel = new List<AssignedTeamData>();
            foreach (var player in _context.Players)
            {
                viewModel.Add(new AssignedTeamData
                {
                    Id = player.PlayerId,
                    Title = player.Name,
                    IsAssigned = teamPlayers.Contains(player.PlayerId)
                });
            }
            return viewModel;
        }

        private void UpdateTeamPlayers(string[] selectedPlayers, Team team)
        {
            if (selectedPlayers == null)
            {
                team.PlayerAssignments = new List<TeamAssignment>();
                return;
            }

            var selectedPlayersHS = new HashSet<string>(selectedPlayers);
            var teamPlayers = new HashSet<int>(team.PlayerAssignments.Select(p => p.PlayerId));
            foreach (var player in _context.Players)
            {
                if (selectedPlayersHS.Contains(player.PlayerId.ToString()))
                {
                    if (!teamPlayers.Contains(player.PlayerId))
                    {
                        team.PlayerAssignments.Add(new TeamAssignment { PlayerId = player.PlayerId, TeamId = team.TeamId });
                    }
                }
                else
                {
                    if (teamPlayers.Contains(player.PlayerId))
                    {
                        TeamAssignment playerToRemove = team.PlayerAssignments.FirstOrDefault(p => p.PlayerId == player.PlayerId);
                        _context.Remove(playerToRemove);
                    }
                }
            }
        }
    }
}
