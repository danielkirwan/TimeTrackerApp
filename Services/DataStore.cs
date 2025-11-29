using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TimeTrackerApp.Models;

namespace TimeTrackerApp.Services
{
    public static class DataStore
    {
        private static readonly string BaseFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "TimeTrackerApp");

        private static readonly string ClientsFile = Path.Combine(BaseFolder, "clients.json");
        private static readonly string SessionsFile = Path.Combine(BaseFolder, "sessions.json");

        private static JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        static DataStore()
        {
            if (!Directory.Exists(BaseFolder))
                Directory.CreateDirectory(BaseFolder);
        }

        public static List<Client> LoadClients()
        {
            if (!File.Exists(ClientsFile)) return new List<Client>();
            var json = File.ReadAllText(ClientsFile);
            return JsonSerializer.Deserialize<List<Client>>(json, _options) ?? new List<Client>();
        }

        public static void SaveClients(List<Client> clients)
        {
            var json = JsonSerializer.Serialize(clients, _options);
            File.WriteAllText(ClientsFile, json);
        }

        public static List<WorkSession> LoadSessions()
        {
            if (!File.Exists(SessionsFile)) return new List<WorkSession>();
            var json = File.ReadAllText(SessionsFile);
            return JsonSerializer.Deserialize<List<WorkSession>>(json, _options) ?? new List<WorkSession>();
        }

        public static void SaveSessions(List<WorkSession> sessions)
        {
            var json = JsonSerializer.Serialize(sessions, _options);
            File.WriteAllText(SessionsFile, json);
        }
    }
}
