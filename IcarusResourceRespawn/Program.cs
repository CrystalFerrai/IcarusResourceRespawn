// Copyright 2022 Crystal Ferrai
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
using UeSaveGame;
using UeSaveGame.PropertyTypes;
using UeSaveGame.StructData;

namespace IcarusResourceRespawn
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.Out.WriteLine("Usage: IcarusResourceRespawn path\n\n    path    The path to the prospect save json file to modify. Recommended to backup file first.");
				return 0;
			}

			if (!File.Exists(args[0]))
			{
				Console.Error.WriteLine($"File not found or not accessible: {args[0]}");
				return 1;
			}

			UpdateProspeect(args[0], Console.Out, Console.Error, Console.Out);

			return 0;
		}

		private static void UpdateProspeect(string path, TextWriter outputLog, TextWriter errorLog, TextWriter warningLog)
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
				return;
			}

			if (prospect == null)
			{
				errorLog.Write("Error reading prospect file. Could not load Json.");
				return;
			}

			ArrayProperty? stateRecorderBlobs = prospect.ProspectData[0] as ArrayProperty;
			if (stateRecorderBlobs?.Value == null)
			{
				errorLog.Write("Error reading prospect file. Failed to locate state recorders.");
				return;
			}

			outputLog.WriteLine("Modifying prospect...");

			HashSet<string> recordersToRemove = new()
			{
				"/Script/Icarus.TreeRecorderComponent",
				"/Script/Icarus.VoxelRecorderComponent"
			};

			List<UProperty> newBlobs = new();
			foreach (UProperty prop in stateRecorderBlobs.Value)
			{
				PropertiesStruct? structData = (PropertiesStruct?)prop.Value;
				if (structData == null)
				{
					errorLog.WriteLine("[Error] Failed to read prospect. A state recorder component has an invalid value.");
					return;
				}

				StrProperty? nameProp = (StrProperty)structData.Properties[0];
				if (nameProp?.Value == null)
				{
					errorLog.WriteLine("[Error] Failed to read prospect. A state recorder component has an invalid or missing name.");
					return;
				}

				if (!recordersToRemove.Contains(nameProp.Value))
				{
					newBlobs.Add(prop);

					if (nameProp.Value == "/Script/Icarus.FLODTileRecorderComponent")
					{
						const string FoliageRecordWarning = "[Warning] Could not read a foliage recorder record. This might prevent some resources from being respawned.";

						IList<UProperty> recorderProperties = ProspectSerlializationUtil.DeserializeRecorderData(structData.Properties[1]);

						foreach (UProperty recorderProp in recorderProperties)
						{
							if (recorderProp.Name != "Record") continue;

							PropertiesStruct? recordStruct = (PropertiesStruct?)recorderProp.Value;
							if (recordStruct == null)
							{
								warningLog.WriteLine(FoliageRecordWarning);
								continue;
							}

							foreach (UProperty recordArrayProp in recordStruct.Properties)
							{
								if (recordArrayProp.Name != "Records") continue;

								ArrayProperty recordArray = (ArrayProperty)recordArrayProp;
								if (recordArray.Value == null)
								{
									warningLog.WriteLine(FoliageRecordWarning);
									continue;
								}

								foreach (UProperty recordProp in recordArray.Value)
								{
									PropertiesStruct? recordPropertiesStruct = (PropertiesStruct?)recordProp.Value;
									if (recordPropertiesStruct == null)
									{
										warningLog.WriteLine(FoliageRecordWarning);
										continue;
									}

									foreach (UProperty recordDataProp in recordPropertiesStruct.Properties)
									{
										if (recordDataProp.Name != "DestroyedInstanceIndices") continue;

										recordDataProp.Value = Array.Empty<UProperty>();

										break;
									}
								}
								break;
							}
							break;
						}

						structData.Properties[1] = ProspectSerlializationUtil.SerializeRecorderData(recorderProperties);
					}
				}
			}

			stateRecorderBlobs.Value = newBlobs.ToArray();

			outputLog.WriteLine("Saving prospect...");

			using (FileStream file = File.Create(path))
			{
				prospect.Save(file);
			}
		}
	}
}
