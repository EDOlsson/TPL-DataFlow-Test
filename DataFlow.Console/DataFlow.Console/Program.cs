using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlow.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // Create members of the pipeline
            //

            // Downloads the requested resource as a string
            var downloadString = new TransformBlock<string, string>(uri =>
            {
                System.Console.WriteLine("Downloading '{0}'...", uri);

                return new WebClient().DownloadString(uri);
            });

            // separates the specified text into an array of words.
            var createWordList = new TransformBlock<string, string[]>(text =>
            {
                System.Console.WriteLine("Creating word list...");

                // Remove common punctuation by replacing all non-letter characters
                // with a space character to [sic].
                var tokens = text.ToArray().Select(c => char.IsLetter(c) ? c : ' ').ToArray();
                text = new string(tokens);

                // Separate the text into an array of words.
                return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            });

            // Removes short words, orders the resulting words alphabetically,
            // and then removes duplicates.
            var filterWordList = new TransformBlock<string[], string[]>(words =>
            {
                System.Console.WriteLine("Filtering word list...");

                return words.Where(w => w.Length > 3)
                            .OrderBy(w => w)
                            .Distinct()
                            .ToArray();
            });

            // Finds  all words in the specified collection whose reverse also
            // exists in the collection.
            var findReversedWords = new TransformManyBlock<string[], string>(words =>
            {
                System.Console.WriteLine("Finding reversed words...");

                var reversedWords = new ConcurrentQueue<string>();

                // Add each word in the original collection to the result whose reversed
                // word also exists in the collection.
                Parallel.ForEach(words, word =>
                {
                    // reverse the word
                    var reverse = new string(word.Reverse().ToArray());

                    // Enqueue the word if the reversed version also exists
                    // in the collection.
                    if (Array.BinarySearch<string>(words, reverse) >= 0 && word != reverse)
                        reversedWords.Enqueue(word);
                });

                return reversedWords;
            });

            // Prints the provided reversed words to the console.
            var printReversedWords = new ActionBlock<string>(reversedWord =>
            {
                System.Console.WriteLine($"Found reversed words {reversedWord}/{new string(reversedWord.Reverse().ToArray())}");
            });

            //
            // Connect the dataflow blocks to form a pipeline
            //

            downloadString.LinkTo(createWordList);
            createWordList.LinkTo(filterWordList);
            filterWordList.LinkTo(findReversedWords);
            findReversedWords.LinkTo(printReversedWords);

            //
            // For each completion task in the pipeline, create a continuation task
            // that marks the next block in the pipeline as completed.
            // A completed dataflow block processes any buffered elements, but does
            // not accept new elements.
            //

            downloadString.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    ((IDataflowBlock)createWordList).Fault(t.Exception);
                else
                    createWordList.Complete();
            });

            createWordList.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    ((IDataflowBlock)filterWordList).Fault(t.Exception);
                else
                    filterWordList.Complete();
            });

            filterWordList.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    ((IDataflowBlock)findReversedWords).Fault(t.Exception);
                else
                    findReversedWords.Complete();
            });

            findReversedWords.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    ((IDataflowBlock)printReversedWords).Fault(t.Exception);
                else
                    printReversedWords.Complete();
            });

            //
            // Process _The Iliad of Homer_ by Homer
            //
            downloadString.Post("http://www.gutenberg.org/files/6130/6130-0.txt");

            // Mark the head of the pipeline as complete. The continuation tasks
            // propagate completion through the pipeline as each part of the
            // pipeline finishes.
            downloadString.Complete();

            printReversedWords.Completion.Wait();
        }
    }
}
