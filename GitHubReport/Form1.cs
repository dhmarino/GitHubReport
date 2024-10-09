using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text;

namespace GitHubReport
{
    public partial class Form1 : Form
    {
        private readonly string githubApiUrl = "https://api.github.com";
        private readonly string githubUsername = "TU_USUARIO"; // Usuario de GitHub, se obtiene del archivo credenciales.json
        private readonly string personalAccessToken = "TU_TOKEN"; // Tu token personal, se obtiene del archivo credenciales.json
        public Form1()
        {
            InitializeComponent();
            
            GitHubCredentialsLoader credentialsLoader = new GitHubCredentialsLoader();
            // Cargar las credenciales desde el archivo credenciales.json
            string filePath = "credenciales.json"; // Cambiar por la ruta correcta del archivo credenciales.json donde estan tus credenciales
            credentialsLoader.LoadCredentials(filePath);
            // Obtener las credenciales
            githubUsername = credentialsLoader.GetUsername();
            personalAccessToken = credentialsLoader.GetToken();
            
            var versiones = typeof(Form1).Assembly.GetName().Version;
            lblVersion.Text = "Versión " + versiones.ToString();
            picBoxWait.Visible = false;
            lblInfo.Text = string.Empty;
        }

        //Boton que lista los repositorios y el ultimo commit en el ListBox
        private async void BtnObtenerRepositorios_Click(object sender, EventArgs e)
        {
            picBoxWait.Visible = true;
            BtnExportPdf.Enabled = false;
            BtnObtenerRepositorios.Enabled = false;

            var repos = await GetAllReposAsync();
            foreach (var repo in repos)
            {
                string lastCommit = await GetLastCommitAsync(repo.Name);
                listBox1.Items.Add($"{repo.Name} - {lastCommit}");
            }
            picBoxWait.Visible = false;
            MessageBox.Show("Termino la consulta");
            
            BtnExportPdf.Enabled = true;
            BtnObtenerRepositorios.Enabled = true;
        }
        //Este GetRepositoriesAsync() tiene el límite de 30 repositorios al hacer una solicitud a la API de GitHub
        //es un comportamiento predeterminado, ya que GitHub API devuelve por defecto solo los primeros 30 resultados
        private async Task<List<Repository>> GetRepositoriesAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppName", "1.0"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", personalAccessToken);

