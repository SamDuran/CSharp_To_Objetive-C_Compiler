using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace SDT_PF_Objetive_C
{
	public partial class MainWindow : Window
	{
		//************************ REGEXs ********************//
		Regex main = new Regex(@"int main(\s*)\(\)(\s*){(\n*\s*).*}");
		Regex import = new Regex("#import <Foundation/Foundation.h>");
		Regex NsLog = new Regex(@"NsLog\s*\(\s*@"+".*"+@"\);");
		//******************  VARIABLES UTILES  ********************//

		string MensajeDeError = "";
		bool HayError = false;
		bool Importa = false;
		bool CalcNeeded = false;
		bool PrintNeeded = false;
		int MainLine = 0;
		string Num1 = "";
		char Operator = ' ';
		string Num2 = "";
		string Cadena1 = "";
		string Cadena2 = "";
		//**********************  Métodos   ********************//
		public MainWindow()
		{
			InitializeComponent();
		}
		private void ClearBTN_Click(object sender, RoutedEventArgs e)
		{
			LimpiarEntrada();
			OutputTB.Text = null;
		}
		private void LimpiarEntrada()
		{
			InputTB.Text = null;
			Importa = false;
		}
		private void RunBTN_Click(object sender, RoutedEventArgs e)
		{
			RestartData();
			OutputTB.Text = null;
			Compilar(InputTB.Text);
			RemoveTempFiles();
		}
		private void Compilar(string texto)
		{
			if (texto == null || texto.Length <= 0)
			{
				MessageBox.Show("CwnC Error \nCompilation error CwnC: Code with no Code.", "CwnC Error");
				return;
			}

			using (StreamWriter sw = new StreamWriter(@"./TempFile.txt"))
				sw.Write(texto);

			var cntLns = File.ReadAllLines(@"./TempFile.txt").Length;

			string[] lines = new string[cntLns];
			string[] interiorMain = new string[0];

			string interiorMainString = "";
			int lastLine = 0;

			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = File.ReadAllLines(@"./TempFile.txt")[i];
				if (lines[i] != null)
				{
					if (import.IsMatch(lines[i]))
						Importa = true;


					else if (lines[i].StartsWith("int main") && !Importa)
					{
						OutputTB.Text = "Libreria no importada";
						return;
					}
					else if (lines[i].StartsWith("int main") && Importa)
					{
						MainLine = i;
						interiorMain = new string[lines.Length - i];
						for (int j = 0; j < lines.Length - i; j++)
						{
							interiorMain[j] = File.ReadAllLines(@"./TempFile.txt")[i + j];
							interiorMainString += interiorMain[j];
							lastLine = j;
						}
						break;
					}
				}
			}
			if (main.IsMatch(interiorMainString))
			{
				int counter = 0;
				foreach (string line in interiorMain)
				{
					if (counter == lastLine) break;
					counter++;
					Execute(line, counter);
					if (HayError)
					{
						OutputTB.Text = MensajeDeError;
						return;
					}
					RestartData();
				}
				if (!HayError)
				{
					OutPut();
					Console.WriteLine("\a");
				}
			}
			else
				OutputTB.Text = "La funcion main fue estructurada de manera incorrecta...";
		}
		private void SaveBTN_Click(object sender, RoutedEventArgs e)
		{
			var OpenFileDialog = new System.Windows.Forms.FolderBrowserDialog();
			string path = "";
			if (OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				path = System.IO.Path.GetDirectoryName(OpenFileDialog.SelectedPath);
			else return;

			var fecha = DateTime.Now.ToString("dd-MMM-yyyy");

			using (StreamWriter sw = new StreamWriter(path + @"\Codigo" + fecha + ".m"))
				sw.Write(InputTB.Text);
		}
		private void Execute(string linea, int indice)
		{
			if (linea.Length == 0 || linea.StartsWith("int main") || HayError)
				return;

			if (NsLog.IsMatch(linea))
			{
				var instruccion2 = GetDataString(linea);
				GetData(instruccion2);
				SaveTemporally();
			}
			else
			{
				HayError = true;
				MensajeDeError = $"Error en la linea {indice + MainLine}.";
			}
		}
		private void RestartData()
		{
			Importa = false;
			HayError = false;
			MensajeDeError = "";
			Cadena1 = "";
			Cadena2 = "";
			Operator = ' ';
			Num1 = "";
			Num2 = "";
			CalcNeeded = false;
			PrintNeeded = false;
			OutputTB.Text = "";
		}
		private void SaveTemporally()
		{
			var input = "";
			if (PrintNeeded)
				input += Cadena1 + Cadena2;

			if (CalcNeeded && Tools.ToFloat(Num1) != -40000 && Tools.ToFloat(Num2) != -40000)
			{
				switch (Operator)
				{
					case '+':
						{
							input += Tools.ToFloat(Num1) + Tools.ToFloat(Num2);
							break;
						}
					case '*':
						{
							input += Tools.ToFloat(Num1) * Tools.ToFloat(Num2);
							break;
						}
					case '-':
						{
							input += Tools.ToFloat(Num1) - Tools.ToFloat(Num2);
							break;
						}
					case '/':
						{
							input += Tools.ToFloat(Num1) / Tools.ToFloat(Num2);
							break;
						}
				}
			}
			if (File.Exists("./TmpCompilationFile.Txt"))
				input = File.ReadAllText("./TmpCompilationFile.Txt", Encoding.UTF8) + input;
			
			FileStream fs = new FileStream("./TmpCompilationFile.Txt", FileMode.OpenOrCreate);
			TextWriter tw = Console.Out;
			StreamWriter sw = new StreamWriter(fs);
			Console.SetOut(sw);
			Console.WriteLine(input);
			Console.SetOut(tw);
			sw.Close();
		}
		private void OutPut()
		{
			OutputTB.Text = File.ReadAllText("./TmpCompilationFile.Txt", Encoding.UTF8);
		}
		private void RemoveTempFiles()
		{
			Process cmd = new Process();
			cmd.StartInfo.FileName = "cmd.exe";
			cmd.StartInfo.RedirectStandardInput = true;
			cmd.StartInfo.RedirectStandardOutput = true;
			cmd.StartInfo.CreateNoWindow = true;
			cmd.StartInfo.UseShellExecute = false;
			cmd.Start();
			cmd.StandardInput.WriteLine("cd " + Directory.GetCurrentDirectory());
			cmd.StandardInput.Flush();
			cmd.StandardInput.WriteLine("del *.txt");
			cmd.StandardInput.Flush();
			cmd.StandardInput.Close();
		}
		private string GetDataString(string line)
		{
			if (line.Length == 0)
				return null;

			string OutString = line.TrimStart();
			OutString = OutString.Replace("NsLog(@", null);
			var aux = OutString.Reverse();
			OutString = "";
			for (int i = aux.Count() - 1; i > 0; i--)
				OutString += aux.ElementAt(i);

			return OutString;
		}
		private void GetData(string DataString)
		{
			if (string.IsNullOrEmpty(DataString))
				return;
			bool StringStart = false;
			bool OperatorExists = false;
			int stringsCount = 0;
			char LastChar = ' ';
			foreach (var letter in DataString)
			{
				if (letter == '"')
				{
					StringStart = !StringStart;

					if (!StringStart)
					{
						PrintNeeded = true;
						stringsCount++;
					}

				}
				else if (StringStart)
				{
					if (stringsCount == 0)
						Cadena1 += letter;
					else
						Cadena2 += letter;
				}
				else if (char.IsNumber(letter))
				{
					if (OperatorExists && (char.IsNumber(LastChar) || LastChar == '.' || LastChar == ' ' || LastChar == '+' || LastChar == '-' || LastChar == '*' || LastChar == '/'))
						Num2 += letter;
					else if (!OperatorExists && (char.IsNumber(LastChar) || LastChar == '.' || LastChar == ' '))
						Num1 += letter;
				}
				else if (letter == '+' || letter == '-' || letter == '*' || letter == '/')
				{
					OperatorExists = true;
					Operator = letter;
					CalcNeeded = true;
				}
				LastChar = letter;
			}
		}
	}
}
