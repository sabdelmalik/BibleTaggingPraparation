using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;

namespace BibleTagging
{
    public class StopWords
    {
        public static Dictionary<string, string> EnglishOT1= new Dictionary<string, string>();
        public static Dictionary<string, string> EnglishOT2 = new Dictionary<string, string>();
        public static Dictionary<string, string> EnglishNT1 = new Dictionary<string, string>();
        public static Dictionary<string, string> EnglishNT2 = new Dictionary<string, string>();


        private static string[] ConjoinedNT = new string[]
        {
            "G1063=γάρ=for",
            "G1161=δέ=then",
            "G2228=ἤ=or",
            "G2504=κἀγώ=and I",
            "G2532=καί=and",
            "G2534=καίγε=even/even though",
            "G3761=οὐδέ=nor",
            "G5037=τε=and/both",
            "G0846=αὐτός=he/she/it/self/",
            "G1473=ἐγώ=I/we",
            "G4771=σύ=you",
            "G2251=ἡμέτερος=our",
            "G1700=ἐμοῦ=of me",
            "G1699=ἐμός=mine",
            "G4674=σός=your",
            "G1438=ἑαυτοῦ=my/your/him-self",
            "G4572=σεαυτοῦ=yourself",
            "G1683=ἐμαυτοῦ=myself",
            "G3588=ὁ=the/this/who",
            "G3778=οὗτος=this/he/she/it",
            "G1565=ἐκεῖνος=that",
            "G3739=ὅς, ἥ=which",
            "G5100=τις=one",
            "G5101=τίς=which?",
            "G3748=ὅστις=who/which",
            "G3754=ὅτι=that/since",
            "G3756=οὐ=no",
            "G3361=μή=not",
            "G3777=οὔτε=neither",
            "G3366=μηδέ=nor",
            "G3780=οὐχί=not",
            "G3383=μήτε=neither",
            "G3768=οὔπω=not yet",
            "G3371=μηκέτι=never again",
            "G1302=διατί=why?",
            "G4219=πότε=when?",
            "G4459=πῶς=how?!",
        };

        private static string[] ConjoinedOT = new string[]
        {
            "H0116=אֱדַ֫יִן=then",
            "H0176A=אוֹ=or",
            "H0227A=אָז=then",
            "H0638=אַף=also",
            "H1571=גַּם=also",
            "H3588A=כִּי=for",
            "H3651C=כֵּן=so",
            "H3652=כֵּן=thus",
            "H0637=אַף=also",
            "H0834A=אֲשֶׁר=which",
            "H2007=הֵנָּה=they(fem.)",
            "H7945=שֶׁל=which",
            "H0411=אֵל=these",
            "H0412=אֵל=these",
            "H0428=אֵ֫לֶּה=these",
            "H0429=אֵלֶּה=these",
            "H0459=אִלֵּין=these",
            "H0479=אִלֵּךְ=these",
            "H0581A=אִנּוּן=they",
            "H0581B=אִנִּין=they(fem.)",
            "H1454=גֵּה=this",
            "H1668=דָּא=this",
            "H1791=דֵּךְ=this",
            "H1797=דִּכֵּן=this",
            "H1836=דְּנָה=this",
            "H1975=הַלָּז=this",
            "H1976=הַלָּזֶה=this",
            "H1977=הַלֵּ֫זוּ=this",
            "H1992=הֵ֫מָּה=they(masc.)",
            "H1994=הִמּוֹ=they",
            "H2063=זֹאת=this",
            "H2088=זֶה=this",
            "H2090=זֹה=this",
            "H2097=זוֹ=this",
            "H2098=זוּ=this",
            "H6422=פַּלְמוֹנִי=certain",
            "H0335=אַי=where?",
            "H0349A=אֵיךְ=how?",
            "H0349B=אֵיכָה=how?",
            "H0349C=אֵיכָ֫כָה=how?",
            "H0370=אַ֫יִן=where?",
            "H0371=אִין=isn't?",
            "H0375=אֵיפֹה=where?",
            "H0575=אָן=where?",
            "H1963=הֵיךְ=how?",
            "H4069=מַדּוּעַ=why?",
            "H4100=מָה=what?",
            "H4101=מָה=what?",
            "H4310=מִי=who?",
            "H4478B=מָן=What?",
            "H4479=מָן=who?",
            "H4970=מָתַי=how",
            "H0336=אִי=not",
            "H0369=אַ֫יִן=nothing",
            "H0408=אַל=not",
            "H0409=אַל=not",
            "H1077=בַּל=not",
            "H1097=בְּלִי=without",
            "H3808=לֹא=not",
            "H3809=לָא=not",
            "H0383=אִיתַי=there is",
            "H0645=אֵפוֹ=then",
            "H0853=אֵת=[Obj.]",
            "H0994=בִּי=please",
            "H1768=דִּי=that",
            "H3487=יָת=whom",
            "H3964=מָא=what",
            "H5705=עַד=till",
            "H0580=אֲנוּ=we",
            "H0586=אֲנַ֫חְנָא=we",
            "H0587=אֲנַ֫חְנוּ=we",
            "H5168=נַ֫חְנוּ=we",
            "H0576A=אֲנָא=me",
            "H0576B=אֲנָה=me",
            "H0589=אֲנִי=I",
            "H0595=אָֽנֹכִ֫י=I",
            "H1931=הוּא=he/she/it",
            "H1932=הוּא=he/she/it",
            "H0859E=אַתֵּן=you(f.p.)",
            "H2004=הֵן=they(fem.)",
            "H0859B=אתִּי=you(f.s.)",
            "H0859C=אַתְּ=you(f.s.)",
            "H0608=אַנְתּוּן=you",
            "H0859D=אַתֶּם=you(m.p.)",
            "H0607=אַנְתָּה=you",
            "H0859A=אַתָּ֫ה=you(m.s.)",
            "H0413=אֶל=to(wards)",
            "H0854=אֵת=with",
            "H0997=בֵּין=between",
            "H1119=בְּמוֹ=in/at/by",
            "H3890=לְוָת=with",
            "H3926=לְמוֹ=upon",
            "H3942=לִפְנָ֑י=before",
            "H4480A=מִן־=from",
            "H4481=מִן־=from",
            "H5049=נֶ֫גֶד=before",
            "H5921A=עַל=upon",
            "H5921B=כִּי עַל כֵּן=upon",
            "H5924=עֵ֫לָּא=above",
            "H5973A=עִם=with",
            "H5973B=מֵעִם=from with",
            "H5974=עִם=with",
            "H5978=עִמָּדִי=with me",
            "H6441=פְּנִ֫ימָה=within",
            "H6903G=קֳבֵל=before",
            "H6905H=קָבָל=before",
            "H6925=קֳדָם=before",
            "H8460=תְּחוֹת=under",
            "H8479=תַּחַת=under",
            "H0834B=בַאֲשֶׁר=in which",
            "H0834C=מֵאֲשֶׁר=whence",
            "H0834D=כַּאֲשֶׁר=as which",
        };


