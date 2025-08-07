// Copyright 2025 Crystal Ferrai
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
	internal class ResourceRespawner
	{
		TextWriter mOutputLog;
		TextWriter mErrorLog;
		TextWriter mWarningLog;

		public ResourceRespawner(TextWriter outputLog, TextWriter errorLog, TextWriter warningLog)
		{
			mOutputLog = outputLog;
			mErrorLog = errorLog;
			mWarningLog = warningLog;
		}

		public void Run(ProspectSave prospect, RespawnOptions options)
		{
			ArrayProperty? stateRecorderBlobs = prospect.ProspectData[0].Property as ArrayProperty;
			if (stateRecorderBlobs?.Value == null)
			{
				mErrorLog.Write("Error reading prospect. Failed to locate state recorder array at index 0.");
				return;
			}

			mOutputLog.WriteLine("Modifying prospect...");

			const string TreeRecorderComponent = "/Script/Icarus.TreeRecorderComponent";
			const string VoxelRecorderComponent = "/Script/Icarus.VoxelRecorderComponent";
			const string RockBaseRecorderComponent = "/Script/Icarus.RockBaseRecorderComponent";

			HashSet<string> recordersToRemove = new();
			if (options.Trees) recordersToRemove.Add(TreeRecorderComponent);
			if (options.Voxels) recordersToRemove.Add(VoxelRecorderComponent);
			if (options.Breakables) recordersToRemove.Add(RockBaseRecorderComponent);

			int foliageCount = 0, treeCount = 0, voxelCount = 0, breakableCount = 0, deepOreCount = 0, deepIceCount = 0;

			List<FProperty> newBlobs = new();
			foreach (FProperty prop in stateRecorderBlobs.Value)
			{
				PropertiesStruct? structData = (PropertiesStruct?)prop.Value;
				if (structData == null)
				{
					mErrorLog.WriteLine("[Error] Failed to read prospect. A state recorder component has an invalid value.");
					return;
				}

				StrProperty? nameProp = (StrProperty?)structData.Properties[0].Property;
				if (nameProp?.Value == null)
				{
					mErrorLog.WriteLine("[Error] Failed to read prospect. A state recorder component has an invalid or missing name.");
					return;
				}

				if (recordersToRemove.Contains(nameProp.Value))
				{
					switch (nameProp.Value)
					{
						case TreeRecorderComponent:
							++treeCount;
							break;
						case VoxelRecorderComponent:
							++voxelCount;
							break;
						case RockBaseRecorderComponent:
							++breakableCount;
							break;
					}
				}
				else
				{
					bool shouldKeep = true;

					if (options.Foliage && nameProp.Value == "/Script/Icarus.FLODTileRecorderComponent")
					{
						ResetFoliage(structData, ref foliageCount);
					}
					else if (nameProp.Value == "/Script/Icarus.ResourceDepositRecorderComponent")
					{
						ResourceDepositType depositType;
						shouldKeep = ShouldKeepResourceDeposit(structData, options, out depositType);

						if (!shouldKeep)
						{
							switch (depositType)
							{
								case ResourceDepositType.Ore:
									++deepOreCount;
									break;
								case ResourceDepositType.Ice:
									++deepIceCount;
									break;
							}
						}
					}

					if (shouldKeep)
					{
						newBlobs.Add(prop);
					}
				}
			}

			stateRecorderBlobs.Value = newBlobs.ToArray();

			mOutputLog.WriteLine("Resources reset/respawned:");
			if (options.Foliage)
			{
				mOutputLog.WriteLine($"  {foliageCount} foliage");
			}
			if (options.Trees)
			{
				mOutputLog.WriteLine($"  {treeCount} trees");
			}
			if (options.Voxels)
			{
				mOutputLog.WriteLine($"  {voxelCount} voxels");
			}
			if (options.Breakables)
			{
				mOutputLog.WriteLine($"  {breakableCount} breakables");
			}
			if (options.DeepOre)
			{
				mOutputLog.WriteLine($"  {deepOreCount} deep ore deposits");
			}
			if (options.DeepIce)
			{
				mOutputLog.WriteLine($"  {deepIceCount} super cooled ice deposits");
			}
		}

		private void ResetFoliage(PropertiesStruct structData, ref int removedCount)
		{
			const string FoliageRecordWarning = "[Warning] Could not read a foliage recorder record. This might prevent some foliage resources from being respawned.";

			IList<FPropertyTag> recorderProperties = ProspectSerlializationUtil.DeserializeRecorderData(structData.Properties[1]);

			foreach (FPropertyTag recorderProp in recorderProperties)
			{
				if (recorderProp.Name != "Record") continue;

				PropertiesStruct? recordStruct = (PropertiesStruct?)recorderProp.Property?.Value;
				if (recordStruct == null)
				{
					mWarningLog.WriteLine(FoliageRecordWarning);
					continue;
				}

				foreach (FPropertyTag recordArrayProp in recordStruct.Properties)
				{
					if (recordArrayProp.Name != "Records") continue;

					ArrayProperty recordArray = (ArrayProperty)recordArrayProp.Property!;
					if (recordArray.Value == null)
					{
						mWarningLog.WriteLine(FoliageRecordWarning);
						continue;
					}

					foreach (FProperty recordProp in recordArray.Value)
					{
						PropertiesStruct? recordPropertiesStruct = (PropertiesStruct?)recordProp.Value;
						if (recordPropertiesStruct == null)
						{
							mWarningLog.WriteLine(FoliageRecordWarning);
							continue;
						}

						foreach (FPropertyTag recordDataProp in recordPropertiesStruct.Properties)
						{
							if (recordDataProp.Name != "DestroyedInstanceIndices") continue;

							removedCount += ((ArrayProperty)recordDataProp.Property!).Value?.Length ?? 0;
							recordDataProp.Property!.Value = Array.Empty<FProperty>();

							break;
						}
					}
					break;
				}
				break;
			}

			structData.Properties[1] = ProspectSerlializationUtil.SerializeRecorderData(structData.Properties[1], recorderProperties);
		}

		private bool ShouldKeepResourceDeposit(PropertiesStruct structData, RespawnOptions options, out ResourceDepositType depositType)
		{
			if (!options.DeepOre && !options.DeepIce)
			{
				depositType = ResourceDepositType.Unknown;
				return true;
			}

			IList<FPropertyTag> recorderProperties = ProspectSerlializationUtil.DeserializeRecorderData(structData.Properties[1]);

			// TODO: Update StrProperty to NameProperty when upgrading to newer version of UeSaveGame
			StrProperty? resourceTypeProp = recorderProperties.FirstOrDefault(p => p.Name.Value == "ResourceDTKey")?.Property as StrProperty;

			if (resourceTypeProp is null)
			{
				// Should not happen, but leave it alone if we don't understand it
				mWarningLog.WriteLine("Encountered a resource deposit with no type. This deposit will not be removed.");

				depositType = ResourceDepositType.Unknown;
				return true;
			}

			string resourceType = resourceTypeProp.Value!.Value;

			if (resourceType.Equals("Exotic", StringComparison.OrdinalIgnoreCase) ||
				resourceType.Equals("Exotic_Red_Raw", StringComparison.OrdinalIgnoreCase))
			{
				// Never remove exotic deposits
				depositType = ResourceDepositType.Exotic;
				return true;
			}

			if (resourceType.Equals("Super_Cooled_Ice", StringComparison.OrdinalIgnoreCase))
			{
				depositType = ResourceDepositType.Ice;
				return !options.DeepIce;
			}

			depositType = ResourceDepositType.Ore;
			return !options.DeepOre;
		}

		private enum ResourceDepositType
		{
			Unknown,
			Ore,
			Ice,
			Exotic
		}
	}
}
