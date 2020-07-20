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
    public class PlayersController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TeamManagerContext _context;

        public PlayersController(TeamManagerContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Players
        public async Task<IActionResult> Index()
        {
            return View(await _context.Players.ToListAsync());
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(s => s.TeamAssignments)
                    .ThenInclude(e => e.Team)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PlayerId == id);

            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // GET: Players/Create
        public IActionResult Create()
        {
            var player = new Player { TeamAssignments = new List<TeamAssignment>() };

            ViewData["Teams"] = PopulateAssignedTeamData(player);
            return View();
        }

        // POST: Players/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Player player, string[] selectedTeams)
        {
            if (selectedTeams != null)
            {
                player.TeamAssignments = new List<TeamAssignment>();
                foreach (var team in selectedTeams)
                {
                    var teamToAdd = new TeamAssignment { PlayerId = player.PlayerId, TeamId = int.Parse(team) };
                    player.TeamAssignments.Add(teamToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(player);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Teams"] = PopulateAssignedTeamData(player);
            return View(player);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var player = await _context.Players
                .Include(t => t.TeamAssignments).
                    ThenInclude(t => t.Team)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.PlayerId == id);

            if (player == null)
            {
                return NotFound();
            }
            ViewData["Teams"] = PopulateAssignedTeamData(player);
            return View(player);
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, string[] selectedTeams)
        {
            if (id == null)
            {
                return NotFound();
            }
            var playerToUpdate = await _context.Players
                .Include(t => t.TeamAssignments)
                    .ThenInclude(t => t.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (await TryUpdateModelAsync<Player>(playerToUpdate, "", s => s.Name, s => s.Description))
            {
                UpdatePlayerTeams(selectedTeams, playerToUpdate);
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
            UpdatePlayerTeams(selectedTeams, playerToUpdate);
            ViewData["Teams"] = PopulateAssignedTeamData(playerToUpdate);
            return View(playerToUpdate);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PlayerId == id);
            if (player == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Delete failed. Try again later.";
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players
                .Include(t => t.TeamAssignments)
                .SingleAsync(t => t.PlayerId == id);

            if (player == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Unable to delete player with id {id}, {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id, saveChangesError = true });
            }
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.PlayerId == id);
        }

        private List<AssignedTeamData> PopulateAssignedTeamData(Player player)
        {
            var playerTeams = new HashSet<int>(player.TeamAssignments.Select(t => t.TeamId));
            var viewModel = new List<AssignedTeamData>();
            foreach (var team in _context.Teams)
            {
                viewModel.Add(new AssignedTeamData
                {
                    Id = team.TeamId,
                    Title = team.Name,
                    IsAssigned = playerTeams.Contains(team.TeamId)
                });
            }
            return viewModel;
        }

        private void UpdatePlayerTeams(string[] selectedTeams, Player player)
        {
            if (selectedTeams == null)
            {
                player.TeamAssignments = new List<TeamAssignment>();
                return;
            }

            var selectedTeamsHS = new HashSet<string>(selectedTeams);
            var playerTeams = new HashSet<int>(player.TeamAssignments.Select(t => t.Team.TeamId));
            foreach (var team in _context.Teams)
            {
                if (selectedTeamsHS.Contains(team.TeamId.ToString()))
                {
                    if (!playerTeams.Contains(team.TeamId))
                    {
                        player.TeamAssignments.Add(new TeamAssignment { PlayerId = player.PlayerId, TeamId = team.TeamId });
                    }
                }
                else
                {
                    if (playerTeams.Contains(team.TeamId))
                    {
                        TeamAssignment teamToRemove = player.TeamAssignments.FirstOrDefault(t => t.TeamId == team.TeamId);
                        _context.Remove(teamToRemove);
                    }
                }
            }
        }


    }
}