        private static string[] commonEnglish = new string[]{
/*"for", "then","or","and i","and","even", "even though",
"nor","and", "both","he", "she", "it", "self","i", "we",
"you","our","of me","mine","your","my", "your", "him-self",
"yourself","myself","the", "this", "who","this", "he", "she", "it",
"that","which","one","which?","who", "which","that", "since",
"why","when","how","no","not","neither","nor","not","neither",
"not yet","never again",*/

          "a", "an", "and", "are", "as", "at", "be", "but", "by",
          "for", "if", "in", "into", "is", "it",
          "no", "not", "of", "on", "or", "such",
          "that", "the", /*"their",*/ "then", "there", /*"these",*/
          "they", "this", "to", "was", "will", "with",
          ",", ":", ".", "‘", ".’", "(", ").", "?", ",’",
          "‘“", ";", ".”", "’", "“", "!", "?’", "–", ",”",
          "!”", "”;", "”", "?”", "!’", "”?", "’;", ")", "[",
          "]", "”,", ");", "),", ".)", "”.", "[[", "]]",
          "”’", "’”", "):", "?)", "‘‘",
      };

        public static void PopulateStopDictionaries(BibleTaggingPreperationForm form)
        {
            PopulateStopDictionaries(ConjoinedOT, EnglishOT1, EnglishOT2, form);
            PopulateStopDictionaries(ConjoinedNT, EnglishNT1, EnglishNT2, form);
        }

        private static void PopulateStopDictionaries(string[] Conjoined,
            Dictionary<string, string> single_words, 
            Dictionary<string, string> multipleWords,
            BibleTaggingPreperationForm form)
        {
            foreach (string s in Conjoined)
            {
                Match match = Regex.Match(s, @"([GH][0-9]{4})=(.+)=((\/?([a-zA-Z -]+))*$)"); //(\/?([a-zA-Z -]+))*");

                if (match.Success)
                {
                    string strongs = match.Groups[1].Value.Replace("G", "").Replace("H","");                    string text = match.Groups[3].Value;
                    if (string.IsNullOrEmpty(text))
                    {
                        form.TraceError(MethodBase.GetCurrentMethod().Name, "Text was null or empty");
                        continue;
                    }
                    string[] strings = text.Split(new char[] { '/' });
                    foreach(string s2 in strings)
                    {
                        string w = s2.Trim().ToLower().Replace("?", "").Replace("!", "");
                        if(w.Contains(' '))
                        {
                            if (!multipleWords.ContainsKey(w))
                                multipleWords.Add(w, strongs);
                        }
                        else
                        {
                            if (!single_words.ContainsKey(w))
                                single_words.Add(w, strongs);
                            if (!single_words.ContainsKey("‘"+w))
                                single_words.Add("‘" + w, strongs);

                        }
                    }
                }
            }
            string sp = string.Empty;
            foreach(string s3 in commonEnglish)
            {
                if (!single_words.ContainsKey(s3.Trim()))
                {
                    single_words.Add(s3.Trim(), string.Empty);
                    sp += "\"" + s3 + "\", ";
                }
            }
            form.Trace(sp, Color.Black);

        }
    }
}
