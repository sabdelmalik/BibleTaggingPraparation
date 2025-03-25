using BibleTagging.PorterStemmerAlgorithm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
//using static System.Net.Mime.MediaTypeNames;

namespace BibleTagging
{
    public class BerkeleyAligner
    {
        private BibleTaggingPreperationForm container;
        private const string alignerFolderName = "berkeleyaligner";
        private const string confsFolder = "Confs";
        private const string confFileName = "tagging.conf";
        private const string otMapFolder = "OT_Map";
        private const string ntMapFolder = "NT_Map";
        private const string textToAlignExtension = "t";
        private const string strongsTagsExtension = "s";
        private const string otTrainFolder = "OT_Dataset\\train";
        private const string otTestFolder = "OT_Dataset\\test";
        private const string ntTrainFolder = "NT_Dataset\\train";
        private const string ntTestFolder = "NT_Dataset\\test";


        private string alignerFolderpath = string.Empty;
        private string confsFolderPath = string.Empty;

        private string otMapPath = string.Empty;
        private string ntMapPath = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        private bool makeTagsForign = true;
        private string foreignSuffix = string.Empty;
        private string englishSuffix = string.Empty;

        private string alignFileName = string.Empty;
        private string otTrainTextPath = string.Empty;
        private string hebrewTrainTagsPath = string.Empty;
        private string otAlignFilePath = string.Empty;

        private string ntTrainTextPath = string.Empty;
        private string greekTrainTagsPath = string.Empty;
        private string greekTraindTagsPath = string.Empty;
        private string ntAlignFilePath = string.Empty;

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

            if (makeTagsForign)
            {
                foreignSuffix = strongsTagsExtension;
                englishSuffix = textToAlignExtension;
            }
            else
            {
                foreignSuffix = textToAlignExtension;
                englishSuffix = strongsTagsExtension;
            }

            confsFolderPath = Path.Combine(alignerFolderpath, confsFolder);

            alignFileName = string.Format("training.{0}-{1}.align", englishSuffix, foreignSuffix);
            
            otMapPath = Path.Combine(alignerFolderpath, otMapFolder);
            otAlignFilePath = Path.Combine(otMapPath, alignFileName);

            otTrainTextPath = Path.Combine(alignerFolderpath, otTrainFolder + "\\oldtestament." + textToAlignExtension);
            hebrewTrainTagsPath = Path.Combine(alignerFolderpath, otTrainFolder + "\\oldtestament." + strongsTagsExtension);

            ntMapPath = Path.Combine(alignerFolderpath, ntMapFolder);
            ntAlignFilePath = Path.Combine(ntMapPath, alignFileName);
            ntTrainTextPath = Path.Combine(alignerFolderpath, ntTrainFolder + "\\newtestament." + textToAlignExtension);
            greekTrainTagsPath = Path.Combine(alignerFolderpath, ntTrainFolder + "\\newtestament." + strongsTagsExtension);
            greekTraindTagsPath = Path.Combine(alignerFolderpath, ntTrainFolder + "\\newtestament.txt");
        }

        public bool AlignOT()
        {
            // TODO: ensure that the aligner is prepared before running this
            bool result = true;

            string confsFilePath = Path.Combine(confsFolderPath, confFileName);
            AlignerConfFile confFile = new AlignerConfFile(
                confsFilePath,
                10,                  // iterations1
                10,                  // iterations2
                1,                  // threads
                otMapFolder,        // mapFolderName
                foreignSuffix,
                englishSuffix,
                otTrainFolder,
                otTestFolder
                );

            var dir = new DirectoryInfo(otMapPath);
            if (dir.Exists) dir.Delete(true); // true => recursive delete

            RunBerkelyAligner(confFileName);

            return result;
        }

