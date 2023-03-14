using BibleTagging.PorterStemmerAlgorithm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
//using static System.Net.Mime.MediaTypeNames;

namespace BibleTagging
{
    public class BerkeleyAligner
    {
        private BibleTaggingPreperationForm container;
        private const string alignerFolderName = "berkeleyaligner";
        private const string otMapFolder = "OT_Map";
        private const string ntMapFolder = "NT_Map";
        private const string textToAlignExtension = "t";
        private const string strongsTagsExtension = "s";
        private const string otTrainFolder = "OT_Dataset\\train";
        private const string ntTrainFolder = "NT_Dataset\\train";

        private string alignerFolderpath = string.Empty;

        private string otMapPath = string.Empty;
        private string ntMapPath = string.Empty;
        private string foreignSuffix = textToAlignExtension;
        private string englishSuffix = strongsTagsExtension;

        private string alignFileName = string.Empty;
        private string otTrainTextPath = string.Empty;
        private string hebrewTrainTagsPath = string.Empty;
        private string otAlignFilePath = string.Empty;

        private string ntTrainTextPath = string.Empty;
        private string greekTrainTagsPath = string.Empty;
        private string ntAlignFilePath = string.Empty;
        private string ntAlignFilePath1 = string.Empty;

        private const string referencePattern1 = @"^([0-9A-Za-z]+)\s([0-9]+):([0-9]+)\s*(.*)";
        private const string referencePattern2 = @"^([0-9A-Za-z]+)\.([0-9]+)\.([0-9]+)\s*(.*)";
        private string textReferencePattern = string.Empty;
        private string tagsReferencePattern = string.Empty;

        int totalTags = 0;
        int unmapped = 0;
        public BerkeleyAligner(BibleTaggingPreperationForm container)
        {
            this.container = container;

            string currentFolder = Application.StartupPath;
            alignerFolderpath = Path.Combine(currentFolder, alignerFolderName);

            alignFileName = string.Format("training.{0}-{1}.align", englishSuffix, foreignSuffix);
            otMapPath = Path.Combine(alignerFolderpath, otMapFolder);
            otAlignFilePath = Path.Combine(otMapPath, alignFileName);

            otTrainTextPath = Path.Combine(alignerFolderpath, otTrainFolder + "\\oldtestament." + textToAlignExtension);
            hebrewTrainTagsPath = Path.Combine(alignerFolderpath, otTrainFolder + "\\oldtestament." + strongsTagsExtension);

            ntMapPath = Path.Combine(alignerFolderpath, ntMapFolder);
            ntAlignFilePath = Path.Combine(ntMapPath, alignFileName);
            ntTrainTextPath = Path.Combine(alignerFolderpath, ntTrainFolder + "\\newtestament." + textToAlignExtension);
            greekTrainTagsPath = Path.Combine(alignerFolderpath, ntTrainFolder + "\\newtestament." + strongsTagsExtension);
        }

        public bool AlignOT()
        {
            // TODO: ensure that the aligner is prepared before running this
            bool result = true;

            var dir = new DirectoryInfo(otMapPath);
            if (dir.Exists) dir.Delete(true); // true => recursive delete

            RunBerkelyAligner("OT.conf");

            return result;
        }

        public bool AlignNT()
        {
            // TODO: ensure that the aligner is prepared before running this

            bool result = true;

            var dir = new DirectoryInfo(ntMapPath);
            if (dir.Exists) dir.Delete(true); // true => recursive delete

            RunBerkelyAligner("NT.conf");

            return result;
        }

        private void RunBerkelyAligner(string confFile)
        {
            try
            {
                //WaitCursorControl(true);
                // java -server -mx1000m -cp berkeleyaligner.jar edu.berkeley.nlp.wordAlignment.Main ++confs/NT.conf\r\n

                string executable = "java";

                Process process = new Process();
                process.StartInfo.FileName = executable;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = alignerFolderpath;


                process.StartInfo.Arguments = "-server -mx1000m -cp berkeleyaligner.jar edu.berkeley.nlp.wordAlignment.Main ++confs/" + confFile;
                process.Start();
                while (!process.HasExited) ;

                //MessageBox.Show("Bible Generation completed!");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Bible Generation Failed \r\n" + ex);
            }
            //WaitCursorControl(false);
        }

