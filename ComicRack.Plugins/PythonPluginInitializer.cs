using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using cYo.Common.IO;
using cYo.Common.Text;

namespace cYo.Projects.ComicRack.Plugins
{
	public class PythonPluginInitializer : PluginInitializer
	{
		private static readonly Regex rxComment = new Regex("#\\s*@(?<name>[A-Za-z][\\w_]*)\\s+(?<value>.*)", RegexOptions.Compiled);

		private static readonly Regex rxFunction = new Regex("def\\s+(?<function>[A-Za-z][\\w_]+)", RegexOptions.Compiled);

		public override IEnumerable<Command> GetCommands(string file)
		{
			List<Command> commands = new List<Command>();
			if (".py".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					string[] lines = File.ReadAllLines(file);
					string name = null, key = null, image = null, description = null, hook = null;
					int pcount = 0;
					bool enabled = true;

					foreach (string line in lines)
					{
						Match match = rxComment.Match(line);
						if (match.Success)
						{
							string propertyName = match.Groups["name"].Value.ToLower();
							string propertyValue = match.Groups["value"].Value.Trim();

							switch (propertyName)
							{
								case "name": name = propertyValue; break;
								case "key": key = propertyValue; break;
								case "image": image = propertyValue; break;
								case "description": description = propertyValue; break;
								case "hook": hook = propertyValue; break;
								case "pcount": int.TryParse(propertyValue, out pcount); break;
								case "enabled": bool.TryParse(propertyValue, out enabled); break;
							}
							continue;
						}

						match = rxFunction.Match(line);
						if (match.Success)
						{
							string functionName = match.Groups["function"].Value;
							if (!string.IsNullOrEmpty(hook))
							{
								commands.Add(new PythonCommand
								{
									Name = name ?? functionName,
									Key = key ?? functionName,
									Image = image,
									Description = description,
									Hook = hook,
									PCount = pcount,
									Enabled = enabled,
									ScriptFile = file,
									Method = functionName
								});
							}
							// Reset metadata for the next function, except possibly for some "global" ones if desired,
							// but usually it's one decorator per function.
							name = key = image = description = hook = null;
							pcount = 0;
							enabled = true;
						}
					}
				}
				catch (Exception ex)
				{
					LogManager.Debug("System", $"Failed to parse Python plugin metadata from {file}: {ex.Message}");
				}
			}
			return commands;
		}
	}
}
