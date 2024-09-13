using labo.signalr.api.Data;
using labo.signalr.api.Models;
using Microsoft.AspNetCore.SignalR;

namespace labo.signalr.api.Hubs
{
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }

    public class TaskHub : Hub
    {
        ApplicationDbContext _context;

        public TaskHub(ApplicationDbContext context)
        {
            _context = context;
        }

        //connexion
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            await Clients.All.SendAsync("UserCount", UserHandler.ConnectedIds.Count);
            //déclenchement de la fonction TaskList sur le client
            await Clients.Caller.SendAsync("TaskList", _context.UselessTasks.ToList());
        }

        //Ajouter une tâche
        public async Task AddTask(string task)
        {
            _context.UselessTasks.Add(new UselessTask() { Text = task });
            _context.SaveChanges(); //sauvegarde dan sla bd
            await Clients.All.SendAsync("TaskList", _context.UselessTasks.ToList()); //déclenchement de la fonction sur tous les clients
        }

        //Compléter une tâche
        public async Task CompleteTask(int taskid)
        {
            var task = _context.UselessTasks.Single(t => t.Id == taskid); //trouver la tâche
            task.Completed = true; //changement d'état
            _context.SaveChanges(); //sauvegarder dans la bd
            await Clients.All.SendAsync("TaskList", _context.UselessTasks.ToList()); //tous les clients sont avertis
        }

        //Déconnexion
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId); //on retire l'utilisateur
            await Clients.All.SendAsync("UserCount", UserHandler.ConnectedIds.Count); //on met à jout le count
            await base.OnDisconnectedAsync(exception);
        }
    }
}
