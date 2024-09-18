using Newtonsoft.Json;
using System;
using System.IO;

namespace GitHubReport
{
    public class GitHubCredentialsLoader
    {
        private string githubUsername;
        private string personalAccessToken;

        public void LoadCredentials(string filePath)
        {
            try
            {
                // Leer el archivo JSON
                string json = File.ReadAllText(filePath);

                // Deserializar el JSON en un objeto Credenciales
                Credenciales credenciales = JsonConvert.DeserializeObject<Credenciales>(json);

                // Asignar los valores a las variables
                githubUsername = credenciales.GithubUsername;
                personalAccessToken = credenciales.PersonalAccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar las credenciales: {ex.Message}");
            }
        }

        public string GetUsername()
        {
            return githubUsername;
        }

        public string GetToken()
        {
            return personalAccessToken;
        }
    }
}
