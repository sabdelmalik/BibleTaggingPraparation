# BibleTaggingPreperation
Prepare Bible text for the Tag Editing Utility

# BibleTaggingPreperation
Prepare Bible text for the Tag Editing Utility

This program is inspired by Dr. Rev. David Instone-Brewer process for
automatically tagging a Bible text with Strong’s numbers using Berkely Aligner.

The program does the following: 
1.	Splits the Bible text into OT and NT parts.
2.	Passes the text through a stemmer if required, and removes a set of stop words if required.
3.	Removes the verse references from the beginning of each verse and tags file.
4.	Places the treated files in their appropriate location within the aligner folder hierarchy.
5.	Runs the aligner for both the OT and NT which generates an aligner map for each.
6.	Uses the resulting maps to generate the tagged OT and NT.
7.	Appends the tagged NT file to the tagged OT file to generate the tagged Bible file. (not yet implemented)
8.	Use the [**BibleTaggingUtility**](https://github.com/sabdelmalik/BibleTaggingUtility) to finalise the tagging by filling in missing tags and correcting as necessary.

The application expects the folder containing the Bible file to be tagged, to contain also a Hebrew tags file (OT_Tags.txt) and a Greek tags file (NT_Tags.txt).<br>
All the files should have a verse per line starting with a verse reference.<br>
The verse reference is expected to be in the format <book> <chapter>:<verse>. e.g. Act 26:17<br>
Example from the bible text file:<br>
Gen 1:1 In the beginning God created the heavens and the earth.<br>
Example from the Hebrew Tags file:<br>
Gen 2:1 3615 8064 0776 3605 6635<br>
Example from the Greek Tags file:<br>
Joh 1:2 3778 1510 1722 0746 4314 3588 2316<br>
The program creates a work folder “.BibleTagging” under the current user home directory
e.g. C:\Users\<user name>\.BibleTagging
# Berkely Aligner Configuration Example
## Training
forwardModels	MODEL2;MODEL1
reverseModels	MODEL1;HMM
mode	JOINT;JOINT
iters	15;15

## Execution
execDir	NT_Map
create	true
overwriteExecDir	true
saveParams	true
numThreads	1
msPerLine	30000
alignTraining

## Language/Data
foreignSuffix	s
englishSuffix	t
lowercase

## Training sources
trainSources	NT_Dataset\test;NT_Dataset\train
sentences	MAX

## Test sources
testSources	NT_Dataset\test
maxTestSentences	MAX
offsetTestSentences	0

## Evaluation
competitiveThresholding