        /// <summary>
        /// Strips off the verse referenc for each verse (for both Bible text, and tags)
        /// Passes the verse text through the stemmer if one is passed.
        /// Strips off the stop words from the verse text.
        /// save the resulting verse text into Berkely Aliner's train folder.
        /// save the resulting tags line into Berkely Aliner's train folder.
        /// </summary>
        /// <param name="sourcePath">the OT Bible Text to be tagged</param>
        /// <param name="tagsPath">the tags file for the Bible text</param>
        /// <param name="stemmer">a stemmer, or null if not reqired</param>
        /// <param name="stopwords">list of stopwords or null if not required</param>
        public void PrepareOT(string sourcePath, string tagsPath, StemmerInterface stemmer, string[] stopwords)
        {
            using (StreamReader sr = new StreamReader(sourcePath))
            using (StreamWriter sw = new StreamWriter(otTrainTextPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) continue;

                    // remove reference
                    int space1 = line.IndexOf(' ');
                    if (space1 == -1) continue;
                    int space2 = line.IndexOf(" ", space1 + 1);
                    line = line.Substring(space2 + 1).Trim();

                    // stem if required
                    if (stemmer != null)
                    {
                        string[] parts = line.Split(new char[] { ' ' });
                        string stemmed = string.Empty;
                        for (int i = 0; i < parts.Length; i++)
                        {
                            stemmed += stemmer.stemTerm(parts[i]) + " ";
                        }
                        line = stemmed.Trim();
                    }
                    sw.WriteLine(line);
                }
            }

