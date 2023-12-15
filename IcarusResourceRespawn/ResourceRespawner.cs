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
using UeSaveGame.PropertyTypes;
using UeSaveGame.StructData;
using UeSaveGame;

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
			ArrayProperty? stateRecorderBlobs = prospect.ProspectData[0] as ArrayProperty;
			if (stateRecorderBlobs?.Value == null)
			{
				mErrorLog.Write("Error reading prospect. Failed to locate state recorder array at index 0.");
				return;
			}

			mOutputLog.WriteLine("Modifying prospect...");

			HashSet<string> recordersToRemove = new();
			if (options.Trees) recordersToRemove.Add("/Script/Icarus.TreeRecorderComponent");
			if (options.Voxels) recordersToRemove.Add("/Script/Icarus.VoxelRecorderComponent");
			if (options.Breakables) recordersToRemove.Add("/Script/Icarus.RockBaseRecorderComponent");

			List<UProperty> newBlobs = new();
			foreach (UProperty prop in stateRecorderBlobs.Value)
			{
				PropertiesStruct? structData = (PropertiesStruct?)prop.Value;
				if (structData == null)
				{
					mErrorLog.WriteLine("[Error] Failed to read prospect. A state recorder component has an invalid value.");
					return;
				}

				StrProperty? nameProp = (StrProperty?)structData.Properties[0];
				if (nameProp?.Value == null)
				{
					mErrorLog.WriteLine("[Error] Failed to read prospect. A state recorder component has an invalid or missing name.");
					return;
				}

				if (!recordersToRemove.Contains(nameProp.Value))
				{
					bool shouldKeep = true;

					if (options.Foliage && nameProp.Value == "/Script/Icarus.FLODTileRecorderComponent")
					{
						ResetFoliage(structData);
					}
					else if (options.DeepOre && nameProp.Value == "/Script/Icarus.ResourceDepositRecorderComponent")
					{
						if (!IsExoticDeposit(structData))
						{
							shouldKeep = false;
						}
					}

					if (shouldKeep)
					{
						newBlobs.Add(prop);
					}
				}
			}

			stateRecorderBlobs.Value = newBlobs.ToArray();
		}

		private void ResetFoliage(PropertiesStruct structData)
		{
			const string FoliageRecordWarning = "[Warning] Could not read a foliage recorder record. This might prevent some foliage resources from being respawned.";

			IList<UProperty> recorderProperties = ProspectSerlializationUtil.DeserializeRecorderData(structData.Properties[1]);

			foreach (UProperty recorderProp in recorderProperties)
			{
				if (recorderProp.Name != "Record") continue;

				PropertiesStruct? recordStruct = (PropertiesStruct?)recorderProp.Value;
				if (recordStruct == null)
				{
					mWarningLog.WriteLine(FoliageRecordWarning);
					continue;
				}

				foreach (UProperty recordArrayProp in recordStruct.Properties)
				{
					if (recordArrayProp.Name != "Records") continue;

					ArrayProperty recordArray = (ArrayProperty)recordArrayProp;
					if (recordArray.Value == null)
					{
						mWarningLog.WriteLine(FoliageRecordWarning);
						continue;
					}

					foreach (UProperty recordProp in recordArray.Value)
					{
						PropertiesStruct? recordPropertiesStruct = (PropertiesStruct?)recordProp.Value;
						if (recordPropertiesStruct == null)
						{
							mWarningLog.WriteLine(FoliageRecordWarning);
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

		private static bool IsExoticDeposit(PropertiesStruct structData)
		{
			IList<UProperty> recorderProperties = ProspectSerlializationUtil.DeserializeRecorderData(structData.Properties[1]);

			// TODO: Update StrProperty to NameProperty when upgrading to newer version of UeSaveGame
			StrProperty? resourceTypeProp = recorderProperties.FirstOrDefault(p => p.Name.Value == "ResourceDTKey") as StrProperty;

			return resourceTypeProp is not null && resourceTypeProp.Value!.Value == "Exotic";
		}
	}
}
