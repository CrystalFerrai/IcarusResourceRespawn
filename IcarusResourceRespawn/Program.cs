// Copyright 2023 Crystal Ferrai
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using IcarusSaveLib;

namespace IcarusResourceRespawn
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			IReadOnlyList<string> remainingArgs;
			RespawnOptions options = RespawnOptions.ParseCommandLine(args, out remainingArgs);

			if (!options.Any() || remainingArgs.Count < 1)
			{
				PrintUsage();
				return OnExit(0);
			}

			IEnumerable<string> unknownOptions = remainingArgs.Where(a => a.StartsWith('-'));
			if (unknownOptions.Any())
			{
				Console.Error.WriteLine($"Error: Unrecognized options: {string.Join(", ", unknownOptions)}");
				return OnExit(1);
			}

			if (remainingArgs.Count > 1)
			{
				Console.Error.WriteLine("Error: Too many parameters");
				return OnExit(1);
			}

			string prospectPath = remainingArgs[0];

			if (!File.Exists(prospectPath))
			{
				Console.Error.WriteLine($"Error: File not found or not accessible: {prospectPath}");
				return OnExit(1);
			}

			bool success;
			try
			{
				success = UpdateProspect(prospectPath, options, Console.Out, Console.Error, Console.Out);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[Error] {ex.GetType().FullName}: {ex.Message}");
				success = false;
			}

			if (success)
			{
				Console.Out.WriteLine("Done.");
			}

			return OnExit(success ? 0 : 1);
		}

		private static int OnExit(int code)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				Console.ReadKey();
			}
			return code;
		}

		private static void PrintUsage()
		{
			string optionIndent = "    ";
			Console.Out.WriteLine($"Usage: IcarusResourceRespawn [options] path\n");
			RespawnOptions.PrintCommandLineOptions(Console.Out, optionIndent);
			Console.Out.WriteLine($"\n{optionIndent}{"path",-RespawnOptions.MaxOptionStringLength}  The path to the prospect save json file to modify. Recommended to backup file first.\n");
			Console.Out.WriteLine("Note: Must select at least one resource type to respawn.");
		}

		private static bool UpdateProspect(string path, RespawnOptions options, TextWriter outputLog, TextWriter errorLog, TextWriter warningLog)
		{
			ProspectSave? prospect;

			outputLog.WriteLine("Loading prospect...");

			try
			{
				using (FileStream file = File.OpenRead(path))
				{
					prospect = ProspectSave.Load(file);
				}
			}
			catch (Exception ex)
			{
				errorLog.Write($"Error reading prospect file. [{ex.GetType().FullName}] {ex.Message}");
				return false;
			}

			if (prospect == null)
			{
				errorLog.Write("Error reading prospect file. Could not load Json.");
				return false;
			}

			ResourceRespawner respawner = new(outputLog, errorLog, warningLog);
			respawner.Run(prospect, options);

			outputLog.WriteLine("Saving prospect...");

			using (FileStream file = File.Create(path))
			{
				prospect.Save(file);
			}

			return true;
		}
	}
}
