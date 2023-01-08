using System.Collections.Generic;

namespace cynofield.mods.utils
{
    public class TagParser
    {
        public string WithoutTokens(string str)
        {
            string[] tokens = Tokenize(str, '#');
            if (tokens == null)
                return str;

            foreach (var token in tokens)
            {
                str = str.Replace(token, "");
            }
            return str.Trim();
        }

        public List<Tag> Parse(string str)
        {
            if (str == null)
                return null;
            if (!str.Contains("#"))
                return null;

            string[] tokens = Tokenize(str, '#');
            if (tokens == null || tokens.Length <= 0)
                return null;

            var result = new List<Tag>();
            foreach (var token in tokens)
            {
                var (name, paramsStr) = SplitTokenParams(token, '-', '_');
                string[] paramz = null;
                if (paramsStr != null)
                    paramz = Tokenize("-" + paramsStr, '-', '_');

                int[] paramsInt = null;
                string joinedParams = null;
                if (paramz != null)
                {
                    paramz = RemovePrefix(paramz);
                    joinedParams = string.Join(", ", paramz);
                    paramsInt = ParamsToInt(paramz);
                }
                result.Add(new Tag { name = name, joinedParams = joinedParams, paramsString = paramz, paramsInt = paramsInt });
            }

            if (result.Count == 0)
                return null;

            return result;
        }

        private string[] RemovePrefix(string[] paramz)
        {
            if (paramz == null)
                return null;

            for (int i = 0; i < paramz.Length; i++)
            {
                string p = paramz[i];
                for (int j = 0; j < p.Length; j++)
                {
                    char ch = p[j];
                    if (char.IsLetterOrDigit(ch))
                    {
                        paramz[i] = p.Substring(j);
                        break;
                    }
                }
            }
            return paramz;
        }

        private int[] ParamsToInt(string[] paramz)
        {
            var result = new List<int>();
            foreach (var p in paramz)
            {
                if (int.TryParse(p, out int i))
                {
                    result.Add(i);
                }
            }

            if (result.Count == 0)
                return null;
            return result.ToArray();
        }

        private (string, string) SplitTokenParams(string str, char delim1, char delim2)
        {
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (ch == delim1 || ch == delim2 || char.IsDigit(ch))
                {
                    return (str.Substring(0, i), str.Substring(i));
                }
            }
            return (str, null);
        }

        private string[] Tokenize(string str, char delim1, char delim2 = '\0')
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            var result = new List<string>();
            int tokenStart = -1;
            int tokenEnd = -1;
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (tokenStart < 0)
                {
                    if (ch == delim1 || ch == delim2)
                        tokenStart = i;
                }
                else if (tokenStart >= 0 && tokenEnd < 0)
                {
                    if (ch == delim1 || ch == delim2)
                        tokenStart = i;
                    else if (ch == ' ')
                        tokenStart = -1;
                    else
                    {
                        tokenEnd = i;
                        if (i == str.Length - 1)
                        {
                            var token = str.Substring(tokenStart, i - tokenStart + 1);
                            if (!string.IsNullOrWhiteSpace(token))
                                result.Add(token);

                            tokenStart = -1;
                            tokenEnd = -1;
                        }
                    }
                }
                else if (tokenStart >= 0 && tokenEnd >= 0)
                {
                    if (ch == delim1 || ch == delim2)
                    {
                        var token = str.Substring(tokenStart, tokenEnd - tokenStart + 1);
                        if (!string.IsNullOrWhiteSpace(token))
                            result.Add(token);

                        tokenStart = i;
                        tokenEnd = -1;
                    }
                    else if (ch == ' ')
                    {
                        var token = str.Substring(tokenStart, tokenEnd - tokenStart + 1);
                        if (!string.IsNullOrWhiteSpace(token))
                            result.Add(token);

                        tokenStart = -1;
                        tokenEnd = -1;
                    }
                    else if (i == str.Length - 1)
                    {
                        var token = str.Substring(tokenStart, tokenEnd - tokenStart + 2);
                        if (!string.IsNullOrWhiteSpace(token))
                            result.Add(token);

                        tokenStart = -1;
                        tokenEnd = -1;
                    }
                    else
                    {
                        tokenEnd = i;
                    }
                }
            }

            if (result.Count == 0)
                return null;

            return result.ToArray();
        }

        public class Tag
        {
            public string name;
            public string joinedParams;
            public string[] paramsString;
            public int[] paramsInt;

            public override bool Equals(object obj)
            {
                return obj is Tag tag &&
                       name == tag.name &&
                       joinedParams == tag.joinedParams;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                if (name != null)
                    hash = hash * 23 + name.GetHashCode();
                if (joinedParams != null)
                    hash = hash * 23 + joinedParams.GetHashCode();
                return hash;
            }

            public override string ToString()
            {
                if (joinedParams == null)
                    return name;

                return name + $"({joinedParams})";
            }
        }
    }
}
