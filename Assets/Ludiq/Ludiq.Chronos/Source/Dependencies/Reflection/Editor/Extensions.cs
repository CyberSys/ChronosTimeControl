using System;
using System.CodeDom;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace Chronos.Reflection.Editor
{
	public static class Extensions
	{
		// Used to print pretty type names for primitives
		private static CSharpCodeProvider csharp = new CSharpCodeProvider();

		/// <summary>
		/// Returns the name for the given type where primitives are in their shortcut form.
		/// </summary>
		public static string PrettyName(this Type type)
		{
			string cSharpOutput = csharp.GetTypeOutput(new CodeTypeReference(type));

			var matches = Regex.Matches(cSharpOutput, @"([a-zA-Z0-9_\.]+)");

			var prettyName = RemoveNamespace(matches[0].Value);

			if (matches.Count > 1)
			{
				prettyName += "<";

				prettyName += string.Join(", ", matches.Cast<Match>().Skip(1).Select(m => RemoveNamespace(m.Value)).ToArray());

				prettyName += ">";
			}

			return prettyName;
		}

		private static string RemoveNamespace(string typeFullName)
		{
			if (!typeFullName.Contains('.'))
			{
				return typeFullName;
			}

			return typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);
		}
	}
}