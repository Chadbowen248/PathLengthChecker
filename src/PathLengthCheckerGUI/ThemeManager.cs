using System.Windows;
using System.Windows.Media;

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Swaps application resource brushes for light/dark display.
	/// </summary>
	public static class ThemeManager
	{
		public static void Apply(bool darkMode)
		{
			var app = Application.Current;
			if (app == null) return;

			if (darkMode)
			{
				SetBrush(app, "AppBackground", "#0F172A");
				SetBrush(app, "CardBackground", "#1E293B");
				SetBrush(app, "AccentBrush", "#3B82F6");
				SetBrush(app, "AccentBrushHover", "#60A5FA");
				SetBrush(app, "MutedText", "#94A3B8");
				SetBrush(app, "BorderBrushSoft", "#334155");
				SetBrush(app, "PrimaryText", "#F1F5F9");
				SetBrush(app, "InputBackground", "#0B1220");
				SetBrush(app, "InputForeground", "#F1F5F9");
				SetBrush(app, "GridBackground", "#1E293B");
				SetBrush(app, "GridAltRow", "#162032");
				SetBrush(app, "SecondaryButtonBackground", "#1E293B");
				SetBrush(app, "SecondaryButtonForeground", "#F1F5F9");
			}
			else
			{
				SetBrush(app, "AppBackground", "#F3F5F9");
				SetBrush(app, "CardBackground", "#FFFFFF");
				SetBrush(app, "AccentBrush", "#2563EB");
				SetBrush(app, "AccentBrushHover", "#1D4ED8");
				SetBrush(app, "MutedText", "#64748B");
				SetBrush(app, "BorderBrushSoft", "#E2E8F0");
				SetBrush(app, "PrimaryText", "#0F172A");
				SetBrush(app, "InputBackground", "#FFFFFF");
				SetBrush(app, "InputForeground", "#0F172A");
				SetBrush(app, "GridBackground", "#FFFFFF");
				SetBrush(app, "GridAltRow", "#F8FAFC");
				SetBrush(app, "SecondaryButtonBackground", "#FFFFFF");
				SetBrush(app, "SecondaryButtonForeground", "#0F172A");
			}
		}

		private static void SetBrush(Application app, string key, string hex)
		{
			var color = (Color)ColorConverter.ConvertFromString(hex)!;
			app.Resources[key] = new SolidColorBrush(color);
		}
	}
}
