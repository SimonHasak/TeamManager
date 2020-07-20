using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamManager.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TeamManager.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new TeamManagerContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<TeamManagerContext>>()))
            {
                // Look for any movies or teams.
                if (context.Players.Any()) {
                    Console.WriteLine("Not Empty database.");
                    return; // DB has been seeded 
                }   

                var players = new Player[]
                {
                    new Player
                    {
                        Name = "Cristiano Ronaldo",
                        Description = "Fotball player.",
                        TeamAssignments = new List<TeamAssignment>()
                    },

                    new Player
                    {
                        Name = "Michael Phelps",
                        Description = "Swimmer with longer wingspan than his height.",
                        TeamAssignments = new List<TeamAssignment>()
                    },

                    new Player
                    {
                        Name = "Michael Jordan",
                        Description = "Basketball player.",
                        TeamAssignments = new List<TeamAssignment>()
                    },

                    new Player
                    {
                        Name = "Conor McGregor",
                        Description = "An Irish retired professional mixed martial artist and boxer.",
                        TeamAssignments = new List<TeamAssignment>()
                    }
                };

                context.Players.AddRange(players);
                context.SaveChanges();

                var teams = new Team[]
                {
                    new Team
                    {
                        Name = "American Patriots",
                        Description = "Perfect for Americans who love their country."
                    },

                    new Team
                    {
                        Name = "Challengers",
                        Description = "You team always knows how to bring a challenge to the game."
                    },

                    new Team
                    {
                        Name = "Money Makers",
                        Description = "And damn good at it!"
                    },

                    new Team
                    {
                        Name = "The Producers",
                        Description = "A team that makes visions a reality."
                    }
                };

                context.Teams.AddRange(teams);
                context.SaveChanges();

                var teamAssignment = new TeamAssignment[]
                {
                    new TeamAssignment
                    {
                        PlayerId = players.Single(p => p.Name == players[0].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[0].Name).TeamId
                    },
                    new TeamAssignment
                    {
                        PlayerId = players.Single(s => s.Name == players[0].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[1].Name).TeamId
                    },
                    new TeamAssignment
                    {
                        PlayerId = players.Single(s => s.Name == players[0].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[3].Name).TeamId
                    },
                    new TeamAssignment
                    {
                        PlayerId = players.Single(s => s.Name == players[1].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[0].Name).TeamId
                    },
                    new TeamAssignment
                    {
                        PlayerId = players.Single(s => s.Name == players[1].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[3].Name).TeamId
                    },
                    new TeamAssignment
                    {
                        PlayerId = players.Single(s => s.Name == players[2].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[3].Name).TeamId
                    },
                    new TeamAssignment
                    {
                        PlayerId = players.Single(s => s.Name == players[2].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[1].Name).TeamId
                    },
                    new TeamAssignment
                    {
                        PlayerId = players.Single(s => s.Name == players[3].Name).PlayerId, 
                        TeamId = teams.Single(t => t.Name == teams[1].Name).TeamId
                    },
                };

                context.TeamAssignments.AddRange(teamAssignment);
                context.SaveChanges();

            }
        }
    }
}
