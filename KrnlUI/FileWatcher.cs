using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using KrnlUI.Controls;

namespace KrnlUI;

public class FileWatcher
{
	private Tab Tab { get; set; }

	public string filePath { get; set; }

	private bool isWatching { get; set; }

	public FileWatcher(Tab tab)
	{
		Tab = tab;
	}

	public void Start()
	{
		if (!isWatching)
		{
			isWatching = true;
			new Task(Initialize).Start();
		}
	}

	private void Initialize()
	{
		Application.Current.Dispatcher.Invoke((Func<Task>)async delegate
		{
			string path = filePath;
			DateTime OldWriteTime = File.GetLastWriteTime(filePath);
			while (isWatching && !Tab.isDeleted)
			{
				if (!File.Exists(filePath))
				{
					isWatching = false;
					break;
				}
				DateTime lastWriteTime = File.GetLastWriteTime(filePath);
				if (OldWriteTime != lastWriteTime)
				{
					if (filePath != path)
					{
						path = filePath;
						OldWriteTime = lastWriteTime;
					}
					else if (Application.Current.MainWindow.IsActive && Tab.isSelected)
					{
						OldWriteTime = lastWriteTime;
						if (Tab.watchSave)
						{
							Tab.watchSave = false;
							continue;
						}
						if (MessageBox.Show("It appears that the file '" + filePath.Split('\\').ToList().Last() + "' has been changed externally, would you like to reload the file?", "Krnl", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (Tab.isSelected)
							{
								Tab.MainInstance.WriteScript(File.ReadAllText(filePath), tabPrompt: false);
							}
							else
							{
								Tab.Script = File.ReadAllText(filePath);
							}
							Tab.isSaved = false;
							Tab.IsSaved = true;
							Tab.TabName = Tab.TabName;
						}
						else
						{
							Tab.isSaved = true;
							Tab.IsSaved = false;
						}
					}
				}
				await Task.Delay(1);
			}
		});
	}
}
