using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chronos
{
	class AddonDetector : AssetPostprocessor
	{
		const string PluginName = "Chronos";
		const string DefinePrefix = "CHRONOS_";

		static readonly Addon[] addons = 
		{
			new Addon()
			{
				name = "PlayMaker",
				define = "PLAYMAKER",
				filePattern = "PlayMakerMainMenu.cs"
			}
		};

		static void CheckForAddons(bool display)
		{
			int foundCount = 0;

			foreach (Addon addon in addons)
			{
				if (addon.Check(display))
				{
					foundCount++;
				}
			}

			if (display)
			{
				Debug.LogFormat("{0}: Addon check complete. {1} / {2} addons found.\n", PluginName, foundCount, addons.Length);
			}
		}

		// Automatic check for addons
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			CheckForAddons(false);
		}

		[MenuItem("Assets/" + PluginName + "/Check for addons...")]
		public static void ManualCheckForAddons()
		{
			CheckForAddons(true);
		}

		public class Addon
		{
			public string name { get; set; }
			public string define { get; set; }
			public string filePattern { get; set; }

			public bool Check(bool display)
			{
				if (Directory.GetFiles(Application.dataPath, filePattern, SearchOption.AllDirectories).Any())
				{
					bool added = AddDefine();

					if (added)
					{
						Debug.LogFormat("{0}: Enabled {1} addon.\n", PluginName, name);
					}
					else if (display)
					{
						Debug.LogFormat("{0}: {1} addon is enabled.\n", PluginName, name);
					}

					return true;
				}
				else
				{
					bool removed = RemoveDefine();

					if (removed)
					{
						Debug.LogFormat("{0}: Disabled {1} addon.\n", PluginName, name);
					}
					else if (display)
					{
						Debug.LogFormat("{0}: {1} addon is disabled.\n", PluginName, name);
					}

					return false;
				}
			}

			// Adapted from Demigiant's awesome voodoo.
			// He makes some rad stuff, have a look! http://demigiant.com/ 

			public bool AddDefine()
			{
				bool added = false;

				string define = DefinePrefix + this.define;

				foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
				{
					if (group == BuildTargetGroup.Unknown)
					{
						continue;
					}

					if (typeof(BuildTargetGroup).GetField(group.ToString()).IsDefined(typeof(ObsoleteAttribute), true))
					{
						continue;
					}

					List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

					if (!defines.Contains(define))
					{
						added = true;
						defines.Add(define);
						PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
					}
				}

				return added;
			}

			public bool RemoveDefine()
			{
				bool removed = false;

				string define = DefinePrefix + this.define;

				foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
				{
					if (group == BuildTargetGroup.Unknown)
					{
						continue;
					}

					if (typeof(BuildTargetGroup).GetField(group.ToString()).IsDefined(typeof(ObsoleteAttribute), true))
					{
						continue;
					}

					List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

					if (defines.Contains(define))
					{
						removed = true;
						defines.Remove(define);
						PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
					}
				}

				return removed;
			}
		}
	}
}
