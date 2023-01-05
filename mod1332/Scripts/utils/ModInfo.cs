namespace cynofield.mods.utils
{
    public class ModInfo
    {
        public static ModInfo Instance;

        public string name;
        public string version;
        public string longName;

        public override string ToString()
        {
            var n = name;
            if (!string.IsNullOrWhiteSpace(longName))
                n = longName;
            return $"{n} v{version}";
        }
    }
}
