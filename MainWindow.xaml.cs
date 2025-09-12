using LiveCharts.Wpf;

using LiveCharts;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ScanFolder
{
    public partial class MainWindow : Window
    {

		private string? path = null;

		public MainWindow()
        {
			InitializeComponent();


			Start();
		}

		public async void Start()
		{
			if (path != null && path.Length > 1)
			{
				FolderNameLabel.Content = System.IO.Path.GetFileName(path);
				await Task.Run(()=>Scan(path));
			}
		}

		public void Scan(string path)
		{
			string ext;
			try
			{
				List<string> D = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();

				Dictionary<string, int> extL = new Dictionary<string, int>();


				foreach (string d in D)
				{
					Debug.WriteLine(d);
					FileNamesListBox.Dispatcher.Invoke(() => { FileNamesListBox.Items.Add(d); });


					ext = System.IO.Path.GetExtension(d);

					if (!extL.ContainsKey(ext))
					{
						extL.Add(ext, 1);
					}
					else
					{
						extL[ext]++;
					}
				}

				foreach (KeyValuePair<string, int> entry in extL)
				{
					FileExtensionListBox.Dispatcher.Invoke(() => { FileExtensionListBox.Items.Add(entry.Key + " x" + entry.Value); });

				}
				this.Dispatcher.Invoke(() =>
				{
					GeneratePieChart(extL);
				});

			}
			catch (System.UnauthorizedAccessException)
			{
				MessageBox.Show("Access denied");
			}
			catch (Exception ex)
			{
				MessageBox.Show("An unknown error has occurred.");
			}
			finally
			{
				///
			}
		}

		public void GeneratePieChart(Dictionary<string, int> values)
		{

			foreach (KeyValuePair<string, int> entry in values)
			{
				MyPieChart.Series.Add(
					 new PieSeries { Title = entry.Key, Values = new ChartValues<int> { entry.Value } });
			}
		}

		private void FolderButtonClick(object sender, RoutedEventArgs e)
		{
			path = Convert.ToString(GetFolderPath());
			Start();
		}

		public string GetFolderPath()
		{
			OpenFolderDialog theDialog = new OpenFolderDialog();
			theDialog.Title = "Open Folder";
			theDialog.InitialDirectory = @"C:\";

			bool? result = theDialog.ShowDialog();

			if (result == true)
			{
				MessageBox.Show("Choosen path: " + theDialog.FolderName);
				return theDialog.FolderName;
			} else
			{
				MessageBox.Show("Error occurred while getting file path");
				return "";
			}

		}
	}
}