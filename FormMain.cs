using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileEditor
{
    public partial class FormMain : Form
    {
        private string filePath;
        private FileSystemWatcher watcher;
        private bool isSaving = false;

        public FormMain()
        {
            InitializeComponent();
            textBoxFile.ReadOnly = true;
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "";
                openFileDialog.Filter = "Все файлы|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                    using (var reader = new StreamReader(fileStream))
                    {
                        textBoxFile.Text = reader.ReadToEnd(); // Считываем всё содержимое файла
                    }
                }
            }

            if (watcher != null) watcher.Dispose();
            if (filePath != null)
            {
                textBoxFile.ReadOnly = false;
                var fileName = Path.GetFileName(filePath); // Получаем имя файла
                var directoryPath = filePath.Substring(0, filePath.Length - fileName.Length); // Получаем путь до файла
                textBoxFileName.Text = fileName;
                watcher = new FileSystemWatcher(directoryPath, fileName);
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
                watcher.SynchronizingObject = this;
                watcher.Changed += (obj, evnt) => {
                    if (isSaving)
                    {
                        isSaving = false;
                        return;
                    }
                    var reload = MessageBox.Show(filePath+"\n\nЭтот файл был изменён другой программой.\nВы хотите обновить его?", "Обновление файла", MessageBoxButtons.YesNo);
                    if (reload == DialogResult.Yes)
                    {
                        using (var reader = new StreamReader(filePath))
                        {
                            textBoxFile.Text = reader.ReadToEnd();
                        }
                    }
                };
            }
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                MessageBox.Show("Нет открытого файла!", "Сохранение файла");
                return;
            }

            isSaving = true;
            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                var writer = new StreamWriter(fileStream);
                writer.Write(textBoxFile.Text);
                writer.Close();
            }
        }

        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                MessageBox.Show("Нет открытого файла!", "Закрытие файла");
                return;
            }
            textBoxFile.ReadOnly = true;
            textBoxFile.Text = "";
            textBoxFileName.Text = "";
            filePath = null;
        }

        private void exitFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
