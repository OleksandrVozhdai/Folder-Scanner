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
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System;

namespace ScanFolder
{
    public partial class MainWindow : Window
    {
		[DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
		public static extern int SHObjectProperties(IntPtr hwnd, uint shopObjectType, string pszPath, string pszPage);

		private string? path = null;
		private long FolderSize;

		List<string> Files;
		Dictionary<string, int> extL;

		public MainWindow()
        {
			InitializeComponent();

			Start();
		}

		public void Draw(List<string> files, Dictionary<string, int> ext)
		{
			//Draw Name

			FolderNameLabel.Content = System.IO.Path.GetFileName(path); 

			//Draw Files

			foreach (string fi in files)
			{
				FileNamesListBox.Items.Add(fi);
			}

			//Draw Extenions

			foreach (KeyValuePair<string, int> entry in ext)
			{
				 FileExtensionListBox.Items.Add(entry.Key + " x" + entry.Value);
			}

			//Draw piechart

			GeneratePieChart(ext);

			//Draw folder size
			if (FolderSize >= 1_000_000_000)
			{
				FolderSize = FolderSize / 1_000_000_000;
				FolderSizeLabel.Content = FolderSize + "Gb";
			}
			else if (FolderSize >= 1_000_000)
			{
				FolderSize = FolderSize / 1_000_000;
				FolderSizeLabel.Content = FolderSize + "Mb";
			}
			else if (FolderSize >= 1000)
			{
				FolderSize = FolderSize / 1000;
				FolderSizeLabel.Content = FolderSize + "Kb";
			}
			else FolderSizeLabel.Content = FolderSize + "bytes";
		}

		public async void Start()
		{
			if (path != null && path.Length > 1)
			{
				await Task.Run(()=> {
					try
					{
						Scan(path); 
						FolderSize = GetFolderSize(path);
						this.Dispatcher.Invoke(() => { Draw(Files, extL); });
					}
					catch (System.UnauthorizedAccessException)
					{
						MessageBox.Show("Access denied");
						Clear();
						return;
					}
					catch (Exception ex)
					{
						MessageBox.Show("An unknown error has occurred.");
						Clear();
						return;
					}
					finally
					{
						///
					}
				});
			}
		}

		public void Scan(string path)
		{
			string ext;
			
				Files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();

				extL = new Dictionary<string, int>();

				foreach (string d in Files)
				{
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
			if (Directory.Exists(path))
			{
				Clear();
				Start();
			}
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

		public long GetFolderSize(string path)
		{
			long CurrentFolderSize = 0;
			if (Directory.Exists(path))
			{
				DirectoryInfo folder = new DirectoryInfo(path);
				CurrentFolderSize = folderSize(folder);
			}
			return CurrentFolderSize; 
		}

		//https://www.geeksforgeeks.org/c-sharp/c-sharp-program-to-estimate-the-size-of-folder/
		private long folderSize(DirectoryInfo folder)
		{
			long totalSizeOfDir = 0;

			// Get all files into the directory
			FileInfo[] allFiles = folder.GetFiles();

			// Loop through every file and get size of it
			foreach (FileInfo file in allFiles)
			{
				totalSizeOfDir += file.Length;
			}

			// Find all subdirectories
			DirectoryInfo[] subFolders = folder.GetDirectories();

			// Loop through every subdirectory and get size of each
			foreach (DirectoryInfo dir in subFolders)
			{
				totalSizeOfDir += folderSize(dir);
			}

			// Return the total size of folder
			return totalSizeOfDir;
		}

		public void Clear()
		{
			if(FileNamesListBox.Items.Count>0)
				FileNamesListBox.Items.Clear();

			if (FileExtensionListBox.Items.Count > 0)
				FileExtensionListBox.Items.Clear();

			this.Dispatcher.Invoke(() => { 
				FolderNameLabel.Content = "Choose Folder";
				MyPieChart.Series.Clear();
				FolderSizeLabel.Content = string.Empty;
				
			});
		}

		private void OpenFileClick(object sender, RoutedEventArgs e)
		{
			string? filePath = FileNamesListBox.SelectedItem as string;
			if (filePath != null)
				Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
			else MessageBox.Show("Please choose a file");
		}

		private void OpenFileLocationClick(object sender, RoutedEventArgs e)
		{
			string? filePath = FileNamesListBox.SelectedItem as string;
			if (filePath != null)
				Process.Start("explorer.exe",  "/select, " + filePath);
			else MessageBox.Show("Please choose a file");
		}

		private void OpenFilePropertiesClick(object sender, RoutedEventArgs e)
		{
			string? filePath = FileNamesListBox.SelectedItem as string;
			if (filePath != null)
				SHObjectProperties(IntPtr.Zero, 0x2, filePath, null);
			else MessageBox.Show("Please choose a file");
		}

		private void DeleteFileClick(object sender, RoutedEventArgs e)
		{
			string? filePath = FileNamesListBox.SelectedItem as string;
			if (filePath != null)
			{
				try
				{
					MessageBoxResult result = MessageBox.Show(
						"Do you want to delete this file?",
						"Confirm Deletion",
						MessageBoxButton.YesNo,
						MessageBoxImage.Question
					);
					if (result == MessageBoxResult.Yes)
					{
						File.Delete(filePath);
						Clear();
						Start();
					}
					else MessageBox.Show("Cancelled");
				} catch(Exception ex) 
				{
#if DEBUG
					MessageBox.Show(ex.Message);
#endif
				}
				
			}
		}

		private void RefreshClick(object sender, RoutedEventArgs e)
		{
			Clear();
			Start();
		}

		private void FilterFiles(object sender, SelectionChangedEventArgs e)
		{
			string? fileExt = FileExtensionListBox.SelectedItem as string;
			if (fileExt != null)
				fileExt = fileExt.Split()[0];
			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(FileNamesListBox.Items);

			view.Filter = item =>
			{
				string? text = item.ToString();
				if(text != null)
					return text.EndsWith(fileExt);
				else return false;
			};

			
			view.Refresh();
		}
	}
}