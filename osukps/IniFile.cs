using System.Collections.Generic;
using System.IO;
using System.Text;

namespace osukps {
	class IniFile {
		private readonly string path;
		private readonly Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

		public IniFile(string path) {
			this.path = path;
			Load();
		}

		public string Read(string section, string key, string defaultValue) {
			Dictionary<string, string> values;
			string value;
			if (data.TryGetValue(section, out values) && values.TryGetValue(key, out value)) {
				return value;
			}
			return defaultValue;
		}

		public void Write(string section, string key, string value) {
			Dictionary<string, string> values;
			if (!data.TryGetValue(section, out values)) {
				values = new Dictionary<string, string>();
				data[section] = values;
			}
			values[key] = value;
		}

		public void Save() {
			using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8)) {
				foreach (KeyValuePair<string, Dictionary<string, string>> section in data) {
					writer.Write('[');
					writer.Write(section.Key);
					writer.WriteLine(']');
					foreach (KeyValuePair<string, string> item in section.Value) {
						writer.Write(item.Key);
						writer.Write('=');
						writer.WriteLine(item.Value ?? "");
					}
					writer.WriteLine();
				}
			}
		}

		private void Load() {
			if (!File.Exists(path)) {
				return;
			}

			string section = "";
			foreach (string rawLine in File.ReadAllLines(path)) {
				string line = rawLine.Trim();
				if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) {
					continue;
				}
				if (line.StartsWith("[") && line.EndsWith("]")) {
					section = line.Substring(1, line.Length - 2).Trim();
					if (!data.ContainsKey(section)) {
						data[section] = new Dictionary<string, string>();
					}
					continue;
				}

				int equals = line.IndexOf('=');
				if (equals < 0) {
					continue;
				}

				Dictionary<string, string> values;
				if (!data.TryGetValue(section, out values)) {
					values = new Dictionary<string, string>();
					data[section] = values;
				}
				values[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
			}
		}
	}
}