        public bool AlignNT()
        {
            // TODO: ensure that the aligner is prepared before running this

            bool result = true;

            string confsFilePath = Path.Combine(confsFolderPath, confFileName);
            AlignerConfFile confFile = new AlignerConfFile(
                confsFilePath,
                4,                  // iterations1
                4,                  // iterations2
                1,                  // threads
                ntMapFolder,        // mapFolderName
                foreignSuffix,
                englishSuffix,
                ntTrainFolder,
                ntTestFolder
                );

            var dir = new DirectoryInfo(ntMapPath);
            if (dir.Exists) dir.Delete(true); // true => recursive delete

            RunBerkelyAligner(confFileName);

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


                process.StartInfo.Arguments = "-server -mx4000m -cp berkeleyaligner.jar edu.berkeley.nlp.wordAlignment.Main ++confs/" + confFile;
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
        public void PrepareNT(string sourcePath, string tagsPath, StemmerInterface stemmer, string[] stopwords, string[] stopTags)
        {
            using (StreamReader srText = new StreamReader(sourcePath ))
            using (StreamReader srTags = new StreamReader(tagsPath))
            using (StreamWriter swText = new StreamWriter(ntTrainTextPath))
            using (StreamWriter swTags = new StreamWriter(greekTrainTagsPath))
            using (StreamWriter swdTags = new StreamWriter(greekTraindTagsPath))
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
                    String verseText = mTx.Groups[4].Value;
                    String tagsBook = mTg.Groups[1].Value;
                    String tagsText = mTg.Groups[4].Value;

                    if (string.IsNullOrEmpty(verseText))
                        verseText = "dummy";
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
                                processed += parts[i].Replace("‘", "").Replace("’", "").Replace("“", "").
                                    Replace("”", "").Replace("!", "").Replace("?", "").Replace(")", "").
                                    Replace("(", "").Replace(":", "").Replace(";", "")/*.Replace(",", "").*/
                                    /*Replace(".", "")*/ + " ";
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
                    string[] tagsParts = tagsText.Split(' ');
                    tagsText = string.Empty;
                    string dtagsText = string.Empty;
                    for (int idx = 0; idx < tagsParts.Length; idx++)
                    {
                        string tag = tagsParts[idx];
                        //if (stopTags.Contains(tag))
                        //    continue;
                        if (!string.IsNullOrEmpty(tag))
                        {
                            dtagsText += " " + tag;
                            tagsText += " " + tag.Substring(0,4);
                        }
                    }
                    swdTags.WriteLine(dtagsText.Trim());
                    swTags.WriteLine(tagsText.Trim());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceFilePath"></param>
        /// <param name="single_words"></param>
        /// <param name="multipleWords"></param>
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
            string unknown = ""; // "????";
            string lastBook = string.Empty; // "Exo";

            var referenceFolder = Path.GetDirectoryName(referenceFilePath);
            var outFileName = Path.GetFileNameWithoutExtension(referenceFilePath) + "Tagged.txt";

            string mapFile = "nt_map.txt";
            string mapExceptions = "nt_mapExceptions.txt";
            string statsFile = "nt_stats.txt";

            int bibleTotal = 0;
            int bibleUnmapped = 0;

            string verse = String.Empty;

            using (StreamReader referenceFile_reader = new StreamReader(referenceFilePath))
            using (StreamReader greekTrainTags_reader = new StreamReader(greekTraindTagsPath))
            using (StreamReader greekTrainText_reader = new StreamReader(ntTrainTextPath))
            using (StreamReader ntAlignFile_reader = new StreamReader(ntAlignFilePath))
            using (StreamWriter writer_mapFile = new StreamWriter(Path.Combine(referenceFolder, mapFile)))
            using (StreamWriter referenceTaggedFile_writer = new StreamWriter(Path.Combine(referenceFolder, outFileName)))
            {
                while (!referenceFile_reader.EndOfStream && !greekTrainTags_reader.EndOfStream && !greekTrainText_reader.EndOfStream && !ntAlignFile_reader.EndOfStream)
                {
                    var referenceLine = referenceFile_reader.ReadLine();

                    // Extract reference and text
                    Match mref = Regex.Match(referenceLine, referencePattern1);
                    if (!mref.Success)
                    {
                        container.TraceError(MethodBase.GetCurrentMethod().Name, "Could not detect reference: " + referenceLine);
                        return;
                    }

                    string bookName = mref.Groups[1].Value;
                    string chapter = mref.Groups[2].Value;
                    string verseNum = mref.Groups[3].Value;
                    string verseRef = string.Format("{0} {1}:{2}", bookName, chapter, verseNum);
                    string chapterRef = string.Format("{0} {1}", bookName, chapter);
                    string referenceLineText = mref.Groups[4].Value;

                    var greekTrainTagsLine = greekTrainTags_reader.ReadLine();
                    var greekTrainTextLine = greekTrainText_reader.ReadLine();
                    if(verseRef == "Act 21:24")
                    {
                        int x = 0;
                    }

                    var alignMapLine = ntAlignFile_reader.ReadLine();

                    #region Write Verse line
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
                    #endregion Write Verse line


                    string[] reference_parts = referenceLineText.Split(' ');
                    string[] aligner_parts = alignMapLine.Split(' ');
                    string[] tag_parts = greekTrainTagsLine.Split(new char[] { ' ', '\t' });
                    string[] train_TextParts = greekTrainTextLine.Split(new char[] { ' '});

                    // 
                    Dictionary<int,string> translation = new Dictionary<int, string>();
                    Dictionary<int, string> tagTranslation = new Dictionary<int, string>();

                    // initialize translation arrays
                    //for (int i = 0; i < translation.Length; i++)
                    //{
                    //    if (string.IsNullOrEmpty(greekTrainTagsLine))
                    //    {
                    //        translation[i] = string.Empty;
                    //        tagTranslation[i] = string.Empty;
                    //    }
                    //    else
                    //    {
                    //        translation[i] = unknown;
                    //        tagTranslation[i] = unknown;
                    //    }
                    //}

                    // populate the map dictionary
                    // key = text word index
                    // value = the Strong's number for the indexed word 
                    for (int i = 0; i < aligner_parts.Length; i++)
                    {
                        if(currentVerseReference == "Mat 1:11")
                        {
                            int x = 0;
                        }
                        int textIndex, strongsIndex;
                        try
                        {
                            if (string.IsNullOrEmpty(aligner_parts[i])) { continue; }   
                            string[] map = aligner_parts[i].Split('-');
                            if (makeTagsForign)
                            {
                                    textIndex = int.Parse(map[1].Trim());
                                    strongsIndex = int.Parse(map[0].Trim());
                            }
                            else
                            {
                                    textIndex = int.Parse(map[0].Trim());
                                    strongsIndex = int.Parse(map[1].Trim());
                            }
                            if (!tagTranslation.ContainsKey(textIndex))
                            {
                                tagTranslation.Add(textIndex, tag_parts[strongsIndex]);
                                translation.Add(textIndex, tag_parts[strongsIndex]);
                            }
                            else if (tag_parts[strongsIndex] != tagTranslation[textIndex])
                            {
                                // this may be a repeated word
                                bool success = false;

                                for (int j = 0; j < train_TextParts.Length; j++)
                                {
                                    if (i != textIndex &&
                                        train_TextParts[j] == train_TextParts[textIndex] &&
                                        !tagTranslation.ContainsKey(j))
                                    {
                                        tagTranslation.Add(j, tag_parts[strongsIndex]);
                                        translation.Add(j, tag_parts[strongsIndex]);
                                        success = true;
                                        break;
                                    }

                                }
                                //if (!success)
                                //    container.TraceError(MethodBase.GetCurrentMethod().Name,
                                //        string.Format("index {0}, tag {1} was previosly mapped to {2}",
                                //        textIndex, tag_parts[strongsIndex], tagTranslation[textIndex]));
                            }
                        }
                        catch (Exception ex)
                        {
                            //container.TraceError(MethodBase.GetCurrentMethod().Name, ex.Message);
                        }
                    }


                    string outLine = string.Empty;
                    int mapIdx = 0;
                    string[] stopwords = single_words.Keys.ToArray();
                    for (int j = 0; j < reference_parts.Length; j++)
                    {
                        try
                        { 
                        string referenceWord = reference_parts[j].Trim();

                            //if (j >= translation.Count)
                            //{
                            //    if (single_words == null || (!stopwords.Contains(referenceWord.ToLower()) && !isPunctuation))
                            //        outLine = String.Format("{0}\t{1}\t{2}", referenceWord, unknown, unknown);
                            //}
                            //else
                            {

                                if (single_words != null && stopwords.Contains(referenceWord.ToLower()))
                                {
                                    //string strongs = single_words[referenceWord.ToLower()];
                                    string strongs = string.Empty;
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
                                if (tagTranslation.Keys.Contains(mapIdx))
                                {
                                    string tag = tagTranslation[mapIdx];
                                    string tagWord = tag.Replace("G", ""); ;
                                    totalTags++;
//                                    if (tagWord.Contains(unknown)) { unmapped++; }
                                    outLine = String.Format("{0}\t{1}\t{2}", referenceWord, tag, tagTranslation[mapIdx]);
                                    verse += string.Format(" {0} <{1}>", referenceWord, tagWord);
                                }
                                else
                                {
                                    unmapped++;
                                    verse += string.Format(" {0} <{1}>", referenceWord, unknown);
                                }
                                mapIdx++;
                            }
                            
                        }
                        catch(Exception ex)
                        {
                            container.TraceError(MethodBase.GetCurrentMethod().Name, ex.ToString());
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
                            container.TraceError(MethodBase.GetCurrentMethod().Name, ex.ToString());
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
                    var alignMapLine = otAlignFile_reader.ReadLine();

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
                    string[] aligner_parts = alignMapLine.Split(' ');
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
                        int textIndex, strongsIndex;
                        try
                        {
                            if (!string.IsNullOrEmpty(aligner_parts[i]) && i < aligner_parts.Length)
                            {
                                string[] map = aligner_parts[i].Split('-');
                                textIndex = int.Parse(map[0].Trim());
                                strongsIndex = int.Parse(map[1].Trim());
                                tagTranslation[textIndex] = tag_parts[strongsIndex];
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
