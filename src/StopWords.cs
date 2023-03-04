using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BibleTagging
{
    public class StopWords
    {
        public static string[] English = new string[]{
          "a", "an", "and", "are", "as", "at", "be", "but", "by",
          "for", "if", "in", "into", "is", "it",
          "no", "not", "of", "on", "or", "such",
          "that", "the", "their", "then", "there", "these",
          "they", "this", "to", "was", "will", "with",
          ",", ":", ".", "‘", ".’", "(", ").", "?", ",’",
          "‘“", ";", ".”", "’", "“", "!", "?’", "–", ",”",
          "!”", "”;", "”", "?”", "!’", "”?", "’;", ")", "[",
          "]", "”,", ");", "),", ".)", "”.", "[[", "]]",
          "”’", "’”", "):", "?)", "‘‘",
      };
    }
}