            using (StreamReader sr = new StreamReader(tagsPath))
            using (StreamWriter sw = new StreamWriter(hebrewTrainTagsPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) continue;

                    // remove reference
                    int space1 = line.IndexOf(' ');
                    if (space1 == -1) continue;
                    int space2 = line.IndexOf(" ", space1 + 1);
                    line = line.Substring(space2 + 1).Trim();

                    sw.WriteLine(line);
                }

            }
        }

        /// <summary>
        /// Strips off the verse referenc for each verse (for both Bible text, and tags)
        /// Passes the verse text through the stemmer if one is passed.
        /// Strips off the stop words from the verse text.
        /// save the resulting verse text into Berkely Aliner's train folder.
        /// save the resulting tags line into Berkely Aliner's train folder.
        /// </summary>
        /// <param name="sourcePath">the NT Bible Text to be tagged</param>
        /// <param name="tagsPath">the tags file for the Bible text</param>
        /// <param name="stemmer">a stemmer, or null if not reqired</param>
        /// <param name="stopwords">list of stopwords or null if not required</param>
        public void PrepareNT1(string sourcePath, string tagsPath, StemmerInterface stemmer, string[] stopwords)
        {
            using (StreamReader sr = new StreamReader(sourcePath))
            using (StreamWriter sw = new StreamWriter(ntTrainTextPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) continue;

                    // remove reference
                    int space1 = line.IndexOf(' ');
                    if (space1 == -1) continue;
                    int space2 = line.IndexOf(" ", space1 + 1);
                    line = line.Substring(space2 + 1).Trim();

                    // stem if required
                    if (stemmer != null || stopwords != null)
                    {
                        string[] parts = line.Split(new char[] { ' ' });

                        string processed = string.Empty;
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (stopwords != null && stopwords.Contains(parts[i].ToLower()))
                                continue;

                            if(stemmer!= null)
                                processed += stemmer.stemTerm(parts[i]) + " ";
                            else
                                processed += parts[i] + " ";
                        }
                        line = processed.Trim();
                    }
                    sw.WriteLine(line);
                }
            }

            using (StreamReader sr = new StreamReader(tagsPath))
            using (StreamWriter sw = new StreamWriter(greekTrainTagsPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) continue;

                    // remove reference
                    /*
                     * temporary workaround for NI tag from David
                     * here, we assume the references follow the format <book> <ch>:<ver> <text>
                     * In David's file it is <book>.<ch>.<ver>\t<text>
                    int space1 = line.IndexOf(' ');
                    if (space1 == -1) continue;
                    int space2 = line.IndexOf(" ", space1 + 1);
                    if (space2 == -1) continue;
                    line = line.Substring(space2 + 1).Trim();
                    */

                    int space1 = line.IndexOf('\t');
                    if (space1 == -1)
                        space1 = line.IndexOf(' ');
                    if (space1 == -1)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "Missing white space in:" + line);
                        continue;
                    }
                    
                    line = line.Substring(space1 + 1).Trim();

                    sw.WriteLine(line);
                }

            }
        }

        private bool SetSearchPattern(string line, out string referancePattern)
        {
            Match mTx = Regex.Match(line, referencePattern1);
            if (mTx.Success)
                referancePattern = referencePattern1;
            else
            {
                mTx = Regex.Match(line, referencePattern2);
                if (mTx.Success)
                    referancePattern = referencePattern2;
                else
                {
                    container.TraceError(MethodBase.GetCurrentMethod().Name, "Could not detect reference pattern: " + line);
                    referancePattern = string.Empty;
                    return false;
                }
            }

            return true;
        }
        public void PrepareNT(string sourcePath, string tagsPath, StemmerInterface stemmer, string[] stopwords)
        {
            using (StreamReader srText = new StreamReader(sourcePath ))
            using (StreamReader srTags = new StreamReader(tagsPath))
            using (StreamWriter swText = new StreamWriter(ntTrainTextPath))
            using (StreamWriter swTags = new StreamWriter(greekTrainTagsPath))
            {
                while (!srText.EndOfStream && !srTags.EndOfStream)
                {
                    var textLine = srText.ReadLine();
                    if (textLine == null)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "Empty line in " + tagsPath);
                        continue;
                    }

                    var tagsLine = srTags.ReadLine();
                    if (tagsLine == null)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "Empty line in " + sourcePath);
                        continue;
                    }

                    if (string.IsNullOrEmpty(textReferencePattern))
                    {
                        if (!SetSearchPattern(textLine, out textReferencePattern))
                            return;
                    }
                    if (string.IsNullOrEmpty(tagsReferencePattern))
                    {
                        if (!SetSearchPattern(tagsLine, out tagsReferencePattern))
                            return;
                    }
                    
                    Match mTx = Regex.Match(textLine, textReferencePattern);
                    if (!mTx.Success)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "Could not detect text reference: " + textLine);
                        return;
                    }
                    Match mTg = Regex.Match(tagsLine, tagsReferencePattern);
                    if (!mTx.Success)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "Could not detect tags reference: " + tagsLine);
                        return;
                    }

                    String textBook = mTx.Groups[1].Value;
                    String verseText = mTx.Groups[4].Value.ToLower();
                    String tagsBook = mTg.Groups[1].Value;
                    String tagsText = mTg.Groups[4].Value;


                    // stem if required
                    if (stemmer != null || stopwords != null)
                    {
                        string[] parts = verseText.Split(new char[] { ' ' });

                        string processed = string.Empty;
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (stopwords != null && stopwords.Contains(parts[i].ToLower()))
                                continue;

                            if (stemmer != null)
                                processed += stemmer.stemTerm(parts[i]) + " ";
                            else
                                processed += parts[i] + " ";
                        }
                        verseText = processed.Trim();
                    }
                    //foreach (string w in stopwords)
                    //{
                    //    if (verseText.Contains(w.ToLower() + " "))
                    //    {
                    //        container.Trace(string.Format("{0}: {1}", w, verseText), Color.DarkGreen);

                    //        break;
                    //    }
                    //}
                    swText.WriteLine(verseText.ToLower());
                    swTags.WriteLine(tagsText);
                }
            }
        }

        public void ProcessAlignerMapNT(string referenceFilePath,
                        Dictionary<string, string> single_words,
                        Dictionary<string, string> multipleWords)
        {
            Dictionary<string, Statistics> detailedStatistics = new Dictionary<string, Statistics>();
            Dictionary<string, Statistics> bookStatistics = new Dictionary<string, Statistics>();
            List<string> exceptions = new List<string>();

            string currentVerseReference = string.Empty;
            string currentChapterReference = string.Empty;
            string currentBook = string.Empty;
            Statistics chapterStats = null;
            Statistics bookStats = null;
            string unknown = "????";
            string lastBook = string.Empty; // "Exo";

            var referenceFolder = Path.GetDirectoryName(referenceFilePath);
            var outFileName = Path.GetFileNameWithoutExtension(referenceFilePath) + "Tagged.txt";

            string mapFile = "nt_map.txt";
            string mapExceptions = "nt_mapExceptions.txt";
            string statsFile = "nt_stats.txt";

            int bibleTotal = 0;
            int bibleUnmapped = 0;

            int offset = 0;

            string verse = String.Empty;

            using (StreamReader referenceFile_reader = new StreamReader(referenceFilePath))
            using (StreamReader greekTrainTags_reader = new StreamReader(greekTrainTagsPath))
            using (StreamReader ntAlignFile_reader = new StreamReader(ntAlignFilePath))
            using (StreamWriter writer_mapFile = new StreamWriter(Path.Combine(referenceFolder, mapFile)))
            using (StreamWriter referenceTaggedFile_writer = new StreamWriter(Path.Combine(referenceFolder, outFileName)))
            {
                while (!referenceFile_reader.EndOfStream && !greekTrainTags_reader.EndOfStream && !ntAlignFile_reader.EndOfStream)
                {
                    var referenceLine = referenceFile_reader.ReadLine();
                    int space1 = referenceLine.IndexOf(' ');
                    if(space1 == -1)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "space1 not found");
                        continue;
                    }
                    int space2 = referenceLine.IndexOf(' ', space1 + 1);
                    if (space2 == -1)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "space2 not found");
                        continue;
                    }

                    string referenceLineText = referenceLine.Substring(space2+1).Trim();
                    
                    string verseRef = referenceLine.Substring(0, space2).Trim();
                    string bookName = referenceLine.Substring(0, space1);

                    var greekTrainTagsLine = greekTrainTags_reader.ReadLine();
                    if(verseRef.Contains("11:25"))
                    {
                        int x = 0;
                    }
                    var ntAlignFileLine = ntAlignFile_reader.ReadLine();

                    int idx = verseRef.IndexOf(':');
                    if (idx != -1)
                    {
                        string chapterRef = verseRef.Substring(0, idx);
                        if (currentVerseReference != verseRef)
                        {
                            if (string.IsNullOrEmpty(currentVerseReference))
                            {
                                // this is the very first verse
                                chapterStats = new Statistics();
                                bookStats = new Statistics();

                                currentChapterReference = chapterRef;
                                currentBook = bookName;
                                currentVerseReference = verseRef;
                                verse = currentVerseReference;
                                writer_mapFile.WriteLine(String.Format("====\t{0}\t====", currentVerseReference));
                            }
                            else
                            {
                                // We are changing Verse
                                // Are we changing chapter
                                if (currentChapterReference != chapterRef)
                                {
                                    // we are changing chapter
                                    // save the previous chapter
                                    detailedStatistics.Add(currentChapterReference, chapterStats);
                                    chapterStats = new Statistics();
                                    currentChapterReference = chapterRef;
                                    if (bookName != currentBook)
                                    {
                                        bookStatistics.Add(currentBook, bookStats);
                                        bookStats = new Statistics();
                                        currentBook = bookName;
                                        if (currentBook == lastBook)
                                            break;
                                    }
                                }
                                referenceTaggedFile_writer.WriteLine(verse);
                                currentVerseReference = verseRef;
                                verse = currentVerseReference;
                                writer_mapFile.WriteLine(String.Format("====\t{0}\t====", currentVerseReference));
                            }
                        }
                    }

                    string[] reference_parts = referenceLineText.Split(' ');
                    string[] aligner_parts = ntAlignFileLine.Split(' ');
                    string[] tag_parts = greekTrainTagsLine.Split(new char[] { ' ', '\t' });
                    string[] translation = new string[reference_parts.Length];
                    string[] tagTranslation = new string[reference_parts.Length];
                    for (int i = 0; i < translation.Length; i++)
                    {
                        if (string.IsNullOrEmpty(greekTrainTagsLine))
                        {
                            translation[i] = string.Empty;
                            tagTranslation[i] = string.Empty;
                        }
                        else
                        {
                            translation[i] = unknown;
                            tagTranslation[i] = unknown;
                        }
                    }
                    for (int i = 0; i < aligner_parts.Length; i++)
                    {
                        int ai, hi;
                        try
                        {
                            if (!string.IsNullOrEmpty(aligner_parts[i]) && (i + offset) < aligner_parts.Length)
                            {
                                string[] map = aligner_parts[i].Split('-');
                                ai = int.Parse(map[0].Trim());
                                hi = int.Parse(map[1].Trim());
                                tagTranslation[ai + offset] = tag_parts[hi + offset];
                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex.Message);
                        }
                    }


                    string outLine = string.Empty;
                    int mapIndex = 0;
                    for (int j = 0; j < reference_parts.Length; j++)
                    {
                        string referenceWord = reference_parts[j].Trim();
                        bool isPunctuation = referenceWord.Length == 1 ? Char.IsPunctuation(referenceWord[0]) : false;

                        string[] stopwords = single_words.Keys.ToArray();
                        if (j >= translation.Length)
                        {
                            if (single_words == null ||(!stopwords.Contains(referenceWord.ToLower()) &&  !isPunctuation))
                                outLine = String.Format("{0}\t{1}\t{2}", referenceWord, unknown, unknown);
                        }
                        else
                        {
                            
                            if(single_words != null && stopwords.Contains(referenceWord.ToLower()))
                            {
                                string strongs = single_words[referenceWord.ToLower()];
                                if (string.IsNullOrEmpty(strongs))
                                {

                                    if (verse.EndsWith(" <>"))
                                        verse = verse.Substring(0, verse.Length - 3);
                                    verse += string.Format(" {0} <>", referenceWord);
                                }
                                else
                                {
                                    verse += string.Format(" {0} <{1}>", referenceWord, strongs);
                                }
                                continue;
                            }
                            string tagWord = tagTranslation[mapIndex].Replace("G", ""); ;
                            totalTags++;
                            if (tagWord.Contains(unknown)) { unmapped++; }
                            outLine = String.Format("{0}\t{1}\t{2}", referenceWord, translation[mapIndex], tagTranslation[mapIndex]);
                            verse += string.Format(" {0} <{1}>", referenceWord, tagWord);
                            mapIndex++;
                        }
                        writer_mapFile.WriteLine(outLine);
                        if (outLine.Contains(unknown))
                        {
                            if (!exceptions.Contains(outLine))
                                exceptions.Add(outLine);
                        }
                        try
                        {
                            chapterStats.TotalWords++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        bookStats.TotalWords++;
                        bibleTotal++;

                        if (outLine.Contains(unknown))
                        {
                            chapterStats.UnmappedWords++;
                            bookStats.UnmappedWords++;
                            bibleUnmapped++;
                        }
                    }

                }
                // Write last verse
                referenceTaggedFile_writer.WriteLine(verse.Trim());
            }

            using (StreamWriter outputFileEx = new StreamWriter(Path.Combine(referenceFolder, mapExceptions)))
            {
                for (int i = 0; i < exceptions.Count; i++)
                {
                    outputFileEx.WriteLine(exceptions[i]);
                }
            }

            container.Trace(string.Format("total = {0}, unmapped = {1}", totalTags, unmapped), Color.Blue);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(referenceFolder, statsFile)))
            {
                outputFile.WriteLine("Total Stats");
                outputFile.WriteLine(string.Format("Total Bible words = {0}", bibleTotal));
                outputFile.WriteLine(string.Format("Total unmapped words = {0} ({1}%)", bibleUnmapped, (100 * bibleUnmapped / bibleTotal)));
                outputFile.WriteLine("");
                outputFile.WriteLine("Book Stats");
                foreach (string bookName in bookStatistics.Keys)
                {
                    outputFile.WriteLine(string.Format("{0}: words = {1}, unmapped = {2} ({3}%)",
                        bookName,
                        bookStatistics[bookName].TotalWords,
                        bookStatistics[bookName].UnmappedWords,
                        (100 * bookStatistics[bookName].UnmappedWords / bookStatistics[bookName].TotalWords)));

                }
                outputFile.WriteLine("");
                outputFile.WriteLine("Chapter Stats");
                foreach (string chapter in detailedStatistics.Keys)
                {
                    outputFile.WriteLine(string.Format("{0}: words = {1}, unmapped = {2} ({3}%)",
                        chapter,
                        detailedStatistics[chapter].TotalWords,
                        detailedStatistics[chapter].UnmappedWords,
                        (100 * detailedStatistics[chapter].UnmappedWords / detailedStatistics[chapter].TotalWords)));
                }
            }
        }

        public void ProcessAlignerMapNT1(string referenceFilePath, string[] stopwords)
        {
            Dictionary<string, Statistics> detailedStatistics = new Dictionary<string, Statistics>();
            Dictionary<string, Statistics> bookStatistics = new Dictionary<string, Statistics>();
            List<string> exceptions = new List<string>();

            string currentVerseReference = string.Empty;
            string currentChapterReference = string.Empty;
            string currentBook = string.Empty;
            Statistics chapterStats = null;
            Statistics bookStats = null;
            string unknown = "????";
            string lastBook = string.Empty; // "Exo";

            var referenceFolder = Path.GetDirectoryName(referenceFilePath);
            var outFileName = Path.GetFileNameWithoutExtension(referenceFilePath) + "Tagged.txt";

            string mapFile = "nt_map.txt";
            string mapExceptions = "nt_mapExceptions.txt";
            string statsFile = "nt_stats.txt";

            int bibleTotal = 0;
            int bibleUnmapped = 0;

            int offset = 0;

            string verse = String.Empty;

            using (StreamReader referenceFile_reader = new StreamReader(referenceFilePath))
            using (StreamReader greekTrainTags_reader = new StreamReader(greekTrainTagsPath))
            using (StreamReader ntAlignFile_reader = new StreamReader(ntAlignFilePath1))
            using (StreamWriter writer_mapFile = new StreamWriter(Path.Combine(referenceFolder, mapFile)))
            using (StreamWriter referenceTaggedFile_writer = new StreamWriter(Path.Combine(referenceFolder, outFileName)))
            {
                while (!referenceFile_reader.EndOfStream && !greekTrainTags_reader.EndOfStream && !ntAlignFile_reader.EndOfStream)
                {
                    var referenceLine = referenceFile_reader.ReadLine();
                    int space1 = referenceLine.IndexOf(' ');
                    if (space1 == -1)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "space1 not found");
                        continue;
                    }
                    int space2 = referenceLine.IndexOf(' ', space1 + 1);
                    if (space2 == -1)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "space2 not found");
                        continue;
                    }

                    string referenceLineText = referenceLine.Substring(space2 + 1).Trim();

                    string verseRef = referenceLine.Substring(0, space2).Trim();
                    string bookName = referenceLine.Substring(0, space1);

                    var greekTrainTagsLine = greekTrainTags_reader.ReadLine();
                    if (verseRef.Contains("11:25"))
                    {
                        int x = 0;
                    }
                    var ntAlignFileLine = ntAlignFile_reader.ReadLine();

                    int idx = verseRef.IndexOf(':');
                    if (idx != -1)
                    {
                        string chapterRef = verseRef.Substring(0, idx);
                        if (currentVerseReference != verseRef)
                        {
                            if (string.IsNullOrEmpty(currentVerseReference))
                            {
                                // this is the very first verse
                                chapterStats = new Statistics();
                                bookStats = new Statistics();

                                currentChapterReference = chapterRef;
                                currentBook = bookName;
                                currentVerseReference = verseRef;
                                verse = currentVerseReference;
                                writer_mapFile.WriteLine(String.Format("====\t{0}\t====", currentVerseReference));
                            }
                            else
                            {
                                // We are changing Verse
                                // Are we changing chapter
                                if (currentChapterReference != chapterRef)
                                {
                                    // we are changing chapter
                                    // save the previous chapter
                                    detailedStatistics.Add(currentChapterReference, chapterStats);
                                    chapterStats = new Statistics();
                                    currentChapterReference = chapterRef;
                                    if (bookName != currentBook)
                                    {
                                        bookStatistics.Add(currentBook, bookStats);
                                        bookStats = new Statistics();
                                        currentBook = bookName;
                                        if (currentBook == lastBook)
                                            break;
                                    }
                                }
                                referenceTaggedFile_writer.WriteLine(verse);
                                currentVerseReference = verseRef;
                                verse = currentVerseReference;
                                writer_mapFile.WriteLine(String.Format("====\t{0}\t====", currentVerseReference));
                            }
                        }
                    }

                    string[] reference_parts = referenceLineText.Split(' ');
                    string[] aligner_parts = ntAlignFileLine.Split(' ');
                    string[] tag_parts = greekTrainTagsLine.Split(new char[] { ' ', '\t' });
                    string[] translation = new string[reference_parts.Length];
                    string[] tagTranslation = new string[reference_parts.Length];
                    for (int i = 0; i < translation.Length; i++)
                    {
                        if (string.IsNullOrEmpty(greekTrainTagsLine))
                        {
                            translation[i] = string.Empty;
                            tagTranslation[i] = string.Empty;
                        }
                        else
                        {
                            translation[i] = unknown;
                            tagTranslation[i] = unknown;
                        }
                    }
                    for (int i = 0; i < aligner_parts.Length; i++)
                    {
                        int ai, hi;
                        try
                        {
                            if (!string.IsNullOrEmpty(aligner_parts[i]) && (i + offset) < aligner_parts.Length)
                            {
                                string[] map = aligner_parts[i].Split('-');
                                hi = int.Parse(map[0].Trim());
                                ai = int.Parse(map[1].Trim());
                                tagTranslation[ai + offset] = tag_parts[hi + offset];
                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex.Message);
                        }
                    }


                    string outLine = string.Empty;
                    int mapIndex = 0;
                    for (int j = 0; j < reference_parts.Length; j++)
                    {
                        string referenceWord = reference_parts[j].Trim();
                        bool isPunctuation = referenceWord.Length == 1 ? Char.IsPunctuation(referenceWord[0]) : false;

                        if (j >= translation.Length)
                        {
                            if (stopwords == null || (!stopwords.Contains(referenceWord.ToLower()) && !isPunctuation))
                                outLine = String.Format("{0}\t{1}\t{2}", referenceWord, unknown, unknown);
                        }
                        else
                        {

                            if (stopwords != null && (stopwords.Contains(referenceWord.ToLower()) || isPunctuation))
                            {
                                verse += string.Format(" {0} <> ", referenceWord);
                                continue;
                            }
                            string tagWord = tagTranslation[mapIndex].Replace("G", ""); ;
                            totalTags++;
                            if (tagWord.Contains(unknown)) { unmapped++; }
                            outLine = String.Format("{0}\t{1}\t{2}", referenceWord, translation[mapIndex], tagTranslation[mapIndex]);
                            verse += string.Format(" {0} <{1}>", referenceWord, tagWord);
                            mapIndex++;
                        }
                        writer_mapFile.WriteLine(outLine);
                        if (outLine.Contains(unknown))
                        {
                            if (!exceptions.Contains(outLine))
                                exceptions.Add(outLine);
                        }
                        try
                        {
                            chapterStats.TotalWords++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        bookStats.TotalWords++;
                        bibleTotal++;

                        if (outLine.Contains(unknown))
                        {
                            chapterStats.UnmappedWords++;
                            bookStats.UnmappedWords++;
                            bibleUnmapped++;
                        }
                    }

                }
                // Write last verse
                referenceTaggedFile_writer.WriteLine(verse);
            }

            using (StreamWriter outputFileEx = new StreamWriter(Path.Combine(referenceFolder, mapExceptions)))
            {
                for (int i = 0; i < exceptions.Count; i++)
                {
                    outputFileEx.WriteLine(exceptions[i]);
                }
            }

            container.Trace(string.Format("total = {0}, unmapped = {1}", totalTags, unmapped), Color.Blue);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(referenceFolder, statsFile)))
            {
                outputFile.WriteLine("Total Stats");
                outputFile.WriteLine(string.Format("Total Bible words = {0}", bibleTotal));
                outputFile.WriteLine(string.Format("Total unmapped words = {0} ({1}%)", bibleUnmapped, (100 * bibleUnmapped / bibleTotal)));
                outputFile.WriteLine("");
                outputFile.WriteLine("Book Stats");
                foreach (string bookName in bookStatistics.Keys)
                {
                    outputFile.WriteLine(string.Format("{0}: words = {1}, unmapped = {2} ({3}%)",
                        bookName,
                        bookStatistics[bookName].TotalWords,
                        bookStatistics[bookName].UnmappedWords,
                        (100 * bookStatistics[bookName].UnmappedWords / bookStatistics[bookName].TotalWords)));

                }
                outputFile.WriteLine("");
                outputFile.WriteLine("Chapter Stats");
                foreach (string chapter in detailedStatistics.Keys)
                {
                    outputFile.WriteLine(string.Format("{0}: words = {1}, unmapped = {2} ({3}%)",
                        chapter,
                        detailedStatistics[chapter].TotalWords,
                        detailedStatistics[chapter].UnmappedWords,
                        (100 * detailedStatistics[chapter].UnmappedWords / detailedStatistics[chapter].TotalWords)));
                }
            }
        }

        public void ProcessAlignerMapOT(string referenceFilePath, string[] stopwords)
        {
            Dictionary<string, Statistics> detailedStatistics = new Dictionary<string, Statistics>();
            Dictionary<string, Statistics> bookStatistics = new Dictionary<string, Statistics>();
            List<string> exceptions = new List<string>();

            string curreotVerseReference = string.Empty;
            string curreotChapterReference = string.Empty;
            string curreotBook = string.Empty;
            Statistics chapterStats = null;
            Statistics bookStats = null;
            string unknown = "????";
            string lastBook = string.Empty; // "Exo";

            var referenceFolder = Path.GetDirectoryName(referenceFilePath);
            var outFileName = Path.GetFileNameWithoutExtension(referenceFilePath) + "Tagged.txt";

            string mapFile = "ot_map.txt";
            string mapExceptions = "ot_mapExceptions.txt";
            string statsFile = "ot_stats.txt";

            int bibleTotal = 0;
            int bibleUnmapped = 0;

            int offset = 0;

            string verse = String.Empty;

            using (StreamReader referenceFile_reader = new StreamReader(referenceFilePath))
            using (StreamReader hebrewTrainTags_reader = new StreamReader(hebrewTrainTagsPath))
            using (StreamReader otAlignFile_reader = new StreamReader(otAlignFilePath))
            using (StreamWriter writer_mapFile = new StreamWriter(Path.Combine(referenceFolder, mapFile)))
            using (StreamWriter referenceTaggedFile_writer = new StreamWriter(Path.Combine(referenceFolder, outFileName)))
            {
                while (!referenceFile_reader.EndOfStream && !hebrewTrainTags_reader.EndOfStream && !otAlignFile_reader.EndOfStream)
                {
                    var referenceLine = referenceFile_reader.ReadLine();
                    int space1 = referenceLine.IndexOf(' ');
                    if (space1 == -1)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "space1 not found");
                        continue;
                    }
                    int space2 = referenceLine.IndexOf(' ', space1 + 1);
                    if (space2 == -1)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "space2 not found");
                        continue;
                    }

                    string referenceLineText = referenceLine.Substring(space2 + 1).Trim();

                    string verseRef = referenceLine.Substring(0, space2).Trim();
                    string bookName = referenceLine.Substring(0, space1);

                    var hebrewTrainTagsLine = hebrewTrainTags_reader.ReadLine();
                    var otAlignFileLine = otAlignFile_reader.ReadLine();

                    int idx = verseRef.IndexOf(':');
                    if (idx != -1)
                    {
                        string chapterRef = verseRef.Substring(0, idx);
                        if (curreotVerseReference != verseRef)
                        {
                            if (string.IsNullOrEmpty(curreotVerseReference))
                            {
                                // this is the very first verse
                                chapterStats = new Statistics();
                                bookStats = new Statistics();

                                curreotChapterReference = chapterRef;
                                curreotBook = bookName;
                                curreotVerseReference = verseRef;
                                verse = curreotVerseReference;
                                writer_mapFile.WriteLine(String.Format("====\t{0}\t====", curreotVerseReference));
                            }
                            else
                            {
                                // We are changing Verse
                                // Are we changing chapter
                                if (curreotChapterReference != chapterRef)
                                {
                                    // we are changing chapter
                                    // save the previous chapter
                                    detailedStatistics.Add(curreotChapterReference, chapterStats);
                                    chapterStats = new Statistics();
                                    curreotChapterReference = chapterRef;
                                    if (bookName != curreotBook)
                                    {
                                        bookStatistics.Add(curreotBook, bookStats);
                                        bookStats = new Statistics();
                                        curreotBook = bookName;
                                        if (curreotBook == lastBook)
                                            break;
                                    }
                                }
                                referenceTaggedFile_writer.WriteLine(verse);
                                curreotVerseReference = verseRef;
                                verse = curreotVerseReference;
                                writer_mapFile.WriteLine(String.Format("====\t{0}\t====", curreotVerseReference));
                            }
                        }
                    }

                    string[] reference_parts = referenceLineText.Split(' ');
                    string[] aligner_parts = otAlignFileLine.Split(' ');
                    string[] tag_parts = hebrewTrainTagsLine.Split(new char[] { ' ', '\t' });
                    string[] translation = new string[reference_parts.Length];
                    string[] tagTranslation = new string[reference_parts.Length];
                    for (int i = 0; i < translation.Length; i++)
                    {
                        translation[i] = unknown;
                        tagTranslation[i] = unknown;
                    }
                    for (int i = 0; i < aligner_parts.Length; i++)
                    {
                        int ai, hi;
                        try
                        {
                            if (!string.IsNullOrEmpty(aligner_parts[i]) && (i + offset) < aligner_parts.Length)
                            {
                                string[] map = aligner_parts[i].Split('-');
                                ai = int.Parse(map[0].Trim());
                                hi = int.Parse(map[1].Trim());
                                tagTranslation[ai + offset] = tag_parts[hi + offset];
                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex.Message);
                        }
                    }


                    string outLine = string.Empty;
                    int mapIndex = 0;
                    for (int j = 0; j < reference_parts.Length; j++)
                    {
                        string referenceWord = reference_parts[j].Trim();
                        bool isPunctuation = referenceWord.Length == 1 ? Char.IsPunctuation(referenceWord[0]) : false;

                        if (j >= translation.Length)
                        {
                            if (stopwords == null || !stopwords.Contains(referenceWord.ToLower()))
                                outLine = String.Format("{0}\t{1}\t{2}", referenceWord, unknown, unknown);
                        }
                        else
                        {

                            if (stopwords != null && stopwords.Contains(referenceWord.ToLower()))
                            {
                                verse += string.Format(" {0} ", referenceWord);
                                continue;
                            }
                            string tagWord = tagTranslation[mapIndex].Replace("H", "");
                            outLine = String.Format("{0}\t{1}\t{2}", referenceWord, translation[mapIndex], tagTranslation[mapIndex]);
                            verse += string.Format(" {0} <{1}>", referenceWord, tagWord);
                            mapIndex++;
                        }
                        writer_mapFile.WriteLine(outLine);
                        if (outLine.Contains(unknown))
                        {
                            if (!exceptions.Contains(outLine))
                                exceptions.Add(outLine);
                        }
                        try
                        {
                            chapterStats.TotalWords++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        bookStats.TotalWords++;
                        bibleTotal++;

                        if (outLine.Contains(unknown))
                        {
                            chapterStats.UnmappedWords++;
                            bookStats.UnmappedWords++;
                            bibleUnmapped++;
                        }
                    }

                }
                // write last verse
                referenceTaggedFile_writer.WriteLine(verse);

            }

            using (StreamWriter outputFileEx = new StreamWriter(Path.Combine(referenceFolder, mapExceptions)))
            {
                for (int i = 0; i < exceptions.Count; i++)
                {
                    outputFileEx.WriteLine(exceptions[i]);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(referenceFolder, statsFile)))
            {
                outputFile.WriteLine("Total Stats");
                outputFile.WriteLine(string.Format("Total Bible words = {0}", bibleTotal));
                outputFile.WriteLine(string.Format("Total unmapped words = {0} ({1}%)", bibleUnmapped, (100 * bibleUnmapped / bibleTotal)));
                outputFile.WriteLine("");
                outputFile.WriteLine("Book Stats");
                foreach (string bookName in bookStatistics.Keys)
                {
                    outputFile.WriteLine(string.Format("{0}: words = {1}, unmapped = {2} ({3}%)",
                        bookName,
                        bookStatistics[bookName].TotalWords,
                        bookStatistics[bookName].UnmappedWords,
                        (100 * bookStatistics[bookName].UnmappedWords / bookStatistics[bookName].TotalWords)));

                }
                outputFile.WriteLine("");
                outputFile.WriteLine("Chapter Stats");
                foreach (string chapter in detailedStatistics.Keys)
                {
                    outputFile.WriteLine(string.Format("{0}: words = {1}, unmapped = {2} ({3}%)",
                        chapter,
                        detailedStatistics[chapter].TotalWords,
                        detailedStatistics[chapter].UnmappedWords,
                        (100 * detailedStatistics[chapter].UnmappedWords / detailedStatistics[chapter].TotalWords)));
                }
            }
        }

    }

    public class Statistics
    {
        public int TotalWords { get; set; }
        public int UnmappedWords { get; set; }


    }

}
