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
            return ColorText("#fffffffe", text);
        }
        public static string Black(string text)
        {
            return ColorText("#000000", text);
        }
        public static string Gray(string text)
        {
            return ColorText("#404040", text);
        }
        public static string Orange(string text)
        {
            return ColorText("#c98332", text);
        }
        public static string Yellow(string text)
        {
            return ColorText("#cfc14a", text);
        }
        public static string Green(string text)
        {
            return ColorText("#56ad3b", text);
        }
        public static string Teal(string text)
        {
            return ColorText("#3b8dad", text);
        }
        public static string Blue(string text)
        {
            return ColorText("#3444a8", text);
        }
        public static string Purple(string text)
        {
            return ColorText("#8b3691", text);
        }
        public static string Pink(string text)
        {
            return ColorText("#b53c8ffe", text);
        }
        public static string Red(string text)
        {
            return ColorText("#ff0000", text);
        }
        public static string SoftRed(string text)
        {
            return ColorText("#b53c40", text);
        }
    }
}
