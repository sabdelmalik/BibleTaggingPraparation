using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleTagging
{
    internal class AlignerConfFile
    {
        public AlignerConfFile(string filePath, 
            int iterations1, int iterations2,
            string mapFolderName,
            int threads,
            string foreignSuffix,
            string englishSuffix,
            string trainSourcesFolder,
            string testSourcesFolder) 
        {
            using(StreamWriter sw = new StreamWriter(filePath))
            {
                /*
                ## example.conf
                ## ----------------------
                ## This is an example training script for two HMM
                ## alignment models trained jointly and then decoded
                ## using the competitive thresholding heuristic.
                ##
                ## Increase the iterations and training set size, and
                ## you'll have high quality alignments.

                ##########################################
                # Training: Defines the training regimen #
                ##########################################
                */

                sw.WriteLine("# Training");
                sw.WriteLine("forwardModels\tMODEL1;HMM");
                sw.WriteLine("reverseModels\tMODEL1;HMM");
                sw.WriteLine("mode\tJOINT;JOINT");
                sw.WriteLine(string.Format("iters\t{0};{1}"), iterations1, iterations2);
                sw.WriteLine();
                /*
                ###############################################
                # Execution: Controls output and program flow #
                ###############################################
                */
                sw.WriteLine("# Execution");
                sw.WriteLine(string.Format("execDir\t{0}", mapFolderName));
                sw.WriteLine("create\ttrue");
                sw.WriteLine("overwriteExecDir\ttrue");

                sw.WriteLine("saveParams\ttrue");
                sw.WriteLine(string.Format("numThreads\t{0}", threads));
                sw.WriteLine("msPerLine\t10000");
                sw.WriteLine("alignTraining");
                //sw.WriteLine("leaveTrainingOnDisk");
                //sw.WriteLine("searchForThreshold");
                sw.WriteLine();
                /*
                #################
                # Language/Data #
                #################
                */
                sw.WriteLine("# Language/Data");
                sw.WriteLine(string.Format("foreignSuffix\t{0}", foreignSuffix));
                sw.WriteLine(string.Format("englishSuffix\t{0}", englishSuffix));
                //sw.WriteLine("lowercase");
                sw.WriteLine();
                /*
                # Choose the training sources, which can either be directories or files that list files/directories
                # Note that training on the test set does not peek at the correct answers (no cheating)
                */
                sw.WriteLine("# Training sources");
                sw.WriteLine(string.Format("trainSources\t{0};{1}", testSourcesFolder, trainSourcesFolder));
                sw.WriteLine("sentences\tMAX");
                sw.WriteLine();
                /*
                # The test sources must have hand alignments for all sentence pairs
                */
                sw.WriteLine("# Test sources");
                sw.WriteLine(string.Format("testSources\t{0}", testSourcesFolder));
                sw.WriteLine("maxTestSentences\tMAX");
                sw.WriteLine("offsetTestSentences\t0");
                sw.WriteLine();
                /*
                ##############
                # Evaluation #
                ##############
                */
                sw.WriteLine("# Evaluation");
                sw.WriteLine("competitiveThresholding");
            }

        }
    }
}
