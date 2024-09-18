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
        }

        private async void BtnObtenerRepositorios_Click(object sender, EventArgs e)
        {
            picBoxWait.Visible = true;
            BtnExportPdf.Enabled = false;
            BtnObtenerRepositorios.Enabled = false;

            var repos = await GetRepositoriesAsync();
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

        private async void BtnExportPdf_Click(object sender, EventArgs e)
        {
            picBoxWait.Visible = true;
            BtnExportPdf.Enabled = false;
            BtnObtenerRepositorios.Enabled = false;
            
            var repos = await GetRepositoriesAsync();
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
    }
}
