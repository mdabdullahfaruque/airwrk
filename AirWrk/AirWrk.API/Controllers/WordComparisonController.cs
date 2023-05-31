using AirWrk.API.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace AirWrk.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WordComparisonController : ControllerBase
    {
        private readonly ILogger<WordComparisonController> _logger;

        public WordComparisonController(ILogger<WordComparisonController> logger)
        {
            _logger = logger;
        }

        [HttpPost("analyze")]
        public ActionResult<AnalyzeResponseModel> Analyze(AnalyzeRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Text))
                throw new BadHttpRequestException("Text is null or empty.");
            var text = model.Text;
            var responseModel = new AnalyzeResponseModel
            {
                CharCount = GetCharacterCount(text),
                WordCount = GetWordCount(text),
                SentenceCount = GetSentenceCount(text),
                MostFrequentWord = new MostFrequentWord
                {
                    Word = GetMostFrequentWord(text)
                },
                LongestWord = new LongestWord
                {
                    Word = GetLongestWord(text)
                }
            };

            responseModel.MostFrequentWord.Frequency = GetWordFrequency(text, responseModel.MostFrequentWord.Word);
            responseModel.LongestWord.Length = responseModel.LongestWord.Word?.Length ?? 0;
            return Ok(responseModel);
        }

        [HttpPost("similarities")]
        public ActionResult<double> Similarities(SimilaritiesRequestModel model)
        {
            var string1 = model.Text1;
            var string2 = model.Text2;

            if (string.IsNullOrEmpty(string1) || string.IsNullOrEmpty(string2))
            {
                return BadRequest("Both input strings are required.");
            }

            var words1 = string1.Split(' ', '\t', '\n', '\r', '.', '!', '?');
            var words2 = string2.Split(' ', '\t', '\n', '\r', '.', '!', '?');

            var commonWordsCount = words1.Count(word1 =>
                words2.Any(word2 => string.Equals(word1, word2, System.StringComparison.OrdinalIgnoreCase)));

            var percentage = (double)commonWordsCount / Math.Max(words1.Length, words2.Length) * 100;
            return Ok(percentage);
        }

        /*
         * POST /analyze: Accepts a string as input and returns an analysis of the input string. The analysis should include:
           
               Number of characters (without spaces)
               Number of words
               Number of sentences
               Most frequent word and its frequency
               Longest word and its length
           POST /similarities: Accepts two strings as input and returns the percentage of words they have in common.
         */

        #region Supported Methods

        static int GetCharacterCount(string text)
        {
            return text.Length;
        }

        private static int GetWordCount(string text)
        {
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' });
            return words.Length;
        }

        private static int GetSentenceCount(string text)
        {
            var sentences = text.Split('.', '!', '?');
            return sentences.Length;
        }

        private static string GetMostFrequentWord(string text)
        {
            var words = text.Split(' ', '\t', '\n', '\r');
            
            var wordFrequencies = words
                .GroupBy(word => word, StringComparer.OrdinalIgnoreCase)
                .Select(group => new { Word = group.Key, Frequency = group.Count() })
                .OrderByDescending(word => word.Frequency);
            
            var mostFrequentWord = wordFrequencies.FirstOrDefault()?.Word ?? "";

            return mostFrequentWord;
        }

        private static int GetWordFrequency(string text, string word)
        {
            if(string.IsNullOrWhiteSpace(word))
                return 0;
            var words = text.Split(' ', '\t', '\n', '\r');
            
            var frequency = words.Count(w => w.Equals(word));

            return frequency;
        }

        static string GetLongestWord(string text)
        {
            // Split the text into words by whitespace characters
            string[] words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Find the longest word using LINQ
            string longestWord = words.OrderByDescending(word => word.Length).FirstOrDefault();

            return longestWord ?? "";
        }

        #endregion
    }
}