namespace RPGMods.Utils
{
    public class Color
    {
        private static string ColorText(string color, string text)
        {
            return $"<color={color}>" + text + "</color>";
        }

        public static string White(string text)
        {
            return ColorText("#ffffffff", text);
        }
        public static string Black(string text)
        {
            return ColorText("#000000ff", text);
        }
        public static string Gray(string text)
        {
            return ColorText("#404040ff", text);
        }
        public static string Orange(string text)
        {
            return ColorText("#c98332ff", text);
        }
        public static string Yellow(string text)
        {
            return ColorText("#cfc14aff", text);
        }
        public static string Green(string text)
        {
            return ColorText("#56ad3bff", text);
        }
        public static string Teal(string text)
        {
            return ColorText("#3b8dadff", text);
        }
        public static string Blue(string text)
        {
            return ColorText("#3444a8ff", text);
        }
        public static string Purple(string text)
        {
            return ColorText("#8b3691ff", text);
        }
        public static string Pink(string text)
        {
            return ColorText("#b53c8fff", text);
        }
        public static string Red(string text)
        {
            return ColorText("#ff0000ff", text);
        }
        public static string SoftRed(string text)
        {
            return ColorText("#b53c40ff", text);
        }
    }
}
