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

namespace IcarusResourceRespawn
{
	internal class RespawnOptions
	{
		public bool Foliage { get; }

		public bool Voxels { get; }

		public bool Breakables { get; }

		public bool DeepOre { get; }

		public bool DeepIce { get; }

		public const int MaxOptionStringLength = 15; // Length of "-breakables"

		public RespawnOptions(bool foliage, bool voxels, bool breakables, bool deepOre, bool deepIce)
		{
			Foliage = foliage;
			Voxels = voxels;
			Breakables = breakables;
			DeepOre = deepOre;
			DeepIce = deepIce;
		}

		public static void PrintCommandLineOptions(TextWriter writer, string indent)
		{
			writer.WriteLine($"{indent}-f, -foliage     Respawn trees, bushes, berries, etc.\n");
			writer.WriteLine($"{indent}-v, -voxels      Respawn minable ores and rocks\n");
			writer.WriteLine($"{indent}-b, -breakables  Respawn obsidian, clay, scoria\n");
			writer.WriteLine($"{indent}-d, -deepore     Remove deep ore deposits so they respawn, possibly with different ore types (may");
			writer.WriteLine($"{indent}                 disconnect existing drills)\n");
			writer.WriteLine($"{indent}-i, -deepice     Remove super cooled ice deposits so they respawn (may disconnect existing borers)");
		}

		public static RespawnOptions ParseCommandLine(IReadOnlyList<string> commandLine, out IReadOnlyList<string> remainingCommandLine)
		{
			List<string> remaining = new();

			bool foliage = false, voxels = false, breakables = false, deepOre = false, deepIce = false;

			for (int i = 0; i < commandLine.Count; ++i)
			{
				if (!commandLine[i].StartsWith('-'))
				{
					remaining.Add(commandLine[i]);
					continue;
				}

				string input = commandLine[i][1..].ToLowerInvariant();

				switch (input)
				{
					case "f":
					case "foliage":
						foliage = true;
						break;
					case "v":
					case "voxels":
						voxels = true;
						break;
					case "b":
					case "breakables":
						breakables = true;
						break;
					case "d":
					case "deepore":
						deepOre = true;
						break;
					case "i":
					case "deepice":
						deepIce = true;
						break;
					default:
						remaining.Add(commandLine[i]);
						break;
				}
			}

			remainingCommandLine = remaining;
			return new RespawnOptions(foliage, voxels, breakables, deepOre, deepIce);
		}

		public bool Any()
		{
			return Foliage || Voxels || Breakables || DeepOre || DeepIce;
		}
	}
}
