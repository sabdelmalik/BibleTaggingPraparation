using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleTagging.PorterStemmerAlgorithm
{
    /**
	  * Stemmer, implementing the Porter Stemming Algorithm
	  *
	  * The Stemmer class transforms a word into its root form.  The input
	  * word can be provided a character at time (by calling add()), or at once
	  * by calling one of the various stem(something) methods.
	  */

    public interface StemmerInterface
    {
        string stemTerm(string s);
    }

}