                var response = await client.GetAsync($"{githubApiUrl}/user/repos");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Repository>>(json);
                }

                MessageBox.Show("Error al obtener los repositorios");
                return new List<Repository>();
            }
        }
        //Esta es la opcion para traer mas de 30 repositorios
        private async Task<List<Repository>> GetAllReposAsync()
        {
            List<Repository> allRepos = new List<Repository>();
            int page = 1;
            int perPage = 30; // Puedes ajustar este valor según necesites (hasta un máximo de 100)
            bool hasMoreRepos = true;

            while (hasMoreRepos)
            {
                // URL de la API con paginación
                var url = $"https://api.github.com/user/repos?per_page={perPage}&page={page}";

                // Hacer la solicitud para obtener la página actual de repositorios
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "request"); // GitHub requiere un User-Agent
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);

                using (var client = new HttpClient())
                {
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var repos = JsonConvert.DeserializeObject<List<Repository>>(jsonResponse);

                        // Añadir los repositorios obtenidos a la lista total
                        allRepos.AddRange(repos);

                        // Verificar si todavía hay más repositorios
                        if (repos.Count < perPage)
                        {
                            hasMoreRepos = false; // Si la cantidad de repos obtenidos es menor al límite, no hay más páginas
                        }
                        else
                        {
                            page++; // Incrementar la página para la siguiente solicitud
                        }
                    }
                    else
                    {
                        throw new Exception("Error al obtener los repositorios: " + response.ReasonPhrase);
                    }
                }
            }

            return allRepos;
        }
        private async Task<string> GetLastCommitAsync(string repoName)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppName", "1.0"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", personalAccessToken);

                var response = await client.GetAsync($"{githubApiUrl}/repos/{githubUsername}/{repoName}/commits");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var commits = JsonConvert.DeserializeObject<List<Commit>>(json);
                    return commits.Count > 0 ? commits[0].CommitInfo.Message : "No hay commits";
                }

                MessageBox.Show($"Error al obtener los commits del repositorio: {repoName}");
                return "Error";
            }
        }
        // Clases para deserializar el JSON de los repositorios y commits
        public class Repository
        {
            public string Name { get; set; }
        }
        public class Commit
        {
            [JsonProperty("commit")]
            public CommitInfo CommitInfo { get; set; }
        }
        public class CommitInfo
        {
            public string Message { get; set; }
        }
        //Boton que exporta el litado Repositorio - Ultimo commit a un PDF
        private async void BtnExportPdf_Click(object sender, EventArgs e)
        {
            picBoxWait.Visible = true;
            BtnExportPdf.Enabled = false;
            BtnObtenerRepositorios.Enabled = false;
            
            var repos = await GetAllReposAsync();
            List<string> repoData = new List<string>();
            foreach (var repo in repos)
            {
                string lastCommit = await GetLastCommitAsync(repo.Name);
                repoData.Add($"{repo.Name} - {lastCommit}");
            }

            ExportToPDF(repoData);

            BtnObtenerRepositorios.Enabled = true;
            BtnExportPdf.Enabled = true;
        }
        // Método para exportar la lista a un PDF
        private void ExportToPDF(List<string> repoData)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Document doc = new Document(PageSize.A4);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();
                    //Fonts
                    iTextSharp.text.Font Font1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font Font2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font Font3 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font Font4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    
                    //Logos de Empresa y GitHub
                    try
                    {
                        string leftImagePath = "logo_valid.jpg"; // Ruta de la imagen izquierda
                        string rightImagePath = "github.png"; // Ruta de la imagen derecha

                        iTextSharp.text.Image leftImage = iTextSharp.text.Image.GetInstance(leftImagePath);
                        iTextSharp.text.Image rightImage = iTextSharp.text.Image.GetInstance(rightImagePath);

                        // Redimensionar las imágenes
                        leftImage.ScaleToFit(230f, 70f);
                        rightImage.ScaleToFit(70f, 70f);

                        // Crear una tabla con 2 columnas para las imágenes
                        PdfPTable table = new PdfPTable(2)
                        {
                            WidthPercentage = 100 // Que ocupe el 100% del ancho del PDF
                        };

                        // Celda para la imagen izquierda
                        PdfPCell leftCell = new PdfPCell(leftImage)
                        {
                            Border = PdfPCell.NO_BORDER, // Sin bordes en la celda
                            HorizontalAlignment = Element.ALIGN_LEFT // Alinear a la izquierda
                        };

                        // Celda para la imagen derecha
                        PdfPCell rightCell = new PdfPCell(rightImage)
                        {
                            Border = PdfPCell.NO_BORDER, // Sin bordes en la celda
                            HorizontalAlignment = Element.ALIGN_RIGHT // Alinear a la derecha
                        };

                        // Añadir las celdas a la tabla
                        table.AddCell(leftCell);
                        table.AddCell(rightCell);

                        // Añadir la tabla (con las imágenes) al documento
                        doc.Add(table);

                        doc.Add(new Paragraph(" ")); // Añadir un espacio en blanco después de las imágenes
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al agregar las imágenes: {ex.Message}");
                    }

                    // Título del documento
                    doc.Add(new Paragraph("Listado de Repositorios y Últimos Commits", Font1));
                    doc.Add(new Paragraph(" ")); // Espacio en blanco

                    int cantRepos = 0;
                    // Agregar cada repositorio, su último commit y llevamos un contador de repositorios
                    foreach (var repoInfo in repoData)
                    {
                        if (!repoInfo.Contains("dhmarino"))
                        {
                            cantRepos++;
                            doc.Add(new Paragraph(repoInfo));
                        }
                    }

                    doc.Add(new Paragraph(" ")); // Espacio en blanco
                    doc.Add(new Paragraph("Son " + cantRepos + " repositorios")); 

                    doc.Close();
                    writer.Close();
                }
                picBoxWait.Visible = false;
                MessageBox.Show("PDF exportado exitosamente");
            }
        }

        //Boton que exporta por cada repositorio un PDF con todos sus commits 
        private async void BtnCommitsPorRepositorio_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;

                    lblInfo.Text = "Obteniendo informacion de la API de GitHub. ¡¡¡POR FAVOR ESPERE!!!";
                    lblInfo.Refresh();
                    picBoxWait.Visible = true;

                    GitHubCredentialsLoader credentialsLoader = new GitHubCredentialsLoader();

                    // Cargar las credenciales desde el archivo credenciales.json
                    string filePath = @"ruta/al/archivo/credenciales.json"; // Cambia por la ruta correcta
                    credentialsLoader.LoadCredentials(filePath);

                    // Obtener los repositorios con todos sus commits
                    var repos = await GetReposWithCommitsAsync();

                    // Exportar los commits de cada repositorio a un PDF en la carpeta seleccionada
                    ExportCommitsToPDF(repos, selectedPath);
                    picBoxWait.Visible = false;
                    lblInfo.Text = string.Empty;
                    lblInfo.Refresh();
                    MessageBox.Show("Se han generado los PDFs de los commits para cada repositorio en la carpeta " + selectedPath, "Info", MessageBoxButtons.OK ,MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se seleccionó ninguna carpeta.");
                }
            }
        }
        public class GitHubRepo
        {
            public string Name { get; set; }
            [JsonProperty("private")]  // Mapea el campo "private" de la respuesta JSON
            public bool IsPrivate { get; set; }
            public List<GitHubCommit> Commits { get; set; } = new List<GitHubCommit>();
        }
        public class GitHubCommit
        {
            public string Message { get; set; }
            public DateTime Date { get; set; }
        }
        //Obtener todos los repositorios (con paginación)
        private async Task<List<GitHubRepo>> GetAllReposForCommitsPorRepositorioAsync()
        {
            List<GitHubRepo> allRepos = new List<GitHubRepo>();
            int page = 1;
            int perPage = 30; // Ajusta este valor hasta un máximo de 100 si es necesario
            bool hasMoreRepos = true;

            while (hasMoreRepos)
            {
                // URL de la API con paginación
                var url = $"https://api.github.com/user/repos?per_page={perPage}&page={page}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "request"); // GitHub requiere un User-Agent
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);

                using (var client = new HttpClient())
                {
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var repos = JsonConvert.DeserializeObject<List<GitHubRepo>>(jsonResponse);

                        allRepos.AddRange(repos);

                        if (repos.Count < perPage)
                        {
                            hasMoreRepos = false;
                        }
                        else
                        {
                            page++;
                        }
                    }
                    else
                    {
                        throw new Exception("Error al obtener los repositorios: " + response.ReasonPhrase);
                    }
                }
            }

            return allRepos;
        }
        //Obtener todos los commits para cada repositorio (con paginación):
        private async Task<List<GitHubCommit>> GetCommitsForRepoAsync(string repoName)
        {
            List<GitHubCommit> allCommits = new List<GitHubCommit>();
            int page = 1;
            int perPage = 100; // Límite de GitHub
            bool hasMoreCommits = true;

            while (hasMoreCommits)
            {
                // URL de la API para obtener commits del repositorio con paginación
                var url = $"https://api.github.com/repos/{githubUsername}/{repoName}/commits?per_page={perPage}&page={page}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "request"); // GitHub requiere un User-Agent
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);

                using (var client = new HttpClient())
                {
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var commits = JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse);

                        // Extraer el mensaje y la fecha del commit
                        foreach (var commit in commits)
                        {
                            var message = (string)commit.commit.message;
                            var date = (DateTime)commit.commit.committer.date;

                            allCommits.Add(new GitHubCommit
                            {
                                Message = message,
                                Date = date
                            });
                        }

                        if (commits.Count < perPage)
                        {
                            hasMoreCommits = false; // No hay más commits
                        }
                        else
                        {
                            page++;
                        }
                    }
                    else
                    {
                        throw new Exception($"Error al obtener los commits para el repositorio {repoName}: " + response.ReasonPhrase);
                    }
                }
            }

            return allCommits;
        }
        //Obtener commits de todos los repositorios:
        private async Task<List<GitHubRepo>> GetReposWithCommitsAsync()
        {
            List<GitHubRepo> reposWithCommits = new List<GitHubRepo>();

            // Obtener todos los repositorios del usuario
            var repos = await GetAllReposForCommitsPorRepositorioAsync();

            foreach (var repo in repos)
            {
                // Obtener todos los commits para el repositorio actual
                var commits = await GetCommitsForRepoAsync(repo.Name);
                repo.Commits = commits;
                reposWithCommits.Add(repo);
            }

            return reposWithCommits;
        }
        private void ExportCommitsToPDF(List<GitHubRepo> repos, string folderPath)
        {
            foreach (var repo in repos)
            {
                // Construir la ruta completa del archivo PDF en la carpeta seleccionada
                string pdfFileName = Path.Combine(folderPath, $"{repo.Name}.pdf");

                using (FileStream fs = new FileStream(pdfFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Document doc = new Document(PageSize.A4);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);

                    // Asignar la clase que maneja el paginado al documento
                    PdfPageNumbers pageEventHandler = new PdfPageNumbers();
                    writer.PageEvent = pageEventHandler;

                    doc.Open();

                    //Fonts
                    iTextSharp.text.Font Font1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font Font2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font Font3 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font Font4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    
                    // Agregar título
                    doc.Add(new Paragraph(repo.Name, Font1));
                    doc.Add(new Paragraph($"Visibilidad: {(repo.IsPrivate ? "Privado" : "Público")}"));
                    doc.Add(new Paragraph("Commits:"));
                    doc.Add(new Paragraph(" ")); // Espacio en blanco

                    // Agregar los commits
                    foreach (var commit in repo.Commits)
                    {
                        // Añadir mensaje y fecha del commit
                        doc.Add(new Paragraph(commit.Date.ToString("yyyy-MM-dd HH:mm:ss") + " - " + commit.Message));
                    }

                    doc.Close();
                    writer.Close();
                }
                lblInfo.Text = "Se genero el pdf de " + repo.Name;
                lblInfo.Refresh();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        ///Agrego paginado
        public class PdfPageNumbers : PdfPageEventHelper
        {
            private PdfContentByte cb;
            private BaseFont bf = null;
            private int pageNumber = 0;
            private Image githubLogo;

            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                // Configurar la fuente y obtener el PdfContentByte para escribir en el PDF
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;

                try
                {
                    // Cargar la imagen del logo de GitHub (debe estar en la ruta especificada)
                    githubLogo = Image.GetInstance("github.png"); // ruta y nombre del logo
                    githubLogo.ScaleToFit(50f, 50f); // Ajusta el tamaño de la imagen
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al cargar la imagen: " + ex.Message);
                }
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                pageNumber++;
                Rectangle pageSize = document.PageSize;

                // Escribir el número de página en el pie
                string text = "Página " + pageNumber.ToString();
                float x = (pageSize.Left + pageSize.Right) / 2;
                float y = pageSize.GetBottom(15);
                cb.BeginText();
                cb.SetFontAndSize(bf, 10); // Tamaño de la fuente
                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, text, x, y, 0);
                cb.EndText();

                // Posicionar el logo en la esquina superior derecha de cada página
                if (githubLogo != null)
                {
                    float logoX = pageSize.GetRight(70); // Ajustar la distancia desde el borde derecho
                    float logoY = pageSize.GetTop(70); // Ajustar la distancia desde el borde superior
                    githubLogo.SetAbsolutePosition(logoX, logoY); // Establecer la posición
                    cb.AddImage(githubLogo); // Agregar la imagen al contenido de la página
                }
            }
        }
        ///Fin agregar paginado
        ////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
