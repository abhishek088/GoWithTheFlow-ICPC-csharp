//Group 3: Mereesh Moothatt, Pengshuo Liu, Pierre Loubateres, Stephen Chintalapudi, Abhishek Sawant
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;


namespace RiverFormation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants and Variables
        //setting global variables
        const string Title_Dialog = "Open Text File";
        const string Filter = "TXT files|*.txt";

        readonly List<string> totalWords = new List<string>();
        readonly List<string> longRiverLines = new List<string>();

        List<string> singleLineWords = new List<string>();

        int numberOfLines = 0;
        int maxWordlength = 0;
        int totalLength = 0;
        int longRiverSize = 0;
        int lineWidthForLongRiverSize = 0;

        char[,] formedRiverLines;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            ResetEverything();
            ReadFileAndStoreWords(); // Read a file and filter text files
            ProcessText();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetEverything();
        }

        #region Methods
        private void ReadFileAndStoreWords()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = Title_Dialog;
            openFileDialog.Filter = Filter;

            if (openFileDialog.ShowDialog() == true)
            {
                //read all lines and words are stored into an array totalWords.
                using StreamReader reader = new StreamReader(openFileDialog.OpenFile());

                while (!reader.EndOfStream)
                {
                    singleLineWords = reader.ReadLine().Split(' ').ToList<string>();
                    numberOfLines++;
                    totalWords.AddRange(singleLineWords.Where(word => !word.Equals("")).Select(word => word.Trim()));//get all words in a list.
                }
            }
        }

        private void ProcessText()
        {
            int tempLength = 0;
            
            foreach (string word in totalWords) // Find the largest word length in the text. to set as minimum line width
            {
                tempLength += (word.Length + 1);
                if (word.Length > maxWordlength)
                    maxWordlength = word.Length;
            }
            //Find the total length of the text. This length is further used
            totalLength = tempLength - numberOfLines;

            //largest word length sets as minimum line width
            int minLineWidth = maxWordlength;

            //Find the second last and the last words of the text to set the maximum line width
            int secondLastWordLength = (totalWords[totalWords.Count - 2]).Length;// find the second last word of the length
            int lastWordLength = (totalWords.Last<string>()).Length;// find the last word of the text
            int maxLineWidth = totalLength - (secondLastWordLength + 1 + lastWordLength); // 1 is the length of the space b/n last two words
            var output = GetLongestRiver(minLineWidth, maxLineWidth);
            longRiverSize = output.Item1;
            lineWidthForLongRiverSize = output.Item2;
            formedRiverLines = FormLinesForEachLineWidth(lineWidthForLongRiverSize);

            for (int i = 0; i < formedRiverLines.GetLength(0); i++)
            {
                string value = "";

                for (int j = 0; j < formedRiverLines.GetLength(1); j++)
                    value += formedRiverLines[i, j].ToString();

                if (formedRiverLines[i, 0] != '\0')
                    longRiverLines.Add(value + "|");
            }
            txtRiverSize.Text = longRiverSize.ToString();
            txtLinewidth.Text = lineWidthForLongRiverSize.ToString();

            foreach (string line in longRiverLines)
                lblRiverFormation.Items.Add(line);
        }

        private (int, int) GetLongestRiver(int minLineWidth, int maxLineWidth)
        {
            int longestRiverSize = 0;
            int lineWidthForLongestRiverSize = 0;

            for (int lineWidth = minLineWidth; lineWidth <= maxLineWidth; lineWidth++)
            {
                char[,] formedLines = FormLinesForEachLineWidth(lineWidth);
                int riverSize = FindRiverSizeForEachLineWidth(formedLines);

                if (riverSize > longestRiverSize)
                {
                    longestRiverSize = riverSize;
                    lineWidthForLongestRiverSize = lineWidth;
                }
            }
            return (longestRiverSize, lineWidthForLongestRiverSize);// returning both longest river size and line width for the longest river size.
        }

        private int FindRiverSizeForEachLineWidth(char[,] formedLines)
        {
            int longRiverSize = 0;
            int linesSize = formedLines.GetLength(0);
            int lineWidth = formedLines.GetLength(1);
            int line = 0;

            for (int k = 0; k < linesSize; k++)// to traverse all the lines
            {
                if (formedLines[k, 0] == '\0')
                    break;

                for (int j = 1; j <= lineWidth; j++)// to traverse all the characters
                {
                    int riverSize = 1;
                    int traverseSpaceIndex = j - 1;

                    if (j == lineWidth)
                        line++;

                    for (int i = line; i < linesSize; i++) // to traverse th subsequent lines to find the space indexes
                    {
                        // setting current line and space index, and next line indexes for the current, previous, and next space index
                        int currentLineIndex = i;
                        int nextLineIndex = currentLineIndex + 1;
                        int currentSpaceIndex = traverseSpaceIndex;
                        int previousSpaceIndex = currentSpaceIndex - 1;
                        int nextSpaceIndex = currentSpaceIndex + 1;

                        bool isValidIndexFound = false;
                        
                        if (formedLines[currentLineIndex, currentSpaceIndex] == ' '
                            && (nextLineIndex < linesSize)
                            && IsValidSpaceIndex(formedLines, currentLineIndex, currentSpaceIndex, lineWidth))
                        {
                            if (IsValidNextLineSpaceIndex(formedLines, nextLineIndex, currentSpaceIndex, lineWidth))
                            {
                                riverSize++;
                                isValidIndexFound = true;
                                traverseSpaceIndex = currentSpaceIndex;//setting the currentspaceindex as traverse space index for the nextline 
                            }
                            else if (IsValidNextLineSpaceIndex(formedLines, nextLineIndex, previousSpaceIndex, lineWidth))
                            {
                                riverSize++;
                                isValidIndexFound = true;
                                traverseSpaceIndex = previousSpaceIndex;//setting the previousSpaceIndex as traverse space index for the nextline 
                            }
                            else if (IsValidNextLineSpaceIndex(formedLines, nextLineIndex, nextSpaceIndex, lineWidth))
                            {
                                riverSize++;
                                isValidIndexFound = true;
                                traverseSpaceIndex = nextSpaceIndex;//setting the nextSpaceIndex as traverse space index for the nextline 
                            }
                            else
                            {
                                riverSize += 0;
                                isValidIndexFound = false;
                                traverseSpaceIndex = 0;
                            }
                        }
                        if (!isValidIndexFound)
                            break;
                    }
                    if (riverSize > longRiverSize)
                        longRiverSize = riverSize;
                }
            }
            return longRiverSize;
        }

        private bool IsValidNextLineSpaceIndex(char[,] formedLines, int nextLineIndex, int spaceIndex, int lineWidth)
        {
            return (formedLines[nextLineIndex, spaceIndex] == ' ' && IsValidSpaceIndex(formedLines, nextLineIndex, spaceIndex, lineWidth));
        }

        private bool IsValidSpaceIndex(char[,] formedLines, int currentLineIndex, int currentSpaceIndex, int lineWidth)
        {
            return ((currentSpaceIndex + 1) < lineWidth
                && formedLines[currentLineIndex, (currentSpaceIndex + 1)] != ' '
                && formedLines[currentLineIndex, (currentSpaceIndex + 1)] != '\0');
        }

        private char[,] FormLinesForEachLineWidth(int lineWidth)
        {
            int linesSize = totalWords.Count;
            int currentWidth = 0;
            int currentLine = 0;
            int count = 0;

            char[,] formedLines = new char[linesSize, lineWidth];

            foreach (string word in totalWords)
            {
                count++;
                bool lastWord = false;
                if (count == totalWords.Count())
                    lastWord = true;

                bool flag = true;//This flag is set to perform when the currentLine is done with LineWdith.
                char[] temp = word.ToCharArray();

                int tempLength = word.Length;
                int diff = lineWidth - currentWidth;

                if (diff >= tempLength)
                {
                    currentWidth = ProcessWord(lineWidth, formedLines, currentWidth, currentLine, temp, tempLength, lastWord);
                    flag = false;
                }
                else
                {
                    // This code is to append spaces as the new word is not able to incorporate in the current line.
                    if (lineWidth > currentWidth)
                        for (int i = currentWidth; i < lineWidth; i++)
                            formedLines[currentLine, i] = ' ';

                    // New line starts with 0th position
                    currentLine++; currentWidth = 0;
                    /* When the flag is true, the unprocessed word is processed and added to the lines formation(new line).  
                    This is the same code  used to process the word in the current line above. Refer the condition if (diff >= tempLEngth)*/
                    if (flag)
                        currentWidth = ProcessWord(lineWidth, formedLines, currentWidth, currentLine, temp, tempLength, lastWord);
                }
            }
            return formedLines;
        }

        private int ProcessWord(int lineWidth, char[,] formedLines, int currentWidth, int currentLine, char[] temp, int tempLength, bool lastWord)
        {
            for (int i = 0; i < tempLength; i++)
            {
                formedLines[currentLine, currentWidth] = temp[i];
                if (currentWidth < lineWidth)
                    currentWidth++;
            }

            if (currentWidth < lineWidth)
            {
                if (!lastWord)
                    formedLines[currentLine, currentWidth] = ' ';
                if (currentWidth < lineWidth)
                    currentWidth++;
            }
            return currentWidth;
        }

        private void ResetEverything()
        {
            longRiverSize = 0;
            lineWidthForLongRiverSize = 0;
            txtLinewidth.Text = "";
            txtRiverSize.Text = "";
            totalWords.Clear();
            longRiverLines.Clear();
            lblRiverFormation.Items.Clear();
        }
        #endregion
    }
}
